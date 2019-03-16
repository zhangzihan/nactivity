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
namespace org.activiti.engine.impl.cmd
{

    
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.task;
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

        public  virtual object  execute(ICommandContext commandContext)
        {
            if (task == null)
            {
                throw new ActivitiIllegalArgumentException("task is null");
            }

            if (task.Revision == 0)
            {
                commandContext.TaskEntityManager.insert(task, null);

                if (commandContext.EventDispatcher.Enabled)
                {
                    commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_CREATED, task));
                    if (!ReferenceEquals(task.Assignee, null))
                    {
                        commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                    }
                }

            }
            else
            {

                ITaskInfo originalTaskEntity = null;
                if (commandContext.ProcessEngineConfiguration.HistoryLevel.isAtLeast(HistoryLevel.AUDIT))
                {
                    originalTaskEntity = commandContext.HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", task.Id));
                }

                if (originalTaskEntity == null)
                {
                    originalTaskEntity = commandContext.TaskEntityManager.findById<ITaskEntity>(new KeyValuePair<string, object>("id", task.Id));
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
                if (commandContext.ProcessEngineConfiguration.HistoryLevel.isAtLeast(HistoryLevel.AUDIT))
                {

                    if (originalName != task.Name)
                    {
                        commandContext.HistoryManager.recordTaskNameChange(task.Id, task.Name);
                    }
                    if (originalDescription != task.Description)
                    {
                        commandContext.HistoryManager.recordTaskDescriptionChange(task.Id, task.Description);
                    }
                    if ((!originalDueDate.HasValue && task.DueDate.HasValue) || (originalDueDate.HasValue && !task.DueDate.HasValue) || (originalDueDate.HasValue && !originalDueDate.Equals(task.DueDate)))
                    {
                        commandContext.HistoryManager.recordTaskDueDateChange(task.Id, task.DueDate.Value);
                    }
                    if (originalPriority != task.Priority)
                    {
                        commandContext.HistoryManager.recordTaskPriorityChange(task.Id, task.Priority);
                    }
                    if (originalCategory != task.Category)
                    {
                        commandContext.HistoryManager.recordTaskCategoryChange(task.Id, task.Category);
                    }
                    if (originalFormKey != task.FormKey)
                    {
                        commandContext.HistoryManager.recordTaskFormKeyChange(task.Id, task.FormKey);
                    }
                    if (originalParentTaskId != task.ParentTaskId)
                    {
                        commandContext.HistoryManager.recordTaskParentTaskIdChange(task.Id, task.ParentTaskId);
                    }
                    if (originalTaskDefinitionKey != task.TaskDefinitionKey)
                    {
                        commandContext.HistoryManager.recordTaskDefinitionKeyChange(task.Id, task.TaskDefinitionKey);
                    }

                }

                if (originalOwner != task.Owner)
                {
                    if (!ReferenceEquals(task.ProcessInstanceId, null))
                    {
                        commandContext.IdentityLinkEntityManager.involveUser(task.ProcessInstance, task.Owner, IdentityLinkType.PARTICIPANT);
                    }
                    commandContext.HistoryManager.recordTaskOwnerChange(task.Id, task.Owner);
                }
                if (originalAssignee != task.Assignee)
                {
                    if (!ReferenceEquals(task.ProcessInstanceId, null))
                    {
                        commandContext.IdentityLinkEntityManager.involveUser(task.ProcessInstance, task.Assignee, IdentityLinkType.PARTICIPANT);
                    }
                    commandContext.HistoryManager.recordTaskAssigneeChange(task.Id, task.Assignee);

                    commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.executeTaskListeners(task, BaseTaskListener_Fields.EVENTNAME_ASSIGNMENT);
                    commandContext.HistoryManager.recordTaskAssignment(task);

                    if (commandContext.EventDispatcher.Enabled)
                    {
                        commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                    }

                }

                commandContext.TaskEntityManager.update(task);

            }


            return null;
        }

    }

}