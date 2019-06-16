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

    /// 
    public class BoundaryEventXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(BoundaryEvent);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_EVENT_BOUNDARY;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            BoundaryEvent boundaryEvent = new BoundaryEvent();
            BpmnXMLUtil.AddXMLLocation(boundaryEvent, xtr);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_BOUNDARY_CANCELACTIVITY)))
            {
                string cancelActivity = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_BOUNDARY_CANCELACTIVITY);
                if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(cancelActivity, StringComparison.CurrentCultureIgnoreCase))
                {
                    boundaryEvent.CancelActivity = false;
                }
            }
            boundaryEvent.AttachedToRefId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_BOUNDARY_ATTACHEDTOREF);
            ParseChildElements(XMLElementName, boundaryEvent, model, xtr);

            // Explicitly set cancel activity to false for error boundary events
            if (boundaryEvent.EventDefinitions.Count == 1)
            {
                EventDefinition eventDef = boundaryEvent.EventDefinitions[0];

                if (eventDef is ErrorEventDefinition)
                {
                    boundaryEvent.CancelActivity = false;
                }
            }

            return boundaryEvent;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            BoundaryEvent boundaryEvent = (BoundaryEvent)element;
            if (boundaryEvent.AttachedToRef != null)
            {
                WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_BOUNDARY_ATTACHEDTOREF, boundaryEvent.AttachedToRef.Id, xtw);
            }

            if (boundaryEvent.EventDefinitions.Count == 1)
            {
                EventDefinition eventDef = boundaryEvent.EventDefinitions[0];

                if (eventDef is ErrorEventDefinition == false)
                {
                    WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_BOUNDARY_CANCELACTIVITY, boundaryEvent.CancelActivity ? "true" : "false", xtw);
                }
            }
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            BoundaryEvent boundaryEvent = (BoundaryEvent)element;
            WriteEventDefinitions(boundaryEvent, boundaryEvent.EventDefinitions, model, xtw);
        }
    }

}