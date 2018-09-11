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

    /// 
    public class DataStoreReferenceXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(DataStoreReference);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DATA_STORE_REFERENCE;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            DataStoreReference dataStoreRef = new DataStoreReference();
            BpmnXMLUtil.addXMLLocation(dataStoreRef, xtr);
            parseChildElements(XMLElementName, dataStoreRef, model, xtr);
            return dataStoreRef;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            DataStoreReference dataStoreRef = (DataStoreReference)element;
            if (!string.IsNullOrWhiteSpace(dataStoreRef.DataStoreRef))
            {
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_DATA_STORE_REF, dataStoreRef.DataStoreRef);
            }

            if (!string.IsNullOrWhiteSpace(dataStoreRef.ItemSubjectRef))
            {
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF, dataStoreRef.ItemSubjectRef);
            }
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            DataStoreReference dataStoreRef = (DataStoreReference)element;
            if (!string.IsNullOrWhiteSpace(dataStoreRef.DataState))
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DATA_STATE);
                xtw.writeCharacters(dataStoreRef.DataState);
                xtw.writeEndElement();
            }
        }
    }

}