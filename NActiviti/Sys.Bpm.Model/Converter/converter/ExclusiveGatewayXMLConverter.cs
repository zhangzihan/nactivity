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
    public class ExclusiveGatewayXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(ExclusiveGateway);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_GATEWAY_EXCLUSIVE;
            }
        }

        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ExclusiveGateway gateway = new ExclusiveGateway();
            BpmnXMLUtil.AddXMLLocation(gateway, xtr);
            ParseChildElements(XMLElementName, gateway, model, xtr);
            return gateway;
        }

        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

        }
    }

}