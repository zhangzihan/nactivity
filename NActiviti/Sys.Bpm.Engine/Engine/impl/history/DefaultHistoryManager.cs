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

namespace org.activiti.engine.impl.history
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.persistence;
    using org.activiti.engine.impl.persistence.cache;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;
    using Sys;

    /// <summary>
    /// Manager class that centralises recording of all history-related operations that are originated from inside the engine.
    /// 
    /// </summary>
    public class DefaultHistoryManager : AbstractManager, IHistoryManager
    {
        private static readonly ILogger<DefaultHistoryManager> log = ProcessEngineServiceProvider.LoggerService<DefaultHistoryManager>();

        private HistoryLevel historyLevel;

        public DefaultHistoryManager(ProcessEngineConfigurationImpl processEngineConfiguration, HistoryLevel historyLevel) : base(processEngineConfiguration)
        {
            this.historyLevel = historyLevel;
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# isHistoryLevelAtLeast(org.activiti.engine.impl.history.HistoryLevel)
         */
        public virtual bool isHistoryLevelAtLeast(HistoryLevel level)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Current history level: {historyLevel}, level required: {level}");
            }
            // Comparing enums actually compares the location of values declared in
            // the enum
            return historyLevel.isAtLeast(level);
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#isHistoryEnabled ()
         */
        public virtual bool HistoryEnabled
        {
            get
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Current history level: {historyLevel}");
                }
                return !historyLevel.Equals(HistoryLevel.NONE);
            }
        }

        // Process related history

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordProcessInstanceEnd(java.lang.String, java.lang.String, java.lang.String)
         */
        public virtual void recordProcessInstanceEnd(string processInstanceId, string deleteReason, string activityId)
        {

            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("processInstanceId", processInstanceId));

                if (historicProcessInstance != null)
                {
                    historicProcessInstance.markEnded(deleteReason);
                    historicProcessInstance.EndActivityId = activityId;

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_ENDED, historicProcessInstance));
                    }

                }
            }
        }

        public virtual void recordProcessInstanceNameChange(string processInstanceId, string newName)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", processInstanceId));

                if (historicProcessInstance != null)
                {
                    historicProcessInstance.Name = newName;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordProcessInstanceStart (org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void recordProcessInstanceStart(IExecutionEntity processInstance, FlowElement startElement)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.create(processInstance);
                historicProcessInstance.StartActivityId = startElement.Id;

                // Insert historic process-instance
                HistoricProcessInstanceEntityManager.insert(historicProcessInstance, false);

                // Fire event
                IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                {
                    activitiEventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_CREATED, historicProcessInstance));
                }

            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordSubProcessInstanceStart (org.activiti.engine.impl.persistence.entity.ExecutionEntity,
         * org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void recordSubProcessInstanceStart(IExecutionEntity parentExecution, IExecutionEntity subProcessInstance, FlowElement initialElement)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {

                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.create(subProcessInstance);

                // Fix for ACT-1728: startActivityId not initialized with subprocess instance
                if (ReferenceEquals(historicProcessInstance.StartActivityId, null))
                {
                    historicProcessInstance.StartActivityId = initialElement.Id;
                }
                HistoricProcessInstanceEntityManager.insert(historicProcessInstance, false);

                // Fire event
                IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                {
                    activitiEventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_CREATED, historicProcessInstance));
                }

                IHistoricActivityInstanceEntity activitiyInstance = findActivityInstance(parentExecution, false, true);
                if (activitiyInstance != null)
                {
                    activitiyInstance.CalledProcessInstanceId = subProcessInstance.ProcessInstanceId;
                }

            }
        }

        // Activity related history

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordActivityStart (org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void recordActivityStart(IExecutionEntity executionEntity)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (executionEntity.ActivityId != null && executionEntity.CurrentFlowElement != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstanceEntity = null;

                    // Historic activity instance could have been created (but only in cache, never persisted)
                    // for example when submitting form properties
                    IHistoricActivityInstanceEntity historicActivityInstanceEntityFromCache = getHistoricActivityInstanceFromCache(executionEntity.Id, executionEntity.ActivityId, true);
                    if (historicActivityInstanceEntityFromCache != null)
                    {
                        historicActivityInstanceEntity = historicActivityInstanceEntityFromCache;
                    }
                    else
                    {
                        historicActivityInstanceEntity = createHistoricActivityInstanceEntity(executionEntity);
                    }

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_CREATED, historicActivityInstanceEntity));
                    }

                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordActivityEnd (org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void recordActivityEnd(IExecutionEntity executionEntity, string deleteReason)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricActivityInstanceEntity historicActivityInstance = findActivityInstance(executionEntity, false, true);
                if (historicActivityInstance != null)
                {
                    historicActivityInstance.markEnded(deleteReason);

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_ENDED, historicActivityInstance));
                    }
                }
            }
        }

        public virtual IHistoricActivityInstanceEntity findActivityInstance(IExecutionEntity execution, bool createOnNotFound, bool endTimeMustBeNull)
        {
            string activityId = null;
            if (execution.CurrentFlowElement is FlowNode)
            {
                activityId = execution.CurrentFlowElement.Id;
            }
            else if (execution.CurrentFlowElement is SequenceFlow && execution.CurrentActivitiListener == null)
            { // while executing sequence flow listeners, we don't want historic activities
                activityId = ((SequenceFlow)(execution.CurrentFlowElement)).SourceFlowElement.Id;
            }

            if (!ReferenceEquals(activityId, null))
            {
                return findActivityInstance(execution, activityId, createOnNotFound, endTimeMustBeNull);
            }

            return null;
        }


        public virtual IHistoricActivityInstanceEntity findActivityInstance(IExecutionEntity execution, string activityId, bool createOnNotFound, bool endTimeMustBeNull)
        {

            // No use looking for the HistoricActivityInstance when no activityId is provided.
            if (ReferenceEquals(activityId, null))
            {
                return null;
            }

            string executionId = execution.Id;

            // Check the cache
            IHistoricActivityInstanceEntity historicActivityInstanceEntityFromCache = getHistoricActivityInstanceFromCache(executionId, activityId, endTimeMustBeNull);
            if (historicActivityInstanceEntityFromCache != null)
            {
                return historicActivityInstanceEntityFromCache;
            }

            // If the execution was freshly created, there is no need to check the database, 
            // there can never be an entry for a historic activity instance with this execution id.
            if (!execution.Inserted && !execution.ProcessInstanceType)
            {

                // Check the database
                IList<IHistoricActivityInstanceEntity> historicActivityInstances = HistoricActivityInstanceEntityManager.findUnfinishedHistoricActivityInstancesByExecutionAndActivityId(executionId, activityId);

                if (historicActivityInstances.Count > 0)
                {
                    return historicActivityInstances[0];
                }

            }

            if (!ReferenceEquals(execution.ParentId, null))
            {
                IHistoricActivityInstanceEntity historicActivityInstanceFromParent = findActivityInstance(execution.Parent, activityId, false, endTimeMustBeNull); // always false for create, we only check if it can be found
                if (historicActivityInstanceFromParent != null)
                {
                    return historicActivityInstanceFromParent;
                }
            }

            if (createOnNotFound && !ReferenceEquals(activityId, null) && ((execution.CurrentFlowElement != null && execution.CurrentFlowElement is FlowNode) || execution.CurrentFlowElement == null))
            {
                return createHistoricActivityInstanceEntity(execution);
            }

            return null;
        }

        protected internal virtual IHistoricActivityInstanceEntity getHistoricActivityInstanceFromCache(string executionId, string activityId, bool endTimeMustBeNull)
        {
            IList<IHistoricActivityInstanceEntity> cachedHistoricActivityInstances = EntityCache.findInCache(typeof(HistoricActivityInstanceEntityImpl)) as IList<IHistoricActivityInstanceEntity>;
            foreach (IHistoricActivityInstanceEntity cachedHistoricActivityInstance in cachedHistoricActivityInstances ?? new List<IHistoricActivityInstanceEntity>())
            {
                if (!ReferenceEquals(activityId, null) && activityId.Equals(cachedHistoricActivityInstance.ActivityId) && (!endTimeMustBeNull || cachedHistoricActivityInstance.EndTime == null))
                {
                    if (executionId.Equals(cachedHistoricActivityInstance.ExecutionId))
                    {
                        return cachedHistoricActivityInstance;
                    }
                }
            }

            return null;
        }

        protected internal virtual IHistoricActivityInstanceEntity createHistoricActivityInstanceEntity(IExecutionEntity execution)
        {
            IIdGenerator idGenerator = ProcessEngineConfiguration.IdGenerator;

            string processDefinitionId = execution.ProcessDefinitionId;
            string processInstanceId = execution.ProcessInstanceId;

            IHistoricActivityInstanceEntity historicActivityInstance = HistoricActivityInstanceEntityManager.create();
            historicActivityInstance.Id = idGenerator.NextId;
            historicActivityInstance.ProcessDefinitionId = processDefinitionId;
            historicActivityInstance.ProcessInstanceId = processInstanceId;
            historicActivityInstance.ExecutionId = execution.Id;
            historicActivityInstance.ActivityId = execution.ActivityId;
            if (execution.CurrentFlowElement != null)
            {
                historicActivityInstance.ActivityName = execution.CurrentFlowElement.Name;
                historicActivityInstance.ActivityType = parseActivityType(execution.CurrentFlowElement);
            }
            DateTime now = Clock.CurrentTime;
            historicActivityInstance.StartTime = now;

            // Inherit tenant id (if applicable)
            if (!ReferenceEquals(execution.TenantId, null))
            {
                historicActivityInstance.TenantId = execution.TenantId;
            }

            HistoricActivityInstanceEntityManager.insert(historicActivityInstance);
            return historicActivityInstance;
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordProcessDefinitionChange(java.lang.String, java.lang.String)
         */
        public virtual void recordProcessDefinitionChange(string processInstanceId, string processDefinitionId)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", processInstanceId));
                if (historicProcessInstance != null)
                {
                    historicProcessInstance.ProcessDefinitionId = processDefinitionId;
                }
            }
        }

        // Task related history

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskCreated (org.activiti.engine.impl.persistence.entity.TaskEntity,
         * org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void recordTaskCreated(ITaskEntity task, IExecutionEntity execution)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.create(task, execution);
                HistoricTaskInstanceEntityManager.insert(historicTaskInstance, false);
            }

            recordTaskId(task);
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskAssignment (org.activiti.engine.impl.persistence.entity.TaskEntity)
         */
        public virtual void recordTaskAssignment(ITaskEntity task)
        {
            IExecutionEntity executionEntity = task.Execution;
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (executionEntity != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = findActivityInstance(executionEntity, false, true);
                    if (historicActivityInstance != null)
                    {
                        historicActivityInstance.Assignee = task.Assignee;
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskClaim (org.activiti.engine.impl.persistence.entity.TaskEntity)
         */

        public virtual void recordTaskClaim(ITaskEntity task)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", task.Id));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ClaimTime = task.ClaimTime;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskId (org.activiti.engine.impl.persistence.entity.TaskEntity)
         */
        public virtual void recordTaskId(ITaskEntity task)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IExecutionEntity execution = task.Execution;
                if (execution != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = findActivityInstance(execution, false, true);
                    if (historicActivityInstance != null)
                    {
                        historicActivityInstance.TaskId = task.Id;
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskEnd (java.lang.String, java.lang.String)
         */
        public virtual void recordTaskEnd(string taskId, string deleteReason)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.markEnded(deleteReason);
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskAssigneeChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskAssigneeChange(string taskId, string assignee)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Assignee = assignee;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskOwnerChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskOwnerChange(string taskId, string owner)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Owner = owner;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskNameChange (java.lang.String, java.lang.String)
         */
        public virtual void recordTaskNameChange(string taskId, string taskName)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Name = taskName;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskDescriptionChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskDescriptionChange(string taskId, string description)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Description = description;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskDueDateChange(java.lang.String, java.util.Date)
         */
        public virtual void recordTaskDueDateChange(string taskId, DateTime dueDate)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.DueDate = dueDate;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskPriorityChange(java.lang.String, int)
         */
        public virtual void recordTaskPriorityChange(string taskId, int? priority)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Priority = priority;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskCategoryChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskCategoryChange(string taskId, string category)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Category = category;
                }
            }
        }

        public virtual void recordTaskFormKeyChange(string taskId, string formKey)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.FormKey = formKey;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskParentTaskIdChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskParentTaskIdChange(string taskId, string parentTaskId)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ParentTaskId = parentTaskId;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskExecutionIdChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskExecutionIdChange(string taskId, string executionId)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ExecutionId = executionId;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordTaskDefinitionKeyChange (org.activiti.engine.impl.persistence.entity.TaskEntity, java.lang.String)
         */
        public virtual void recordTaskDefinitionKeyChange(string taskId, string taskDefinitionKey)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.TaskDefinitionKey = taskDefinitionKey;
                }
            }
        }

        /* (non-Javadoc)
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordTaskProcessDefinitionChange(java.lang.String, java.lang.String)
         */
        public virtual void recordTaskProcessDefinitionChange(string taskId, string processDefinitionId)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ProcessDefinitionId = processDefinitionId;
                }
            }
        }

        // Variables related history

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordVariableCreate (org.activiti.engine.impl.persistence.entity.VariableInstanceEntity)
         */
        public virtual void recordVariableCreate(IVariableInstanceEntity variable)
        {
            // Historic variables
            if (isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                HistoricVariableInstanceEntityManager.copyAndInsert(variable);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordHistoricDetailVariableCreate (org.activiti.engine.impl.persistence.entity.VariableInstanceEntity,
         * org.activiti.engine.impl.persistence.entity.ExecutionEntity, boolean)
         */
        public virtual void recordHistoricDetailVariableCreate(IVariableInstanceEntity variable, IExecutionEntity sourceActivityExecution, bool useActivityId)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.FULL))
            {

                IHistoricDetailVariableInstanceUpdateEntity historicVariableUpdate = HistoricDetailEntityManager.copyAndInsertHistoricDetailVariableInstanceUpdateEntity(variable);

                if (useActivityId && sourceActivityExecution != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = findActivityInstance(sourceActivityExecution, false, false);
                    if (historicActivityInstance != null)
                    {
                        historicVariableUpdate.ActivityInstanceId = historicActivityInstance.Id;
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface#recordVariableUpdate (org.activiti.engine.impl.persistence.entity.VariableInstanceEntity)
         */
        public virtual void recordVariableUpdate(IVariableInstanceEntity variable)
        {
            if (variable != null && isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricVariableInstanceEntity historicProcessVariable = EntityCache.findInCache(variable.GetType(), variable.Id) as IHistoricVariableInstanceEntity;
                if (historicProcessVariable == null)
                {
                    historicProcessVariable = HistoricVariableInstanceEntityManager.findHistoricVariableInstanceByVariableInstanceId(variable.Id);
                }

                if (historicProcessVariable != null)
                {
                    HistoricVariableInstanceEntityManager.copyVariableValue(historicProcessVariable, variable);
                }
                else
                {
                    HistoricVariableInstanceEntityManager.copyAndInsert(variable);
                }
            }
        }

        // Comment related history



        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# createIdentityLinkComment(java.lang.String, java.lang.String, java.lang.String, java.lang.String, boolean)
         */
        public virtual void createIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create)
        {
            createIdentityLinkComment(taskId, userId, groupId, type, create, false);
        }

        public virtual void createUserIdentityLinkComment(string taskId, string userId, string type, bool create)
        {
            createIdentityLinkComment(taskId, userId, null, type, create, false);
        }

        public virtual void createGroupIdentityLinkComment(string taskId, string groupId, string type, bool create)
        {
            createIdentityLinkComment(taskId, null, groupId, type, create, false);
        }

        public virtual void createUserIdentityLinkComment(string taskId, string userId, string type, bool create, bool forceNullUserId)
        {
            createIdentityLinkComment(taskId, userId, null, type, create, forceNullUserId);
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# createIdentityLinkComment(java.lang.String, java.lang.String, java.lang.String, java.lang.String, boolean, boolean)
         */
        public virtual void createIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create, bool forceNullUserId)
        {
            if (HistoryEnabled)
            {
                string authenticatedUserId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.create();
                comment.UserId = authenticatedUserId;
                comment.Type = CommentEntity_Fields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.TaskId = taskId;
                if (!ReferenceEquals(userId, null) || forceNullUserId)
                {
                    if (create)
                    {
                        comment.Action = Event_Fields.ACTION_ADD_USER_LINK;
                    }
                    else
                    {
                        comment.Action = Event_Fields.ACTION_DELETE_USER_LINK;
                    }
                    comment.MessageParts = new string[] { userId, type };
                }
                else
                {
                    if (create)
                    {
                        comment.Action = Event_Fields.ACTION_ADD_GROUP_LINK;
                    }
                    else
                    {
                        comment.Action = Event_Fields.ACTION_DELETE_GROUP_LINK;
                    }
                    comment.MessageParts = new string[] { groupId, type };
                }

                CommentEntityManager.insert(comment);
            }
        }

        public virtual void createProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create)
        {
            createProcessInstanceIdentityLinkComment(processInstanceId, userId, groupId, type, create, false);
        }

        public virtual void createProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create, bool forceNullUserId)
        {
            if (HistoryEnabled)
            {
                string authenticatedUserId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.create();
                comment.UserId = authenticatedUserId;
                comment.Type = CommentEntity_Fields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.ProcessInstanceId = processInstanceId;
                if (!ReferenceEquals(userId, null) || forceNullUserId)
                {
                    if (create)
                    {
                        comment.Action = Event_Fields.ACTION_ADD_USER_LINK;
                    }
                    else
                    {
                        comment.Action = Event_Fields.ACTION_DELETE_USER_LINK;
                    }
                    comment.MessageParts = new string[] { userId, type };
                }
                else
                {
                    if (create)
                    {
                        comment.Action = Event_Fields.ACTION_ADD_GROUP_LINK;
                    }
                    else
                    {
                        comment.Action = Event_Fields.ACTION_DELETE_GROUP_LINK;
                    }
                    comment.MessageParts = new string[] { groupId, type };
                }
                CommentEntityManager.insert(comment);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# createAttachmentComment(java.lang.String, java.lang.String, java.lang.String, boolean)
         */
        public virtual void createAttachmentComment(string taskId, string processInstanceId, string attachmentName, bool create)
        {
            if (HistoryEnabled)
            {
                string userId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.create();
                comment.UserId = userId;
                comment.Type = CommentEntity_Fields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.TaskId = taskId;
                comment.ProcessInstanceId = processInstanceId;
                if (create)
                {
                    comment.Action = Event_Fields.ACTION_ADD_ATTACHMENT;
                }
                else
                {
                    comment.Action = Event_Fields.ACTION_DELETE_ATTACHMENT;
                }
                comment.MessageParts = new string[] { attachmentName };
                CommentEntityManager.insert(comment);
            }
        }

        // Identity link related history
        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# recordIdentityLinkCreated (org.activiti.engine.impl.persistence.entity.IdentityLinkEntity)
         */
        public virtual void recordIdentityLinkCreated(IIdentityLinkEntity identityLink)
        {
            // It makes no sense storing historic counterpart for an identity-link
            // that is related
            // to a process-definition only as this is never kept in history
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT) && (!ReferenceEquals(identityLink.ProcessInstanceId, null) || !ReferenceEquals(identityLink.TaskId, null)))
            {
                IHistoricIdentityLinkEntity historicIdentityLinkEntity = HistoricIdentityLinkEntityManager.create();
                historicIdentityLinkEntity.Id = identityLink.Id;
                historicIdentityLinkEntity.GroupId = identityLink.GroupId;
                historicIdentityLinkEntity.ProcessInstanceId = identityLink.ProcessInstanceId;
                historicIdentityLinkEntity.TaskId = identityLink.TaskId;
                historicIdentityLinkEntity.Type = identityLink.Type;
                historicIdentityLinkEntity.UserId = identityLink.UserId;
                HistoricIdentityLinkEntityManager.insert(historicIdentityLinkEntity, false);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# deleteHistoricIdentityLink(java.lang.String)
         */
        public virtual void deleteHistoricIdentityLink(string id)
        {
            if (isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                HistoricIdentityLinkEntityManager.delete(new KeyValuePair<string, object>("id", id));
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see org.activiti.engine.impl.history.HistoryManagerInterface# updateProcessBusinessKeyInHistory (org.activiti.engine.impl.persistence.entity.ExecutionEntity)
         */
        public virtual void updateProcessBusinessKeyInHistory(IExecutionEntity processInstance)
        {
            if (HistoryEnabled)
            {
                //if (log.DebugEnabled)
                //{
                //    log.debug("updateProcessBusinessKeyInHistory : {}", processInstance.Id);
                //}
                if (processInstance != null)
                {
                    IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", processInstance.Id));
                    if (historicProcessInstance != null)
                    {
                        historicProcessInstance.BusinessKey = processInstance.ProcessInstanceBusinessKey;
                        HistoricProcessInstanceEntityManager.update(historicProcessInstance, false);
                    }
                }
            }
        }

        public virtual void recordVariableRemoved(IVariableInstanceEntity variable)
        {
            if (variable != null && isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricVariableInstanceEntity historicProcessVariable = EntityCache.findInCache(variable.GetType(), variable.Id) as IHistoricVariableInstanceEntity;
                if (historicProcessVariable == null)
                {
                    historicProcessVariable = HistoricVariableInstanceEntityManager.findHistoricVariableInstanceByVariableInstanceId(variable.Id);
                }

                if (historicProcessVariable != null)
                {
                    HistoricVariableInstanceEntityManager.delete(historicProcessVariable);
                }
            }
        }

        protected internal virtual string parseActivityType(FlowElement element)
        {
            string elementType = element.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

        protected internal virtual IEntityCache EntityCache
        {
            get
            {
                return getSession<IEntityCache>();
            }
        }

        public virtual HistoryLevel HistoryLevel
        {
            get
            {
                return historyLevel;
            }
            set
            {
                this.historyLevel = value;
            }
        }


    }

}