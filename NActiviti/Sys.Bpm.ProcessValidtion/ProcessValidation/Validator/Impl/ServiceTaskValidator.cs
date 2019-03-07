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
    public class ServiceTaskValidator : ExternalInvocationTaskValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ServiceTask> serviceTasks = process.findFlowElementsOfType<ServiceTask>();
            foreach (ServiceTask serviceTask in serviceTasks)
            {
                verifyImplementation(process, serviceTask, errors);
                verifyType(process, serviceTask, errors);
                verifyResultVariableName(process, serviceTask, errors);
                verifyWebservice(bpmnModel, process, serviceTask, errors);
            }
        }

        protected internal virtual void verifyImplementation(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(serviceTask.Type) && string.IsNullOrWhiteSpace(serviceTask.Implementation))
            {
                addError(errors, ProblemsConstants.SERVICE_TASK_MISSING_IMPLEMENTATION, process, serviceTask, "One of the attributes 'implementation', 'class', 'delegateExpression', 'type', 'operation', or 'expression' is mandatory on serviceTask.");
            }
        }

        protected internal virtual void verifyType(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {

                if (!serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("dmn", StringComparison.CurrentCultureIgnoreCase))
                {

                    addError(errors, ProblemsConstants.SERVICE_TASK_INVALID_TYPE, process, serviceTask, "Invalid or unsupported service task type");
                }

                if (serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
                {
                    validateFieldDeclarationsForEmail(process, serviceTask, serviceTask.FieldExtensions, errors);
                }
                else if (serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase))
                {
                    validateFieldDeclarationsForShell(process, serviceTask, serviceTask.FieldExtensions, errors);
                }
                else if (serviceTask.Type.Equals("dmn", StringComparison.CurrentCultureIgnoreCase))
                {
                    validateFieldDeclarationsForDmn(process, serviceTask, serviceTask.FieldExtensions, errors);
                }

            }
        }

        protected internal virtual void verifyResultVariableName(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(serviceTask.ResultVariableName) && (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType) || ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType)))
            {
                addError(errors, ProblemsConstants.SERVICE_TASK_RESULT_VAR_NAME_WITH_DELEGATE, process, serviceTask, "'resultVariableName' not supported for service tasks using 'class' or 'delegateExpression");
            }
        }

        protected internal virtual void verifyWebservice(BpmnModel bpmnModel, Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(serviceTask.OperationRef))
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
                                if (!ReferenceEquals(operation.Id, null) && operation.Id.Equals(serviceTask.OperationRef))
                                {
                                    operationFound = true;
                                }
                            }
                        }
                    }
                }

                if (!operationFound)
                {
                    addError(errors, ProblemsConstants.SERVICE_TASK_WEBSERVICE_INVALID_OPERATION_REF, process, serviceTask, "Invalid operation reference");
                }

            }
        }

    }

}