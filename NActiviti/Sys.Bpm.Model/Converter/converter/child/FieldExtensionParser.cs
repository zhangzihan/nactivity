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
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using Sys.Bpm;

    /// 
    public class FieldExtensionParser : BaseChildElementParser
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<FieldExtensionParser>();

        public override string ElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD;
            }
        }

        public override bool accepts(BaseElement element)
        {
            return ((element is ActivitiListener) || (element is ServiceTask) || (element is SendTask));
        }
        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {

            if (!accepts(parentElement))
            {
                return;
            }

            FieldExtension extension = new FieldExtension();
            BpmnXMLUtil.addXMLLocation(extension, xtr);
            extension.FieldName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_NAME);

            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_STRING)))
            {
                extension.StringValue = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_STRING);

            }
            else if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION)))
            {
                extension.Expression = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION);

            }
            else
            {
                bool readyWithFieldExtension = false;
                try
                {
                    while (readyWithFieldExtension == false && xtr.hasNext())
                    {
                        //xtr.next();

                        if (xtr.IsStartElement() && org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD_STRING.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            extension.StringValue = xtr.ElementText.Trim();

                        }
                        else if (xtr.IsStartElement() && org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            extension.Expression = xtr.ElementText.Trim();

                        }
                        else if (xtr.EndElement && ElementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            readyWithFieldExtension = true;
                        }

                        if (xtr.IsEmptyElement && ElementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                        {
                            readyWithFieldExtension = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogWarning(e, "Error parsing field extension child elements");
                }
            }

            if (parentElement is ActivitiListener)
            {
                ((ActivitiListener)parentElement).FieldExtensions.Add(extension);
            }
            else if (parentElement is ServiceTask)
            {
                ((ServiceTask)parentElement).FieldExtensions.Add(extension);
            }
            else
            {
                ((SendTask)parentElement).FieldExtensions.Add(extension);
            }
        }
    }

}