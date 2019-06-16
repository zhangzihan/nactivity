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
namespace Sys.Workflow.validation
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.validation.validator;

    /// <summary>
    /// Validates a process definition against the rules of the Activiti engine to be executable
    /// 
    /// 
    /// </summary>
    public interface IProcessValidator
    {

        /// <summary>
        /// Validates the provided <seealso cref="BpmnModel"/> and returns a list of all <seealso cref="ValidationError"/> occurences found.
        /// </summary>
        IList<ValidationError> Validate(BpmnModel bpmnModel);

        /// <summary>
        /// Returns the <seealso cref="ValidatorSet"/> instances for this process validator. Useful if some validation rules need to be disabled.
        /// </summary>
        IList<ValidatorSet> ValidatorSets { get; }

    }

}