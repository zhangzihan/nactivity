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

    
    using Sys.Workflow.Engine.Impl.Events;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    /// 
    [Serializable]
    public class MessageEventReceivedCmd : NeedsActiveExecutionCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal readonly IDictionary<string, object> payload;
        protected internal readonly string messageName;
        protected internal readonly bool async;

        public MessageEventReceivedCmd(string messageName, string executionId, IDictionary<string, object> processVariables) : base(executionId)
        {
            this.messageName = messageName;

            if (processVariables != null)
            {
                this.payload = new Dictionary<string, object>(processVariables);

            }
            else
            {
                this.payload = null;
            }
            this.async = false;
        }

        public MessageEventReceivedCmd(string messageName, string executionId, bool async) : base(executionId)
        {
            this.messageName = messageName;
            this.payload = null;
            this.async = async;
        }

        protected internal override object Execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (messageName is null)
            {
                throw new ActivitiIllegalArgumentException("messageName cannot be null");
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
            IList<IEventSubscriptionEntity> eventSubscriptions = eventSubscriptionEntityManager.FindEventSubscriptionsByNameAndExecution(MessageEventHandler.EVENT_HANDLER_TYPE, messageName, executionId);

            if (eventSubscriptions.Count == 0)
            {
                throw new ActivitiException("Execution with id '" + executionId + "' does not have a subscription to a message event with name '" + messageName + "'");
            }

            // there can be only one:
            IEventSubscriptionEntity eventSubscriptionEntity = eventSubscriptions[0];
            eventSubscriptionEntityManager.EventReceived(eventSubscriptionEntity, payload, async);

            return null;
        }

    }

}