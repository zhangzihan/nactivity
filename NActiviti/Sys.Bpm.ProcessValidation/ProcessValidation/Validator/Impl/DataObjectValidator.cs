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
namespace Sys.Workflow.validation.validator.impl
{

    using Sys.Workflow.bpmn.model;

    /// 
    public class DataObjectValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {

            // Gather data objects
            IList<ValuedDataObject> allDataObjects = new List<ValuedDataObject>();
            ((List<ValuedDataObject>)allDataObjects).AddRange(process.DataObjects);
            IList<SubProcess> subProcesses = process.FindFlowElementsOfType<SubProcess>(true);
            foreach (SubProcess subProcess in subProcesses)
            {
                ((List<ValuedDataObject>)allDataObjects).AddRange(subProcess.DataObjects);
            }

            // Validate
            foreach (ValuedDataObject dataObject in allDataObjects)
            {
                if (string.IsNullOrWhiteSpace(dataObject.Name))
                {
                    AddError(errors, ProblemsConstants.DATA_OBJECT_MISSING_NAME, process, dataObject, ProcessValidatorResource.DATA_OBJECT_MISSING_NAME);
                }
            }
        }

    }

}