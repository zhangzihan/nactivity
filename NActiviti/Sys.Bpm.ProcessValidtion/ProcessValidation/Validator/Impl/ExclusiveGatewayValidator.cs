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

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ExclusiveGateway> gateways = process.findFlowElementsOfType<ExclusiveGateway>();
            foreach (ExclusiveGateway gateway in gateways)
            {
                validateExclusiveGateway(process, gateway, errors);
            }
        }

        public virtual void validateExclusiveGateway(Process process, ExclusiveGateway exclusiveGateway, IList<ValidationError> errors)
        {
            if (exclusiveGateway.OutgoingFlows.Count == 0)
            {
                addError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_NO_OUTGOING_SEQ_FLOW, process, exclusiveGateway, "Exclusive gateway has no outgoing sequence flow");
            }
            else if (exclusiveGateway.OutgoingFlows.Count == 1)
            {
                SequenceFlow sequenceFlow = exclusiveGateway.OutgoingFlows[0];
                if (!string.IsNullOrWhiteSpace(sequenceFlow.ConditionExpression))
                {
                    addError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_CONDITION_NOT_ALLOWED_ON_SINGLE_SEQ_FLOW, process, exclusiveGateway, "Exclusive gateway has only one outgoing sequence flow. This is not allowed to have a condition.");
                }
            }
            else
            {
                string defaultSequenceFlow = exclusiveGateway.DefaultFlow;

                IList<SequenceFlow> flowsWithoutCondition = new List<SequenceFlow>();
                foreach (SequenceFlow flow in exclusiveGateway.OutgoingFlows)
                {
                    string condition = flow.ConditionExpression;
                    bool isDefaultFlow = !ReferenceEquals(flow.Id, null) && flow.Id.Equals(defaultSequenceFlow);
                    bool hasConditon = !string.IsNullOrWhiteSpace(condition);

                    if (!hasConditon && !isDefaultFlow)
                    {
                        flowsWithoutCondition.Add(flow);
                    }
                    if (hasConditon && isDefaultFlow)
                    {
                        addError(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_CONDITION_ON_DEFAULT_SEQ_FLOW, process, exclusiveGateway, "Default sequenceflow has a condition, which is not allowed");
                    }
                }

                if (flowsWithoutCondition.Count > 0)
                {
                    addWarning(errors, ProblemsConstants.EXCLUSIVE_GATEWAY_SEQ_FLOW_WITHOUT_CONDITIONS, process, exclusiveGateway, "Exclusive gateway has at least one outgoing sequence flow without a condition (which isn't the default one)");
                }

            }
        }

    }

}