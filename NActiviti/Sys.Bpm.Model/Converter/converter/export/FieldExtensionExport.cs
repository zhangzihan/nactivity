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
namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    public class FieldExtensionExport : IBpmnXMLConstants
    {
        public static bool writeFieldExtensions(IList<FieldExtension> fieldExtensionList, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {

            foreach (FieldExtension fieldExtension in fieldExtensionList)
            {

                if (!string.IsNullOrWhiteSpace(fieldExtension.FieldName))
                {

                    if (!string.IsNullOrWhiteSpace(fieldExtension.StringValue) || !string.IsNullOrWhiteSpace(fieldExtension.Expression))
                    {

                        if (!didWriteExtensionStartElement)
                        {
                            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EXTENSIONS);
                            didWriteExtensionStartElement = true;
                        }

                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                        BpmnXMLUtil.writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_NAME, fieldExtension.FieldName, xtw);

                        if (!string.IsNullOrWhiteSpace(fieldExtension.StringValue))
                        {
                            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD_STRING, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                            xtw.writeCData(fieldExtension.StringValue);
                        }
                        else
                        {
                            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                            xtw.writeCData(fieldExtension.Expression);
                        }
                        xtw.writeEndElement();
                        xtw.writeEndElement();
                    }
                }
            }
            return didWriteExtensionStartElement;
        }
    }

}