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

    public class StartEvent : Event
    {

        protected internal string initiator;
        protected internal string formKey;
        protected internal bool isInterrupting;
        protected internal IList<FormProperty> formProperties = new List<FormProperty>();

        public virtual string Initiator
        {
            get
            {
                return initiator;
            }
            set
            {
                this.initiator = value;
            }
        }


        public virtual string FormKey
        {
            get
            {
                return formKey;
            }
            set
            {
                this.formKey = value;
            }
        }


        public virtual bool Interrupting
        {
            get
            {
                return isInterrupting;
            }
            set
            {
                this.isInterrupting = value;
            }
        }


        public virtual IList<FormProperty> FormProperties
        {
            get
            {
                return formProperties;
            }
            set
            {
                this.formProperties = value;
            }
        }


        public override BaseElement Clone()
        {
            StartEvent clone = new StartEvent
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as StartEvent;

                Initiator = val.Initiator;
                FormKey = val.FormKey;
                Interrupting = val.isInterrupting;

                formProperties = new List<FormProperty>();
                if (val.FormProperties != null && val.FormProperties.Count > 0)
                {
                    foreach (FormProperty property in val.FormProperties)
                    {
                        formProperties.Add(property.Clone() as FormProperty);
                    }
                }
            }
        }
    }

}