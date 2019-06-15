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
                if (source == null)
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_SRC);
                }
                if (target == null)
                {
                    AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                }

                if (source != null && target != null)
                {
                    IFlowElementsContainer sourceContainer = process.GetFlowElementsContainer(source.Id);
                    IFlowElementsContainer targetContainer = process.GetFlowElementsContainer(target.Id);

                    if (sourceContainer == null)
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_SRC);
                    }
                    if (targetContainer == null)
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                    }
                    if (sourceContainer != null && targetContainer != null && !sourceContainer.Equals(targetContainer))
                    {
                        AddError(errors, ProblemsConstants.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, ProcessValidatorResource.SEQ_FLOW_INVALID_TARGET);
                    }
                }
            }
        }
    }
}