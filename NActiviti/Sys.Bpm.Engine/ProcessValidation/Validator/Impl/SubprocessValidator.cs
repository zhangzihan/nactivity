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
    public class SubprocessValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SubProcess> subProcesses = process.findFlowElementsOfType<SubProcess>();
            foreach (SubProcess subProcess in subProcesses)
            {

                if (!(subProcess is EventSubProcess))
                {

                    // Verify start events
                    IList<StartEvent> startEvents = process.findFlowElementsInSubProcessOfType<StartEvent>(subProcess, false);
                    if (startEvents.Count > 1)
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.SUBPROCESS_MULTIPLE_START_EVENTS, process, subProcess, "Multiple start events not supported for subprocess");
                    }

                    foreach (StartEvent startEvent in startEvents)
                    {
                        if (startEvent.EventDefinitions.Count > 0)
                        {
                            addError(errors, org.activiti.validation.validator.Problems_Fields.SUBPROCESS_START_EVENT_EVENT_DEFINITION_NOT_ALLOWED, process, startEvent, "event definitions only allowed on start event if subprocess is an event subprocess");
                        }
                    }

                }

            }

        }

    }

}