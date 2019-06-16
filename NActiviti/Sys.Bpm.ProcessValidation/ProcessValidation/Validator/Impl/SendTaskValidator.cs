using System;
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
namespace Sys.Workflow.validation.validator.impl
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.model;

    /// 
    public class SendTaskValidator : ExternalInvocationTaskValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SendTask> sendTasks = process.FindFlowElementsOfType<SendTask>();
            foreach (SendTask sendTask in sendTasks)
            {
                // Verify implementation
                if (!string.IsNullOrWhiteSpace(sendTask.ImplementationType) && string.IsNullOrWhiteSpace(sendTask.Implementation))
                {
                    AddError(errors, ProblemsConstants.SEND_TASK_INVALID_IMPLEMENTATION, process, sendTask, ProcessValidatorResource.SEND_TASK_INVALID_IMPLEMENTATION);
                }

                sendTask.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out IList<ExtensionElement> pElements);

                if (pElements == null)
                {
                    AddError(errors, ProblemsConstants.SEND_TASK_INVALID_IMPLEMENTATION, process, sendTask, ProcessValidatorResource.SEND_TASK_TEMPLATE_NULL);
                    continue;
                }

                string email = pElements.GetAttributeValue("email");
                string wechat = pElements.GetAttributeValue("wechat");
                string sms = pElements.GetAttributeValue("sms");

                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(wechat) && string.IsNullOrWhiteSpace(sms))
                {
                    AddError(errors, ProblemsConstants.SEND_TASK_INVALID_IMPLEMENTATION, process, sendTask, ProcessValidatorResource.SEND_TASK_TEMPLATE_NULL);
                }

                if (string.IsNullOrWhiteSpace(sendTask.Name) || sendTask.Name.Length > Constraints.BPMN_MODEL_NAME_MAX_LENGTH)
                {
                    AddError(errors, ProblemsConstants.SEND_TASK_NAME_TOO_LONG, process, sendTask, ProcessValidatorResource.NAME_TOO_LONG);
                }
                //if (string.IsNullOrWhiteSpace(sendTask.Type) && !ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(sendTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                //{
                //    addError(errors, ProblemsConstants.SEND_TASK_INVALID_IMPLEMENTATION, process, sendTask, ProcessValidatorResource.SEND_TASK_INVALID_IMPLEMENTATION);
                //}

                // Verify type
                //if (!string.IsNullOrWhiteSpace(sendTask.Type))
                //{

                //    if (!sendTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase) && !sendTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase) && !sendTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        addError(errors, ProblemsConstants.SEND_TASK_INVALID_TYPE, process, sendTask, ProcessValidatorResource.SEND_TASK_INVALID_TYPE);
                //    }

                //    if (sendTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        validateFieldDeclarationsForEmail(process, sendTask, sendTask.FieldExtensions, errors);
                //    }

                //}

                // Web service
                //verifyWebservice(bpmnModel, process, sendTask, errors);
            }
        }

        //protected internal virtual void verifyWebservice(BpmnModel bpmnModel, Process process, SendTask sendTask, IList<ValidationError> errors)
        //{
        //    if (ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(sendTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(sendTask.OperationRef))
        //    {

        //        bool operationFound = false;
        //        if (bpmnModel.Interfaces != null && bpmnModel.Interfaces.Count > 0)
        //        {
        //            foreach (Interface bpmnInterface in bpmnModel.Interfaces)
        //            {
        //                if (bpmnInterface.Operations != null && bpmnInterface.Operations.Count > 0)
        //                {
        //                    foreach (Operation operation in bpmnInterface.Operations)
        //                    {
        //                        if (!ReferenceEquals(operation.Id, null) && operation.Id.Equals(sendTask.OperationRef))
        //                        {
        //                            operationFound = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (!operationFound)
        //        {
        //            addError(errors, ProblemsConstants.SEND_TASK_WEBSERVICE_INVALID_OPERATION_REF, process, sendTask, ProcessValidatorResource.SEND_TASK_WEBSERVICE_INVALID_OPERATION_REF);
        //        }
        //    }
        //}
    }
}