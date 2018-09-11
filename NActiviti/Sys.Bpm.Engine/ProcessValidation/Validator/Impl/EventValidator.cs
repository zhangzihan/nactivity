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

    /// <summary>
    /// Validates rules that apply to all events (start event, boundary event, etc.)
    /// 
    /// 
    /// </summary>
    public class EventValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<Event> events = process.findFlowElementsOfType<Event>();
            foreach (Event @event in events)
            {
                if (@event.EventDefinitions != null)
                {
                    foreach (EventDefinition eventDefinition in @event.EventDefinitions)
                    {

                        if (eventDefinition is MessageEventDefinition)
                        {
                            handleMessageEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is SignalEventDefinition)
                        {
                            handleSignalEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is TimerEventDefinition)
                        {
                            handleTimerEventDefinition(process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is CompensateEventDefinition)
                        {
                            handleCompensationEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }

                    }
                }
            }
        }

        protected internal virtual void handleMessageEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;

            if (string.IsNullOrWhiteSpace(messageEventDefinition.MessageRef))
            {

                if (string.IsNullOrWhiteSpace(messageEventDefinition.MessageExpression))
                {
                    // message ref should be filled in
                    addError(errors, org.activiti.validation.validator.Problems_Fields.MESSAGE_EVENT_MISSING_MESSAGE_REF, process, @event, "attribute 'messageRef' is required");
                }

            }
            else if (!bpmnModel.containsMessageId(messageEventDefinition.MessageRef))
            {
                // message ref should exist
                addError(errors, org.activiti.validation.validator.Problems_Fields.MESSAGE_EVENT_INVALID_MESSAGE_REF, process, @event, "Invalid 'messageRef': no message with that id can be found in the model");
            }
        }

        protected internal virtual void handleSignalEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;

            if (string.IsNullOrWhiteSpace(signalEventDefinition.SignalRef))
            {

                if (string.IsNullOrWhiteSpace(signalEventDefinition.SignalExpression))
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SIGNAL_EVENT_MISSING_SIGNAL_REF, process, @event, "signalEventDefinition does not have mandatory property 'signalRef'");
                }

            }
            else if (!bpmnModel.containsSignalId(signalEventDefinition.SignalRef))
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.SIGNAL_EVENT_INVALID_SIGNAL_REF, process, @event, "Invalid 'signalRef': no signal with that id can be found in the model");
            }
        }

        protected internal virtual void handleTimerEventDefinition(Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            TimerEventDefinition timerEventDefinition = (TimerEventDefinition)eventDefinition;
            if (string.IsNullOrWhiteSpace(timerEventDefinition.TimeDate) && string.IsNullOrWhiteSpace(timerEventDefinition.TimeCycle) && string.IsNullOrWhiteSpace(timerEventDefinition.TimeDuration))
            {
                // neither date, cycle or duration configured
                addError(errors, org.activiti.validation.validator.Problems_Fields.EVENT_TIMER_MISSING_CONFIGURATION, process, @event, "Timer needs configuration (either timeDate, timeCycle or timeDuration is needed)");
            }
        }

        protected internal virtual void handleCompensationEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            CompensateEventDefinition compensateEventDefinition = (CompensateEventDefinition)eventDefinition;

            // Check activityRef
            if ((!string.IsNullOrWhiteSpace(compensateEventDefinition.ActivityRef) && process.getFlowElement(compensateEventDefinition.ActivityRef, true) == null))
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.COMPENSATE_EVENT_INVALID_ACTIVITY_REF, process, @event, "Invalid attribute value for 'activityRef': no activity with the given id");
            }
        }

    }

}