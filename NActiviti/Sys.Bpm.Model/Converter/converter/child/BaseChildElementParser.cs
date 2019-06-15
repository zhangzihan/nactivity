using System;

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
namespace org.activiti.bpmn.converter.child
{
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    /// 
    public abstract class BaseChildElementParser : IBpmnXMLConstants
    {
        public abstract string ElementName { get; }

        public abstract void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model);

        protected internal virtual void ParseChildElements(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model, BaseChildElementParser parser)
        {
            bool readyWithChildElements = false;
            while (!readyWithChildElements && xtr.HasNext())
            {
                //xtr.next();
                if (xtr.IsStartElement())
                {
                    if (parser.ElementName.Equals(xtr.LocalName))
                    {
                        parser.ParseChildElement(xtr, parentElement, model);
                    }

                }
                else if (xtr.EndElement && ElementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    readyWithChildElements = true;
                }

                if (xtr.IsEmptyElement && ElementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    readyWithChildElements = true;
                }
            } 
        }

        public virtual bool Accepts(BaseElement element)
        {
            return element != null;
        }
    }

}