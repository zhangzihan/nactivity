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
namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.repository;
    using Sys.Workflow.engine.runtime;

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

        public virtual object Execute(ICommandContext commandContext)
        {

            IList<IProcessDefinitionEntity> processDefinitions = FindProcessDefinition(commandContext);

            if (executionDate.HasValue)
            {
                // Process definition state change is delayed
                CreateTimerForDelayedExecution(commandContext, processDefinitions);
            }
            else
            { // Process definition state is changed now
                ChangeProcessDefinitionState(commandContext, processDefinitions);
            }

            return null;
        }

        protected internal virtual IList<IProcessDefinitionEntity> FindProcessDefinition(ICommandContext commandContext)
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

                IProcessDefinitionEntity processDefinitionEntity = processDefinitionManager.FindById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("id", processDefinitionId));
                if (processDefinitionEntity == null)
                {
                    throw new ActivitiObjectNotFoundException("Cannot find process definition for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }
                processDefinitionEntities.Add(processDefinitionEntity);

            }
            else
            {

                IProcessDefinitionQuery query = new ProcessDefinitionQueryImpl(commandContext).SetProcessDefinitionKey(processDefinitionKey);

                if (tenantId is null || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
                {
                    query.SetProcessDefinitionWithoutTenantId();
                }
                else
                {
                    query.SetProcessDefinitionTenantId(tenantId);
                }

                IList<IProcessDefinition> processDefinitions = query.List();
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

        protected internal virtual void CreateTimerForDelayedExecution(ICommandContext commandContext, IList<IProcessDefinitionEntity> processDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                ITimerJobEntity timer = commandContext.TimerJobEntityManager.Create();
                timer.JobType = JobFields.JOB_TYPE_TIMER;
                timer.ProcessDefinitionId = processDefinition.Id;

                // Inherit tenant identifier (if applicable)
                if (!string.IsNullOrWhiteSpace(processDefinition.TenantId))
                {
                    timer.TenantId = processDefinition.TenantId;
                }

                timer.Duedate = executionDate;
                timer.JobHandlerType = DelayedExecutionJobHandlerType;
                timer.JobHandlerConfiguration = TimerChangeProcessDefinitionSuspensionStateJobHandler.CreateJobHandlerConfiguration(includeProcessInstances);
                commandContext.JobManager.ScheduleTimerJob(timer);
            }
        }

        protected internal virtual void ChangeProcessDefinitionState(ICommandContext commandContext, IList<IProcessDefinitionEntity> processDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                SuspensionStateUtil.SetSuspensionState(processDefinition, ProcessDefinitionSuspensionState);

                // Evict cache
                commandContext.ProcessEngineConfiguration.DeploymentManager.ProcessDefinitionCache.Remove(processDefinition.Id);

                // Suspend process instances (if needed)
                if (includeProcessInstances)
                {

                    int currentStartIndex = 0;
                    IList<IProcessInstance> processInstances = FetchProcessInstancesPage(commandContext, processDefinition, currentStartIndex);
                    while (processInstances.Count > 0)
                    {

                        foreach (IProcessInstance processInstance in processInstances)
                        {
                            AbstractSetProcessInstanceStateCmd processInstanceCmd = GetProcessInstanceChangeStateCmd(processInstance);
                            processInstanceCmd.Execute(commandContext);
                        }

                        // Fetch new batch of process instances
                        currentStartIndex += processInstances.Count;
                        processInstances = FetchProcessInstancesPage(commandContext, processDefinition, currentStartIndex);
                    }
                }
            }
        }

        protected internal virtual IList<IProcessInstance> FetchProcessInstancesPage(ICommandContext commandContext, IProcessDefinition processDefinition, int currentPageStartIndex)
        {

            if (SuspensionStateProvider.ACTIVE.Equals(ProcessDefinitionSuspensionState))
            {
                return (new ProcessInstanceQueryImpl(commandContext)).SetProcessDefinitionId(processDefinition.Id).Suspended().ListPage(currentPageStartIndex, commandContext.ProcessEngineConfiguration.BatchSizeProcessInstances);
            }
            else
            {
                return (new ProcessInstanceQueryImpl(commandContext)).SetProcessDefinitionId(processDefinition.Id).Active().ListPage(currentPageStartIndex, commandContext.ProcessEngineConfiguration.BatchSizeProcessInstances);
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
        protected internal abstract AbstractSetProcessInstanceStateCmd GetProcessInstanceChangeStateCmd(IProcessInstance processInstance);

    }

}