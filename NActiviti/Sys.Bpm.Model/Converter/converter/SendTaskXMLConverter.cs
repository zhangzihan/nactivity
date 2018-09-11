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
    using org.activiti.bpmn.converter.export;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class SendTaskXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(SendTask);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TASK_SEND;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            SendTask sendTask = new SendTask();
            BpmnXMLUtil.addXMLLocation(sendTask, xtr);
            sendTask.Type = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TYPE);

            if ("##WebService".Equals(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION)))
            {
                sendTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE;
                sendTask.OperationRef = parseOperationRef(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_OPERATION_REF), model);
            }

            parseChildElements(XMLElementName, sendTask, model, xtr);

            return sendTask;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

            SendTask sendTask = (SendTask)element;

            if (!string.IsNullOrWhiteSpace(sendTask.Type))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TYPE, sendTask.Type, xtw);
            }
        }
        protected internal override bool writeExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            SendTask sendTask = (SendTask)element;
            didWriteExtensionStartElement = FieldExtensionExport.writeFieldExtensions(sendTask.FieldExtensions, didWriteExtensionStartElement, xtw);
            return didWriteExtensionStartElement;
        }
        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        protected internal virtual string parseOperationRef(string operationRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(operationRef))
            {
                int indexOfP = operationRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = operationRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.getNamespace(prefix);
                    result = resolvedNamespace + ":" + operationRef.Substring(indexOfP + 1);
                }
                else
                {
                    result = model.TargetNamespace + ":" + operationRef;
                }
            }
            return result;
        }
    }

}