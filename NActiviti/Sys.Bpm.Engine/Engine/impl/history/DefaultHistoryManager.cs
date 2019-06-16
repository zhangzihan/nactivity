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

namespace Sys.Workflow.Engine.Impl.Histories
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Persistence;
    using Sys.Workflow.Engine.Impl.Persistence.Caches;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow;

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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# isHistoryLevelAtLeast(Sys.Workflow.Engine.Impl.Histories.HistoryLevel)
         */
        public virtual bool IsHistoryLevelAtLeast(HistoryLevel level)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Current history level: {historyLevel}, level required: {level}");
            }
            // Comparing enums actually compares the location of values declared in
            // the enum
            return historyLevel.IsAtLeast(level);
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#isHistoryEnabled ()
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordProcessInstanceEnd(java.lang.String, java.lang.String, java.lang.String)
         */
        public virtual void RecordProcessInstanceEnd(string processInstanceId, string deleteReason, string activityId)
        {

            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("processInstanceId", processInstanceId));

                if (historicProcessInstance != null)
                {
                    historicProcessInstance.markEnded(deleteReason);
                    historicProcessInstance.EndActivityId = activityId;

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_ENDED, historicProcessInstance));
                    }

                }
            }
        }

        public virtual void RecordProcessInstanceNameChange(string processInstanceId, string newName)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("processInstanceId", processInstanceId));

                if (historicProcessInstance != null)
                {
                    historicProcessInstance.Name = newName;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordProcessInstanceStart (Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void RecordProcessInstanceStart(IExecutionEntity processInstance, FlowElement startElement)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.Create(processInstance);
                historicProcessInstance.StartActivityId = startElement.Id;

                // Insert historic process-instance
                HistoricProcessInstanceEntityManager.Insert(historicProcessInstance, false);

                // Fire event
                IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                {
                    activitiEventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_CREATED, historicProcessInstance));
                }

            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordSubProcessInstanceStart (Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity,
         * Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void RecordSubProcessInstanceStart(IExecutionEntity parentExecution, IExecutionEntity subProcessInstance, FlowElement initialElement)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {

                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.Create(subProcessInstance);

                // Fix for ACT-1728: startActivityId not initialized with subprocess instance
                if (historicProcessInstance.StartActivityId is null)
                {
                    historicProcessInstance.StartActivityId = initialElement.Id;
                }
                HistoricProcessInstanceEntityManager.Insert(historicProcessInstance, false);

                // Fire event
                IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                {
                    activitiEventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_PROCESS_INSTANCE_CREATED, historicProcessInstance));
                }

                IHistoricActivityInstanceEntity activitiyInstance = FindActivityInstance(parentExecution, false, true);
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordActivityStart (Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void RecordActivityStart(IExecutionEntity executionEntity)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (executionEntity.ActivityId != null && executionEntity.CurrentFlowElement != null)
                {

                    // Historic activity instance could have been created (but only in cache, never persisted)
                    // for example when submitting form properties
                    IHistoricActivityInstanceEntity historicActivityInstanceEntityFromCache = GetHistoricActivityInstanceFromCache(executionEntity.Id, executionEntity.ActivityId, true);
                    IHistoricActivityInstanceEntity historicActivityInstanceEntity;
                    if (historicActivityInstanceEntityFromCache != null)
                    {
                        historicActivityInstanceEntity = historicActivityInstanceEntityFromCache;
                    }
                    else
                    {
                        historicActivityInstanceEntity = CreateHistoricActivityInstanceEntity(executionEntity);
                    }

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_CREATED, historicActivityInstanceEntity));
                    }

                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordActivityEnd (Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void RecordActivityEnd(IExecutionEntity executionEntity, string deleteReason)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricActivityInstanceEntity historicActivityInstance = FindActivityInstance(executionEntity, false, true);
                if (historicActivityInstance != null)
                {
                    historicActivityInstance.markEnded(deleteReason);

                    // Fire event
                    IActivitiEventDispatcher activitiEventDispatcher = EventDispatcher;
                    if (activitiEventDispatcher != null && activitiEventDispatcher.Enabled)
                    {
                        activitiEventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_ENDED, historicActivityInstance));
                    }
                }
            }
        }

        public virtual IHistoricActivityInstanceEntity FindActivityInstance(IExecutionEntity execution, bool createOnNotFound, bool endTimeMustBeNull)
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

            if (!(activityId is null))
            {
                return FindActivityInstance(execution, activityId, createOnNotFound, endTimeMustBeNull);
            }

            return null;
        }


        public virtual IHistoricActivityInstanceEntity FindActivityInstance(IExecutionEntity execution, string activityId, bool createOnNotFound, bool endTimeMustBeNull)
        {

            // No use looking for the HistoricActivityInstance when no activityId is provided.
            if (activityId is null)
            {
                return null;
            }

            string executionId = execution.Id;

            // Check the cache
            IHistoricActivityInstanceEntity historicActivityInstanceEntityFromCache = GetHistoricActivityInstanceFromCache(executionId, activityId, endTimeMustBeNull);
            if (historicActivityInstanceEntityFromCache != null)
            {
                return historicActivityInstanceEntityFromCache;
            }

            // If the execution was freshly created, there is no need to check the database, 
            // there can never be an entry for a historic activity instance with this execution id.
            if (!execution.Inserted && !execution.ProcessInstanceType)
            {
                // Check the database
                IList<IHistoricActivityInstanceEntity> historicActivityInstances = HistoricActivityInstanceEntityManager.FindUnfinishedHistoricActivityInstancesByExecutionAndActivityId(executionId, activityId);

                if (historicActivityInstances.Count > 0)
                {
                    return historicActivityInstances[0];
                }
            }

            if (!(execution.ParentId is null))
            {
                IHistoricActivityInstanceEntity historicActivityInstanceFromParent = FindActivityInstance(execution.Parent, activityId, false, endTimeMustBeNull); // always false for create, we only check if it can be found
                if (historicActivityInstanceFromParent != null)
                {
                    return historicActivityInstanceFromParent;
                }
            }

            if (createOnNotFound && !(activityId is null) && ((execution.CurrentFlowElement != null && execution.CurrentFlowElement is FlowNode) || execution.CurrentFlowElement == null))
            {
                return CreateHistoricActivityInstanceEntity(execution);
            }

            return null;
        }

        protected internal virtual IHistoricActivityInstanceEntity GetHistoricActivityInstanceFromCache(string executionId, string activityId, bool endTimeMustBeNull)
        {
            IList<IHistoricActivityInstanceEntity> cachedHistoricActivityInstances = EntityCache.FindInCache(typeof(HistoricActivityInstanceEntityImpl)) as IList<IHistoricActivityInstanceEntity>;
            foreach (IHistoricActivityInstanceEntity cachedHistoricActivityInstance in cachedHistoricActivityInstances ?? new List<IHistoricActivityInstanceEntity>())
            {
                if (!(activityId is null) && activityId.Equals(cachedHistoricActivityInstance.ActivityId) && (!endTimeMustBeNull || cachedHistoricActivityInstance.EndTime == null))
                {
                    if (executionId.Equals(cachedHistoricActivityInstance.ExecutionId))
                    {
                        return cachedHistoricActivityInstance;
                    }
                }
            }

            return null;
        }

        protected internal virtual IHistoricActivityInstanceEntity CreateHistoricActivityInstanceEntity(IExecutionEntity execution)
        {
            IIdGenerator idGenerator = ProcessEngineConfiguration.IdGenerator;

            string processDefinitionId = execution.ProcessDefinitionId;
            string processInstanceId = execution.ProcessInstanceId;

            IHistoricActivityInstanceEntity historicActivityInstance = HistoricActivityInstanceEntityManager.Create();
            historicActivityInstance.Id = idGenerator.GetNextId();
            historicActivityInstance.ProcessDefinitionId = processDefinitionId;
            historicActivityInstance.ProcessInstanceId = processInstanceId;
            historicActivityInstance.ExecutionId = execution.Id;
            historicActivityInstance.ActivityId = execution.ActivityId;
            if (execution.CurrentFlowElement != null)
            {
                historicActivityInstance.ActivityName = execution.CurrentFlowElement.Name;
                historicActivityInstance.ActivityType = ParseActivityType(execution.CurrentFlowElement);
            }
            DateTime now = Clock.CurrentTime;
            historicActivityInstance.StartTime = now;

            // Inherit tenant id (if applicable)
            if (!(execution.TenantId is null))
            {
                historicActivityInstance.TenantId = execution.TenantId;
            }

            HistoricActivityInstanceEntityManager.Insert(historicActivityInstance);
            return historicActivityInstance;
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordProcessDefinitionChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordProcessDefinitionChange(string processInstanceId, string processDefinitionId)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", processInstanceId));
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskCreated (Sys.Workflow.Engine.Impl.Persistence.Entity.TaskEntity,
         * Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void RecordTaskCreated(ITaskEntity task, IExecutionEntity execution)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.Create(task, execution);
                HistoricTaskInstanceEntityManager.Insert(historicTaskInstance, false);
            }

            RecordTaskId(task);
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskAssignment (Sys.Workflow.Engine.Impl.Persistence.Entity.TaskEntity)
         */
        public virtual void RecordTaskAssignment(ITaskEntity task)
        {
            IExecutionEntity executionEntity = task.Execution;
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (executionEntity != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = FindActivityInstance(executionEntity, false, true);
                    if (historicActivityInstance != null)
                    {
                        historicActivityInstance.Assignee = task.Assignee;
                        historicActivityInstance.AssigneeUser = task.AssigneeUser;
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskClaim (Sys.Workflow.Engine.Impl.Persistence.Entity.TaskEntity)
         */

        public virtual void RecordTaskClaim(ITaskEntity task)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", task.Id));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ClaimTime = task.ClaimTime;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskId (Sys.Workflow.Engine.Impl.Persistence.Entity.TaskEntity)
         */
        public virtual void RecordTaskId(ITaskEntity task)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IExecutionEntity execution = task.Execution;
                if (execution != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = FindActivityInstance(execution, false, true);
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskEnd (java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskEnd(string taskId, string deleteReason)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.markEnded(deleteReason);
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskAssigneeChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskAssigneeChange(string taskId, string assignee, string assigneeUser)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Assignee = assignee;
                    historicTaskInstance.AssigneeUser = assigneeUser;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskOwnerChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskOwnerChange(string taskId, string owner)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Owner = owner;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskNameChange (java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskNameChange(string taskId, string taskName)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Name = taskName;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskDescriptionChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskDescriptionChange(string taskId, string description)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Description = description;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskDueDateChange(java.lang.String, java.util.Date)
         */
        public virtual void RecordTaskDueDateChange(string taskId, DateTime dueDate)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.DueDate = dueDate;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskPriorityChange(java.lang.String, int)
         */
        public virtual void RecordTaskPriorityChange(string taskId, int? priority)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Priority = priority;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskCategoryChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskCategoryChange(string taskId, string category)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.Category = category;
                }
            }
        }

        public virtual void RecordTaskFormKeyChange(string taskId, string formKey)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.FormKey = formKey;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskParentTaskIdChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskParentTaskIdChange(string taskId, string parentTaskId)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ParentTaskId = parentTaskId;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskExecutionIdChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskExecutionIdChange(string taskId, string executionId)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.ExecutionId = executionId;
                }
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordTaskDefinitionKeyChange (Sys.Workflow.Engine.Impl.Persistence.Entity.TaskEntity, java.lang.String)
         */
        public virtual void RecordTaskDefinitionKeyChange(string taskId, string taskDefinitionKey)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
                if (historicTaskInstance != null)
                {
                    historicTaskInstance.TaskDefinitionKey = taskDefinitionKey;
                }
            }
        }

        /* (non-Javadoc)
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordTaskProcessDefinitionChange(java.lang.String, java.lang.String)
         */
        public virtual void RecordTaskProcessDefinitionChange(string taskId, string processDefinitionId)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IHistoricTaskInstanceEntity historicTaskInstance = HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", taskId));
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordVariableCreate (Sys.Workflow.Engine.Impl.Persistence.Entity.VariableInstanceEntity)
         */
        public virtual void RecordVariableCreate(IVariableInstanceEntity variable)
        {
            // Historic variables
            if (IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                HistoricVariableInstanceEntityManager.CopyAndInsert(variable);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordHistoricDetailVariableCreate (Sys.Workflow.Engine.Impl.Persistence.Entity.VariableInstanceEntity,
         * Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity, boolean)
         */
        public virtual void RecordHistoricDetailVariableCreate(IVariableInstanceEntity variable, IExecutionEntity sourceActivityExecution, bool useActivityId)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.FULL))
            {

                IHistoricDetailVariableInstanceUpdateEntity historicVariableUpdate = HistoricDetailEntityManager.CopyAndInsertHistoricDetailVariableInstanceUpdateEntity(variable);

                if (useActivityId && sourceActivityExecution != null)
                {
                    IHistoricActivityInstanceEntity historicActivityInstance = FindActivityInstance(sourceActivityExecution, false, false);
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
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface#recordVariableUpdate (Sys.Workflow.Engine.Impl.Persistence.Entity.VariableInstanceEntity)
         */
        public virtual void RecordVariableUpdate(IVariableInstanceEntity variable)
        {
            if (variable != null && IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (!(EntityCache.FindInCache(variable.GetType(), variable.Id) is IHistoricVariableInstanceEntity historicProcessVariable))
                {
                    historicProcessVariable = HistoricVariableInstanceEntityManager.FindHistoricVariableInstanceByVariableInstanceId(variable.Id);
                }

                if (historicProcessVariable != null)
                {
                    HistoricVariableInstanceEntityManager.CopyVariableValue(historicProcessVariable, variable);
                }
                else
                {
                    HistoricVariableInstanceEntityManager.CopyAndInsert(variable);
                }
            }
        }

        // Comment related history



        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# createIdentityLinkComment(java.lang.String, java.lang.String, java.lang.String, java.lang.String, boolean)
         */
        public virtual void CreateIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create)
        {
            CreateIdentityLinkComment(taskId, userId, groupId, type, create, false);
        }

        public virtual void CreateUserIdentityLinkComment(string taskId, string userId, string type, bool create)
        {
            CreateIdentityLinkComment(taskId, userId, null, type, create, false);
        }

        public virtual void CreateGroupIdentityLinkComment(string taskId, string groupId, string type, bool create)
        {
            CreateIdentityLinkComment(taskId, null, groupId, type, create, false);
        }

        public virtual void CreateUserIdentityLinkComment(string taskId, string userId, string type, bool create, bool forceNullUserId)
        {
            CreateIdentityLinkComment(taskId, userId, null, type, create, forceNullUserId);
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# createIdentityLinkComment(java.lang.String, java.lang.String, java.lang.String, java.lang.String, boolean, boolean)
         */
        public virtual void CreateIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create, bool forceNullUserId)
        {
            if (HistoryEnabled)
            {
                string authenticatedUserId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.Create();
                comment.UserId = authenticatedUserId;
                comment.Type = CommentEntityFields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.TaskId = taskId;
                if (!(userId is null) || forceNullUserId)
                {
                    if (create)
                    {
                        comment.Action = EventFields.ACTION_ADD_USER_LINK;
                    }
                    else
                    {
                        comment.Action = EventFields.ACTION_DELETE_USER_LINK;
                    }
                    comment.MessageParts = new string[] { userId, type };
                }
                else
                {
                    if (create)
                    {
                        comment.Action = EventFields.ACTION_ADD_GROUP_LINK;
                    }
                    else
                    {
                        comment.Action = EventFields.ACTION_DELETE_GROUP_LINK;
                    }
                    comment.MessageParts = new string[] { groupId, type };
                }

                CommentEntityManager.Insert(comment);
            }
        }

        public virtual void CreateProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create)
        {
            CreateProcessInstanceIdentityLinkComment(processInstanceId, userId, groupId, type, create, false);
        }

        public virtual void CreateProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create, bool forceNullUserId)
        {
            if (HistoryEnabled)
            {
                string authenticatedUserId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.Create();
                comment.UserId = authenticatedUserId;
                comment.Type = CommentEntityFields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.ProcessInstanceId = processInstanceId;
                if (!(userId is null) || forceNullUserId)
                {
                    if (create)
                    {
                        comment.Action = EventFields.ACTION_ADD_USER_LINK;
                    }
                    else
                    {
                        comment.Action = EventFields.ACTION_DELETE_USER_LINK;
                    }
                    comment.MessageParts = new string[] { userId, type };
                }
                else
                {
                    if (create)
                    {
                        comment.Action = EventFields.ACTION_ADD_GROUP_LINK;
                    }
                    else
                    {
                        comment.Action = EventFields.ACTION_DELETE_GROUP_LINK;
                    }
                    comment.MessageParts = new string[] { groupId, type };
                }
                CommentEntityManager.Insert(comment);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# createAttachmentComment(java.lang.String, java.lang.String, java.lang.String, boolean)
         */
        public virtual void CreateAttachmentComment(string taskId, string processInstanceId, string attachmentName, bool create)
        {
            if (HistoryEnabled)
            {
                string userId = Authentication.AuthenticatedUser.Id;
                ICommentEntity comment = CommentEntityManager.Create();
                comment.UserId = userId;
                comment.Type = CommentEntityFields.TYPE_EVENT;
                comment.Time = Clock.CurrentTime;
                comment.TaskId = taskId;
                comment.ProcessInstanceId = processInstanceId;
                if (create)
                {
                    comment.Action = EventFields.ACTION_ADD_ATTACHMENT;
                }
                else
                {
                    comment.Action = EventFields.ACTION_DELETE_ATTACHMENT;
                }
                comment.MessageParts = new string[] { attachmentName };
                CommentEntityManager.Insert(comment);
            }
        }

        // Identity link related history
        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# recordIdentityLinkCreated (Sys.Workflow.Engine.Impl.Persistence.Entity.IdentityLinkEntity)
         */
        public virtual void RecordIdentityLinkCreated(IIdentityLinkEntity identityLink)
        {
            // It makes no sense storing historic counterpart for an identity-link
            // that is related
            // to a process-definition only as this is never kept in history
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT) && (!(identityLink.ProcessInstanceId is null) || !(identityLink.TaskId is null)))
            {
                IHistoricIdentityLinkEntity historicIdentityLinkEntity = HistoricIdentityLinkEntityManager.Create();
                historicIdentityLinkEntity.Id = identityLink.Id;
                historicIdentityLinkEntity.GroupId = identityLink.GroupId;
                historicIdentityLinkEntity.ProcessInstanceId = identityLink.ProcessInstanceId;
                historicIdentityLinkEntity.TaskId = identityLink.TaskId;
                historicIdentityLinkEntity.Type = identityLink.Type;
                historicIdentityLinkEntity.UserId = identityLink.UserId;
                HistoricIdentityLinkEntityManager.Insert(historicIdentityLinkEntity, false);
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# deleteHistoricIdentityLink(java.lang.String)
         */
        public virtual void DeleteHistoricIdentityLink(string id)
        {
            if (IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                HistoricIdentityLinkEntityManager.Delete(new KeyValuePair<string, object>("id", id));
            }
        }

        /*
         * (non-Javadoc)
         * 
         * @see Sys.Workflow.Engine.Impl.Histories.HistoryManagerInterface# updateProcessBusinessKeyInHistory (Sys.Workflow.Engine.Impl.Persistence.Entity.ExecutionEntity)
         */
        public virtual void UpdateProcessBusinessKeyInHistory(IExecutionEntity processInstance)
        {
            if (HistoryEnabled)
            {
                //if (log.DebugEnabled)
                //{
                //    log.debug("updateProcessBusinessKeyInHistory : {}", processInstance.Id);
                //}
                if (processInstance != null)
                {
                    IHistoricProcessInstanceEntity historicProcessInstance = HistoricProcessInstanceEntityManager.FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", processInstance.Id));
                    if (historicProcessInstance != null)
                    {
                        historicProcessInstance.BusinessKey = processInstance.ProcessInstanceBusinessKey;
                        HistoricProcessInstanceEntityManager.Update(historicProcessInstance, false);
                    }
                }
            }
        }

        public virtual void RecordVariableRemoved(IVariableInstanceEntity variable)
        {
            if (variable != null && IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                if (!(EntityCache.FindInCache(variable.GetType(), variable.Id) is IHistoricVariableInstanceEntity historicProcessVariable))
                {
                    historicProcessVariable = HistoricVariableInstanceEntityManager.FindHistoricVariableInstanceByVariableInstanceId(variable.Id);
                }

                if (historicProcessVariable != null)
                {
                    HistoricVariableInstanceEntityManager.Delete(historicProcessVariable);
                }
            }
        }

        protected internal virtual string ParseActivityType(FlowElement element)
        {
            string elementType = element.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

        protected internal virtual IEntityCache EntityCache
        {
            get
            {
                return GetSession<IEntityCache>();
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