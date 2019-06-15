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
    public class StartEventValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<StartEvent> startEvents = process.FindFlowElementsOfType<StartEvent>(false);
            ValidateEventDefinitionTypes(startEvents, process, errors);
            ValidateMultipleStartEvents(startEvents, process, errors);
        }

        protected internal virtual void ValidateEventDefinitionTypes(IList<StartEvent> startEvents, Process process, IList<ValidationError> errors)
        {
            foreach (StartEvent startEvent in startEvents)
            {
                if (startEvent.EventDefinitions != null && startEvent.EventDefinitions.Count > 0)
                {
                    EventDefinition eventDefinition = startEvent.EventDefinitions[0];
                    if (!(eventDefinition is MessageEventDefinition) && !(eventDefinition is TimerEventDefinition) && !(eventDefinition is SignalEventDefinition))
                    {
                        AddError(errors, ProblemsConstants.START_EVENT_INVALID_EVENT_DEFINITION, process, startEvent, ProcessValidatorResource.START_EVENT_INVALID_EVENT_DEFINITION);
                    }
                }
            }
        }

        protected internal virtual void ValidateMultipleStartEvents(IList<StartEvent> startEvents, Process process, IList<ValidationError> errors)
        {

            // Multiple none events are not supported
            IList<StartEvent> noneStartEvents = new List<StartEvent>();
            foreach (StartEvent startEvent in startEvents)
            {
                if (startEvent.EventDefinitions == null || startEvent.EventDefinitions.Count == 0)
                {
                    noneStartEvents.Add(startEvent);
                }
            }

            if (noneStartEvents.Count > 1)
            {
                foreach (StartEvent startEvent in noneStartEvents)
                {
                    AddError(errors, ProblemsConstants.START_EVENT_MULTIPLE_FOUND, process, startEvent, ProcessValidatorResource.START_EVENT_MULTIPLE_FOUND);
                }
            }
        }
    }
}