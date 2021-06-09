using System;
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
namespace Sys.Workflow.Validation.Validators
{

    /// 
    public class ValidatorSet
    {

        protected internal string name;

        protected internal IDictionary<Type, IValidator> validators;

        public ValidatorSet(string name)
        {
            this.name = name;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual ICollection<IValidator> Validators
        {
            get
            {
                return validators.Values;
            }
            set
            {
                foreach (IValidator validator in value)
                {
                    AddValidator(validator);
                }
            }
        }


        public virtual void RemoveValidator(Type validatorClass)
        {
            validators.Remove(validatorClass);
        }

        public virtual void AddValidator(IValidator validator)
        {
            if (validators is null)
            {
                validators = new Dictionary<Type, IValidator>();
            }
            validators[validator.GetType()] = validator;
        }

    }

}