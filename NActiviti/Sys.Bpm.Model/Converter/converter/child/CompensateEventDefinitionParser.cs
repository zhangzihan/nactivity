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
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class CompensateEventDefinitionParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_EVENT_COMPENSATEDEFINITION;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (parentElement is not Event)
            {
                return;
            }

            CompensateEventDefinition eventDefinition = new CompensateEventDefinition();
            BpmnXMLUtil.AddXMLLocation(eventDefinition, xtr);
            eventDefinition.ActivityRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_COMPENSATE_ACTIVITYREF);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_COMPENSATE_WAITFORCOMPLETION)))
            {
                eventDefinition.WaitForCompletion = bool.Parse(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_COMPENSATE_WAITFORCOMPLETION));
            }

            BpmnXMLUtil.ParseChildElements(BpmnXMLConstants.ELEMENT_EVENT_COMPENSATEDEFINITION, eventDefinition, xtr, model);

            ((Event)parentElement).EventDefinitions.Add(eventDefinition);
        }
    }

}