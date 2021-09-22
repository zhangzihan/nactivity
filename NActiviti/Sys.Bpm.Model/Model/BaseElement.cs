using System;
using System.Collections.Concurrent;
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
namespace Sys.Workflow.Bpmn.Models
{
    public abstract class BaseElement : IHasExtensionAttributes
    {

        protected internal string id;
        protected internal int xmlRowNumber;
        protected internal int xmlColumnNumber;
        protected internal IDictionary<string, IList<ExtensionElement>> extensionElements = new ConcurrentDictionary<string, IList<ExtensionElement>>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// extension attributes could be part of each element </summary>
        protected internal IDictionary<string, IList<ExtensionAttribute>> attributes = new ConcurrentDictionary<string, IList<ExtensionAttribute>>(StringComparer.OrdinalIgnoreCase);

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }


        public virtual int XmlRowNumber
        {
            get
            {
                return xmlRowNumber;
            }
            set
            {
                this.xmlRowNumber = value;
            }
        }


        public virtual int XmlColumnNumber
        {
            get
            {
                return xmlColumnNumber;
            }
            set
            {
                this.xmlColumnNumber = value;
            }
        }


        public virtual IDictionary<string, IList<ExtensionElement>> ExtensionElements
        {
            get
            {
                if (extensionElements is null)
                {
                    extensionElements = new ConcurrentDictionary<string, IList<ExtensionElement>>();
                }

                return extensionElements;
            }
            set
            {
                this.extensionElements = value;
            }
        }

        public virtual void AddExtensionElement(ExtensionElement extensionElement)
        {
            if (extensionElement is not null && !string.IsNullOrWhiteSpace(extensionElement.Name))
            {
                if (!this.extensionElements.ContainsKey(extensionElement.Name))
                {
                    IList<ExtensionElement> elementList = new List<ExtensionElement>();
                    this.extensionElements[extensionElement.Name] = elementList;
                }
                this.extensionElements[extensionElement.Name].Add(extensionElement);
            }
        }


        public virtual IDictionary<string, IList<ExtensionAttribute>> Attributes
        {
            get
            {
                return attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        public virtual string GetAttributeValue(string @namespace, string name)
        {
            if (Attributes.TryGetValue(name, out IList<ExtensionAttribute> attributes) && attributes.Count > 0)
            {
                foreach (ExtensionAttribute attribute in attributes)
                {
                    if ((@namespace is null && attribute.Namespace is null) || @namespace.Equals(attribute.Namespace))
                    {
                        return attribute.Value;
                    }
                }
            }
            return null;
        }

        public virtual void AddAttribute(ExtensionAttribute attribute)
        {
            if (attribute is not null && !string.IsNullOrWhiteSpace(attribute.Name))
            {
                if (!this.attributes.ContainsKey(attribute.Name))
                {
                    IList<ExtensionAttribute> attributeList = new List<ExtensionAttribute>();
                    this.attributes[attribute.Name] = attributeList;
                }
                this.attributes[attribute.Name].Add(attribute);
            }
        }


        public virtual BaseElement Values
        {
            set
            {
                Id = value.Id;

                extensionElements = new ConcurrentDictionary<string, IList<ExtensionElement>>();
                if (value.ExtensionElements is object && value.ExtensionElements.Count > 0)
                {
                    foreach (string key in value.ExtensionElements.Keys)
                    {
                        IList<ExtensionElement> otherElementList = value.ExtensionElements[key];
                        if (otherElementList is object && otherElementList.Count > 0)
                        {
                            IList<ExtensionElement> elementList = new List<ExtensionElement>();
                            foreach (ExtensionElement extensionElement in otherElementList)
                            {
                                elementList.Add(extensionElement.Clone() as ExtensionElement);
                            }
                            extensionElements[key] = elementList;
                        }
                    }
                }

                attributes = new Dictionary<string, IList<ExtensionAttribute>>();
                if (value.Attributes is object && value.Attributes.Count > 0)
                {
                    foreach (string key in value.Attributes.Keys)
                    {
                        IList<ExtensionAttribute> otherAttributeList = value.Attributes[key];
                        if (otherAttributeList is object && otherAttributeList.Count > 0)
                        {
                            IList<ExtensionAttribute> attributeList = new List<ExtensionAttribute>();
                            foreach (ExtensionAttribute extensionAttribute in otherAttributeList)
                            {
                                attributeList.Add(extensionAttribute.Clone());
                            }
                            attributes[key] = attributeList;
                        }
                    }
                }
            }
        }

        public abstract BaseElement Clone();

        public static bool operator ==(BaseElement objA, BaseElement objB)
        {
            if (objA is null && objB is null)
            {
                return true;
            }

            if ((objA is null && objB is not null) || (objA is not null && objB is null))
            {
                return false;
            }

            return objA.Equals(objB);
        }

        public static bool operator !=(BaseElement objA, BaseElement objB)
        {
            return !(objA == objB);
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseElement == false || obj is null)
            {
                return false;
            }

            return string.Equals(Id, ((BaseElement)obj).Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() * 2 + this.GetType().GetHashCode();
        }
    }

}