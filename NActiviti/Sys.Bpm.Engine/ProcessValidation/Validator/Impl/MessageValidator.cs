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
    public class MessageValidator : ValidatorImpl
    {

        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (bpmnModel.Messages != null && bpmnModel.Messages.Count > 0)
            {
                foreach (Message message in bpmnModel.Messages)
                {

                    // Item ref
                    if (!string.IsNullOrWhiteSpace(message.ItemRef))
                    {
                        if (!bpmnModel.ItemDefinitions.ContainsKey(message.ItemRef))
                        {
                            addError(errors, org.activiti.validation.validator.Problems_Fields.MESSAGE_INVALID_ITEM_REF, null, message, "Item reference is invalid: not found");
                        }
                    }

                }
            }
        }

    }

}