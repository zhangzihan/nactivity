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
    using org.activiti.bpmn.model;

    public class CollaborationExport : IBpmnXMLConstants
    {
        public static void writePools(BpmnModel model, XMLStreamWriter xtw)
        {
            if (model.Pools?.Count > 0)
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_COLLABORATION);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "Collaboration");
                foreach (Pool pool in model.Pools)
                {
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_PARTICIPANT);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, pool.Id);
                    if (!string.IsNullOrWhiteSpace(pool.Name))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, pool.Name);
                    }
                    if (!string.IsNullOrWhiteSpace(pool.ProcessRef))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_PROCESS_REF, pool.ProcessRef);
                    }
                    xtw.writeEndElement();
                }

                foreach (MessageFlow messageFlow in model.MessageFlows.Values)
                {
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MESSAGE_FLOW);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, messageFlow.Id);
                    if (!string.IsNullOrWhiteSpace(messageFlow.Name))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, messageFlow.Name);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.SourceRef))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF, messageFlow.SourceRef);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.TargetRef))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF, messageFlow.TargetRef);
                    }
                    if (!string.IsNullOrWhiteSpace(messageFlow.MessageRef))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_MESSAGE_REF, messageFlow.MessageRef);
                    }
                    xtw.writeEndElement();
                }

                xtw.writeEndElement();
            }
        }
    }

}