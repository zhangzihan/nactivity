using System;
using System.Collections.Generic;

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
    using Sys.Workflow.engine.repository;
    using Sys.Workflow.engine.task;

    /// 
    [Serializable]
    public class GetIdentityLinksForProcessDefinitionCmd : ICommand<IList<IIdentityLink>>
    {

        private const long serialVersionUID = 1L;
        protected internal string processDefinitionId;

        public GetIdentityLinksForProcessDefinitionCmd(string processDefinitionId)
        {
            this.processDefinitionId = processDefinitionId;
        }

        public virtual IList<IIdentityLink> Execute(ICommandContext commandContext)
        {
            IProcessDefinitionEntity processDefinition = commandContext.ProcessDefinitionEntityManager.FindById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("id", processDefinitionId));

            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find process definition with id " + processDefinitionId, typeof(IProcessDefinition));
            }

            IList<IIdentityLink> identityLinks = (IList<IIdentityLink>)processDefinition.IdentityLinks;

            return identityLinks;
        }

    }

}