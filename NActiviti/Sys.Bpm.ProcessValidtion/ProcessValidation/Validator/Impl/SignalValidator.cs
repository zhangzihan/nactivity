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
    public class SignalValidator : ValidatorImpl
    {

        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            ICollection<Signal> signals = bpmnModel.Signals;
            if (signals != null && signals.Count > 0)
            {

                foreach (Signal signal in signals)
                {
                    if (string.IsNullOrWhiteSpace(signal.Id))
                    {
                        addError(errors, ProblemsConstants.SIGNAL_MISSING_ID, signal, "Signal must have an id");
                    }

                    if (string.IsNullOrWhiteSpace(signal.Name))
                    {
                        addError(errors, ProblemsConstants.SIGNAL_MISSING_NAME, signal, "Signal must have a name");
                    }

                    if (!string.IsNullOrWhiteSpace(signal.Name) && duplicateName(signals, signal.Id, signal.Name))
                    {
                        addError(errors, ProblemsConstants.SIGNAL_DUPLICATE_NAME, signal, "Duplicate signal name found");
                    }

                    if (!ReferenceEquals(signal.Scope, null) && !signal.Scope.Equals(Signal.SCOPE_GLOBAL) && !signal.Scope.Equals(Signal.SCOPE_PROCESS_INSTANCE))
                    {
                        addError(errors, ProblemsConstants.SIGNAL_INVALID_SCOPE, signal, "Invalid value for 'scope'. Only values 'global' and 'processInstance' are supported");
                    }
                }

            }
        }

        protected internal virtual bool duplicateName(ICollection<Signal> signals, string id, string name)
        {
            foreach (Signal signal in signals)
            {
                if (!ReferenceEquals(id, null) && !ReferenceEquals(signal.Id, null))
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