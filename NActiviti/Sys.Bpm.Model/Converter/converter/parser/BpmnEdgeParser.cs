using System;
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
namespace org.activiti.bpmn.converter.parser
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class BpmnEdgeParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {

            string id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT);
            IList<GraphicInfo> wayPointList = new List<GraphicInfo>();
            while (xtr.HasNext())
            {
                //xtr.next();

                if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DI_LABEL.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    while (xtr.HasNext())
                    {
                        //xtr.next();

                        if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DI_BOUNDS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            GraphicInfo graphicInfo = new GraphicInfo();
                            BpmnXMLUtil.AddXMLLocation(graphicInfo, xtr);
                            graphicInfo.X = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_X));
                            graphicInfo.Y = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_Y));
                            graphicInfo.Width = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_WIDTH));
                            graphicInfo.Height = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT));
                            model.AddLabelGraphicInfo(id, graphicInfo);
                            break;
                        }
                        else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_DI_LABEL.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            break;
                        }

                        if (xtr.IsEmptyElement && BpmnXMLConstants.ELEMENT_DI_LABEL.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            break;
                        }
                    }

                }
                else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DI_WAYPOINT.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    GraphicInfo graphicInfo = new GraphicInfo();
                    BpmnXMLUtil.AddXMLLocation(graphicInfo, xtr);
                    graphicInfo.X = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_X));
                    graphicInfo.Y = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_Y));
                    wayPointList.Add(graphicInfo);

                }
                else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_DI_EDGE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }

                if (xtr.IsEmptyElement && BpmnXMLConstants.ELEMENT_DI_EDGE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
            model.AddFlowGraphicInfoList(id, wayPointList);
        }

        public virtual BaseElement ParseElement()
        {
            return null;
        }
    }

}