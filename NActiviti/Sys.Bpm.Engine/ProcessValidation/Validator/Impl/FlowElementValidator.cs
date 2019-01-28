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

    /// <summary>
    /// A validator for stuff that is shared across all flow elements
    /// 
    /// 
    /// 
    /// </summary>
    public class FlowElementValidator : ProcessLevelValidator
    {

        protected internal const int ID_MAX_LENGTH = 255;

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            foreach (FlowElement flowElement in process.FlowElements)
            {

                if (flowElement is Activity)
                {
                    Activity activity = (Activity)flowElement;
                    handleConstraints(process, activity, errors);
                    handleMultiInstanceLoopCharacteristics(process, activity, errors);
                    handleDataAssociations(process, activity, errors);
                }

            }

        }

        protected internal virtual void handleConstraints(Process process, Activity activity, IList<ValidationError> errors)
        {
            if (!ReferenceEquals(activity.Id, null) && activity.Id.Length > ID_MAX_LENGTH)
            {
                addError(errors, Problems_Fields.FLOW_ELEMENT_ID_TOO_LONG, process, activity, "The id of a flow element must not contain more than " + ID_MAX_LENGTH + " characters");
            }
        }

        protected internal virtual void handleMultiInstanceLoopCharacteristics(Process process, Activity activity, IList<ValidationError> errors)
        {
            MultiInstanceLoopCharacteristics multiInstanceLoopCharacteristics = activity.LoopCharacteristics;
            if (multiInstanceLoopCharacteristics != null)
            {

                if (string.IsNullOrWhiteSpace(multiInstanceLoopCharacteristics.LoopCardinality) && string.IsNullOrWhiteSpace(multiInstanceLoopCharacteristics.InputDataItem))
                {

                    addError(errors, Problems_Fields.MULTI_INSTANCE_MISSING_COLLECTION, process, activity, "Either loopCardinality or loopDataInputRef/activiti:collection must been set");
                }

            }
        }

        protected internal virtual void handleDataAssociations(Process process, Activity activity, IList<ValidationError> errors)
        {
            if (activity.DataInputAssociations != null)
            {
                foreach (DataAssociation dataAssociation in activity.DataInputAssociations)
                {
                    if (string.IsNullOrWhiteSpace(dataAssociation.TargetRef))
                    {
                        addError(errors, Problems_Fields.DATA_ASSOCIATION_MISSING_TARGETREF, process, activity, "Targetref is required on a data association");
                    }
                }
            }
            if (activity.DataOutputAssociations != null)
            {
                foreach (DataAssociation dataAssociation in activity.DataOutputAssociations)
                {
                    if (string.IsNullOrWhiteSpace(dataAssociation.TargetRef))
                    {
                        addError(errors, Problems_Fields.DATA_ASSOCIATION_MISSING_TARGETREF, process, activity, "Targetref is required on a data association");
                    }
                }
            }
        }

    }

}