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
namespace Sys.Workflow.Bpmn.Converters.Childs
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Model;

    public class FormPropertyParser : BaseChildElementParser
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<FormPropertyParser>();

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_FORMPROPERTY;
            }
        }

        public override bool Accepts(BaseElement element)
        {
            return ((element is UserTask) || (element is StartEvent));
        }

        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!Accepts(parentElement))
            {
                return;
            }
            FormProperty property = new FormProperty();
            BpmnXMLUtil.AddXMLLocation(property, xtr);
            property.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_ID);
            property.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_NAME);
            property.Type = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_TYPE);
            property.Variable = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_VARIABLE);
            property.Expression = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_EXPRESSION);
            property.DefaultExpression = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_DEFAULT);
            property.DatePattern = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_DATEPATTERN);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED)))
            {
                property.Required = Convert.ToBoolean(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED));
            }
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE)))
            {
                property.Readable = Convert.ToBoolean(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE));
            }
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE)))
            {
                property.Writeable = Convert.ToBoolean(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE));
            }

            bool readyWithFormProperty = false;
            try
            {
                while (!readyWithFormProperty && xtr.HasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_VALUE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormValue value = new FormValue();
                        BpmnXMLUtil.AddXMLLocation(value, xtr);
                        value.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        value.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
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
                log.LogWarning(e, "Error parsing form properties child elements");
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