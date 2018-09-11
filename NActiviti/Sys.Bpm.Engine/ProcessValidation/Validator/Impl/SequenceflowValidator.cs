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

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SequenceFlow> sequenceFlows = process.findFlowElementsOfType<SequenceFlow>();
            foreach (SequenceFlow sequenceFlow in sequenceFlows)
            {

                string sourceRef = sequenceFlow.SourceRef;
                string targetRef = sequenceFlow.TargetRef;

                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, "Invalid source for sequenceflow");
                }
                if (string.IsNullOrWhiteSpace(targetRef))
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, "Invalid target for sequenceflow");
                }

                // Implicit check: sequence flow cannot cross (sub) process
                // boundaries, hence we check the parent and not the process
                // (could be subprocess for example)
                FlowElement source = process.getFlowElement(sourceRef, true);
                FlowElement target = process.getFlowElement(targetRef, true);

                // Src and target validation
                if (source == null)
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, "Invalid source for sequenceflow");
                }
                if (target == null)
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, "Invalid target for sequenceflow");
                }

                if (source != null && target != null)
                {
                    IFlowElementsContainer sourceContainer = process.getFlowElementsContainer(source.Id);
                    IFlowElementsContainer targetContainer = process.getFlowElementsContainer(target.Id);

                    if (sourceContainer == null)
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_SRC, process, sequenceFlow, "Invalid source for sequenceflow");
                    }
                    if (targetContainer == null)
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, "Invalid target for sequenceflow");
                    }
                    if (sourceContainer != null && targetContainer != null && !sourceContainer.Equals(targetContainer))
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.SEQ_FLOW_INVALID_TARGET, process, sequenceFlow, "Invalid target for sequenceflow, the target isn't defined in the same scope as the source");
                    }
                }
            }
        }

    }

}