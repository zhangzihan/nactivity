using System;

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
    /// 
    public class BpmnShapeParser : IBpmnXMLConstants
    {
        public virtual void parse(XMLStreamReader xtr, BpmnModel model)
        {
            string id = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT);
            GraphicInfo graphicInfo = new GraphicInfo();

            string strIsExpanded = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_IS_EXPANDED);
            if ("true".Equals(strIsExpanded, StringComparison.CurrentCultureIgnoreCase))
            {
                graphicInfo.Expanded = true;
            }

            BpmnXMLUtil.addXMLLocation(graphicInfo, xtr);
            while (xtr.hasNext())
            {
                //xtr.next();

                if (xtr.IsStartElement() && org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_BOUNDS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    graphicInfo.X = Convert.ToDouble(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_X));
                    graphicInfo.Y = Convert.ToDouble(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_Y));
                    graphicInfo.Width = Convert.ToDouble(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_WIDTH));
                    graphicInfo.Height = Convert.ToDouble(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT));

                    model.addGraphicInfo(id, graphicInfo);
                    break;
                }
                else if (xtr.EndElement && org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }

                if (xtr.IsEmptyElement && org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
        }

        public virtual BaseElement parseElement()
        {
            return null;
        }
    }

}