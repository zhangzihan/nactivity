﻿using System;

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

    /// 
    public class CatchEventXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(IntermediateCatchEvent);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_EVENT_CATCH;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            IntermediateCatchEvent catchEvent = new IntermediateCatchEvent();
            BpmnXMLUtil.AddXMLLocation(catchEvent, xtr);
            ParseChildElements(XMLElementName, catchEvent, model, xtr);
            return catchEvent;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            IntermediateCatchEvent catchEvent = (IntermediateCatchEvent)element;
            WriteEventDefinitions(catchEvent, catchEvent.EventDefinitions, model, xtw);
        }
    }

}