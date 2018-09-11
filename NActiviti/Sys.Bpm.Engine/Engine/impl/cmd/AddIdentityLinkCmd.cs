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
    
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.task;

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

        public AddIdentityLinkCmd(string taskId, string identityId, int identityIdType, string identityType) : base(taskId)
        {
            validateParams(taskId, identityId, identityIdType, identityType);
            this.taskId = taskId;
            this.identityId = identityId;
            this.identityIdType = identityIdType;
            this.identityType = identityType;
        }

        protected internal virtual void validateParams(string taskId, string identityId, int identityIdType, string identityType)
        {
            if (string.ReferenceEquals(taskId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            if (string.ReferenceEquals(identityType, null))
            {
                throw new ActivitiIllegalArgumentException("type is required when adding a new task identity link");
            }

            if (string.ReferenceEquals(identityId, null) && (identityIdType == IDENTITY_GROUP || (!IdentityLinkType.ASSIGNEE.Equals(identityType) && !IdentityLinkType.OWNER.Equals(identityType))))
            {

                throw new ActivitiIllegalArgumentException("identityId is null");
            }

            if (identityIdType != IDENTITY_USER && identityIdType != IDENTITY_GROUP)
            {
                throw new ActivitiIllegalArgumentException("identityIdType allowed values are 1 and 2");
            }
        }

        protected internal override object execute(ICommandContext commandContext, ITaskEntity task)
        {
            bool assignedToNoOne = false;
            if (IdentityLinkType.ASSIGNEE.Equals(identityType))
            {
                commandContext.TaskEntityManager.changeTaskAssignee(task, identityId);
                assignedToNoOne = string.ReferenceEquals(identityId, null);
            }
            else if (IdentityLinkType.OWNER.Equals(identityType))
            {
                commandContext.TaskEntityManager.changeTaskOwner(task, identityId);
            }
            else if (IDENTITY_USER == identityIdType)
            {
                task.addUserIdentityLink(identityId, identityType);
            }
            else if (IDENTITY_GROUP == identityIdType)
            {
                task.addGroupIdentityLink(identityId, identityType);
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
                commandContext.HistoryManager.createUserIdentityLinkComment(taskId, identityId, identityType, true, forceNullUserId);
            }
            else
            {
                commandContext.HistoryManager.createGroupIdentityLinkComment(taskId, identityId, identityType, true);
            }

            return null;
        }

    }

}