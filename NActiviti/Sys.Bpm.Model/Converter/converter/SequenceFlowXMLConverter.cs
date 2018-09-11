using System;
using org.activiti.bpmn.constants;
using org.activiti.bpmn.converter.util;
using org.activiti.bpmn.model;

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
{    /// 
    public class SequenceFlowXMLConverter : BaseBpmnXMLConverter
    {
        public override Type BpmnElementType
        {
            get
            {
                return typeof(SequenceFlow);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_SEQUENCE_FLOW;
            }
        }

        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            SequenceFlow sequenceFlow = new SequenceFlow();
            BpmnXMLUtil.addXMLLocation(sequenceFlow, xtr);
            sequenceFlow.SourceRef = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF);
            sequenceFlow.TargetRef = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF);
            sequenceFlow.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
            sequenceFlow.SkipExpression = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_FLOW_SKIP_EXPRESSION);

            parseChildElements(XMLElementName, sequenceFlow, model, xtr);

            return sequenceFlow;
        }

        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            SequenceFlow sequenceFlow = (SequenceFlow)element;
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_SOURCE_REF, sequenceFlow.SourceRef, xtw);
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_TARGET_REF, sequenceFlow.TargetRef, xtw);
            if (!string.IsNullOrWhiteSpace(sequenceFlow.SkipExpression))
            {
                writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FLOW_SKIP_EXPRESSION, sequenceFlow.SkipExpression, xtw);
            }
        }

        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            SequenceFlow sequenceFlow = (SequenceFlow)element;

            if (!string.IsNullOrWhiteSpace(sequenceFlow.ConditionExpression))
            {
                xtw.writeStartElement(BpmnXMLConstants.ELEMENT_FLOW_CONDITION);
                xtw.writeAttribute(BpmnXMLConstants.XSI_PREFIX, BpmnXMLConstants.XSI_NAMESPACE, "type", "tFormalExpression");
                xtw.writeCData(sequenceFlow.ConditionExpression);
                xtw.writeEndElement();
            }
        }
    }

}