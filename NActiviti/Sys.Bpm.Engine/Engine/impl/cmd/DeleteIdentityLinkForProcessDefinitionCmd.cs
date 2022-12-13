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
    using Sys.Workflow.Engine.Repository;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class DeleteIdentityLinkForProcessDefinitionCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId;

        protected internal string userId;

        protected internal string groupId;

        public DeleteIdentityLinkForProcessDefinitionCmd(string processDefinitionId, string userId, string groupId)
        {
            ValidateParams(userId, groupId, processDefinitionId);
            this.processDefinitionId = processDefinitionId;
            this.userId = userId;
            this.groupId = groupId;
        }

        protected internal virtual void ValidateParams(string userId, string groupId, string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("processDefinitionId is null");
            }

            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(groupId))
            {
                throw new ActivitiIllegalArgumentException("userId and groupId cannot both be null");
            }
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IProcessDefinitionEntity processDefinition = commandContext.ProcessDefinitionEntityManager.FindById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));

            if (processDefinition is null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find process definition with id " + processDefinitionId, typeof(IProcessDefinition));
            }

            commandContext.IdentityLinkEntityManager.DeleteIdentityLink(processDefinition, userId, groupId);

            return null;
        }

    }

}