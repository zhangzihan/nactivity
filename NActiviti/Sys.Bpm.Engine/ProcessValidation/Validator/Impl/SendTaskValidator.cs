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
namespace org.activiti.validation.validator.impl
{

    using org.activiti.bpmn.model;

    /// 
    public class SendTaskValidator : ExternalInvocationTaskValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<SendTask> sendTasks = process.findFlowElementsOfType<SendTask>();
            foreach (SendTask sendTask in sendTasks)
            {

                // Verify implementation
                if (string.IsNullOrWhiteSpace(sendTask.Type) && !ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(sendTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEND_TASK_INVALID_IMPLEMENTATION, process, sendTask, "One of the attributes 'type' or 'operation' is mandatory on sendTask");
                }

                // Verify type
                if (!string.IsNullOrWhiteSpace(sendTask.Type))
                {

                    if (!sendTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase) && !sendTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase) && !sendTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase))
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.SEND_TASK_INVALID_TYPE, process, sendTask, "Invalid or unsupported type for send task");
                    }

                    if (sendTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
                    {
                        validateFieldDeclarationsForEmail(process, sendTask, sendTask.FieldExtensions, errors);
                    }

                }

                // Web service
                verifyWebservice(bpmnModel, process, sendTask, errors);
            }
        }

        protected internal virtual void verifyWebservice(BpmnModel bpmnModel, Process process, SendTask sendTask, IList<ValidationError> errors)
        {
            if (ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(sendTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(sendTask.OperationRef))
            {

                bool operationFound = false;
                if (bpmnModel.Interfaces != null && bpmnModel.Interfaces.Count > 0)
                {
                    foreach (Interface bpmnInterface in bpmnModel.Interfaces)
                    {
                        if (bpmnInterface.Operations != null && bpmnInterface.Operations.Count > 0)
                        {
                            foreach (Operation operation in bpmnInterface.Operations)
                            {
                                if (!string.ReferenceEquals(operation.Id, null) && operation.Id.Equals(sendTask.OperationRef))
                                {
                                    operationFound = true;
                                }
                            }
                        }
                    }
                }

                if (!operationFound)
                {
                    addError(errors, org.activiti.validation.validator.Problems_Fields.SEND_TASK_WEBSERVICE_INVALID_OPERATION_REF, process, sendTask, "Invalid operation reference for send task");
                }

            }
        }

    }

}