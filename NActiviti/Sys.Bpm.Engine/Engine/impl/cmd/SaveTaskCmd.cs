using System;

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
namespace Sys.Workflow.Engine.Impl.Cmd
{

    
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Histories;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Tasks;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class SaveTaskCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal ITaskEntity task;

        public SaveTaskCmd(ITask task)
        {
            this.task = (ITaskEntity)task;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {
            if (task == null)
            {
                throw new ActivitiIllegalArgumentException("task is null");
            }

            if (task.Revision == 0)
            {
                commandContext.TaskEntityManager.Insert(task, null);

                if (commandContext.EventDispatcher.Enabled)
                {
                    commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_CREATED, task));
                    if (task.Assignee is object)
                    {
                        commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                    }
                }

            }
            else
            {

                ITaskInfo originalTaskEntity = null;
                if (commandContext.ProcessEngineConfiguration.HistoryLevel.IsAtLeast(HistoryLevel.AUDIT))
                {
                    originalTaskEntity = commandContext.HistoricTaskInstanceEntityManager.FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", task.Id));
                }

                if (originalTaskEntity == null)
                {
                    originalTaskEntity = commandContext.TaskEntityManager.FindById<ITaskEntity>(task.Id);
                }

                string originalName = originalTaskEntity.Name;
                string originalAssignee = originalTaskEntity.Assignee;
                string originalOwner = originalTaskEntity.Owner;
                string originalDescription = originalTaskEntity.Description;
                DateTime? originalDueDate = originalTaskEntity.DueDate;
                int? originalPriority = originalTaskEntity.Priority;
                string originalCategory = originalTaskEntity.Category;
                string originalFormKey = originalTaskEntity.FormKey;
                string originalParentTaskId = originalTaskEntity.ParentTaskId;
                string originalTaskDefinitionKey = originalTaskEntity.TaskDefinitionKey;

                // Only update history if history is enabled
                if (commandContext.ProcessEngineConfiguration.HistoryLevel.IsAtLeast(HistoryLevel.AUDIT))
                {

                    if (originalName != task.Name)
                    {
                        commandContext.HistoryManager.RecordTaskNameChange(task.Id, task.Name);
                    }
                    if (originalDescription != task.Description)
                    {
                        commandContext.HistoryManager.RecordTaskDescriptionChange(task.Id, task.Description);
                    }
                    if ((!originalDueDate.HasValue && task.DueDate.HasValue) || (originalDueDate.HasValue && !task.DueDate.HasValue) || (originalDueDate.HasValue && !originalDueDate.Equals(task.DueDate)))
                    {
                        commandContext.HistoryManager.RecordTaskDueDateChange(task.Id, task.DueDate.Value);
                    }
                    if (originalPriority != task.Priority)
                    {
                        commandContext.HistoryManager.RecordTaskPriorityChange(task.Id, task.Priority);
                    }
                    if (originalCategory != task.Category)
                    {
                        commandContext.HistoryManager.RecordTaskCategoryChange(task.Id, task.Category);
                    }
                    if (originalFormKey != task.FormKey)
                    {
                        commandContext.HistoryManager.RecordTaskFormKeyChange(task.Id, task.FormKey);
                    }
                    if (originalParentTaskId != task.ParentTaskId)
                    {
                        commandContext.HistoryManager.RecordTaskParentTaskIdChange(task.Id, task.ParentTaskId);
                    }
                    if (originalTaskDefinitionKey != task.TaskDefinitionKey)
                    {
                        commandContext.HistoryManager.RecordTaskDefinitionKeyChange(task.Id, task.TaskDefinitionKey);
                    }

                }

                if (originalOwner != task.Owner)
                {
                    if (task.ProcessInstanceId is object)
                    {
                        commandContext.IdentityLinkEntityManager.InvolveUser(task.ProcessInstance, task.Owner, IdentityLinkType.PARTICIPANT);
                    }
                    commandContext.HistoryManager.RecordTaskOwnerChange(task.Id, task.Owner);
                }
                if (originalAssignee != task.Assignee)
                {
                    if (task.ProcessInstanceId is object)
                    {
                        commandContext.IdentityLinkEntityManager.InvolveUser(task.ProcessInstance, task.Assignee, IdentityLinkType.PARTICIPANT);
                    }
                    commandContext.HistoryManager.RecordTaskAssigneeChange(task.Id, task.Assignee, task.AssigneeUser);

                    commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(task, BaseTaskListenerFields.EVENTNAME_ASSIGNMENT);
                    commandContext.HistoryManager.RecordTaskAssignment(task);

                    if (commandContext.EventDispatcher.Enabled)
                    {
                        commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                    }

                }

                commandContext.TaskEntityManager.Update(task);

            }


            return null;
        }

    }

}