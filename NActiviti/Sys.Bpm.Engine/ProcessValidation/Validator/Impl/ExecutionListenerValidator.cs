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
    public class ExecutionListenerValidator : ProcessLevelValidator
    {
        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {

            validateListeners(process, process, process.ExecutionListeners, errors);

            foreach (FlowElement flowElement in process.FlowElements)
            {
                validateListeners(process, flowElement, flowElement.ExecutionListeners, errors);
            }
        }

        protected internal virtual void validateListeners(Process process, BaseElement baseElement, IList<ActivitiListener> listeners, IList<ValidationError> errors)
        {
            if (listeners != null)
            {
                foreach (ActivitiListener listener in listeners)
                {
                    if (string.ReferenceEquals(listener.Implementation, null) || string.ReferenceEquals(listener.ImplementationType, null))
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.EXECUTION_LISTENER_IMPLEMENTATION_MISSING, process, baseElement, "Element 'class' or 'expression' is mandatory on executionListener");
                    }
                    if (!string.ReferenceEquals(listener.OnTransaction, null) && ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(listener.ImplementationType))
                    {
                        addError(errors, org.activiti.validation.validator.Problems_Fields.EXECUTION_LISTENER_INVALID_IMPLEMENTATION_TYPE, process, baseElement, "Expression cannot be used when using 'onTransaction'");
                    }
                }
            }
        }
    }

}