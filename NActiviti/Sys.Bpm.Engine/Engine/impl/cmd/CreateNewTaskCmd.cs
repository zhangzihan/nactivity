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
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System;

    /// 
    /// 
    // Not Serializable
    public class CreateNewTaskCmd : ICommand<ITask>
    {
        protected internal string taskName;
        protected internal string description;
        protected internal DateTime? dueDate;
        protected internal int? priority;
        protected internal string parentTaskId;
        protected internal string assignee;
        protected readonly string tenantId;
        private readonly IUserServiceProxy userService;

        public CreateNewTaskCmd(string taskName, string description, DateTime? dueDate, int? priority, string parentTaskId, string assignee, string tenantId)
        {
            this.taskName = taskName;
            this.description = description;
            this.dueDate = dueDate;
            this.parentTaskId = parentTaskId;
            this.priority = priority;
            this.assignee = assignee;
            this.tenantId = tenantId;
            userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
        }

        public virtual ITask Execute(ICommandContext commandContext)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
            ITaskService taskService = processEngineConfiguration.TaskService;
            string id = processEngineConfiguration.IdGenerator.GetNextId();
            ITask task = taskService.NewTask(id);
            task.Name = taskName;
            task.Description = description;
            task.DueDate = dueDate;
            task.Priority = priority;
            task.Assignee = assignee;
            if (string.IsNullOrWhiteSpace(assignee) == false)
            {
                task.AssigneeUser = AsyncHelper.RunSync(() => userService.GetUser(assignee))?.FullName;
            }
            taskService.SaveTask(task);

            return task;
        }
    }
}