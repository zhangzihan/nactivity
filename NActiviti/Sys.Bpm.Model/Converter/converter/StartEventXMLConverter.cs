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
namespace Sys.Workflow.bpmn.converter
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.bpmn.model.alfresco;

    public class StartEventXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(StartEvent);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_EVENT_START;
            }
        }

        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            string formKey = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY);
            StartEvent startEvent = null;

            if (!string.IsNullOrWhiteSpace(formKey) && model.StartEventFormTypes != null && model.StartEventFormTypes.Contains(formKey))
            {
                startEvent = new AlfrescoStartEvent();
            }
            if (startEvent == null)
            {
                startEvent = new StartEvent();
            }
            BpmnXMLUtil.AddXMLLocation(startEvent, xtr);
            startEvent.Initiator = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_EVENT_START_INITIATOR);
            bool interrupting = true;
            string interruptingAttribute = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_EVENT_START_INTERRUPTING);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(interruptingAttribute, StringComparison.CurrentCultureIgnoreCase))
            {
                interrupting = false;
            }
            startEvent.Interrupting = interrupting;
            startEvent.FormKey = formKey;

            ParseChildElements(XMLElementName, startEvent, model, xtr);

            return startEvent;
        }

        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            StartEvent startEvent = (StartEvent)element;
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_EVENT_START_INITIATOR, startEvent.Initiator, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY, startEvent.FormKey, xtw);

            if (startEvent.EventDefinitions != null && startEvent.EventDefinitions.Count > 0)
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_EVENT_START_INTERRUPTING, startEvent.Interrupting.ToString(), xtw);
            }
        }

        protected internal override bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            StartEvent startEvent = (StartEvent)element;
            didWriteExtensionStartElement = WriteFormProperties(startEvent, didWriteExtensionStartElement, xtw);
            return didWriteExtensionStartElement;
        }

        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            StartEvent startEvent = (StartEvent)element;
            WriteEventDefinitions(startEvent, startEvent.EventDefinitions, model, xtw);
        }
    }

}