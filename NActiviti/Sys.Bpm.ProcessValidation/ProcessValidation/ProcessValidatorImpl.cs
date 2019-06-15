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
namespace org.activiti.validation
{

    using org.activiti.bpmn.model;
    using org.activiti.validation.validator;

    /// 
    public class ProcessValidatorImpl : IProcessValidator
    {

        protected internal IList<ValidatorSet> validatorSets;

        public virtual IList<ValidationError> Validate(BpmnModel bpmnModel)
        {

            IList<ValidationError> allErrors = new List<ValidationError>();

            foreach (ValidatorSet validatorSet in validatorSets)
            {
                foreach (IValidator validator in validatorSet.Validators)
                {
                    IList<ValidationError> validatorErrors = new List<ValidationError>();
                    validator.Validate(bpmnModel, validatorErrors);
                    if (validatorErrors.Count > 0)
                    {
                        foreach (ValidationError error in validatorErrors)
                        {
                            error.ValidatorSetName = validatorSet.Name;
                        }
                      ((List<ValidationError>)allErrors).AddRange(validatorErrors);
                    }
                }
            }
            return allErrors;
        }

        public virtual IList<ValidatorSet> ValidatorSets
        {
            get
            {
                return validatorSets;
            }
            set
            {
                this.validatorSets = value;
            }
        }


        public virtual void AddValidatorSet(ValidatorSet validatorSet)
        {
            if (validatorSets == null)
            {
                validatorSets = new List<ValidatorSet>();
            }
            validatorSets.Add(validatorSet);
        }

    }

}