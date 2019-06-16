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
namespace Sys.Workflow.bpmn.converter.export
{

    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.model;

    public class BPMNDIExport : IBpmnXMLConstants
    {
        public static void WriteBPMNDI(BpmnModel model, XMLStreamWriter xtw)
        {
            // BPMN DI information
            xtw.WriteStartElement(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_DIAGRAM, BpmnXMLConstants.BPMNDI_NAMESPACE);

            string processId;
            if ((model.Pools?.Count).GetValueOrDefault() > 0)
            {
                processId = "Collaboration";
            }
            else
            {
                processId = model.MainProcess.Id;
            }

            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "BPMNDiagram_" + processId);

            xtw.WriteStartElement(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_PLANE, BpmnXMLConstants.BPMNDI_NAMESPACE);
            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, processId);
            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "BPMNPlane_" + processId);

            foreach (string elementId in model.LocationMap.Keys)
            {

                if (model.GetFlowElement(elementId) != null || model.GetArtifact(elementId) != null || model.GetPool(elementId) != null || model.GetLane(elementId) != null)
                {

                    xtw.WriteStartElement(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_SHAPE, BpmnXMLConstants.BPMNDI_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, elementId);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "BPMNShape_" + elementId);

                    GraphicInfo graphicInfo = model.GetGraphicInfo(elementId);
                    FlowElement flowElement = model.GetFlowElement(elementId);
                    if (flowElement is SubProcess && graphicInfo.Expanded != null)
                    {
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_IS_EXPANDED, graphicInfo.Expanded.ToString());
                    }

                    xtw.WriteStartElement(BpmnXMLConstants.OMGDC_PREFIX, BpmnXMLConstants.ELEMENT_DI_BOUNDS, BpmnXMLConstants.OMGDC_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT, "" + graphicInfo.Height);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_WIDTH, "" + graphicInfo.Width);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_X, "" + graphicInfo.X);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + graphicInfo.Y);
                    xtw.WriteEndElement();

                    xtw.WriteEndElement();
                }
            }

            foreach (string elementId in model.FlowLocationMap.Keys)
            {

                if (model.GetFlowElement(elementId) != null || model.GetArtifact(elementId) != null || model.GetMessageFlow(elementId) != null)
                {

                    xtw.WriteStartElement(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_EDGE, BpmnXMLConstants.BPMNDI_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT, elementId);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, "BPMNEdge_" + elementId);

                    IList<GraphicInfo> graphicInfoList = model.GetFlowLocationGraphicInfo(elementId) ?? new List<GraphicInfo>();
                    foreach (GraphicInfo graphicInfo in graphicInfoList)
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.OMGDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_WAYPOINT, BpmnXMLConstants.OMGDI_NAMESPACE);
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_X, "" + graphicInfo.X);
                        xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + graphicInfo.Y);
                        xtw.WriteEndElement();
                    }

                    GraphicInfo labelGraphicInfo = model.GetLabelGraphicInfo(elementId);
                    if (labelGraphicInfo != null)
                    {
                        FlowElement flowElement = model.GetFlowElement(elementId);
                        MessageFlow messageFlow = null;
                        if (flowElement == null)
                        {
                            messageFlow = model.GetMessageFlow(elementId);
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
                            xtw.WriteStartElement(BpmnXMLConstants.BPMNDI_PREFIX, BpmnXMLConstants.ELEMENT_DI_LABEL, BpmnXMLConstants.BPMNDI_NAMESPACE);
                            xtw.WriteStartElement(BpmnXMLConstants.OMGDC_PREFIX, BpmnXMLConstants.ELEMENT_DI_BOUNDS, BpmnXMLConstants.OMGDC_NAMESPACE);
                            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT, "" + labelGraphicInfo.Height);
                            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_WIDTH, "" + labelGraphicInfo.Width);
                            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_X, "" + labelGraphicInfo.X);
                            xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DI_Y, "" + labelGraphicInfo.Y);
                            xtw.WriteEndElement();
                            xtw.WriteEndElement();
                        }
                    }

                    xtw.WriteEndElement();
                }
            }

            // end BPMN DI elements
            xtw.WriteEndElement();
            xtw.WriteEndElement();
        }
    }
}