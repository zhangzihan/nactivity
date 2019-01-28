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

namespace org.activiti.engine.impl.persistence.entity
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.task;

    /// 
    /// 
    public class TaskEntityManagerImpl : AbstractEntityManager<ITaskEntity>, ITaskEntityManager
    {

        protected internal ITaskDataManager taskDataManager;

        public TaskEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, ITaskDataManager taskDataManager) : base(processEngineConfiguration)
        {
            this.taskDataManager = taskDataManager;
        }

        protected internal override IDataManager<ITaskEntity> DataManager
        {
            get
            {
                return taskDataManager;
            }
        }

        public override ITaskEntity create()
        {
            ITaskEntity taskEntity = base.create();
            taskEntity.CreateTime = Clock.CurrentTime;
            return taskEntity;
        }

        public override void insert(ITaskEntity taskEntity, bool fireCreateEvent)
        {

            if (!ReferenceEquals(taskEntity.Owner, null))
            {
                addOwnerIdentityLink(taskEntity, taskEntity.Owner);
            }
            if (!ReferenceEquals(taskEntity.Assignee, null))
            {
                addAssigneeIdentityLinks(taskEntity);
            }

            base.insert(taskEntity, fireCreateEvent);

        }

        public virtual void insert(ITaskEntity taskEntity, IExecutionEntity execution)
        {

            // Inherit tenant id (if applicable)
            if (execution != null && !ReferenceEquals(execution.TenantId, null))
            {
                taskEntity.TenantId = execution.TenantId;
            }

            if (execution != null)
            {
                execution.Tasks.Add(taskEntity);
                taskEntity.ExecutionId = execution.Id;
                taskEntity.ProcessInstanceId = execution.ProcessInstanceId;
                taskEntity.ProcessDefinitionId = execution.ProcessDefinitionId;

                HistoryManager.recordTaskExecutionIdChange(taskEntity.Id, taskEntity.ExecutionId);
            }

            insert(taskEntity, true);

            if (execution != null && isExecutionRelatedEntityCountEnabled(execution))
            {
                ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                countingExecutionEntity.TaskCount = countingExecutionEntity.TaskCount + 1;
            }

            HistoryManager.recordTaskCreated(taskEntity, execution);
            HistoryManager.recordTaskId(taskEntity);
            if (!ReferenceEquals(taskEntity.FormKey, null))
            {
                HistoryManager.recordTaskFormKeyChange(taskEntity.Id, taskEntity.FormKey);
            }
        }


        public virtual void changeTaskAssignee(ITaskEntity taskEntity, string assignee)
        {
            changeTaskAssignee(taskEntity, assignee, true);
        }

        public virtual void changeTaskAssigneeNoEvents(ITaskEntity taskEntity, string assignee)
        {
            changeTaskAssignee(taskEntity, assignee, false);
        }

        private void changeTaskAssignee(ITaskEntity taskEntity, string assignee, bool fireEvents)
        {
            if ((!ReferenceEquals(taskEntity.Assignee, null) && !taskEntity.Assignee.Equals(assignee)) || (ReferenceEquals(taskEntity.Assignee, null) && !ReferenceEquals(assignee, null)))
            {
                taskEntity.Assignee = assignee;
                if (fireEvents)
                {
                    fireAssignmentEvents(taskEntity);
                }
                else
                {
                    recordTaskAssignment(taskEntity);
                }

                if (!ReferenceEquals(taskEntity.Id, null))
                {
                    HistoryManager.recordTaskAssigneeChange(taskEntity.Id, taskEntity.Assignee);
                    addAssigneeIdentityLinks(taskEntity);
                    update(taskEntity);
                }
            }
        }

        public virtual void changeTaskOwner(ITaskEntity taskEntity, string owner)
        {
            if ((!ReferenceEquals(taskEntity.Owner, null) && !taskEntity.Owner.Equals(owner)) || (ReferenceEquals(taskEntity.Owner, null) && !ReferenceEquals(owner, null)))
            {
                taskEntity.Owner = owner;

                if (!ReferenceEquals(taskEntity.Id, null))
                {
                    HistoryManager.recordTaskOwnerChange(taskEntity.Id, taskEntity.Owner);
                    addOwnerIdentityLink(taskEntity, taskEntity.Owner);
                    update(taskEntity);
                }
            }
        }

        protected internal virtual void fireAssignmentEvents(ITaskEntity taskEntity)
        {
            recordTaskAssignment(taskEntity);
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_ASSIGNED, taskEntity));
            }

        }

        protected internal virtual void recordTaskAssignment(ITaskEntity taskEntity)
        {
            ProcessEngineConfiguration.ListenerNotificationHelper.executeTaskListeners(taskEntity, BaseTaskListener_Fields.EVENTNAME_ASSIGNMENT);
            HistoryManager.recordTaskAssignment(taskEntity);

        }

        private void addAssigneeIdentityLinks(ITaskEntity taskEntity)
        {
            if (!ReferenceEquals(taskEntity.Assignee, null) && taskEntity.ProcessInstance != null)
            {
                IdentityLinkEntityManager.involveUser(taskEntity.ProcessInstance, taskEntity.Assignee, IdentityLinkType.PARTICIPANT);
            }
        }

        protected internal virtual void addOwnerIdentityLink(ITaskEntity taskEntity, string owner)
        {
            if (ReferenceEquals(owner, null) && ReferenceEquals(taskEntity.Owner, null))
            {
                return;
            }

            if (!ReferenceEquals(owner, null) && !ReferenceEquals(taskEntity.ProcessInstanceId, null))
            {
                IdentityLinkEntityManager.involveUser(taskEntity.ProcessInstance, owner, IdentityLinkType.PARTICIPANT);
            }
        }

        public virtual void deleteTasksByProcessInstanceId(string processInstanceId, string deleteReason, bool cascade)
        {
            IList<ITaskEntity> tasks = findTasksByProcessInstanceId(processInstanceId);

            foreach (ITaskEntity task in tasks)
            {
                if (EventDispatcher.Enabled && !task.Canceled)
                {
                    task.Canceled = true;
                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityCancelledEvent(task.Execution.ActivityId, task.Name, task.ExecutionId, task.ProcessInstanceId, task.ProcessDefinitionId, "userTask", deleteReason));
                }

                deleteTask(task, deleteReason, cascade, false);
            }
        }

        public virtual void deleteTask(ITaskEntity task, string deleteReason, bool cascade, bool cancel)
        {
            if (!task.Deleted)
            {
                ProcessEngineConfiguration.ListenerNotificationHelper.executeTaskListeners(task, BaseTaskListener_Fields.EVENTNAME_DELETE);
                task.Deleted = true;

                string taskId = task.Id;

                IList<ITask> subTasks = findTasksByParentTaskId(taskId);
                foreach (ITask subTask in subTasks)
                {
                    deleteTask((ITaskEntity)subTask, deleteReason, cascade, cancel);
                }

                IdentityLinkEntityManager.deleteIdentityLinksByTaskId(taskId);
                VariableInstanceEntityManager.deleteVariableInstanceByTask(task);

                if (cascade)
                {
                    HistoricTaskInstanceEntityManager.delete(new KeyValuePair<string, object>("id", taskId));
                }
                else
                {
                    HistoryManager.recordTaskEnd(taskId, deleteReason);
                }

                delete(task, false);

                if (EventDispatcher.Enabled)
                {
                    if (cancel && !task.Canceled)
                    {
                        task.Canceled = true;
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityCancelledEvent(task.Execution != null ? task.Execution.ActivityId : null, task.Name, task.ExecutionId, task.ProcessInstanceId, task.ProcessDefinitionId, "userTask", deleteReason));
                    }

                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, task));
                }
            }
        }

        public override void delete(ITaskEntity entity, bool fireDeleteEvent)
        {
            base.delete(entity, fireDeleteEvent);

            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)entity.Execution;
                if (isExecutionRelatedEntityCountEnabled(countingExecutionEntity))
                {
                    countingExecutionEntity.TaskCount = countingExecutionEntity.TaskCount - 1;
                }
            }
        }

        public virtual IList<ITaskEntity> findTasksByExecutionId(string executionId)
        {
            return taskDataManager.findTasksByExecutionId(executionId);
        }

        public virtual IList<ITaskEntity> findTasksByProcessInstanceId(string processInstanceId)
        {
            return taskDataManager.findTasksByProcessInstanceId(processInstanceId);
        }

        public virtual IList<ITask> findTasksByQueryCriteria(TaskQueryImpl taskQuery)
        {
            return taskDataManager.findTasksByQueryCriteria(taskQuery);
        }

        public virtual IList<ITask> findTasksAndVariablesByQueryCriteria(TaskQueryImpl taskQuery)
        {
            return taskDataManager.findTasksAndVariablesByQueryCriteria(taskQuery);
        }

        public virtual long findTaskCountByQueryCriteria(TaskQueryImpl taskQuery)
        {
            return taskDataManager.findTaskCountByQueryCriteria(taskQuery);
        }

        public virtual IList<ITask> findTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return taskDataManager.findTasksByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findTaskCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return taskDataManager.findTaskCountByNativeQuery(parameterMap);
        }

        public virtual IList<ITask> findTasksByParentTaskId(string parentTaskId)
        {
            return taskDataManager.findTasksByParentTaskId(parentTaskId);
        }

        public virtual void deleteTask(string taskId, string deleteReason, bool cascade)
        {

            ITaskEntity task = findById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task != null)
            {
                if (!ReferenceEquals(task.ExecutionId, null))
                {
                    throw new ActivitiException("The task cannot be deleted because is part of a running process");
                }

                deleteTask(task, deleteReason, cascade, false);
            }
            else if (cascade)
            {
                HistoricTaskInstanceEntityManager.delete(new KeyValuePair<string, object>("id", taskId));
            }
        }

        public virtual void updateTaskTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            taskDataManager.updateTaskTenantIdForDeployment(deploymentId, newTenantId);
        }

        public virtual ITaskDataManager TaskDataManager
        {
            get
            {
                return taskDataManager;
            }
            set
            {
                this.taskDataManager = value;
            }
        }


    }

}