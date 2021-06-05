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
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow.Services.Api.Commands;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;

    /// 
    /// 
    // Not Serializable
    public class UpdateTaskCmd : ICommand<ITask>
    {
        protected internal IUpdateTaskCmd updateTaskCmd;
        private readonly IUserServiceProxy userService;

        public UpdateTaskCmd(IUpdateTaskCmd cmd)
        {
            this.updateTaskCmd = cmd;
            userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
        }

        public virtual ITask Execute(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;
            ITask task = taskService.CreateTaskQuery().SetTaskId(updateTaskCmd.TaskId).SingleResult();
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + updateTaskCmd.TaskId);
            }
            task.Assignee = updateTaskCmd.Assignee;
            //TODO: 考虑性能问题，暂时不要获取人员信息
            if (string.IsNullOrWhiteSpace(task.Assignee) == false)
            {
                task.AssigneeUser = userService.GetUser(task.Assignee).GetAwaiter().GetResult()?.FullName;
            }
            task.Name = updateTaskCmd.Name;
            task.Description = updateTaskCmd.Description;
            task.DueDate = updateTaskCmd.DueDate;
            task.Priority = updateTaskCmd.Priority;
            task.ParentTaskId = string.IsNullOrWhiteSpace(updateTaskCmd.ParentTaskId) ? task.ParentTaskId : updateTaskCmd.ParentTaskId;
            taskService.SaveTask(task);

            return task;
        }
    }
}