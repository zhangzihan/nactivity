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

namespace Sys.Workflow.engine.impl.@event
{
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class MessageEventHandler : AbstractEventHandler
    {

        public const string EVENT_HANDLER_TYPE = "message";

        public override string EventHandlerType
        {
            get
            {
                return EVENT_HANDLER_TYPE;
            }
        }

        public override void HandleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {
            // As stated in the ActivitiEventType java-doc, the message-event is
            // thrown before the actual message has been sent
            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateMessageEvent(ActivitiEventType.ACTIVITY_MESSAGE_RECEIVED, eventSubscription.ActivityId, eventSubscription.EventName, payload, eventSubscription.ExecutionId, eventSubscription.ProcessInstanceId, eventSubscription.Execution.ProcessDefinitionId));
            }

            base.HandleEvent(eventSubscription, payload, commandContext);
        }
    }
}