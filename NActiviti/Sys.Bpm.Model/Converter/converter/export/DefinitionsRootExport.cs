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
namespace Sys.Workflow.Bpmn.Converters.Exports
{

    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    public class DefinitionsRootExport : IBpmnXMLConstants
    {

        /// <summary>
        /// default namespaces for definitions </summary>
        protected internal static readonly IList<string> defaultNamespaces = new List<string>
        (new string[]{
            BpmnXMLConstants.BPMN_PREFIX,
            BpmnXMLConstants.XSI_PREFIX,
            BpmnXMLConstants.XSD_PREFIX,
            BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX,
            BpmnXMLConstants.BPMNDI_PREFIX,
            BpmnXMLConstants.OMGDC_PREFIX,
            BpmnXMLConstants.OMGDI_PREFIX
        });

        protected internal static readonly List<ExtensionAttribute> defaultAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.TYPE_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(BpmnXMLConstants.EXPRESSION_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE)
        };

        public static void WriteRootElement(BpmnModel model, XMLStreamWriter xtw, string encoding)
        {
            xtw.WriteStartDocument(encoding, "1.0");

            // start definitions root element
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DEFINITIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.DefaultNamespace = BpmnXMLConstants.BPMN2_NAMESPACE;
            xtw.WriteDefaultNamespace(BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.XSI_PREFIX, BpmnXMLConstants.XSI_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.XSD_PREFIX, BpmnXMLConstants.SCHEMA_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.BPMNDI_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.OMGDC_PREFIX, BpmnXMLConstants.OMGDC_NAMESPACE);
            xtw.WriteNamespace(BpmnXMLConstants.OMGDI_PREFIX, BpmnXMLConstants.OMGDI_NAMESPACE);
            foreach (string prefix in model.Namespaces.Keys)
            {
                if (!defaultNamespaces.Contains(prefix) && !string.IsNullOrWhiteSpace(prefix))
                {
                    xtw.WriteNamespace(prefix, model.Namespaces[prefix]);
                }
            }
            xtw.WriteAttribute(BpmnXMLConstants.TYPE_LANGUAGE_ATTRIBUTE, BpmnXMLConstants.SCHEMA_NAMESPACE);
            xtw.WriteAttribute(BpmnXMLConstants.EXPRESSION_LANGUAGE_ATTRIBUTE, BpmnXMLConstants.XPATH_NAMESPACE);
            if (!string.IsNullOrWhiteSpace(model.TargetNamespace))
            {
                xtw.WriteAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE, model.TargetNamespace);
            }
            else
            {
                xtw.WriteAttribute(BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE, BpmnXMLConstants.PROCESS_NAMESPACE);
            }

            //BpmnXMLUtil.WriteCustomAttributes(model.DefinitionsAttributes.Values, xtw, model.Namespaces, defaultAttributes);
        }
    }

}