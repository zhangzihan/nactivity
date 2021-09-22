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
    public class SequenceflowValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SequenceFlow> sequenceFlows = process.FindFlowElementsOfType<SequenceFlow>();
            foreach (SequenceFlow sequenceFlow in sequenceFlows)
            {

                string sourceRef = sequenceFlow.SourceRef;
                string targetRef = sequenceFlow.TargetRef;

                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_SRC);
                }
                if (string.IsNullOrWhiteSpace(targetRef))
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                }

                // Implicit check: sequence flow cannot cross (sub) process
                // boundaries, hence we check the parent and not the process
                // (could be subprocess for example)
                FlowElement source = process.GetFlowElement(sourceRef, true);
                FlowElement target = process.GetFlowElement(targetRef, true);

                // Src and target validation
                if (source is null)
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_SRC);
                }
                if (target is null)
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                }

                if (source is not null && target is not null)
                {
                    IFlowElementsContainer sourceContainer = process.GetFlowElementsContainer(source.Id);
                    IFlowElementsContainer targetContainer = process.GetFlowElementsContainer(target.Id);

                    if (sourceContainer is null)
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_SRC);
                    }
                    if (targetContainer is null)
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                    }
                    if (sourceContainer is object && targetContainer is object && !sourceContainer.Equals(targetContainer))
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                    }
                }
            }
        }
    }
}