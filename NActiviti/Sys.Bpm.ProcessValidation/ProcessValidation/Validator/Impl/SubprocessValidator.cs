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
    public class SubprocessValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SubProcess> subProcesses = process.FindFlowElementsOfType<SubProcess>();
            foreach (SubProcess subProcess in subProcesses)
            {

                if (!(subProcess is EventSubProcess))
                {

                    // Verify start events
                    IList<StartEvent> startEvents = process.FindFlowElementsInSubProcessOfType<StartEvent>(subProcess, false);
                    if (startEvents.Count > 1)
                    {
                        AddError(errors, ProblemsConstants.SUBPROCESS_MULTIPLE_START_EVENTS, process, subProcess, ProcessValidatorResource.SUBPROCESS_MULTIPLE_START_EVENTS);
                    }

                    foreach (StartEvent startEvent in startEvents)
                    {
                        if (startEvent.EventDefinitions.Count > 0)
                        {
                            AddError(errors, ProblemsConstants.SUBPROCESS_START_EVENT_EVENT_DEFINITION_NOT_ALLOWED, process, startEvent, ProcessValidatorResource.SUBPROCESS_START_EVENT_EVENT_DEFINITION_NOT_ALLOWED);
                        }
                    }
                }
            }
        }
    }
}