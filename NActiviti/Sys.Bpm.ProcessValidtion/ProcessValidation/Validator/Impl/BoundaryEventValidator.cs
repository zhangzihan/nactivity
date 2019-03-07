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
namespace org.activiti.validation.validator.impl
{

    using org.activiti.bpmn.model;

    /// 
    public class BoundaryEventValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<BoundaryEvent> boundaryEvents = process.findFlowElementsOfType<BoundaryEvent>();

            // Only one boundary event of type 'cancel' can be attached to the same
            // element, so we store the count temporarily here
            Dictionary<string, int> cancelBoundaryEventsCounts = new Dictionary<string, int>();

            // Only one boundary event of type 'compensate' can be attached to the
            // same element, so we store the count temporarily here
            Dictionary<string, int> compensateBoundaryEventsCounts = new Dictionary<string, int>();

            for (int i = 0; i < boundaryEvents.Count; i++)
            {

                BoundaryEvent boundaryEvent = boundaryEvents[i];

                if (boundaryEvent.EventDefinitions != null && boundaryEvent.EventDefinitions.Count > 0)
                {

                    EventDefinition eventDefinition = boundaryEvent.EventDefinitions[0];
                    if (!(eventDefinition is TimerEventDefinition) && !(eventDefinition is ErrorEventDefinition) && !(eventDefinition is SignalEventDefinition) && !(eventDefinition is CancelEventDefinition) && !(eventDefinition is MessageEventDefinition) && !(eventDefinition is CompensateEventDefinition))
                    {

                        addError(errors, ProblemsConstants.BOUNDARY_EVENT_INVALID_EVENT_DEFINITION, process, boundaryEvent, "Invalid or unsupported event definition");

                    }

                    if (eventDefinition is CancelEventDefinition)
                    {

                        FlowElement attachedToFlowElement = bpmnModel.getFlowElement(boundaryEvent.AttachedToRefId);
                        if (!(attachedToFlowElement is Transaction))
                        {
                            addError(errors, ProblemsConstants.BOUNDARY_EVENT_CANCEL_ONLY_ON_TRANSACTION, process, boundaryEvent, "boundary event with cancelEventDefinition only supported on transaction subprocesses");
                        }
                        else
                        {
                            if (!cancelBoundaryEventsCounts.ContainsKey(attachedToFlowElement.Id))
                            {
                                cancelBoundaryEventsCounts[attachedToFlowElement.Id] = 0;
                            }
                            cancelBoundaryEventsCounts[attachedToFlowElement.Id] = cancelBoundaryEventsCounts[attachedToFlowElement.Id] + 1;
                        }

                    }
                    else if (eventDefinition is CompensateEventDefinition)
                    {

                        if (!compensateBoundaryEventsCounts.ContainsKey(boundaryEvent.AttachedToRefId))
                        {
                            compensateBoundaryEventsCounts[boundaryEvent.AttachedToRefId] = 0;
                        }
                        compensateBoundaryEventsCounts[boundaryEvent.AttachedToRefId] = compensateBoundaryEventsCounts[boundaryEvent.AttachedToRefId] + 1;

                    }
                    else if (eventDefinition is MessageEventDefinition)
                    {

                        // Check if other message boundary events with same message
                        // id
                        for (int j = 0; j < boundaryEvents.Count; j++)
                        {
                            if (j != i)
                            {
                                BoundaryEvent otherBoundaryEvent = boundaryEvents[j];
                                if (!ReferenceEquals(otherBoundaryEvent.AttachedToRefId, null) && otherBoundaryEvent.AttachedToRefId.Equals(boundaryEvent.AttachedToRefId))
                                {
                                    if (otherBoundaryEvent.EventDefinitions != null && otherBoundaryEvent.EventDefinitions.Count > 0)
                                    {
                                        EventDefinition otherEventDefinition = otherBoundaryEvent.EventDefinitions[0];
                                        if (otherEventDefinition is MessageEventDefinition)
                                        {
                                            MessageEventDefinition currentMessageEventDefinition = (MessageEventDefinition)eventDefinition;
                                            MessageEventDefinition otherMessageEventDefinition = (MessageEventDefinition)otherEventDefinition;
                                            if (!ReferenceEquals(otherMessageEventDefinition.MessageRef, null) && otherMessageEventDefinition.MessageRef.Equals(currentMessageEventDefinition.MessageRef))
                                            {
                                                addError(errors, ProblemsConstants.MESSAGE_EVENT_MULTIPLE_ON_BOUNDARY_SAME_MESSAGE_ID, process, boundaryEvent, "Multiple message events with same message id not supported");
                                            }
                                        }
                                    }
                                }
                            }

                        }

                    }

                }
                else
                {

                    addError(errors, ProblemsConstants.BOUNDARY_EVENT_NO_EVENT_DEFINITION, process, boundaryEvent, "Event definition is missing from boundary event");

                }
            }

            foreach (string elementId in cancelBoundaryEventsCounts.Keys)
            {
                if (cancelBoundaryEventsCounts[elementId] > 1)
                {
                    addError(errors, ProblemsConstants.BOUNDARY_EVENT_MULTIPLE_CANCEL_ON_TRANSACTION, process, bpmnModel.getFlowElement(elementId), "multiple boundary events with cancelEventDefinition not supported on same transaction subprocess.");
                }
            }

            foreach (string elementId in compensateBoundaryEventsCounts.Keys)
            {
                if (compensateBoundaryEventsCounts[elementId] > 1)
                {
                    addError(errors, ProblemsConstants.COMPENSATE_EVENT_MULTIPLE_ON_BOUNDARY, process, bpmnModel.getFlowElement(elementId), "Multiple boundary events of type 'compensate' is invalid");
                }
            }

        }
    }

}