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
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    public class FormPropertyParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_FORMPROPERTY;
            }
        }

        public override bool accepts(BaseElement element)
        {
            return ((element is UserTask) || (element is StartEvent));
        }

        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!accepts(parentElement))
            {
                return;
            }
            FormProperty property = new FormProperty();
            BpmnXMLUtil.addXMLLocation(property, xtr);
            property.Id = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_ID);
            property.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_NAME);
            property.Type = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_TYPE);
            property.Variable = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_VARIABLE);
            property.Expression = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_EXPRESSION);
            property.DefaultExpression = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_DEFAULT);
            property.DatePattern = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_DATEPATTERN);
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED)))
            {
                property.Required = Convert.ToBoolean(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED));
            }
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE)))
            {
                property.Readable = Convert.ToBoolean(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE));
            }
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE)))
            {
                property.Writeable = Convert.ToBoolean(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE));
            }

            bool readyWithFormProperty = false;
            try
            {
                while (!readyWithFormProperty && xtr.hasNext())
                {
                    //xtr.next();

                    if (xtr.StartElement && BpmnXMLConstants.ELEMENT_VALUE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormValue value = new FormValue();
                        BpmnXMLUtil.addXMLLocation(value, xtr);
                        value.Id = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        value.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                        property.FormValues.Add(value);
                    }
                    else if (xtr.EndElement && ElementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        readyWithFormProperty = true;
                    }

                    if (xtr.IsEmptyElement && ElementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        readyWithFormProperty = true;
                    }
                } 
            }
            catch (Exception e)
            {
                throw e;
                //LOGGER.warn("Error parsing form properties child elements", e);
            }

            if (parentElement is UserTask)
            {
                ((UserTask)parentElement).FormProperties.Add(property);
            }
            else
            {
                ((StartEvent)parentElement).FormProperties.Add(property);
            }
        }
    }

}