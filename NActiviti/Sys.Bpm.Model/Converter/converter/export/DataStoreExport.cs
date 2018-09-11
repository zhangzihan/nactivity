namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    public class DataStoreExport : IBpmnXMLConstants
    {
        public static void writeDataStores(BpmnModel model, XMLStreamWriter xtw)
        {
            foreach (DataStore dataStore in model.DataStores.Values)
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DATA_STORE);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, dataStore.Id);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, dataStore.Name);
                if (!string.IsNullOrWhiteSpace(dataStore.ItemSubjectRef))
                {
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF, dataStore.ItemSubjectRef);
                }

                if (!string.IsNullOrWhiteSpace(dataStore.DataState))
                {
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_DATA_STATE);
                    xtw.writeCharacters(dataStore.DataState);
                    xtw.writeEndElement();
                }

                xtw.writeEndElement();
            }
        }
    }
}