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
    public class IntermediateThrowEventValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ThrowEvent> throwEvents = process.findFlowElementsOfType<ThrowEvent>();
            foreach (ThrowEvent throwEvent in throwEvents)
            {
                EventDefinition eventDefinition = null;
                if (throwEvent.EventDefinitions.Count > 0)
                {
                    eventDefinition = throwEvent.EventDefinitions[0];
                }

                if (eventDefinition != null && !(eventDefinition is SignalEventDefinition) && !(eventDefinition is CompensateEventDefinition))
                {
                    addError(errors, Problems_Fields.THROW_EVENT_INVALID_EVENTDEFINITION, process, throwEvent, "Unsupported intermediate throw event type");
                }
            }
        }

    }

}