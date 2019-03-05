using System;
using System.Collections.Generic;

namespace org.activiti.bpmn.converter
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using Sys.Bpm;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// 
    /// 
    public class ValuedDataObjectXMLConverter : BaseBpmnXMLConverter
    {
        private static readonly ILogger logger = BpmnModelLoggerFactory.LoggerService<ValuedDataObjectXMLConverter>();

        private readonly Regex xmlChars = new Regex("[<>&]");
        private string sdf = "yyyy-MM-dd'T'HH:mm:ss";
        protected internal bool didWriteExtensionStartElement = false;

        public override Type BpmnElementType
        {
            get
            {
                return typeof(ValuedDataObject);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_DATA_OBJECT;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ValuedDataObject dataObject = null;
            ItemDefinition itemSubjectRef = new ItemDefinition();

            string structureRef = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_DATA_ITEM_REF);
            if (!string.IsNullOrWhiteSpace(structureRef) && structureRef.Contains(":"))
            {
                string dataType = structureRef.Substring(structureRef.IndexOf(':') + 1);

                if (dataType.Equals("string"))
                {
                    dataObject = new StringDataObject();
                }
                else if (dataType.Equals("int"))
                {
                    dataObject = new IntegerDataObject();
                }
                else if (dataType.Equals("long"))
                {
                    dataObject = new LongDataObject();
                }
                else if (dataType.Equals("double"))
                {
                    dataObject = new DoubleDataObject();
                }
                else if (dataType.Equals("boolean"))
                {
                    dataObject = new BooleanDataObject();
                }
                else if (dataType.Equals("datetime"))
                {
                    dataObject = new DateDataObject();
                }
                else
                {
                    logger.LogError($"Error converting {xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_DATA_NAME)}, invalid data type: {dataType}");
                }
            }
            else
            {
                // use String as default type
                dataObject = new StringDataObject();
                structureRef = "xsd:string";
            }

            if (dataObject != null)
            {
                dataObject.Id = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_DATA_ID);
                dataObject.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_DATA_NAME);

                BpmnXMLUtil.addXMLLocation(dataObject, xtr);

                itemSubjectRef.StructureRef = structureRef;
                dataObject.ItemSubjectRef = itemSubjectRef;

                parseChildElements(XMLElementName, dataObject, model, xtr);

                 dataObject.ExtensionElements.TryGetValue("value", out IList<ExtensionElement> valuesElement);
                if (valuesElement != null && valuesElement.Count > 0)
                {
                    ExtensionElement valueElement = valuesElement[0];
                    if (!string.IsNullOrWhiteSpace(valueElement.ElementText))
                    {
                        if (dataObject is DateDataObject)
                        {
                            try
                            {
                                dataObject.Value = DateTime.Parse(valueElement.ElementText, new DateTimeFormatInfo()
                                {
                                    FullDateTimePattern = sdf
                                });
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, $"Error converting {dataObject.Name} \r\n {e.Message}");
                            }
                        }
                        else
                        {
                            dataObject.Value = valueElement.ElementText;
                        }
                    }

                    // remove value element
                    dataObject.ExtensionElements.Remove("value");
                }
            }

            return dataObject;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            ValuedDataObject dataObject = (ValuedDataObject)element;
            if (dataObject.ItemSubjectRef != null && !string.IsNullOrWhiteSpace(dataObject.ItemSubjectRef.StructureRef))
            {
                writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_DATA_ITEM_REF, dataObject.ItemSubjectRef.StructureRef, xtw);
            }
        }
        protected internal override bool writeExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            ValuedDataObject dataObject = (ValuedDataObject)element;

            if (!string.IsNullOrWhiteSpace(dataObject.Id) && dataObject.Value != null)
            {

                if (!didWriteExtensionStartElement)
                {
                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EXTENSIONS);
                    didWriteExtensionStartElement = true;
                }

                xtw.writeStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_DATA_VALUE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                if (dataObject.Value != null)
                {
                    string value = null;
                    if (dataObject is DateDataObject)
                    {
                        value = ((DateTime)dataObject.Value).ToString(sdf);
                    }
                    else
                    {
                        value = dataObject.Value.ToString();
                    }

                    if (dataObject is StringDataObject && xmlChars.IsMatch(value))
                    {
                        xtw.writeCData(value);
                    }
                    else
                    {
                        xtw.writeCharacters(value);
                    }
                }
                xtw.writeEndElement();
            }

            return didWriteExtensionStartElement;
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
    }

}