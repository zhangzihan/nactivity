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
namespace org.activiti.bpmn.converter
{

    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

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
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_ASSOCIATION;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            Association association = new Association();
            BpmnXMLUtil.addXMLLocation(association, xtr);
            association.SourceRef = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF);
            association.TargetRef = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF);
            association.Id = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID);

            string asociationDirectionString = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ASSOCIATION_DIRECTION);
            if (!string.IsNullOrWhiteSpace(asociationDirectionString))
            {
                AssociationDirection associationDirection = AssociationDirection.valueOf(asociationDirectionString.ToUpper());

                association.AssociationDirection = associationDirection;
            }

            parseChildElements(XMLElementName, association, model, xtr);

            return association;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            Association association = (Association)element;
            writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF, association.SourceRef, xtw);
            writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF, association.TargetRef, xtw);
            AssociationDirection associationDirection = association.AssociationDirection;
            if (associationDirection != null)
            {
                writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ASSOCIATION_DIRECTION, associationDirection.Value, xtw);
            }
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
    }

}