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
    using Sys.Workflow.Engine.Tasks;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;

    /// 
    /// 
    [Serializable]
    public class DelegateTaskCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string userId;
        private readonly IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

        public DelegateTaskCmd(string taskId, string userId) : base(taskId)
        {
            this.userId = userId;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            task.DelegationState = DelegationState.PENDING;
            if (task.Owner is null)
            {
                task.Owner = task.Assignee;
            }
            string userName = null;
            //TODO: 考虑性能问题，暂时不要获取人员信息
            //if (string.IsNullOrWhiteSpace(userId) == false)
            //{
            //    userName = AsyncHelper.RunSync(() => userService.GetUser(userId))?.FullName;
            //}
            commandContext.TaskEntityManager.ChangeTaskAssignee(task, userId, userName);
            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot delegate a suspended task";
            }
        }

    }

}