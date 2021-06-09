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
    using System.Collections.Generic;


    /// 
    /// 
    [Serializable]
    public class AddIdentityLinkForProcessInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string processInstanceId;

        protected internal string userId;

        protected internal string groupId;

        protected internal string type;

        public AddIdentityLinkForProcessInstanceCmd(string processInstanceId, string userId, string groupId, string type)
        {
            ValidateParams(processInstanceId, userId, groupId, type);
            this.processInstanceId = processInstanceId;
            this.userId = userId;
            this.groupId = groupId;
            this.type = type;
        }

        protected internal virtual void ValidateParams(string processInstanceId, string userId, string groupId, string type)
        {

            if (processInstanceId is null)
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }

            if (type is null)
            {
                throw new ActivitiIllegalArgumentException("type is required when adding a new process instance identity link");
            }

            if (userId is null && groupId is null)
            {
                throw new ActivitiIllegalArgumentException("userId and groupId cannot both be null");
            }

        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity processInstance = executionEntityManager.FindById<IExecutionEntity>(processInstanceId);

            if (processInstance is null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find process instance with id " + processInstanceId, typeof(IExecutionEntity));
            }

            IIdentityLinkEntityManager identityLinkEntityManager = commandContext.IdentityLinkEntityManager;
            identityLinkEntityManager.AddIdentityLink(processInstance, userId, groupId, type);
            commandContext.HistoryManager.CreateProcessInstanceIdentityLinkComment(processInstanceId, userId, groupId, type, true);

            return null;
        }
    }
}