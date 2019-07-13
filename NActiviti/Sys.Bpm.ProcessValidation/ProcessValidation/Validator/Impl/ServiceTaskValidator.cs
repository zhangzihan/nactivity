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
namespace Sys.Workflow.Validation.Validators.Impl
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class ServiceTaskValidator : ExternalInvocationTaskValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<ServiceTask> serviceTasks = process.FindFlowElementsOfType<ServiceTask>();
            foreach (ServiceTask serviceTask in serviceTasks)
            {
                //可以不校验实现类,如果没有填写实现类,默认使用ServiceWebApiBehavior
                //verifyImplementation(process, serviceTask, errors);
                //verifyType(process, serviceTask, errors);
                //verifyResultVariableName(process, serviceTask, errors);
                //verifyWebservice(bpmnModel, process, serviceTask, errors);

                serviceTask.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY,
                out IList<ExtensionElement> pElements);

                if (pElements == null || pElements.Count == 0 || string.IsNullOrWhiteSpace(pElements.GetAttributeValue("url")))
                {
                    AddError(errors, ProblemsConstants.SERVICE_TASK_WEBSERVICE_INVALID_URL, process, serviceTask, ProcessValidatorResource.SERVICE_TASK_WEBSERVICE_INVALID_URL);
                }

                if (string.IsNullOrWhiteSpace(serviceTask.Name) || serviceTask.Name.Length > Constraints.BPMN_MODEL_NAME_MAX_LENGTH)
                {
                    AddError(errors, ProblemsConstants.SERVICE_TASK_NAME_TOO_LONG, process, serviceTask, ProcessValidatorResource.NAME_TOO_LONG);
                }
            }
        }

        protected internal virtual void VerifyImplementation(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(serviceTask.Type) && string.IsNullOrWhiteSpace(serviceTask.Implementation))
            {
                AddError(errors, ProblemsConstants.SERVICE_TASK_MISSING_IMPLEMENTATION, process, serviceTask, ProcessValidatorResource.SERVICE_TASK_MISSING_IMPLEMENTATION);
            }
        }

        protected internal virtual void VerifyType(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {

                if (!serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase) && !serviceTask.Type.Equals("dmn", StringComparison.CurrentCultureIgnoreCase))
                {

                    AddError(errors, ProblemsConstants.SERVICE_TASK_INVALID_TYPE, process, serviceTask, ProcessValidatorResource.SERVICE_TASK_INVALID_TYPE);
                }

                if (serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
                {
                    ValidateFieldDeclarationsForEmail(process, serviceTask, serviceTask.FieldExtensions, errors);
                }
                else if (serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase))
                {
                    ValidateFieldDeclarationsForShell(process, serviceTask, serviceTask.FieldExtensions, errors);
                }
                else if (serviceTask.Type.Equals("dmn", StringComparison.CurrentCultureIgnoreCase))
                {
                    ValidateFieldDeclarationsForDmn(process, serviceTask, serviceTask.FieldExtensions, errors);
                }

            }
        }

        protected internal virtual void VerifyResultVariableName(Process process, ServiceTask serviceTask, IList<ValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(serviceTask.ResultVariableName) && (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType) || ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType)))
            {
                AddError(errors, ProblemsConstants.SERVICE_TASK_RESULT_VAR_NAME_WITH_DELEGATE, process, serviceTask, ProcessValidatorResource.SERVICE_TASK_RESULT_VAR_NAME_WITH_DELEGATE);
            }
        }

        protected internal virtual void VerifyWebservice(BpmnModel bpmnModel, Process process, ServiceTask serviceTask, IList<ValidationError> errors)
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
                                if (operation.Id is object && operation.Id.Equals(serviceTask.OperationRef))
                                {
                                    operationFound = true;
                                }
                            }
                        }
                    }
                }

                if (!operationFound)
                {
                    AddError(errors, ProblemsConstants.SERVICE_TASK_WEBSERVICE_INVALID_OPERATION_REF, process, serviceTask, ProcessValidatorResource.SERVICE_TASK_WEBSERVICE_INVALID_OPERATION_REF);
                }
            }
        }
    }
}