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
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    [Serializable]
    public class IntermediateCatchSignalEventActivityBehavior : IntermediateCatchEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal SignalEventDefinition signalEventDefinition;
        protected internal Signal signal;

        public IntermediateCatchSignalEventActivityBehavior(SignalEventDefinition signalEventDefinition, Signal signal)
        {
            this.signalEventDefinition = signalEventDefinition;
            this.signal = signal;
        }

        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;

            string signalName;
            if (!string.IsNullOrWhiteSpace(signalEventDefinition.SignalRef))
            {
                signalName = signalEventDefinition.SignalRef;
            }
            else
            {
                IExpression signalExpression = commandContext.ProcessEngineConfiguration.ExpressionManager.CreateExpression(signalEventDefinition.SignalExpression);
                signalName = signalExpression.GetValue(execution).ToString();
            }

            commandContext.EventSubscriptionEntityManager.InsertSignalEvent(signalName, signal, execution);
        }

        public override void Trigger(IExecutionEntity execution, string triggerName, object triggerData, bool throwError = true)
        {
            IExecutionEntity executionEntity = DeleteSignalEventSubscription(execution);
            LeaveIntermediateCatchEvent(executionEntity);
        }

        public override void EventCancelledByEventGateway(IExecutionEntity execution)
        {
            DeleteSignalEventSubscription(execution);
            Context.CommandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, DeleteReasonFields.EVENT_BASED_GATEWAY_CANCEL, false);
        }

        protected internal virtual IExecutionEntity DeleteSignalEventSubscription(IExecutionEntity execution)
        {
            string eventName;
            if (signal is not null)
            {
                eventName = signal.Name;
            }
            else
            {
                eventName = signalEventDefinition.SignalRef;
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
            IList<IEventSubscriptionEntity> eventSubscriptions = execution.EventSubscriptions;
            foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
            {
                if (eventSubscription.EventType == SignalEventSubscriptionEntityFields.EVENT_TYPE && eventSubscription.EventName.Equals(eventName))
                {

                    eventSubscriptionEntityManager.Delete(eventSubscription);
                }
            }
            return execution;
        }
    }

}