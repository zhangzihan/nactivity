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
    using org.activiti.bpmn.model;

    /// 
    public class MessageFlowParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            string id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            if (!string.IsNullOrWhiteSpace(id))
            {
                MessageFlow messageFlow = new MessageFlow
                {
                    Id = id
                };

                string name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    messageFlow.Name = name;
                }

                string sourceRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF);
                if (!string.IsNullOrWhiteSpace(sourceRef))
                {
                    messageFlow.SourceRef = sourceRef;
                }

                string targetRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF);
                if (!string.IsNullOrWhiteSpace(targetRef))
                {
                    messageFlow.TargetRef = targetRef;
                }

                string messageRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_MESSAGE_REF);
                if (!string.IsNullOrWhiteSpace(messageRef))
                {
                    messageFlow.MessageRef = messageRef;
                }

                model.AddMessageFlow(messageFlow);
            }
        }
    }

}