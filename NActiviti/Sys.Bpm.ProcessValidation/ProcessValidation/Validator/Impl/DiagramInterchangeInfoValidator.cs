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

    /// 
    public class DiagramInterchangeInfoValidator : ValidatorImpl
    {
        public override void Validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (bpmnModel.LocationMap.Count > 0)
            {

                // Location map
                foreach (string bpmnReference in bpmnModel.LocationMap.Keys)
                {
                    if (bpmnModel.GetFlowElement(bpmnReference) is null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from
                        // DI
                        if (bpmnModel.GetArtifact(bpmnReference) is null)
                        {
                            // check if it's a Pool or Lane, then DI is ok
                            if (bpmnModel.GetPool(bpmnReference) is null && bpmnModel.GetLane(bpmnReference) is null)
                            {
                                AddWarning(errors, ProblemsConstants.DI_INVALID_REFERENCE, null, bpmnModel.GetFlowElement(bpmnReference), string.Format(ProcessValidatorResource.DI_INVALID_REFERENCE, bpmnReference));
                            }
                        }
                    }
                    else if (!(bpmnModel.GetFlowElement(bpmnReference) is FlowNode))
                    {
                        AddWarning(errors, ProblemsConstants.DI_DOES_NOT_REFERENCE_FLOWNODE, null, bpmnModel.GetFlowElement(bpmnReference), string.Format(ProcessValidatorResource.DI_DOES_NOT_REFERENCE_FLOWNODE, bpmnReference));
                    }
                }

            }

            if (bpmnModel.FlowLocationMap.Count > 0)
            {
                // flowlocation map
                foreach (string bpmnReference in bpmnModel.FlowLocationMap.Keys)
                {
                    if (bpmnModel.GetFlowElement(bpmnReference) is null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from
                        // DI
                        if (bpmnModel.GetArtifact(bpmnReference) is null)
                        {
                            AddWarning(errors, ProblemsConstants.DI_INVALID_REFERENCE, null, bpmnModel.GetFlowElement(bpmnReference), string.Format(ProcessValidatorResource.DI_INVALID_REFERENCE, bpmnReference));
                        }
                    }
                    else if (!(bpmnModel.GetFlowElement(bpmnReference) is SequenceFlow))
                    {
                        AddWarning(errors, ProblemsConstants.DI_DOES_NOT_REFERENCE_SEQ_FLOW, null, bpmnModel.GetFlowElement(bpmnReference), string.Format(ProcessValidatorResource.DI_DOES_NOT_REFERENCE_SEQ_FLOW, bpmnReference));
                    }
                }
            }
        }
    }

}