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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Tasks;

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

        public override ITaskEntity Create()
        {
            ITaskEntity taskEntity = base.Create();
            taskEntity.CreateTime = Clock.CurrentTime;
            return taskEntity;
        }

        public override void Insert(ITaskEntity taskEntity, bool fireCreateEvent)
        {
            if (taskEntity.Owner is object)
            {
                AddOwnerIdentityLink(taskEntity, taskEntity.Owner);
            }
            if (taskEntity.Assignee is object)
            {
                AddAssigneeIdentityLinks(taskEntity);
            }

            base.Insert(taskEntity, fireCreateEvent);

        }

        public virtual void Insert(ITaskEntity taskEntity, IExecutionEntity execution)
        {

            // Inherit tenant id (if applicable)
            if (execution != null && execution.TenantId is object)
            {
                taskEntity.TenantId = execution.TenantId;
            }

            if (execution != null)
            {
                execution.Tasks.Add(taskEntity);
                taskEntity.ProcessInstanceId = execution.ProcessInstanceId;
                taskEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
                taskEntity.ExecutionId = execution.Id;

                HistoryManager.RecordTaskExecutionIdChange(taskEntity.Id, taskEntity.ExecutionId);
            }

            Insert(taskEntity, true);

            if (execution != null && IsExecutionRelatedEntityCountEnabled(execution))
            {
                ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                countingExecutionEntity.TaskCount += 1;
            }

            HistoryManager.RecordTaskCreated(taskEntity, execution);
            HistoryManager.RecordTaskId(taskEntity);
            if (taskEntity.FormKey is object)
            {
                HistoryManager.RecordTaskFormKeyChange(taskEntity.Id, taskEntity.FormKey);
            }
        }


        public virtual void ChangeTaskAssignee(ITaskEntity taskEntity, string assignee, string assigneeUser)
        {
            ChangeTaskAssignee(taskEntity, assignee, true, assigneeUser);
        }

        public virtual void ChangeTaskAssigneeNoEvents(ITaskEntity taskEntity, string assignee, string assigneeUser)
        {
            ChangeTaskAssignee(taskEntity, assignee, false, assigneeUser);
        }

        private void ChangeTaskAssignee(ITaskEntity taskEntity, string assignee, bool fireEvents, string assigneeUser)
        {
            if ((taskEntity.Assignee is object && !taskEntity.Assignee.Equals(assignee)) || (taskEntity.Assignee is null && assignee is object))
            {
                taskEntity.Assignee = assignee;
                taskEntity.AssigneeUser = assigneeUser;
                if (fireEvents)
                {
                    FireAssignmentEvents(taskEntity);
                }
                else
                {
                    RecordTaskAssignment(taskEntity);
                }

                if (taskEntity.Id is object)
                {
                    HistoryManager.RecordTaskAssigneeChange(taskEntity.Id, taskEntity.Assignee, taskEntity.AssigneeUser);
                    AddAssigneeIdentityLinks(taskEntity);
                    Update(taskEntity);
                }
            }
        }

        public virtual void ChangeTaskOwner(ITaskEntity taskEntity, string owner)
        {
            if ((taskEntity.Owner is object && !taskEntity.Owner.Equals(owner)) || (taskEntity.Owner is null && owner is object))
            {
                taskEntity.Owner = owner;

                if (taskEntity.Id is object)
                {
                    HistoryManager.RecordTaskOwnerChange(taskEntity.Id, taskEntity.Owner);
                    AddOwnerIdentityLink(taskEntity, taskEntity.Owner);
                    Update(taskEntity);
                }
            }
        }

        protected internal virtual void FireAssignmentEvents(ITaskEntity taskEntity)
        {
            RecordTaskAssignment(taskEntity);
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_ASSIGNED, taskEntity));
            }

        }

        protected internal virtual void RecordTaskAssignment(ITaskEntity taskEntity)
        {
            ProcessEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(taskEntity, BaseTaskListenerFields.EVENTNAME_ASSIGNMENT);
            HistoryManager.RecordTaskAssignment(taskEntity);

        }

        private void AddAssigneeIdentityLinks(ITaskEntity taskEntity)
        {
            if (taskEntity.Assignee is object && taskEntity.ProcessInstance is object)
            {
                IdentityLinkEntityManager.InvolveUser(taskEntity.ProcessInstance, taskEntity.Assignee, IdentityLinkType.PARTICIPANT);
            }
        }

        protected internal virtual void AddOwnerIdentityLink(ITaskEntity taskEntity, string owner)
        {
            if (owner is null && taskEntity.Owner is null)
            {
                return;
            }

            if (owner is object && taskEntity.ProcessInstanceId is object)
            {
                IdentityLinkEntityManager.InvolveUser(taskEntity.ProcessInstance, owner, IdentityLinkType.PARTICIPANT);
            }
        }

        public virtual void DeleteTasksByProcessInstanceId(string processInstanceId, string deleteReason, bool cascade)
        {
            IList<ITaskEntity> tasks = FindTasksByProcessInstanceId(processInstanceId);

            foreach (ITaskEntity task in tasks)
            {
                if (EventDispatcher.Enabled && !task.Canceled)
                {
                    task.Canceled = true;
                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(task.Execution.ActivityId, task.Name, task.ExecutionId, task.ProcessInstanceId, task.ProcessDefinitionId, "userTask", deleteReason));
                }

                DeleteTask(task, deleteReason, cascade, false);
            }
        }

        public virtual void DeleteTask(ITaskEntity task, string deleteReason, bool cascade, bool cancel)
        {
            if (!task.Deleted)
            {
                ProcessEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(task, BaseTaskListenerFields.EVENTNAME_DELETE);
                task.Deleted = true;

                string taskId = task.Id;

                IList<ITask> subTasks = FindTasksByParentTaskId(taskId);
                foreach (ITask subTask in subTasks)
                {
                    DeleteTask((ITaskEntity)subTask, deleteReason, cascade, cancel);
                }

                IdentityLinkEntityManager.DeleteIdentityLinksByTaskId(taskId);
                VariableInstanceEntityManager.DeleteVariableInstanceByTask(task);

                if (cascade)
                {
                    HistoricTaskInstanceEntityManager.Delete(new KeyValuePair<string, object>("id", taskId));
                }
                else
                {
                    HistoryManager.RecordTaskEnd(taskId, deleteReason);
                }

                Delete(task, false);

                if (EventDispatcher.Enabled)
                {
                    if (cancel && !task.Canceled)
                    {
                        task.Canceled = true;
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(task.Execution?.ActivityId, task.Name, task.ExecutionId, task.ProcessInstanceId, task.ProcessDefinitionId, "userTask", deleteReason));
                    }

                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, task));
                }
            }
        }

        public override void Delete(ITaskEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, fireDeleteEvent);

            if (entity.ExecutionId is object && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)entity.Execution;
                if (IsExecutionRelatedEntityCountEnabled(countingExecutionEntity))
                {
                    countingExecutionEntity.TaskCount -= 1;
                }
            }
        }

        public virtual IList<ITaskEntity> FindTasksByExecutionId(string executionId)
        {
            return taskDataManager.FindTasksByExecutionId(executionId);
        }

        public virtual IList<ITaskEntity> FindTasksByProcessInstanceId(string processInstanceId)
        {
            return taskDataManager.FindTasksByProcessInstanceId(processInstanceId);
        }

        public virtual IList<ITask> FindTasksByQueryCriteria(ITaskQuery taskQuery)
        {
            return taskDataManager.FindTasksByQueryCriteria(taskQuery);
        }

        public virtual IList<ITask> FindTasksAndVariablesByQueryCriteria(ITaskQuery taskQuery)
        {
            return taskDataManager.FindTasksAndVariablesByQueryCriteria(taskQuery);
        }

        public virtual long FindTaskCountByQueryCriteria(ITaskQuery taskQuery)
        {
            return taskDataManager.FindTaskCountByQueryCriteria(taskQuery);
        }

        public virtual IList<ITask> FindTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return taskDataManager.FindTasksByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindTaskCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return taskDataManager.FindTaskCountByNativeQuery(parameterMap);
        }

        public virtual IList<ITask> FindTasksByParentTaskId(string parentTaskId)
        {
            return taskDataManager.FindTasksByParentTaskId(parentTaskId);
        }

        public virtual void DeleteTask(string taskId, string deleteReason, bool cascade)
        {

            ITaskEntity task = FindById<ITaskEntity>(taskId);

            if (task != null)
            {
                if (task.ExecutionId is object)
                {
                    throw new ActivitiException("The task cannot be deleted because is part of a running process");
                }

                DeleteTask(task, deleteReason, cascade, false);
            }
            else if (cascade)
            {
                HistoricTaskInstanceEntityManager.Delete(new KeyValuePair<string, object>("id", taskId));
            }
        }

        public virtual void UpdateTaskTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            taskDataManager.UpdateTaskTenantIdForDeployment(deploymentId, newTenantId);
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