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
namespace org.activiti.bpmn.model
{

    public class FormProperty : BaseElement
    {

        protected internal string name;
        protected internal string expression;
        protected internal string variable;
        protected internal string type;
        protected internal string defaultExpression;
        protected internal string datePattern;
        protected internal bool readable = true;
        protected internal bool writeable = true;
        protected internal bool required;
        protected internal IList<FormValue> formValues = new List<FormValue>();

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


        public virtual string Expression
        {
            get
            {
                return expression;
            }
            set
            {
                this.expression = value;
            }
        }


        public virtual string Variable
        {
            get
            {
                return variable;
            }
            set
            {
                this.variable = value;
            }
        }


        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }

        public virtual string DefaultExpression
        {
            get
            {
                return defaultExpression;
            }
            set
            {
                this.defaultExpression = value;
            }
        }



        public virtual string DatePattern
        {
            get
            {
                return datePattern;
            }
            set
            {
                this.datePattern = value;
            }
        }


        public virtual bool Readable
        {
            get
            {
                return readable;
            }
            set
            {
                this.readable = value;
            }
        }


        public virtual bool Writeable
        {
            get
            {
                return writeable;
            }
            set
            {
                this.writeable = value;
            }
        }


        public virtual bool Required
        {
            get
            {
                return required;
            }
            set
            {
                this.required = value;
            }
        }


        public virtual IList<FormValue> FormValues
        {
            get
            {
                return formValues;
            }
            set
            {
                this.formValues = value;
            }
        }


        public override BaseElement clone()
        {
            FormProperty clone = new FormProperty();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as FormProperty;

                Name = val.Name;
                Expression = val.Expression;
                Variable = val.Variable;
                Type = val.Type;
                DefaultExpression = val.DefaultExpression;
                DatePattern = val.DatePattern;
                Readable = val.Readable;
                Writeable = val.Writeable;
                Required = val.Required;

                formValues = new List<FormValue>();
                if (val.FormValues != null && val.FormValues.Count > 0)
                {
                    foreach (FormValue formValue in val.FormValues)
                    {
                        formValues.Add(val.clone() as FormValue);
                    }
                }
            }
        }
    }

}