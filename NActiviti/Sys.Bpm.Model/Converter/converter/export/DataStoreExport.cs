namespace Sys.Workflow.Bpmn.Converters.Exports
{

    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    public class DataStoreExport : IBpmnXMLConstants
    {
        public static void WriteDataStores(BpmnModel model, XMLStreamWriter xtw)
        {
            foreach (DataStore dataStore in model.DataStores.Values)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DATA_STORE, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, dataStore.Id);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, dataStore.Name);
                if (!string.IsNullOrWhiteSpace(dataStore.ItemSubjectRef))
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF, dataStore.ItemSubjectRef);
                }

                if (!string.IsNullOrWhiteSpace(dataStore.DataState))
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DATA_STATE, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteCharacters(dataStore.DataState);
                    xtw.WriteEndElement();
                }

                xtw.WriteEndElement();
            }
        }
    }
}