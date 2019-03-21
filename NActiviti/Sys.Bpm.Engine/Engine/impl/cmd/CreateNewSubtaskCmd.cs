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


    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using System;
    using System.Collections.Generic;

    /// 
    /// 
    // Not Serializable
    public class CreateNewSubtaskCmd : ICommand<ITask>
    {

        protected internal string taskName;
        protected internal string description;
        protected internal DateTime? dueDate;
        protected internal int? priority;
        protected internal string parentTaskId;
        protected internal string assignee;
        protected readonly string tenantId;

        public CreateNewSubtaskCmd(string taskName, string description, DateTime? dueDate, int? priority, string parentTaskId, string assignee, string tenantId)
        {
            this.taskName = taskName;
            this.description = description;
            this.dueDate = dueDate;
            this.parentTaskId = parentTaskId;
            this.priority = priority;
            this.assignee = assignee;
            this.tenantId = tenantId;
        }

        public virtual ITask execute(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;
            if (taskService.createTaskQuery().taskId(parentTaskId).singleResult() == null)
            {
                throw new ActivitiObjectNotFoundException("Parent task with id " + parentTaskId + " was not found");
            }

            string id = commandContext.ProcessEngineConfiguration.IdGenerator.NextId;
            ITask task = taskService.newTask(id);
            task.Name = taskName;
            task.Description = description;
            task.DueDate = dueDate;
            task.Priority = priority;
            task.ParentTaskId = parentTaskId;
            task.Assignee = assignee;
            task.TenantId = tenantId;
            taskService.saveTask(task);

            return task;
        }
    }
}