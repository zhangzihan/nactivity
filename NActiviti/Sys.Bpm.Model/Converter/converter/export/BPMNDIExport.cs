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
namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    public class BPMNDIExport : IBpmnXMLConstants
    {
        public static void writeBPMNDI(BpmnModel model, XMLStreamWriter xtw)
        {
            // BPMN DI information
            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_DIAGRAM, org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_NAMESPACE);

            string processId = null;
            if (model.Pools?.Count > 0)
            {
                processId = "Collaboration";
            }
            else
            {
                processId = model.MainProcess.Id;
            }

            xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "BPMNDiagram_" + processId);

            xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_PLANE, org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_NAMESPACE);
            xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, processId);
            xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "BPMNPlane_" + processId);

            foreach (string elementId in model.LocationMap.Keys)
            {

                if (model.getFlowElement(elementId) != null || model.getArtifact(elementId) != null || model.getPool(elementId) != null || model.getLane(elementId) != null)
                {

                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_SHAPE, org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_NAMESPACE);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, elementId);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "BPMNShape_" + elementId);

                    GraphicInfo graphicInfo = model.getGraphicInfo(elementId);
                    FlowElement flowElement = model.getFlowElement(elementId);
                    if (flowElement is SubProcess && graphicInfo.Expanded != null)
                    {
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_IS_EXPANDED, graphicInfo.Expanded.ToString());
                    }

                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.OMGDC_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_BOUNDS, org.activiti.bpmn.constants.BpmnXMLConstants.OMGDC_NAMESPACE);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT, "" + graphicInfo.Height);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_WIDTH, "" + graphicInfo.Width);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_X, "" + graphicInfo.X);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + graphicInfo.Y);
                    xtw.writeEndElement();

                    xtw.writeEndElement();
                }
            }

            foreach (string elementId in model.FlowLocationMap.Keys)
            {

                if (model.getFlowElement(elementId) != null || model.getArtifact(elementId) != null || model.getMessageFlow(elementId) != null)
                {

                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_EDGE, org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_NAMESPACE);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, elementId);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, "BPMNEdge_" + elementId);

                    IList<GraphicInfo> graphicInfoList = model.getFlowLocationGraphicInfo(elementId);
                    foreach (GraphicInfo graphicInfo in graphicInfoList)
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.OMGDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_WAYPOINT, org.activiti.bpmn.constants.BpmnXMLConstants.OMGDI_NAMESPACE);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_X, "" + graphicInfo.X);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + graphicInfo.Y);
                        xtw.writeEndElement();
                    }

                    GraphicInfo labelGraphicInfo = model.getLabelGraphicInfo(elementId);
                    FlowElement flowElement = model.getFlowElement(elementId);
                    MessageFlow messageFlow = null;
                    if (flowElement == null)
                    {
                        messageFlow = model.getMessageFlow(elementId);
                    }

                    bool hasName = false;
                    if (flowElement != null && !string.IsNullOrWhiteSpace(flowElement.Name))
                    {
                        hasName = true;

                    }
                    else if (messageFlow != null && !string.IsNullOrWhiteSpace(messageFlow.Name))
                    {
                        hasName = true;
                    }

                    if (labelGraphicInfo != null && hasName)
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_LABEL, org.activiti.bpmn.constants.BpmnXMLConstants.BPMNDI_NAMESPACE);
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.OMGDC_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_BOUNDS, org.activiti.bpmn.constants.BpmnXMLConstants.OMGDC_NAMESPACE);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT, "" + labelGraphicInfo.Height);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_WIDTH, "" + labelGraphicInfo.Width);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_X, "" + labelGraphicInfo.X);
                        xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + labelGraphicInfo.Y);
                        xtw.writeEndElement();
                        xtw.writeEndElement();
                    }

                    xtw.writeEndElement();
                }
            }

            // end BPMN DI elements
            xtw.writeEndElement();
            xtw.writeEndElement();
        }
    }
}