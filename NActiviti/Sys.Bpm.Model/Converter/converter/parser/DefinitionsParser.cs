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
namespace org.activiti.bpmn.converter.parser
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using System.Linq;

    /// 
    public class DefinitionsParser : IBpmnXMLConstants
    {
        protected internal static readonly IList<ExtensionAttribute> defaultAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.TYPE_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.EXPRESSION_LANGUAGE_ATTRIBUTE),
            new ExtensionAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE)
        };
        public virtual void parse(XMLStreamReader xtr, BpmnModel model)
        {
            model.TargetNamespace = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE);
            for (int i = 0; i < xtr.NamespaceCount; i++)
            {
                string prefix = xtr.getNamespacePrefix(i);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    model.addNamespace(prefix, xtr.getNamespaceURI(i));
                }
            }

            for (int i = 0; i < xtr.AttributeCount; i++)
            {
                var attr = xtr.element.Attributes().ElementAt(i);
                ExtensionAttribute extensionAttribute = new ExtensionAttribute();
                extensionAttribute.Name = attr.Name.LocalName;
                extensionAttribute.Value = attr.Value;
                if (!string.IsNullOrWhiteSpace(attr.Name.NamespaceName))
                {
                    extensionAttribute.Namespace = attr.Name.NamespaceName;
                }
                if (!string.IsNullOrWhiteSpace(xtr.element.GetPrefixOfNamespace(attr.Name.Namespace)))
                {
                    extensionAttribute.NamespacePrefix = xtr.element.GetPrefixOfNamespace(attr.Name.Namespace);
                }
                if (!BpmnXMLUtil.isBlacklisted(extensionAttribute, defaultAttributes))
                {
                    model.addDefinitionsAttribute(extensionAttribute);
                }
            }
        }
    }

}