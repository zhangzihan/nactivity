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
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System;

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
        private readonly IUserServiceProxy userService;

        public CreateNewSubtaskCmd(string taskName, string description, DateTime? dueDate, int? priority, string parentTaskId, string assignee, string tenantId)
        {
            this.taskName = taskName;
            this.description = description;
            this.dueDate = dueDate;
            this.parentTaskId = parentTaskId;
            this.priority = priority;
            this.assignee = assignee;
            this.tenantId = tenantId;
            this.userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
        }

        public virtual ITask Execute(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;
            if (taskService.CreateTaskQuery().SetTaskId(parentTaskId).SingleResult() is null)
            {
                throw new ActivitiObjectNotFoundException("Parent task with id " + parentTaskId + " was not found");
            }

            string id = commandContext.ProcessEngineConfiguration.IdGenerator.GetNextId();
            ITask task = taskService.NewTask(id);
            task.Name = taskName;
            task.Description = description;
            task.DueDate = dueDate;
            task.Priority = priority;
            task.ParentTaskId = parentTaskId;
            task.Assignee = assignee;
            //TODO: 考虑性能问题，暂时不要获取人员信息
            if (string.IsNullOrWhiteSpace(assignee) == false)
            {
                task.AssigneeUser = userService.GetUser(assignee).GetAwaiter().GetResult()?.FullName;
            }
            task.TenantId = tenantId;
            taskService.SaveTask(task);

            return task;
        }
    }
}