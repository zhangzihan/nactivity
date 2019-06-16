using System;

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
namespace Sys.Workflow.engine.impl.bpmn.parser.handler
{
    using Sys.Workflow.bpmn.model;

    /// 
    public class SubProcessParseHandler : AbstractActivityBpmnParseHandler<SubProcess>
    {

        protected internal override Type HandledType
        {
            get
            {
                return typeof(SubProcess);
            }
        }

        protected internal override void ExecuteParse(BpmnParse bpmnParse, SubProcess subProcess)
        {

            subProcess.Behavior = bpmnParse.ActivityBehaviorFactory.CreateSubprocessActivityBehavior(subProcess);

            bpmnParse.ProcessFlowElements(subProcess.FlowElements);
            ProcessArtifacts(bpmnParse, subProcess.Artifacts);

            // no data objects for event subprocesses
            /*
             * if (!(subProcess instanceof EventSubProcess)) { // parse out any data objects from the template in order to set up the necessary process variables Map<String, Object> variables =
             * processDataObjects(bpmnParse, subProcess.getDataObjects(), activity); activity.setVariables(variables); }
             * 
             * bpmnParse.removeCurrentScope(); bpmnParse.removeCurrentSubProcess();
             * 
             * if (subProcess.getIoSpecification() != null) { IOSpecification ioSpecification = createIOSpecification(bpmnParse, subProcess.getIoSpecification()); activity.setIoSpecification(ioSpecification);
             * }
             */

        }

    }

}