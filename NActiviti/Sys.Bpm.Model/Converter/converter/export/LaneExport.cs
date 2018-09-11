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

    public class LaneExport : IBpmnXMLConstants
    {
        public static void writeLanes(Process process, XMLStreamWriter xtw)
        {
            if (process.Lanes.Count > 0)
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_LANESET);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "laneSet_" + process.Id);
                foreach (Lane lane in process.Lanes)
                {
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_LANE);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, lane.Id);

                    if (!string.IsNullOrWhiteSpace(lane.Name))
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, lane.Name);
                    }

                    bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(lane, false, xtw);
                    if (didWriteExtensionStartElement)
                    {
                        xtw.writeEndElement();
                    }

                    foreach (string flowNodeRef in lane.FlowReferences)
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FLOWNODE_REF);
                        xtw.writeCharacters(flowNodeRef);
                        xtw.writeEndElement();
                    }

                    xtw.writeEndElement();
                }
                xtw.writeEndElement();
            }
        }
    }

}