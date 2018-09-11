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

    public class SignalEventDefinitionParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION;
            }
        }
        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is Event))
            {
                return;
            }

            SignalEventDefinition eventDefinition = new SignalEventDefinition();
            BpmnXMLUtil.addXMLLocation(eventDefinition, xtr);
            eventDefinition.SignalRef = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_SIGNAL_REF);
            eventDefinition.SignalExpression = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_SIGNAL_EXPRESSION);
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS)))
            {
                eventDefinition.Async = bool.Parse(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS));
            }

            BpmnXMLUtil.parseChildElements(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION, eventDefinition, xtr, model);

            ((Event)parentElement).EventDefinitions.Add(eventDefinition);
        }
    }

}