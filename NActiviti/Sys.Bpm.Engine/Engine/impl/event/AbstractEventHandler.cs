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

namespace org.activiti.engine.impl.@event
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;


    /// 
    public abstract class AbstractEventHandler : IEventHandler
    {
        public abstract string EventHandlerType { get; }

        public virtual void handleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
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
                    dispatchActivitiesCanceledIfNeeded(eventSubscription, execution, currentFlowElement, commandContext);

                }
                catch (Exception e)
                {
                    throw new ActivitiException("exception while sending signal for event subscription '" + eventSubscription + "':" + e.Message, e);
                }
            }

            Context.Agenda.planTriggerExecutionOperation(execution);
        }

        protected internal virtual void dispatchActivitiesCanceledIfNeeded(IEventSubscriptionEntity eventSubscription, IExecutionEntity execution, FlowElement currentFlowElement, ICommandContext commandContext)
        {
            if (currentFlowElement is BoundaryEvent)
            {
                BoundaryEvent boundaryEvent = (BoundaryEvent)currentFlowElement;
                if (boundaryEvent.CancelActivity)
                {
                    dispatchExecutionCancelled(eventSubscription, execution, commandContext);
                }
            }
        }

        protected internal virtual void dispatchExecutionCancelled(IEventSubscriptionEntity eventSubscription, IExecutionEntity execution, ICommandContext commandContext)
        {
            // subprocesses
            foreach (IExecutionEntity subExecution in execution.Executions)
            {
                dispatchExecutionCancelled(eventSubscription, subExecution, commandContext);
            }

            // call activities
            IExecutionEntity subProcessInstance = commandContext.ExecutionEntityManager.findSubProcessInstanceBySuperExecutionId(execution.Id);
            if (subProcessInstance != null)
            {
                dispatchExecutionCancelled(eventSubscription, subProcessInstance, commandContext);
            }

            // activity with message/signal boundary events
            FlowElement flowElement = execution.CurrentFlowElement;
            if (flowElement is BoundaryEvent)
            {
                BoundaryEvent boundaryEvent = (BoundaryEvent)flowElement;
                if (boundaryEvent.AttachedToRef != null)
                {
                    dispatchActivityCancelled(eventSubscription, execution, boundaryEvent.AttachedToRef, commandContext);
                }
            }
        }

        protected internal virtual void dispatchActivityCancelled(IEventSubscriptionEntity eventSubscription, IExecutionEntity boundaryEventExecution, FlowNode flowNode, ICommandContext commandContext)
        {

            // Scope
            commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityCancelledEvent(flowNode.Id, flowNode.Name, boundaryEventExecution.Id, boundaryEventExecution.ProcessInstanceId, boundaryEventExecution.ProcessDefinitionId, parseActivityType(flowNode), eventSubscription));

            if (flowNode is SubProcess)
            {
                // The parent of the boundary event execution will be the one on which the boundary event is set
                IExecutionEntity parentExecutionEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", boundaryEventExecution.ParentId));
                if (parentExecutionEntity != null)
                {
                    dispatchActivityCancelledForChildExecution(eventSubscription, parentExecutionEntity, boundaryEventExecution, commandContext);
                }
            }
        }

        protected internal virtual void dispatchActivityCancelledForChildExecution(IEventSubscriptionEntity eventSubscription, IExecutionEntity parentExecutionEntity, IExecutionEntity boundaryEventExecution, ICommandContext commandContext)
        {

            IList<IExecutionEntity> executionEntities = commandContext.ExecutionEntityManager.findChildExecutionsByParentExecutionId(parentExecutionEntity.Id);
            foreach (IExecutionEntity childExecution in executionEntities)
            {

                if (!boundaryEventExecution.Id.Equals(childExecution.Id) && childExecution.CurrentFlowElement != null && childExecution.CurrentFlowElement is FlowNode)
                {

                    FlowNode flowNode = (FlowNode)childExecution.CurrentFlowElement;
                    commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityCancelledEvent(flowNode.Id, flowNode.Name, childExecution.Id, childExecution.ProcessInstanceId, childExecution.ProcessDefinitionId, parseActivityType(flowNode), eventSubscription));

                    if (childExecution.IsScope)
                    {
                        dispatchActivityCancelledForChildExecution(eventSubscription, childExecution, boundaryEventExecution, commandContext);
                    }

                }

            }

        }

        protected internal virtual string parseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

    }

}