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
namespace org.activiti.engine.impl.bpmn.helper
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// An <seealso cref="IActivitiEventListener"/> that throws a message event when an event is dispatched to it. Sends the message to the execution the event was fired from. If the execution is not subscribed to a
    /// message, the process-instance is checked.
    /// 
    /// 
    /// 
    /// </summary>
    public class MessageThrowingEventListener : BaseDelegateEventListener
    {

        protected internal string messageName;
        protected internal new Type entityClass;

        public override void onEvent(IActivitiEvent @event)
        {
            if (isValidEvent(@event))
            {
                if (ReferenceEquals(@event.ProcessInstanceId, null))
                {
                    throw new ActivitiIllegalArgumentException("Cannot throw process-instance scoped message, since the dispatched event is not part of an ongoing process instance");
                }

                IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
                IList<IMessageEventSubscriptionEntity> subscriptionEntities = eventSubscriptionEntityManager.findMessageEventSubscriptionsByProcessInstanceAndEventName(@event.ProcessInstanceId, messageName);

                foreach (IEventSubscriptionEntity messageEventSubscriptionEntity in subscriptionEntities)
                {
                    eventSubscriptionEntityManager.eventReceived(messageEventSubscriptionEntity, null, false);
                }
            }
        }

        public virtual string MessageName
        {
            set
            {
                this.messageName = value;
            }
        }

        public override bool FailOnException
        {
            get
            {
                return true;
            }
        }
    }

}