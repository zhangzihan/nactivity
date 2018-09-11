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
namespace org.activiti.engine.impl.bpmn.behavior
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.history;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

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

        public override void execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntity executionEntity = (IExecutionEntity)execution;

            string signalName = null;
            if (!string.IsNullOrWhiteSpace(signalEventDefinition.SignalRef))
            {
                signalName = signalEventDefinition.SignalRef;
            }
            else
            {
                IExpression signalExpression = commandContext.ProcessEngineConfiguration.ExpressionManager.createExpression(signalEventDefinition.SignalExpression);
                signalName = signalExpression.getValue(execution).ToString();
            }

            commandContext.EventSubscriptionEntityManager.insertSignalEvent(signalName, signal, executionEntity);
        }

        public override void trigger(IExecutionEntity execution, string triggerName, object triggerData)
        {
            IExecutionEntity executionEntity = deleteSignalEventSubscription(execution);
            leaveIntermediateCatchEvent(executionEntity);
        }

        public override void eventCancelledByEventGateway(IExecutionEntity execution)
        {
            deleteSignalEventSubscription(execution);
            Context.CommandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(execution, DeleteReason_Fields.EVENT_BASED_GATEWAY_CANCEL, false);
        }

        protected internal virtual IExecutionEntity deleteSignalEventSubscription(IExecutionEntity execution)
        {
            IExecutionEntity executionEntity = (IExecutionEntity)execution;

            string eventName = null;
            if (signal != null)
            {
                eventName = signal.Name;
            }
            else
            {
                eventName = signalEventDefinition.SignalRef;
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
            IList<IEventSubscriptionEntity> eventSubscriptions = executionEntity.EventSubscriptions;
            foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
            {
                if (eventSubscription is ISignalEventSubscriptionEntity && eventSubscription.EventName.Equals(eventName))
                {

                    eventSubscriptionEntityManager.delete(eventSubscription);
                }
            }
            return executionEntity;
        }
    }

}