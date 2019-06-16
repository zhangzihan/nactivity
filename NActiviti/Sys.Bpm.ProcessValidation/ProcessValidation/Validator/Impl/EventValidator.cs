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
namespace Sys.Workflow.Validation.Validators.Impl
{

    using Sys.Workflow.Bpmn.Models;

    /// <summary>
    /// Validates rules that apply to all events (start event, boundary event, etc.)
    /// 
    /// 
    /// </summary>
    public class EventValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<Event> events = process.FindFlowElementsOfType<Event>();
            foreach (Event @event in events)
            {
                if (@event.EventDefinitions != null)
                {
                    foreach (EventDefinition eventDefinition in @event.EventDefinitions)
                    {

                        if (eventDefinition is MessageEventDefinition)
                        {
                            HandleMessageEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is SignalEventDefinition)
                        {
                            HandleSignalEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is TimerEventDefinition)
                        {
                            HandleTimerEventDefinition(process, @event, eventDefinition, errors);
                        }
                        else if (eventDefinition is CompensateEventDefinition)
                        {
                            HandleCompensationEventDefinition(bpmnModel, process, @event, eventDefinition, errors);
                        }

                    }
                }
            }
        }

        protected internal virtual void HandleMessageEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;

            if (string.IsNullOrWhiteSpace(messageEventDefinition.MessageRef))
            {

                if (string.IsNullOrWhiteSpace(messageEventDefinition.MessageExpression))
                {
                    // message ref should be filled in
                    AddError(errors, ProblemsConstants.MESSAGE_EVENT_MISSING_MESSAGE_REF, process, @event, ProcessValidatorResource.MESSAGE_EVENT_MISSING_MESSAGE_REF);
                }

            }
            else if (!bpmnModel.ContainsMessageId(messageEventDefinition.MessageRef))
            {
                // message ref should exist
                AddError(errors, ProblemsConstants.MESSAGE_EVENT_INVALID_MESSAGE_REF, process, @event, ProcessValidatorResource.MESSAGE_EVENT_INVALID_MESSAGE_REF);
            }
        }

        protected internal virtual void HandleSignalEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;

            if (string.IsNullOrWhiteSpace(signalEventDefinition.SignalRef))
            {

                if (string.IsNullOrWhiteSpace(signalEventDefinition.SignalExpression))
                {
                    AddError(errors, ProblemsConstants.SIGNAL_EVENT_MISSING_SIGNAL_REF, process, @event, ProcessValidatorResource.SIGNAL_EVENT_MISSING_SIGNAL_REF);
                }

            }
            else if (!bpmnModel.ContainsSignalId(signalEventDefinition.SignalRef))
            {
                AddError(errors, ProblemsConstants.SIGNAL_EVENT_INVALID_SIGNAL_REF, process, @event, ProcessValidatorResource.SIGNAL_EVENT_INVALID_SIGNAL_REF);
            }
        }

        protected internal virtual void HandleTimerEventDefinition(Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            TimerEventDefinition timerEventDefinition = (TimerEventDefinition)eventDefinition;
            if (string.IsNullOrWhiteSpace(timerEventDefinition.TimeDate) && string.IsNullOrWhiteSpace(timerEventDefinition.TimeCycle) && string.IsNullOrWhiteSpace(timerEventDefinition.TimeDuration))
            {
                // neither date, cycle or duration configured
                AddError(errors, ProblemsConstants.EVENT_TIMER_MISSING_CONFIGURATION, process, @event, ProcessValidatorResource.EVENT_TIMER_MISSING_CONFIGURATION);
            }
        }

        protected internal virtual void HandleCompensationEventDefinition(BpmnModel bpmnModel, Process process, Event @event, EventDefinition eventDefinition, IList<ValidationError> errors)
        {
            CompensateEventDefinition compensateEventDefinition = (CompensateEventDefinition)eventDefinition;

            // Check activityRef
            if ((!string.IsNullOrWhiteSpace(compensateEventDefinition.ActivityRef) && process.GetFlowElement(compensateEventDefinition.ActivityRef, true) == null))
            {
                AddError(errors, ProblemsConstants.COMPENSATE_EVENT_INVALID_ACTIVITY_REF, process, @event, ProcessValidatorResource.COMPENSATE_EVENT_INVALID_ACTIVITY_REF);
            }
        }
    }
}