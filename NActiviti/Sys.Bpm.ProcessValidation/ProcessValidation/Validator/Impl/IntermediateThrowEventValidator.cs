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
namespace Sys.Workflow.validation.validator.impl
{

    using Sys.Workflow.bpmn.model;

    /// 
    public class IntermediateThrowEventValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ThrowEvent> throwEvents = process.FindFlowElementsOfType<ThrowEvent>();
            foreach (ThrowEvent throwEvent in throwEvents)
            {
                EventDefinition eventDefinition = null;
                if (throwEvent.EventDefinitions.Count > 0)
                {
                    eventDefinition = throwEvent.EventDefinitions[0];
                }

                if (eventDefinition != null && !(eventDefinition is SignalEventDefinition) && !(eventDefinition is CompensateEventDefinition))
                {
                    AddError(errors, ProblemsConstants.THROW_EVENT_INVALID_EVENTDEFINITION, process, throwEvent, ProcessValidatorResource.THROW_EVENT_INVALID_EVENTDEFINITION);
                }
            }
        }
    }
}