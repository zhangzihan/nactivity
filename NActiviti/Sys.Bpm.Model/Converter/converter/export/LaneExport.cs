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
        public static void WriteLanes(Process process, XMLStreamWriter xtw)
        {
            if (process.Lanes.Count > 0)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_LANESET, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "laneSet_" + process.Id);
                foreach (Lane lane in process.Lanes)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_LANE, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, lane.Id);

                    if (!string.IsNullOrWhiteSpace(lane.Name))
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, lane.Name);
                    }

                    bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(lane, false, xtw);
                    if (didWriteExtensionStartElement)
                    {
                        xtw.WriteEndElement();
                    }

                    foreach (string flowNodeRef in lane.FlowReferences)
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_FLOWNODE_REF, BpmnXMLConstants.BPMN2_NAMESPACE);
                        xtw.WriteCharacters(flowNodeRef);
                        xtw.WriteEndElement();
                    }

                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();
            }
        }
    }
}