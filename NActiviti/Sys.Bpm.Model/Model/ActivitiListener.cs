using Newtonsoft.Json;
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
    public class ActivitiListener : BaseElement
    {

        protected internal string @event;
        protected internal string implementationType;
        protected internal string implementation;
        protected internal IList<FieldExtension> fieldExtensions = new List<FieldExtension>();
        protected internal string onTransaction;
        protected internal string customPropertiesResolverImplementationType;
        protected internal string customPropertiesResolverImplementation;

        [JsonIgnore]
        protected internal object instance; // Can be used to set an instance of the listener directly. That instance will then always be reused.

        public virtual string Event
        {
            get
            {
                return @event;
            }
            set
            {
                this.@event = value;
            }
        }


        public virtual string ImplementationType
        {
            get
            {
                return implementationType;
            }
            set
            {
                this.implementationType = value;
            }
        }


        public virtual string Implementation
        {
            get
            {
                return implementation;
            }
            set
            {
                this.implementation = value;
            }
        }


        public virtual IList<FieldExtension> FieldExtensions
        {
            get
            {
                return fieldExtensions;
            }
            set
            {
                this.fieldExtensions = value;
            }
        }


        public virtual string OnTransaction
        {
            get
            {
                return onTransaction;
            }
            set
            {
                this.onTransaction = value;
            }
        }


        public virtual string CustomPropertiesResolverImplementationType
        {
            get
            {
                return customPropertiesResolverImplementationType;
            }
            set
            {
                this.customPropertiesResolverImplementationType = value;
            }
        }


        public virtual string CustomPropertiesResolverImplementation
        {
            get
            {
                return customPropertiesResolverImplementation;
            }
            set
            {
                this.customPropertiesResolverImplementation = value;
            }
        }

        [JsonIgnore]
        public virtual object Instance
        {
            get
            {
                return instance;
            }
            set
            {
                this.instance = value;
            }
        }


        public override BaseElement clone()
        {
            ActivitiListener clone = new ActivitiListener();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as ActivitiListener;

                Event = val.Event;

                Implementation = val.Implementation;
                ImplementationType = val.ImplementationType;

                fieldExtensions = new List<FieldExtension>();
                if (val.FieldExtensions != null && val.FieldExtensions.Count > 0)
                {
                    foreach (FieldExtension extension in val.FieldExtensions)
                    {
                        fieldExtensions.Add(extension.clone() as FieldExtension);
                    }
                }
            }
        }
    }

}