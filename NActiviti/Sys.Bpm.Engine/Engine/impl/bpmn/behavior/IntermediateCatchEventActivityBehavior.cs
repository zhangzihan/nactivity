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
    public class IntermediateCatchEventActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public override void Execute(IExecutionEntity execution)
        {
            // Do nothing: waitstate behavior
        }

        public override void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            LeaveIntermediateCatchEvent(execution);
        }

        /// <summary>
        /// Specific leave method for intermediate events: does a normal leave(), except
        /// when behind an event based gateway. In that case, the other events are cancelled 
        /// (we're only supporting the exclusive event based gateway type currently).
        /// and the process instance is continued through the triggered event. 
        /// </summary>
        public virtual void LeaveIntermediateCatchEvent(IExecutionEntity execution)
        {
            EventGateway eventGateway = GetPrecedingEventBasedGateway(execution);
            if (eventGateway != null)
            {
                DeleteOtherEventsRelatedToEventBasedGateway(execution, eventGateway);
            }

            Leave(execution); // Normal leave
        }

        /// <summary>
        /// Should be subclassed by the more specific types.
        /// For an intermediate catch without type, it's simply leaving the event. 
        /// </summary>
        public virtual void EventCancelledByEventGateway(IExecutionEntity execution)
        {
            Context.CommandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, DeleteReasonFields.EVENT_BASED_GATEWAY_CANCEL, false);
        }

        protected internal virtual EventGateway GetPrecedingEventBasedGateway(IExecutionEntity execution)
        {
            FlowElement currentFlowElement = execution.CurrentFlowElement;
            if (currentFlowElement is IntermediateCatchEvent intermediateCatchEvent)
            {
                IList<SequenceFlow> incomingSequenceFlow = intermediateCatchEvent.IncomingFlows;

                // If behind an event based gateway, there is only one incoming sequence flow that originates from said gateway
                if (incomingSequenceFlow != null && incomingSequenceFlow.Count == 1)
                {
                    SequenceFlow sequenceFlow = incomingSequenceFlow[0];
                    FlowElement sourceFlowElement = sequenceFlow.SourceFlowElement;
                    if (sourceFlowElement is EventGateway)
                    {
                        return (EventGateway)sourceFlowElement;
                    }
                }

            }
            return null;
        }

        protected internal virtual void DeleteOtherEventsRelatedToEventBasedGateway(IExecutionEntity execution, EventGateway eventGateway)
        {

            // To clean up the other events behind the event based gateway, we must gather the 
            // activity ids of said events and check the _sibling_ executions of the incoming execution.
            // Note that it can happen that there are multiple such execution in those activity ids,
            // (for example a parallel gw going twice to the event based gateway, kinda silly, but valid)
            // so we only take _one_ result of such a query for deletion.

            // Gather all activity ids for the events after the event based gateway that need to be destroyed
            IList<SequenceFlow> outgoingSequenceFlows = eventGateway.OutgoingFlows;
            ISet<string> eventActivityIds = new HashSet<string>();//outgoingSequenceFlows.Count - 1); // -1, the event being triggered does not need to be deleted
            foreach (SequenceFlow outgoingSequenceFlow in outgoingSequenceFlows)
            {
                if (outgoingSequenceFlow.TargetFlowElement != null && !outgoingSequenceFlow.TargetFlowElement.Id.Equals(execution.CurrentActivityId))
                {
                    eventActivityIds.Add(outgoingSequenceFlow.TargetFlowElement.Id);
                }
            }

            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            // Find the executions
            IList<IExecutionEntity> executionEntities = executionEntityManager.FindExecutionsByParentExecutionAndActivityIds(execution.ParentId, eventActivityIds);

            // Execute the cancel behaviour of the IntermediateCatchEvent
            foreach (IExecutionEntity executionEntity in executionEntities)
            {
                if (eventActivityIds.Contains(executionEntity.ActivityId) && execution.CurrentFlowElement is IntermediateCatchEvent)
                {
                    IntermediateCatchEvent intermediateCatchEvent = (IntermediateCatchEvent)execution.CurrentFlowElement;
                    if (intermediateCatchEvent.Behavior is IntermediateCatchEventActivityBehavior)
                    {
                        ((IntermediateCatchEventActivityBehavior)intermediateCatchEvent.Behavior).EventCancelledByEventGateway(executionEntity);
                        eventActivityIds.Remove(executionEntity.ActivityId); // We only need to delete ONE execution at the event.
                    }
                }
            }
        }

    }

}