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
    public class IntermediateThrowSignalEventActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal readonly SignalEventDefinition signalEventDefinition;
        protected internal string signalEventName;
        protected internal string signalExpression;
        protected internal bool processInstanceScope;

        public IntermediateThrowSignalEventActivityBehavior(SignalEventDefinition signalEventDefinition, Signal signal)
        {
            if (signal is object)
            {
                signalEventName = signal.Name;
                if (Signal.SCOPE_PROCESS_INSTANCE.Equals(signal.Scope))
                {
                    this.processInstanceScope = true;
                }
            }
            else if (!string.IsNullOrWhiteSpace(signalEventDefinition.SignalRef))
            {
                signalEventName = signalEventDefinition.SignalRef;
            }
            else
            {
                signalExpression = signalEventDefinition.SignalExpression;
            }

            this.signalEventDefinition = signalEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;

            string eventSubscriptionName;
            if (signalEventName is object)
            {
                eventSubscriptionName = signalEventName;
            }
            else
            {
                IExpression expressionObject = commandContext.ProcessEngineConfiguration.ExpressionManager.CreateExpression(signalExpression);
                eventSubscriptionName = expressionObject.GetValue(execution).ToString();
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
            IList<ISignalEventSubscriptionEntity> subscriptionEntities;
            if (processInstanceScope)
            {
                subscriptionEntities = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByProcessInstanceAndEventName(execution.ProcessInstanceId, eventSubscriptionName);
            }
            else
            {
                subscriptionEntities = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByEventName(eventSubscriptionName, execution.TenantId);
            }

            foreach (ISignalEventSubscriptionEntity signalEventSubscriptionEntity in subscriptionEntities)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateSignalEvent(ActivitiEventType.ACTIVITY_SIGNALED, signalEventSubscriptionEntity.ActivityId, eventSubscriptionName, null, signalEventSubscriptionEntity.ExecutionId, signalEventSubscriptionEntity.ProcessInstanceId, signalEventSubscriptionEntity.ProcessDefinitionId));

                eventSubscriptionEntityManager.EventReceived(signalEventSubscriptionEntity, null, signalEventDefinition.Async);
            }

            Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(execution, true);
        }
    }

}