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

namespace Sys.Workflow.Engine.Impl.Events
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;


    /// 
    public abstract class AbstractEventHandler : IEventHandler
    {
        public abstract string EventHandlerType { get; }

        public virtual void HandleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {
            IExecutionEntity execution = eventSubscription.Execution;
            FlowNode currentFlowElement = (FlowNode)execution.CurrentFlowElement;

            if (currentFlowElement == null)
            {
                throw new ActivitiException("Error while sending signal for event subscription '" + eventSubscription.Id + "': " + "no activity associated with event subscription");
            }

            if (payload is System.Collections.IDictionary)
            {
                IDictionary<string, object> processVariables = (IDictionary<string, object>)payload;
                execution.Variables = processVariables;
            }

            if (currentFlowElement is BoundaryEvent || currentFlowElement is EventSubProcess)
            {
                try
                {
                    DispatchActivitiesCanceledIfNeeded(eventSubscription, execution, currentFlowElement, commandContext);

                }
                catch (Exception e)
                {
                    throw new ActivitiException("exception while sending signal for event subscription '" + eventSubscription + "':" + e.Message, e);
                }
            }

            Context.Agenda.PlanTriggerExecutionOperation(execution);
        }

        protected internal virtual void DispatchActivitiesCanceledIfNeeded(IEventSubscriptionEntity eventSubscription, IExecutionEntity execution, FlowElement currentFlowElement, ICommandContext commandContext)
        {
            if (currentFlowElement is BoundaryEvent boundaryEvent)
            {
                if (boundaryEvent.CancelActivity)
                {
                    DispatchExecutionCancelled(eventSubscription, execution, commandContext);
                }
            }
        }

        protected internal virtual void DispatchExecutionCancelled(IEventSubscriptionEntity eventSubscription, IExecutionEntity execution, ICommandContext commandContext)
        {
            // subprocesses
            foreach (IExecutionEntity subExecution in execution.Executions)
            {
                DispatchExecutionCancelled(eventSubscription, subExecution, commandContext);
            }

            // call activities
            IExecutionEntity subProcessInstance = commandContext.ExecutionEntityManager.FindSubProcessInstanceBySuperExecutionId(execution.Id);
            if (subProcessInstance != null)
            {
                DispatchExecutionCancelled(eventSubscription, subProcessInstance, commandContext);
            }

            // activity with message/signal boundary events
            FlowElement flowElement = execution.CurrentFlowElement;
            if (flowElement is BoundaryEvent boundaryEvent)
            {
                if (boundaryEvent.AttachedToRef != null)
                {
                    DispatchActivityCancelled(eventSubscription, execution, boundaryEvent.AttachedToRef, commandContext);
                }
            }
        }

        protected internal virtual void DispatchActivityCancelled(IEventSubscriptionEntity eventSubscription, IExecutionEntity boundaryEventExecution, FlowNode flowNode, ICommandContext commandContext)
        {
            // Scope
            commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(flowNode.Id, flowNode.Name, boundaryEventExecution.Id, boundaryEventExecution.ProcessInstanceId, boundaryEventExecution.ProcessDefinitionId, ParseActivityType(flowNode), eventSubscription));

            if (flowNode is SubProcess)
            {
                // The parent of the boundary event execution will be the one on which the boundary event is set
                IExecutionEntity parentExecutionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(boundaryEventExecution.ParentId);
                if (parentExecutionEntity != null)
                {
                    DispatchActivityCancelledForChildExecution(eventSubscription, parentExecutionEntity, boundaryEventExecution, commandContext);
                }
            }
        }

        protected internal virtual void DispatchActivityCancelledForChildExecution(IEventSubscriptionEntity eventSubscription, IExecutionEntity parentExecutionEntity, IExecutionEntity boundaryEventExecution, ICommandContext commandContext)
        {
            IList<IExecutionEntity> executionEntities = commandContext.ExecutionEntityManager.FindChildExecutionsByParentExecutionId(parentExecutionEntity.Id);
            foreach (IExecutionEntity childExecution in executionEntities)
            {

                if (!boundaryEventExecution.Id.Equals(childExecution.Id) && childExecution.CurrentFlowElement != null && childExecution.CurrentFlowElement is FlowNode)
                {

                    FlowNode flowNode = (FlowNode)childExecution.CurrentFlowElement;
                    commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(flowNode.Id, flowNode.Name, childExecution.Id, childExecution.ProcessInstanceId, childExecution.ProcessDefinitionId, ParseActivityType(flowNode), eventSubscription));

                    if (childExecution.IsScope)
                    {
                        DispatchActivityCancelledForChildExecution(eventSubscription, childExecution, boundaryEventExecution, commandContext);
                    }
                }
            }
        }

        protected internal virtual string ParseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }
    }
}