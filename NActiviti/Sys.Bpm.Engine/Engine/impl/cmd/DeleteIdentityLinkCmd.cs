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

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.task;

    /// 
    /// 
    /// 
    [Serializable]
    public class DeleteIdentityLinkCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        public static int IDENTITY_USER = 1;
        public static int IDENTITY_GROUP = 2;

        protected internal string userId;

        protected internal string groupId;

        protected internal string type;

        public DeleteIdentityLinkCmd(string taskId, string userId, string groupId, string type) : base(taskId)
        {
            ValidateParams(userId, groupId, type, taskId);
            this.taskId = taskId;
            this.userId = userId;
            this.groupId = groupId;
            this.type = type;
        }

        protected internal virtual void ValidateParams(string userId, string groupId, string type, string taskId)
        {
            if (taskId is null)
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            if (type is null)
            {
                throw new ActivitiIllegalArgumentException("type is required when adding a new task identity link");
            }

            // Special treatment for assignee and owner: group cannot be used and
            // userId may be null
            if (IdentityLinkType.ASSIGNEE.Equals(type) || IdentityLinkType.OWNER.Equals(type))
            {
                if (!(groupId is null))
                {
                    throw new ActivitiIllegalArgumentException("Incompatible usage: cannot use type '" + type + "' together with a groupId");
                }
            }
            else
            {
                if (userId is null && groupId is null)
                {
                    throw new ActivitiIllegalArgumentException("userId and groupId cannot both be null");
                }
            }
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (IdentityLinkType.ASSIGNEE.Equals(type))
            {
                commandContext.TaskEntityManager.ChangeTaskAssignee(task, null, null);
            }
            else if (IdentityLinkType.OWNER.Equals(type))
            {
                commandContext.TaskEntityManager.ChangeTaskOwner(task, null);
            }
            else
            {
                commandContext.IdentityLinkEntityManager.DeleteIdentityLink(task, userId, groupId, type);
            }

            commandContext.HistoryManager.CreateIdentityLinkComment(taskId, userId, groupId, type, false);

            return null;
        }

    }

}