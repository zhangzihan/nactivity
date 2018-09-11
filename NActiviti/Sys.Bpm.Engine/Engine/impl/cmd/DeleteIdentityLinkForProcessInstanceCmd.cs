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
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class DeleteIdentityLinkForProcessInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string processInstanceId;

        protected internal string userId;

        protected internal string groupId;

        protected internal string type;

        public DeleteIdentityLinkForProcessInstanceCmd(string processInstanceId, string userId, string groupId, string type)
        {
            validateParams(userId, groupId, processInstanceId, type);
            this.processInstanceId = processInstanceId;
            this.userId = userId;
            this.groupId = groupId;
            this.type = type;
        }

        protected internal virtual void validateParams(string userId, string groupId, string processInstanceId, string type)
        {
            if (string.ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }

            if (string.ReferenceEquals(type, null))
            {
                throw new ActivitiIllegalArgumentException("type is required when deleting a process identity link");
            }

            if (string.ReferenceEquals(userId, null) && string.ReferenceEquals(groupId, null))
            {
                throw new ActivitiIllegalArgumentException("userId and groupId cannot both be null");
            }
        }

        public virtual object execute(ICommandContext commandContext)
        {
            IExecutionEntity processInstance = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find process instance with id " + processInstanceId, typeof(IExecutionEntity));
            }

            commandContext.IdentityLinkEntityManager.deleteIdentityLink(processInstance, userId, groupId, type);
            commandContext.HistoryManager.createProcessInstanceIdentityLinkComment(processInstanceId, userId, groupId, type, false);

            return null;
        }

    }

}