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
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;

    /// 
    [Serializable]
    public class GetHistoricIdentityLinksForTaskCmd : ICommand<IList<IHistoricIdentityLink>>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal string processInstanceId;

        public GetHistoricIdentityLinksForTaskCmd(string taskId, string processInstanceId)
        {
            if (ReferenceEquals(taskId, null) && ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId or processInstanceId is required");
            }
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
        }

        public virtual IList<IHistoricIdentityLink> execute(ICommandContext commandContext)
        {
            if (!ReferenceEquals(taskId, null))
            {
                return getLinksForTask(commandContext);
            }
            else
            {
                return getLinksForProcessInstance(commandContext);
            }
        }

        protected internal virtual IList<IHistoricIdentityLink> getLinksForTask(ICommandContext commandContext)
        {
            IHistoricTaskInstanceEntity task = commandContext.HistoricTaskInstanceEntityManager.findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("No historic task exists with the given id: " + taskId, typeof(IHistoricTaskInstance));
            }

            IList<IHistoricIdentityLink> identityLinks = (IList<IHistoricIdentityLink>)commandContext.HistoricIdentityLinkEntityManager.findHistoricIdentityLinksByTaskId(taskId);

            // Similar to GetIdentityLinksForTask, return assignee and owner as
            // identity link
            if (!ReferenceEquals(task.Assignee, null))
            {
                IHistoricIdentityLinkEntity identityLink = commandContext.HistoricIdentityLinkEntityManager.create();
                identityLink.UserId = task.Assignee;
                identityLink.TaskId = task.Id;
                identityLink.Type = IdentityLinkType.ASSIGNEE;
                identityLinks.Add(identityLink);
            }
            if (!ReferenceEquals(task.Owner, null))
            {
                IHistoricIdentityLinkEntity identityLink = commandContext.HistoricIdentityLinkEntityManager.create();
                identityLink.TaskId = task.Id;
                identityLink.UserId = task.Owner;
                identityLink.Type = IdentityLinkType.OWNER;
                identityLinks.Add(identityLink);
            }

            return identityLinks;
        }

        protected internal virtual IList<IHistoricIdentityLink> getLinksForProcessInstance(ICommandContext commandContext)
        {
            return (IList<IHistoricIdentityLink>)commandContext.HistoricIdentityLinkEntityManager.findHistoricIdentityLinksByProcessInstanceId(processInstanceId);
        }

    }

}