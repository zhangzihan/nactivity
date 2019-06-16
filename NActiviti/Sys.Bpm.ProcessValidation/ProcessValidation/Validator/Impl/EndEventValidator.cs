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
    public class EndEventValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<EndEvent> endEvents = process.FindFlowElementsOfType<EndEvent>();
            foreach (EndEvent endEvent in endEvents)
            {
                if (endEvent.EventDefinitions != null && endEvent.EventDefinitions.Count > 0)
                {

                    EventDefinition eventDefinition = endEvent.EventDefinitions[0];

                    // Error end event
                    if (eventDefinition is CancelEventDefinition)
                    {

                        IFlowElementsContainer parent = process.FindParent(endEvent);
                        if (!(parent is Transaction))
                        {
                            AddError(errors, ProblemsConstants.END_EVENT_CANCEL_ONLY_INSIDE_TRANSACTION, process, endEvent, ProcessValidatorResource.END_EVENT_CANCEL_ONLY_INSIDE_TRANSACTION);
                        }
                    }
                }
            }
        }
    }
}