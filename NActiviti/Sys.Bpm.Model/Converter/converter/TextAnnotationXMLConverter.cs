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
namespace org.activiti.bpmn.converter
{
    using org.activiti.bpmn.converter.child;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class TextAnnotationXMLConverter : BaseBpmnXMLConverter
    {

        protected internal IDictionary<string, BaseChildElementParser> childParserMap = new Dictionary<string, BaseChildElementParser>();

        public TextAnnotationXMLConverter()
        {
            TextAnnotationTextParser annotationTextParser = new TextAnnotationTextParser();
            childParserMap[annotationTextParser.ElementName] = annotationTextParser;
        }

        public override Type BpmnElementType
        {
            get
            {
                return typeof(TextAnnotation);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TEXT_ANNOTATION;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            TextAnnotation textAnnotation = new TextAnnotation();
            BpmnXMLUtil.addXMLLocation(textAnnotation, xtr);
            textAnnotation.TextFormat = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TEXTFORMAT);
            parseChildElements(XMLElementName, textAnnotation, childParserMap, model, xtr);
            return textAnnotation;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            TextAnnotation textAnnotation = (TextAnnotation)element;
            writeDefaultAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TEXTFORMAT, textAnnotation.TextFormat, xtw);
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            TextAnnotation textAnnotation = (TextAnnotation)element;
            if (!string.IsNullOrWhiteSpace(textAnnotation.Text))
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TEXT_ANNOTATION_TEXT);
                xtw.writeCharacters(textAnnotation.Text);
                xtw.writeEndElement();
            }
        }
    }

}