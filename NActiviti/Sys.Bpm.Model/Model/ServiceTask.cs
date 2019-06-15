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

    public class ServiceTask : TaskWithFieldExtensions
    {

        public const string DMN_TASK = "dmn";
        public const string MAIL_TASK = "mail";

        protected internal string implementation;
        protected internal string implementationType;
        protected internal string resultVariableName;
        protected internal string type;
        protected internal string operationRef;
        protected internal string extensionId;
        protected internal IList<CustomProperty> customProperties = new List<CustomProperty>();
        protected internal string skipExpression;

        public ServiceTask() : base()
        {

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


        public virtual string ResultVariableName
        {
            get
            {
                return resultVariableName;
            }
            set
            {
                this.resultVariableName = value;
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


        public virtual IList<CustomProperty> CustomProperties
        {
            get
            {
                return customProperties;
            }
            set
            {
                this.customProperties = value;
            }
        }


        public virtual string OperationRef
        {
            get
            {
                return operationRef;
            }
            set
            {
                this.operationRef = value;
            }
        }


        public virtual string ExtensionId
        {
            get
            {
                return extensionId;
            }
            set
            {
                this.extensionId = value;
            }
        }


        public virtual bool Extended
        {
            get
            {
                return !(extensionId is null) && extensionId.Length > 0;
            }
        }

        public virtual string SkipExpression
        {
            get
            {
                return skipExpression;
            }
            set
            {
                this.skipExpression = value;
            }
        }


        public override BaseElement Clone()
        {
            ServiceTask clone = new ServiceTask
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

                var val = value as ServiceTask;

                Implementation = val.Implementation;
                ImplementationType = val.ImplementationType;
                ResultVariableName = val.ResultVariableName;
                Type = val.Type;
                OperationRef = val.OperationRef;
                ExtensionId = val.ExtensionId;
                SkipExpression = val.SkipExpression;

                fieldExtensions = new List<FieldExtension>();
                if (val.FieldExtensions != null && val.FieldExtensions.Count > 0)
                {
                    foreach (FieldExtension extension in val.FieldExtensions)
                    {
                        fieldExtensions.Add(extension.Clone() as FieldExtension);
                    }
                }

                customProperties = new List<CustomProperty>();
                if (val.CustomProperties != null && val.CustomProperties.Count > 0)
                {
                    foreach (CustomProperty property in val.CustomProperties)
                    {
                        customProperties.Add(property.Clone() as CustomProperty);
                    }
                }
            }
        }
    }

}