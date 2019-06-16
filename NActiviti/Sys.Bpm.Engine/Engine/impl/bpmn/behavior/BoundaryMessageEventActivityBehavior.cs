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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class BoundaryMessageEventActivityBehavior : BoundaryEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal MessageEventDefinition messageEventDefinition;

        public BoundaryMessageEventActivityBehavior(MessageEventDefinition messageEventDefinition, bool interrupting) : base(interrupting)
        {
            this.messageEventDefinition = messageEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;

            string messageName;
            if (!string.IsNullOrWhiteSpace(messageEventDefinition.MessageRef))
            {
                messageName = messageEventDefinition.MessageRef;
            }
            else
            {
                IExpression messageExpression = commandContext.ProcessEngineConfiguration.ExpressionManager.CreateExpression(messageEventDefinition.MessageExpression);
                messageName = messageExpression.GetValue(execution).ToString();
            }

            commandContext.EventSubscriptionEntityManager.InsertMessageEvent(messageName, execution);

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateMessageEvent(ActivitiEventType.ACTIVITY_MESSAGE_WAITING, execution.ActivityId, messageName, null, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId));
            }
        }

        public override void Trigger(IExecutionEntity execution, string triggerName, object triggerData, bool throwError = true)
        {
            IExecutionEntity executionEntity = execution;
            BoundaryEvent boundaryEvent = (BoundaryEvent)execution.CurrentFlowElement;

            if (boundaryEvent.CancelActivity)
            {
                IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
                IList<IEventSubscriptionEntity> eventSubscriptions = executionEntity.EventSubscriptions;
                foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
                {
                    if (eventSubscription.EventType == MessageEventSubscriptionEntityFields.EVENT_TYPE && eventSubscription.EventName.Equals(messageEventDefinition.MessageRef))
                    {

                        eventSubscriptionEntityManager.Delete(eventSubscription);
                    }
                }
            }

            base.Trigger(executionEntity, triggerName, triggerData, throwError);
        }
    }
}