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
namespace Sys.Workflow.bpmn.converter.parser
{

    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    /// 
    public class BpmnShapeParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            string id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_BPMNELEMENT);
            GraphicInfo graphicInfo = new GraphicInfo();

            string strIsExpanded = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_IS_EXPANDED);
            if ("true".Equals(strIsExpanded, StringComparison.CurrentCultureIgnoreCase))
            {
                graphicInfo.Expanded = true;
            }

            BpmnXMLUtil.AddXMLLocation(graphicInfo, xtr);
            while (xtr.HasNext())
            {
                //xtr.next();

                if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DI_BOUNDS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    graphicInfo.X = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_X));
                    graphicInfo.Y = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_Y));
                    graphicInfo.Width = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_WIDTH));
                    graphicInfo.Height = Convert.ToDouble(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DI_HEIGHT));

                    model.AddGraphicInfo(id, graphicInfo);
                    break;
                }
                else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }

                if (xtr.IsEmptyElement && BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
        }

        public virtual BaseElement ParseElement()
        {
            return null;
        }
    }

}