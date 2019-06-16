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
namespace Sys.Workflow.Bpmn.Converters
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    public class AssociationXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(Association);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_ASSOCIATION;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            Association association = new Association();
            BpmnXMLUtil.AddXMLLocation(association, xtr);
            association.SourceRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF);
            association.TargetRef = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF);
            association.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);

            string asociationDirectionString = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ASSOCIATION_DIRECTION);
            if (!string.IsNullOrWhiteSpace(asociationDirectionString))
            {
                AssociationDirection associationDirection = AssociationDirection.ValueOf(asociationDirectionString.ToUpper());

                association.AssociationDirection = associationDirection;
            }

            ParseChildElements(XMLElementName, association, model, xtr);

            return association;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            Association association = (Association)element;
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF, association.SourceRef, xtw);
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF, association.TargetRef, xtw);
            AssociationDirection associationDirection = association.AssociationDirection;
            if (associationDirection != null)
            {
                WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ASSOCIATION_DIRECTION, associationDirection.Value, xtw);
            }
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
    }

}