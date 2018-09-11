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
    public class OperationValidator : ValidatorImpl
    {

        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (bpmnModel.Interfaces != null)
            {
                foreach (Interface bpmnInterface in bpmnModel.Interfaces)
                {
                    if (bpmnInterface.Operations != null)
                    {
                        foreach (Operation operation in bpmnInterface.Operations)
                        {
                            if (bpmnModel.getMessage(operation.InMessageRef) == null)
                            {
                                addError(errors, org.activiti.validation.validator.Problems_Fields.OPERATION_INVALID_IN_MESSAGE_REFERENCE, null, operation, "Invalid inMessageRef for operation");
                            }
                        }
                    }
                }
            }
        }

    }

}