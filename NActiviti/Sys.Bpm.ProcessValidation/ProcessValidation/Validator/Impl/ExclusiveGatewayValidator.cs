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
    public class ExclusiveGatewayValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ExclusiveGateway> gateways = process.FindFlowElementsOfType<ExclusiveGateway>();
            foreach (ExclusiveGateway gateway in gateways)
            {
                ValidateExclusiveGateway(process, gateway, errors);
            }
        }

        public virtual void ValidateExclusiveGateway(Process process, ExclusiveGateway exclusiveGateway, IList<ValidationError> errors)
        {
            if (exclusiveGateway.OutgoingFlows.Count == 0)
            {
                AddError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_NO_OUTGOING_SEQ_FLOW, process, exclusiveGateway, ProcessValidatorResource.EXCLUSIVE_GATEWAY_NO_OUTGOING_SEQ_FLOW);
            }
            else if (exclusiveGateway.OutgoingFlows.Count == 1)
            {
                SequenceFlow sequenceFlow = exclusiveGateway.OutgoingFlows[0];
                if (!string.IsNullOrWhiteSpace(sequenceFlow.ConditionExpression))
                {
                    AddError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_CONDITION_NOT_ALLOWED_ON_SINGLE_SEQ_FLOW, process, exclusiveGateway, ProcessValidatorResource.EXCLUSIVE_GATEWAY_CONDITION_NOT_ALLOWED_ON_SINGLE_SEQ_FLOW);
                }
            }
            else
            {
                string defaultSequenceFlow = exclusiveGateway.DefaultFlow;

                IList<SequenceFlow> flowsWithoutCondition = new List<SequenceFlow>();
                foreach (SequenceFlow flow in exclusiveGateway.OutgoingFlows)
                {
                    string condition = flow.ConditionExpression;
                    bool isDefaultFlow = !(flow.Id is null) && flow.Id.Equals(defaultSequenceFlow);
                    bool hasConditon = !string.IsNullOrWhiteSpace(condition);

                    if (!hasConditon && !isDefaultFlow)
                    {
                        flowsWithoutCondition.Add(flow);
                    }
                    if (hasConditon && isDefaultFlow)
                    {
                        AddError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_CONDITION_ON_DEFAULT_SEQ_FLOW, process, exclusiveGateway, ProcessValidatorResource.EXCLUSIVE_GATEWAY_CONDITION_ON_DEFAULT_SEQ_FLOW);
                    }
                }

                if (flowsWithoutCondition.Count > 0)
                {
                    AddWarning(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_SEQ_FLOW_WITHOUT_CONDITIONS, process, exclusiveGateway, ProcessValidatorResource.EXCLUSIVE_GATEWAY_SEQ_FLOW_WITHOUT_CONDITIONS);
                }
            }
        }
    }
}