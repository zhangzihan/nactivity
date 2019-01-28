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
    public class DiagramInterchangeInfoValidator : ValidatorImpl
    {
        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (bpmnModel.LocationMap.Count > 0)
            {

                // Location map
                foreach (string bpmnReference in bpmnModel.LocationMap.Keys)
                {
                    if (bpmnModel.getFlowElement(bpmnReference) == null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from
                        // DI
                        if (bpmnModel.getArtifact(bpmnReference) == null)
                        {
                            // check if it's a Pool or Lane, then DI is ok
                            if (bpmnModel.getPool(bpmnReference) == null && bpmnModel.getLane(bpmnReference) == null)
                            {
                                addWarning(errors, Problems_Fields.DI_INVALID_REFERENCE, null, bpmnModel.getFlowElement(bpmnReference), "Invalid reference in diagram interchange definition: could not find " + bpmnReference);
                            }
                        }
                    }
                    else if (!(bpmnModel.getFlowElement(bpmnReference) is FlowNode))
                    {
                        addWarning(errors, Problems_Fields.DI_DOES_NOT_REFERENCE_FLOWNODE, null, bpmnModel.getFlowElement(bpmnReference), "Invalid reference in diagram interchange definition: " + bpmnReference + " does not reference a flow node");
                    }
                }

            }

            if (bpmnModel.FlowLocationMap.Count > 0)
            {
                // flowlocation map
                foreach (string bpmnReference in bpmnModel.FlowLocationMap.Keys)
                {
                    if (bpmnModel.getFlowElement(bpmnReference) == null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from
                        // DI
                        if (bpmnModel.getArtifact(bpmnReference) == null)
                        {
                            addWarning(errors, Problems_Fields.DI_INVALID_REFERENCE, null, bpmnModel.getFlowElement(bpmnReference), "Invalid reference in diagram interchange definition: could not find " + bpmnReference);
                        }
                    }
                    else if (!(bpmnModel.getFlowElement(bpmnReference) is SequenceFlow))
                    {
                        addWarning(errors, Problems_Fields.DI_DOES_NOT_REFERENCE_SEQ_FLOW, null, bpmnModel.getFlowElement(bpmnReference), "Invalid reference in diagram interchange definition: " + bpmnReference + " does not reference a sequence flow");
                    }
                }
            }
        }
    }

}