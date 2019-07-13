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
namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;

    /// 
    /// 
    [Serializable]
    public class GetIdentityLinksForTaskCmd : ICommand<IList<IIdentityLink>>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;

        public GetIdentityLinksForTaskCmd(string taskId)
        {
            this.taskId = taskId;
        }

        public virtual IList<IIdentityLink> Execute(ICommandContext commandContext)
        {
            ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            IList<IIdentityLink> identityLinks = (IList<IIdentityLink>)task.IdentityLinks;

            // assignee is not part of identity links in the db.
            // so if there is one, we add it here.
            // @Tom: we discussed this long on skype and you agreed ;-)
            // an assignee *is* an identityLink, and so must it be reflected in the
            // API
            //
            // Note: we cant move this code to the TaskEntity (which would be
            // cleaner),
            // since the task.delete cascaded to all associated identityLinks
            // and of course this leads to exception while trying to delete a
            // non-existing identityLink
            if (task.Assignee is object)
            {
                IIdentityLinkEntity identityLink = commandContext.IdentityLinkEntityManager.Create();
                identityLink.UserId = task.Assignee;
                identityLink.Type = IdentityLinkType.ASSIGNEE;
                identityLink.TaskId = task.Id;
                identityLinks.Add(identityLink);
            }
            if (task.Owner is object)
            {
                IIdentityLinkEntity identityLink = commandContext.IdentityLinkEntityManager.Create();
                identityLink.UserId = task.Owner;
                identityLink.TaskId = task.Id;
                identityLink.Type = IdentityLinkType.OWNER;
                identityLinks.Add(identityLink);
            }

            return (IList<IIdentityLink>)task.IdentityLinks;
        }

    }

}