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
    using org.activiti.bpmn.constants;
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
                return BpmnXMLConstants.ELEMENT_DATA_STORE_REFERENCE;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            DataStoreReference dataStoreRef = new DataStoreReference();
            BpmnXMLUtil.AddXMLLocation(dataStoreRef, xtr);
            ParseChildElements(XMLElementName, dataStoreRef, model, xtr);
            return dataStoreRef;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            DataStoreReference dataStoreRef = (DataStoreReference)element;
            if (!string.IsNullOrWhiteSpace(dataStoreRef.DataStoreRef))
            {
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_DATA_STORE_REF, dataStoreRef.DataStoreRef);
            }

            if (!string.IsNullOrWhiteSpace(dataStoreRef.ItemSubjectRef))
            {
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF, dataStoreRef.ItemSubjectRef);
            }
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            DataStoreReference dataStoreRef = (DataStoreReference)element;
            if (!string.IsNullOrWhiteSpace(dataStoreRef.DataState))
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DATA_STATE, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteCharacters(dataStoreRef.DataState);
                xtw.WriteEndElement();
            }
        }
    }

}