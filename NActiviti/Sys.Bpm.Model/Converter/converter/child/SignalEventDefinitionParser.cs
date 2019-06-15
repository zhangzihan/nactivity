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

    public class SignalEventDefinitionParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is Event))
            {
                return;
            }

            SignalEventDefinition eventDefinition = new SignalEventDefinition();
            BpmnXMLUtil.AddXMLLocation(eventDefinition, xtr);
            eventDefinition.SignalRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_SIGNAL_REF);
            eventDefinition.SignalExpression = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_SIGNAL_EXPRESSION);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS)))
            {
                eventDefinition.Async = bool.Parse(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS));
            }

            BpmnXMLUtil.ParseChildElements(BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION, eventDefinition, xtr, model);

            ((Event)parentElement).EventDefinitions.Add(eventDefinition);
        }
    }

}