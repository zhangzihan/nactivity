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

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;

    /// 
    [Serializable]
    public class ClaimTaskCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string userId;

        private readonly IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

        public ClaimTaskCmd(string taskId, string userId) : base(taskId)
        {
            this.userId = userId;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (userId is object)
            {
                task.ClaimTime = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

                if (task.Assignee is object)
                {
                    if (!task.Assignee.Equals(userId))
                    {
                        // When the task is already claimed by another user, throw
                        // exception. Otherwise, ignore
                        // this, post-conditions of method already met.
                        throw new ActivitiTaskAlreadyClaimedException(task.Id, task.Assignee);
                    }
                }
                else
                {
                    //TODO: 考虑性能问题，暂时不要获取人员信息
                    IUserInfo user = userService.GetUser(userId).GetAwaiter().GetResult();
                    task.SetVariable(userId, new { id = userId });
                    commandContext.TaskEntityManager.ChangeTaskAssignee(task, userId, user?.FullName);
                }
            }
            else
            {
                // Task claim time should be null
                task.ClaimTime = null;
                task.RemoveVariable(task.Assignee);

                // Task should be assigned to no one
                commandContext.TaskEntityManager.ChangeTaskAssignee(task, null, null);
            }

            // Add claim time to historic task instance
            commandContext.HistoryManager.RecordTaskClaim(task);

            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot claim a suspended task";
            }
        }

    }

}