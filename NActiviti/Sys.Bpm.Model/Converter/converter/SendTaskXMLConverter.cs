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
                return BpmnXMLConstants.ELEMENT_TASK_SEND;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            SendTask sendTask = new SendTask();
            BpmnXMLUtil.AddXMLLocation(sendTask, xtr);
            //sendTask.Type = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TYPE);

            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS)))
            {
                sendTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
                sendTask.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS);
            }

            //if ("##WebService".Equals(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION)))
            //{
            //    sendTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE;
            //    sendTask.OperationRef = parseOperationRef(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_OPERATION_REF), model);
            //}

            ParseChildElements(XMLElementName, sendTask, model, xtr);

            return sendTask;
        }

        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

            SendTask sendTask = (SendTask)element;

            if (!string.IsNullOrWhiteSpace(sendTask.Type))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TYPE, sendTask.Type, xtw);
            }
        }
        protected internal override bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            SendTask sendTask = (SendTask)element;
            didWriteExtensionStartElement = FieldExtensionExport.WriteFieldExtensions(sendTask.FieldExtensions, didWriteExtensionStartElement, xtw);
            return didWriteExtensionStartElement;
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        protected internal virtual string ParseOperationRef(string operationRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(operationRef))
            {
                int indexOfP = operationRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = operationRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.GetNamespace(prefix);
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