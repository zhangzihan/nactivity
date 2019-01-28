using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public abstract class AbstractSetProcessDefinitionStateCmd : ICommand<object>
    {

        protected internal string processDefinitionId;
        protected internal string processDefinitionKey;
        protected internal IProcessDefinitionEntity processDefinitionEntity;
        protected internal bool includeProcessInstances = false;
        protected internal DateTime? executionDate;
        protected internal string tenantId;

        public AbstractSetProcessDefinitionStateCmd(IProcessDefinitionEntity processDefinitionEntity, bool includeProcessInstances, DateTime? executionDate, string tenantId)
        {
            this.processDefinitionEntity = processDefinitionEntity;
            this.includeProcessInstances = includeProcessInstances;
            this.executionDate = executionDate;
            this.tenantId = tenantId;
        }

        public AbstractSetProcessDefinitionStateCmd(string processDefinitionId, string processDefinitionKey, bool includeProcessInstances, DateTime? executionDate, string tenantId)
        {
            this.processDefinitionId = processDefinitionId;
            this.processDefinitionKey = processDefinitionKey;
            this.includeProcessInstances = includeProcessInstances;
            this.executionDate = executionDate;
            this.tenantId = tenantId;
        }

        public virtual object execute(ICommandContext commandContext)
        {

            IList<IProcessDefinitionEntity> processDefinitions = findProcessDefinition(commandContext);

            if (executionDate.HasValue)
            {
                // Process definition state change is delayed
                createTimerForDelayedExecution(commandContext, processDefinitions);
            }
            else
            { // Process definition state is changed now
                changeProcessDefinitionState(commandContext, processDefinitions);
            }

            return null;
        }

        protected internal virtual IList<IProcessDefinitionEntity> findProcessDefinition(ICommandContext commandContext)
        {

            // If process definition is already provided (eg. when command is called through the DeployCmd)
            // we don't need to do an extra database fetch and we can simply return it, wrapped in a list
            if (processDefinitionEntity != null)
            {
                return new List<IProcessDefinitionEntity>(new IProcessDefinitionEntity[] { processDefinitionEntity });
            }

            // Validation of input parameters
            if (string.IsNullOrWhiteSpace(processDefinitionId) && string.IsNullOrWhiteSpace(processDefinitionKey))
            {
                throw new ActivitiIllegalArgumentException("Process definition id or key cannot be null");
            }

            IList<IProcessDefinitionEntity> processDefinitionEntities = new List<IProcessDefinitionEntity>();
            IProcessDefinitionEntityManager processDefinitionManager = commandContext.ProcessDefinitionEntityManager;

            if (!string.IsNullOrWhiteSpace(processDefinitionId))
            {

                IProcessDefinitionEntity processDefinitionEntity = processDefinitionManager.findById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("id", processDefinitionId));
                if (processDefinitionEntity == null)
                {
                    throw new ActivitiObjectNotFoundException("Cannot find process definition for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }
                processDefinitionEntities.Add(processDefinitionEntity);

            }
            else
            {

                IProcessDefinitionQuery query = new ProcessDefinitionQueryImpl(commandContext).processDefinitionKey(processDefinitionKey);

                if (ReferenceEquals(tenantId, null) || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
                {
                    query.processDefinitionWithoutTenantId();
                }
                else
                {
                    query.processDefinitionTenantId(tenantId);
                }

                IList<IProcessDefinition> processDefinitions = query.list();
                if (processDefinitions.Count == 0)
                {
                    throw new ActivitiException("Cannot find process definition for key '" + processDefinitionKey + "'");
                }

                foreach (IProcessDefinition processDefinition in processDefinitions)
                {
                    processDefinitionEntities.Add((IProcessDefinitionEntity)processDefinition);
                }

            }
            return processDefinitionEntities;
        }

        protected internal virtual void createTimerForDelayedExecution(ICommandContext commandContext, IList<IProcessDefinitionEntity> processDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                ITimerJobEntity timer = commandContext.TimerJobEntityManager.create();
                timer.JobType = Job_Fields.JOB_TYPE_TIMER;
                timer.ProcessDefinitionId = processDefinition.Id;

                // Inherit tenant identifier (if applicable)
                if (!string.IsNullOrWhiteSpace(processDefinition.TenantId))
                {
                    timer.TenantId = processDefinition.TenantId;
                }

                timer.Duedate = executionDate;
                timer.JobHandlerType = DelayedExecutionJobHandlerType;
                timer.JobHandlerConfiguration = TimerChangeProcessDefinitionSuspensionStateJobHandler.createJobHandlerConfiguration(includeProcessInstances);
                commandContext.JobManager.scheduleTimerJob(timer);
            }
        }

        protected internal virtual void changeProcessDefinitionState(ICommandContext commandContext, IList<IProcessDefinitionEntity> processDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                SuspensionState_SuspensionStateUtil.setSuspensionState(processDefinition, ProcessDefinitionSuspensionState);

                // Evict cache
                commandContext.ProcessEngineConfiguration.DeploymentManager.ProcessDefinitionCache.remove(processDefinition.Id);

                // Suspend process instances (if needed)
                if (includeProcessInstances)
                {

                    int currentStartIndex = 0;
                    IList<IProcessInstance> processInstances = fetchProcessInstancesPage(commandContext, processDefinition, currentStartIndex);
                    while (processInstances.Count > 0)
                    {

                        foreach (IProcessInstance processInstance in processInstances)
                        {
                            AbstractSetProcessInstanceStateCmd processInstanceCmd = getProcessInstanceChangeStateCmd(processInstance);
                            processInstanceCmd.execute(commandContext);
                        }

                        // Fetch new batch of process instances
                        currentStartIndex += processInstances.Count;
                        processInstances = fetchProcessInstancesPage(commandContext, processDefinition, currentStartIndex);
                    }
                }
            }
        }

        protected internal virtual IList<IProcessInstance> fetchProcessInstancesPage(ICommandContext commandContext, IProcessDefinition processDefinition, int currentPageStartIndex)
        {

            if (SuspensionState_Fields.ACTIVE.Equals(ProcessDefinitionSuspensionState))
            {
                return (new ProcessInstanceQueryImpl(commandContext)).processDefinitionId(processDefinition.Id).suspended().listPage(currentPageStartIndex, commandContext.ProcessEngineConfiguration.BatchSizeProcessInstances);
            }
            else
            {
                return (new ProcessInstanceQueryImpl(commandContext)).processDefinitionId(processDefinition.Id).active().listPage(currentPageStartIndex, commandContext.ProcessEngineConfiguration.BatchSizeProcessInstances);
            }
        }

        // ABSTRACT METHODS
        // ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Subclasses should return the wanted <seealso cref="ISuspensionState"/> here.
        /// </summary>
        protected internal abstract ISuspensionState ProcessDefinitionSuspensionState { get; }

        /// <summary>
        /// Subclasses should return the type of the <seealso cref="IJobHandler"/> here. it will be used when the user provides an execution date on which the actual state change will happen.
        /// </summary>
        protected internal abstract string DelayedExecutionJobHandlerType { get; }

        /// <summary>
        /// Subclasses should return a <seealso cref="Command"/> implementation that matches the process definition state change.
        /// </summary>
        protected internal abstract AbstractSetProcessInstanceStateCmd getProcessInstanceChangeStateCmd(IProcessInstance processInstance);

    }

}