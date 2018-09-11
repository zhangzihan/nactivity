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
    public class AssociationValidator : ValidatorImpl
    {

        public override void validate(BpmnModel bpmnModel, IList<ValidationError> errors)
        {

            // Global associations
            ICollection<Artifact> artifacts = bpmnModel.GlobalArtifacts;
            if (artifacts != null)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact is Association)
                    {
                        validate(null, (Association)artifact, errors);
                    }
                }
            }

            // Process associations
            foreach (Process process in bpmnModel.Processes)
            {
                artifacts = process.Artifacts;
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact is Association)
                    {
                        validate(process, (Association)artifact, errors);
                    }
                }
            }

        }

        protected internal virtual void validate(Process process, Association association, IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(association.SourceRef))
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.ASSOCIATION_INVALID_SOURCE_REFERENCE, process, association, "association element missing attribute 'sourceRef'");
            }
            if (string.IsNullOrWhiteSpace(association.TargetRef))
            {
                addError(errors, org.activiti.validation.validator.Problems_Fields.ASSOCIATION_INVALID_TARGET_REFERENCE, process, association, "association element missing attribute 'targetRef'");
            }
        }

    }

}