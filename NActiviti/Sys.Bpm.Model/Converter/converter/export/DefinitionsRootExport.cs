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

    public class DefinitionsRootExport : IBpmnXMLConstants
    {

        /// <summary>
        /// default namespaces for definitions </summary>
        protected internal static readonly IList<string> defaultNamespaces = new List<string>
        (new string[]{
            BpmnXMLConstants.XSI_PREFIX,
            BpmnXMLConstants.XSD_PREFIX,
            BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX,
            BpmnXMLConstants.BPMNDI_PREFIX,
            BpmnXMLConstants.OMGDC_PREFIX,
            BpmnXMLConstants.OMGDI_PREFIX
        });

        protected internal static readonly IList<ExtensionAttribute> defaultAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.TYPE_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(BpmnXMLConstants.EXPRESSION_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE)
        };
        public static void writeRootElement(BpmnModel model, XMLStreamWriter xtw, string encoding)
        {
            xtw.writeStartDocument(encoding, "1.0");

            // start definitions root element
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_DEFINITIONS);
            xtw.DefaultNamespace = BpmnXMLConstants.BPMN2_NAMESPACE;
            xtw.writeDefaultNamespace(BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.XSI_PREFIX, BpmnXMLConstants.XSI_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.XSD_PREFIX, BpmnXMLConstants.SCHEMA_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.BPMNDI_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.OMGDC_PREFIX, BpmnXMLConstants.OMGDC_NAMESPACE);
            xtw.writeNamespace(BpmnXMLConstants.OMGDI_PREFIX, BpmnXMLConstants.OMGDI_NAMESPACE);
            foreach (string prefix in model.Namespaces.Keys)
            {
                if (!defaultNamespaces.Contains(prefix) && !string.IsNullOrWhiteSpace(prefix))
                {
                    xtw.writeNamespace(prefix, model.Namespaces[prefix]);
                }
            }
            xtw.writeAttribute(BpmnXMLConstants.TYPE_LANGUAGE_ATTRIBUTE, BpmnXMLConstants.SCHEMA_NAMESPACE);
            xtw.writeAttribute(BpmnXMLConstants.EXPRESSION_LANGUAGE_ATTRIBUTE, BpmnXMLConstants.XPATH_NAMESPACE);
            if (!string.IsNullOrWhiteSpace(model.TargetNamespace))
            {
                xtw.writeAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE, model.TargetNamespace);
            }
            else
            {
                xtw.writeAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE, BpmnXMLConstants.PROCESS_NAMESPACE);
            }

            BpmnXMLUtil.writeCustomAttributes(model.DefinitionsAttributes.Values, xtw, model.Namespaces, defaultAttributes);
        }
    }

}