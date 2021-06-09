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

    using Sys.Workflow.Bpmn.Models;

    /// 
    public class ActivitiEventListenerValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<EventListener> eventListeners = process.EventListeners;
            if (eventListeners is object)
            {
                foreach (EventListener eventListener in eventListeners)
                {

                    if (string.IsNullOrWhiteSpace(eventListener.ImplementationType) || eventListener.ImplementationType.Equals(ImplementationType.IMPLEMENTATION_TYPE_INVALID_THROW_EVENT))
                    {

                        AddError(errors, ProblemsConstants.EVENT_LISTENER_INVALID_THROW_EVENT_TYPE, process, eventListener, ProcessValidatorResource.EVENT_LISTENER_INVALID_THROW_EVENT_TYPE);

                    }
                    else if (string.IsNullOrWhiteSpace(eventListener.ImplementationType) || eventListener.ImplementationType.Length == 0)
                    {

                        AddError(errors, ProblemsConstants.EVENT_LISTENER_IMPLEMENTATION_MISSING, process, eventListener, ProcessValidatorResource.EVENT_LISTENER_IMPLEMENTATION_MISSING);

                    }
                    else if (!string.IsNullOrWhiteSpace(eventListener.ImplementationType))
                    {

                        if (!ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_SIGNAL_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_GLOBAL_SIGNAL_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_MESSAGE_EVENT.Equals(eventListener.ImplementationType) && !ImplementationType.IMPLEMENTATION_TYPE_THROW_ERROR_EVENT.Equals(eventListener.ImplementationType))
                        {
                            AddError(errors, ProblemsConstants.EVENT_LISTENER_INVALID_IMPLEMENTATION, process, eventListener, ProcessValidatorResource.EVENT_LISTENER_IMPLEMENTATION_MISSING);
                        }

                    }

                }

            }
        }

    }

}