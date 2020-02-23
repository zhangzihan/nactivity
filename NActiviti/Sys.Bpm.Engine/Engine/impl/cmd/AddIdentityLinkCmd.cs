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
    using Sys.Workflow.Engine.Tasks;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;

    /// 
    [Serializable]
    public class AddIdentityLinkCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        public static int IDENTITY_USER = 1;
        public static int IDENTITY_GROUP = 2;

        protected internal string identityId;

        protected internal int identityIdType;

        protected internal string identityType;

        private readonly IUserServiceProxy userService;

        public AddIdentityLinkCmd(string taskId, string identityId, int identityIdType, string identityType) : base(taskId)
        {
            ValidateParams(taskId, identityId, identityIdType, identityType);
            this.taskId = taskId;
            this.identityId = identityId;
            this.identityIdType = identityIdType;
            this.identityType = identityType;
            userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
        }

        protected internal virtual void ValidateParams(string taskId, string identityId, int identityIdType, string identityType)
        {
            if (taskId is null)
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            if (identityType is null)
            {
                throw new ActivitiIllegalArgumentException("type is required when adding a new task identity link");
            }

            if (identityId is null && (identityIdType == IDENTITY_GROUP || (!IdentityLinkType.ASSIGNEE.Equals(identityType) && !IdentityLinkType.OWNER.Equals(identityType))))
            {

                throw new ActivitiIllegalArgumentException("identityId is null");
            }

            if (identityIdType != IDENTITY_USER && identityIdType != IDENTITY_GROUP)
            {
                throw new ActivitiIllegalArgumentException("identityIdType allowed values are 1 and 2");
            }
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            bool assignedToNoOne = false;
            if (IdentityLinkType.ASSIGNEE.Equals(identityType))
            {
                string assigneeUser = null;
                if (string.IsNullOrWhiteSpace(identityId) == false)
                {
                    //TODO: 考虑性能问题，暂时不要获取人员信息
                    //assigneeUser = userService.GetUser(identityId).GetAwaiter().GetResult()?.FullName;
                }
                commandContext.TaskEntityManager.ChangeTaskAssignee(task, identityId, assigneeUser);
                assignedToNoOne = identityId is null;
            }
            else if (IdentityLinkType.OWNER.Equals(identityType))
            {
                commandContext.TaskEntityManager.ChangeTaskOwner(task, identityId);
            }
            else if (IDENTITY_USER == identityIdType)
            {
                task.AddUserIdentityLink(identityId, identityType);
            }
            else if (IDENTITY_GROUP == identityIdType)
            {
                task.AddGroupIdentityLink(identityId, identityType);
            }

            bool forceNullUserId = false;
            if (assignedToNoOne)
            {
                // ACT-1317: Special handling when assignee is set to NULL, a
                // CommentEntity notifying of assignee-delete should be created
                forceNullUserId = true;

            }

            if (IDENTITY_USER == identityIdType)
            {
                commandContext.HistoryManager.CreateUserIdentityLinkComment(taskId, identityId, identityType, true, forceNullUserId);
            }
            else
            {
                commandContext.HistoryManager.CreateGroupIdentityLinkComment(taskId, identityId, identityType, true);
            }

            return null;
        }

    }

}