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

    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    /// 
    public class TerminateEventDefinitionParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EVENT_TERMINATEDEFINITION;
            }
        }
        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is EndEvent))
            {
                return;
            }

            TerminateEventDefinition eventDefinition = new TerminateEventDefinition();

            parseTerminateAllAttribute(xtr, eventDefinition);
            parseTerminateMultiInstanceAttribute(xtr, eventDefinition);

            BpmnXMLUtil.addXMLLocation(eventDefinition, xtr);
            BpmnXMLUtil.parseChildElements(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EVENT_TERMINATEDEFINITION, eventDefinition, xtr, model);

            ((Event)parentElement).EventDefinitions.Add(eventDefinition);
        }

        protected internal virtual void parseTerminateAllAttribute(XMLStreamReader xtr, TerminateEventDefinition eventDefinition)
        {
            string terminateAllValue = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TERMINATE_ALL);
            if (!string.ReferenceEquals(terminateAllValue, null) && "true".Equals(terminateAllValue))
            {
                eventDefinition.TerminateAll = true;
            }
            else
            {
                eventDefinition.TerminateAll = false;
            }
        }

        protected internal virtual void parseTerminateMultiInstanceAttribute(XMLStreamReader xtr, TerminateEventDefinition eventDefinition)
        {
            string terminateMiValue = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TERMINATE_MULTI_INSTANCE);
            if (!string.ReferenceEquals(terminateMiValue, null) && "true".Equals(terminateMiValue))
            {
                eventDefinition.TerminateMultiInstance = true;
            }
            else
            {
                eventDefinition.TerminateMultiInstance = false;
            }
        }
    }

}