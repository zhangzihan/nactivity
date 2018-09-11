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
    public class ActivitiEventListenerValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<EventListener> eventListeners = process.EventListeners;
            if (eventListeners != null)
            {
                foreach (EventListener eventListener in eventListeners)
                {

                    if (!string.ReferenceEquals(eventListener.ImplementationType, null) && eventListener.ImplementationType.Equals(ImplementationType.IMPLEMENTATION_TYPE_INVALID_THROW_EVENT))
                    {

                        addError(errors, org.activiti.validation.validator.Problems_Fields.EVENT_LISTENER_INVALID_THROW_EVENT_TYPE, process, eventListener, "Invalid or unsupported throw event type on event listener");

                    }
                    else if (string.ReferenceEquals(eventListener.ImplementationType, null) || eventListener.ImplementationType.Length == 0)
                    {

                        addError(errors, org.activiti.validation.validator.Problems_Fields.EVENT_LISTENER_IMPLEMENTATION_MISSING, process, eventListener, "Element 'class', 'delegateExpression' or 'throwEvent' is mandatory on eventListener");

                    }
                    else if (!string.ReferenceEquals(eventListener.ImplementationType, null))
                    {

                        if (!ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_SIGNAL_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_GLOBAL_SIGNAL_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_MESSAGE_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_ERROR_EVENT.Equals(eventListener.ImplementationType))
                        {
                            addError(errors, org.activiti.validation.validator.Problems_Fields.EVENT_LISTENER_INVALID_IMPLEMENTATION, process, eventListener, "Unsupported implementation type for event listener");
                        }

                    }

                }

            }
        }

    }

}