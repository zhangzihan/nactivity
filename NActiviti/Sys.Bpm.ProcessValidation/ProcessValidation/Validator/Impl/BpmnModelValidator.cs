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
    using Microsoft.CSharp;
    using Sys.Workflow.Bpmn.Models;
    using System.Text.RegularExpressions;

    /// 
    /// 
    public class BpmnModelValidator : ValidatorImpl
    {
        public override void Validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            // If all process definitions of this bpmnModel are not executable, raise an error
            bool isAtLeastOneExecutable = ValidateAtLeastOneExecutable(bpmnModel, errors);

            // If at least one process definition is executable, show a warning for each of the none-executables
            if (isAtLeastOneExecutable)
            {
                foreach (Process process in bpmnModel.Processes)
                {
                    if (!process.Executable)
                    {
                        AddWarning(errors, ProblemsConstants.PROCESS_DEFINITION_NOT_EXECUTABLE, process, process, ProcessValidatorResource.PROCESS_DEFINITION_NOT_EXECUTABLE);
                    }
                    HandleProcessConstraints(bpmnModel, process, errors);
                }
            }
            HandleBPMNModelConstraints(bpmnModel, errors);
        }

        protected internal virtual void HandleProcessConstraints(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(process.Id?.Trim()) || process.Id.Length > Constraints.PROCESS_DEFINITION_ID_MAX_LENGTH)
            {
                AddError(errors, ProblemsConstants.PROCESS_DEFINITION_ID_TOO_LONG, process,
                    string.Format(ProcessValidatorResource.PROCESS_DEFINITION_ID_TOO_LONG, Constraints.PROCESS_DEFINITION_ID_MAX_LENGTH));
            }

            if (new Regex("[\\u0100-\\u9fa5]+|^[^a-zA-Z_]").IsMatch(process.Id))
            {
                AddError(errors, ProblemsConstants.PROCESS_DEFINITION_ID_NOTSUPPORT_IDENTIFIER, process,
                    string.Format(ProcessValidatorResource.PROCESS_DEFINITION_ID_NOTSUPPORT_IDENTIFIER, process.Id));
            }

            if (string.IsNullOrWhiteSpace(process.Name?.Trim()) || process.Name.Length > Constraints.PROCESS_DEFINITION_NAME_MAX_LENGTH)
            {
                AddError(errors, ProblemsConstants.PROCESS_DEFINITION_NAME_TOO_LONG, process, string.Format(ProcessValidatorResource.PROCESS_DEFINITION_NAME_TOO_LONG, Constraints.PROCESS_DEFINITION_NAME_MAX_LENGTH));
            }
            if (string.IsNullOrWhiteSpace(process.Documentation?.Trim()) == false && process.Documentation.Length > Constraints.PROCESS_DEFINITION_DOCUMENTATION_MAX_LENGTH)
            {
                AddError(errors, ProblemsConstants.PROCESS_DEFINITION_DOCUMENTATION_TOO_LONG, process,
                    string.Format(ProcessValidatorResource.PROCESS_DEFINITION_DOCUMENTATION_TOO_LONG, Constraints.PROCESS_DEFINITION_DOCUMENTATION_MAX_LENGTH));
            }
        }

        protected internal virtual void HandleBPMNModelConstraints(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (bpmnModel.TargetNamespace is object && bpmnModel.TargetNamespace.Length > Constraints.BPMN_MODEL_TARGET_NAMESPACE_MAX_LENGTH)
            {
                AddError(errors, ProblemsConstants.BPMN_MODEL_TARGET_NAMESPACE_TOO_LONG,
                    string.Format(ProcessValidatorResource.BPMN_MODEL_TARGET_NAMESPACE_TOO_LONG, Constraints.BPMN_MODEL_TARGET_NAMESPACE_MAX_LENGTH));
            }
        }

        /// <summary>
        /// Returns 'true' if at least one process definition in the <seealso cref="BpmnModel"/> is executable.
        /// </summary>
        protected internal virtual bool ValidateAtLeastOneExecutable(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            int nrOfExecutableDefinitions = 0;
            foreach (Process process in bpmnModel.Processes)
            {
                if (process.Executable)
                {
                    nrOfExecutableDefinitions++;
                }
            }

            if (nrOfExecutableDefinitions == 0)
            {
                AddError(errors, ProblemsConstants.ALL_PROCESS_DEFINITIONS_NOT_EXECUTABLE,
                    ProcessValidatorResource.ALL_PROCESS_DEFINITIONS_NOT_EXECUTABLE);
            }

            return nrOfExecutableDefinitions > 0;
        }
    }
}