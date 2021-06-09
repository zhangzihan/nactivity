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

    [Serializable]
    public class IntermediateThrowMessageEventActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal readonly MessageEventDefinition messageEventDefinition;
        protected internal string messageEventName;
        protected internal string messageExpression;

        public IntermediateThrowMessageEventActivityBehavior(MessageEventDefinition messageEventDefinition, Message message)
        {
            if (message is object)
            {
                messageEventName = message.Name;
            }
            else if (!string.IsNullOrWhiteSpace(messageEventDefinition.MessageRef))
            {
                messageEventName = messageEventDefinition.MessageRef;
            }
            else
            {
                messageExpression = messageEventDefinition.MessageExpression;
            }

            this.messageEventDefinition = messageEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;

            string eventSubscriptionName;
            if (messageEventName is object)
            {
                eventSubscriptionName = messageEventName;
            }
            else
            {
                IExpression expressionObject = commandContext.ProcessEngineConfiguration.ExpressionManager.CreateExpression(messageExpression);
                eventSubscriptionName = expressionObject.GetValue(execution).ToString();
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
            IList<IMessageEventSubscriptionEntity> subscriptionEntities = eventSubscriptionEntityManager.FindMessageEventSubscriptionsByProcessInstanceAndEventName(execution.ProcessInstanceId, eventSubscriptionName);

            foreach (IMessageEventSubscriptionEntity messageEventSubscriptionEntity in subscriptionEntities)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateMessageEvent(ActivitiEventType.ACTIVITY_MESSAGE_RECEIVED, messageEventSubscriptionEntity.ActivityId, eventSubscriptionName, null, messageEventSubscriptionEntity.ExecutionId, messageEventSubscriptionEntity.ProcessInstanceId, messageEventSubscriptionEntity.ProcessDefinitionId));

                eventSubscriptionEntityManager.EventReceived(messageEventSubscriptionEntity, null, false);
            }

            Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(execution, true);
        }
    }
}