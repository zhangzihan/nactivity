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
    /// 
    public class BpmnModelValidator : ValidatorImpl
    {

        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {

            // If all process definitions of this bpmnModel are not executable, raise an error
            bool isAtLeastOneExecutable = validateAtLeastOneExecutable(bpmnModel, errors);

            // If at least one process definition is executable, show a warning for each of the none-executables
            if (isAtLeastOneExecutable)
            {
                foreach (Process process in bpmnModel.Processes)
                {
                    if (!process.Executable)
                    {
                        addWarning(errors, org.activiti.validation.validator.Problems_Fields.PROCESS_DEFINITION_NOT_EXECUTABLE, process, process, "Process definition is not executable. Please verify that this is intentional.");
                    }
                    handleProcessConstraints(bpmnModel, process, errors);
                }
            }
            handleBPMNModelConstraints(bpmnModel, errors);
        }

        protected internal virtual void handleProcessConstraints(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            if (!string.ReferenceEquals(process.Id, null) && process.Id.Length > Constraints.PROCESS_DEFINITION_ID_MAX_LENGTH)
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.PROCESS_DEFINITION_ID_TOO_LONG, process, "The id of the process definition must not contain more than " + Constraints.PROCESS_DEFINITION_ID_MAX_LENGTH + " characters");
            }
            if (!string.ReferenceEquals(process.Name, null) && process.Name.Length > Constraints.PROCESS_DEFINITION_NAME_MAX_LENGTH)
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.PROCESS_DEFINITION_NAME_TOO_LONG, process, "The name of the process definition must not contain more than " + Constraints.PROCESS_DEFINITION_NAME_MAX_LENGTH + " characters");
            }
            if (!string.ReferenceEquals(process.Documentation, null) && process.Documentation.Length > Constraints.PROCESS_DEFINITION_DOCUMENTATION_MAX_LENGTH)
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.PROCESS_DEFINITION_DOCUMENTATION_TOO_LONG, process, "The documentation of the process definition must not contain more than " + Constraints.PROCESS_DEFINITION_DOCUMENTATION_MAX_LENGTH + " characters");
            }
        }

        protected internal virtual void handleBPMNModelConstraints(BpmnModel bpmnModel, IList<ValidationError> errors)
        {
            if (!string.ReferenceEquals(bpmnModel.TargetNamespace, null) && bpmnModel.TargetNamespace.Length > Constraints.BPMN_MODEL_TARGET_NAMESPACE_MAX_LENGTH)
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.BPMN_MODEL_TARGET_NAMESPACE_TOO_LONG, "The targetNamespace of the bpmn model must not contain more than " + Constraints.BPMN_MODEL_TARGET_NAMESPACE_MAX_LENGTH + " characters");
            }
        }

        /// <summary>
        /// Returns 'true' if at least one process definition in the <seealso cref="BpmnModel"/> is executable.
        /// </summary>
        protected internal virtual bool validateAtLeastOneExecutable(BpmnModel bpmnModel, IList<ValidationError> errors)
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
                addError(errors, org.activiti.validation.validator.Problems_Fields.ALL_PROCESS_DEFINITIONS_NOT_EXECUTABLE, "All process definition are set to be non-executable (property 'isExecutable' on process). This is not allowed.");
            }

            return nrOfExecutableDefinitions > 0;
        }

    }

}