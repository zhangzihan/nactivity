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
    public class SignalValidator : ValidatorImpl
    {

        public override void Validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            ICollection<Signal> signals = bpmnModel.Signals;
            if (signals != null && signals.Count > 0)
            {

                foreach (Signal signal in signals)
                {
                    if (string.IsNullOrWhiteSpace(signal.Id))
                    {
                        AddError(errors, ProblemsConstants.SIGNAL_MISSING_ID, signal, ProcessValidatorResource.SIGNAL_MISSING_ID);
                    }

                    if (string.IsNullOrWhiteSpace(signal.Name))
                    {
                        AddError(errors, ProblemsConstants.SIGNAL_MISSING_NAME, signal, ProcessValidatorResource.SIGNAL_MISSING_NAME);
                    }

                    if (!string.IsNullOrWhiteSpace(signal.Name) && DuplicateName(signals, signal.Id, signal.Name))
                    {
                        AddError(errors, ProblemsConstants.SIGNAL_DUPLICATE_NAME, signal, ProcessValidatorResource.SIGNAL_DUPLICATE_NAME);
                    }

                    if (!(signal.Scope is null) && !signal.Scope.Equals(Signal.SCOPE_GLOBAL) && !signal.Scope.Equals(Signal.SCOPE_PROCESS_INSTANCE))
                    {
                        AddError(errors, ProblemsConstants.SIGNAL_INVALID_SCOPE, signal, ProcessValidatorResource.SIGNAL_INVALID_SCOPE);
                    }
                }

            }
        }

        protected internal virtual bool DuplicateName(ICollection<Signal> signals, string id, string name)
        {
            foreach (Signal signal in signals)
            {
                if (!(id is null) && !(signal.Id is null))
                {
                    if (name.Equals(signal.Name) && !id.Equals(signal.Id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }

}