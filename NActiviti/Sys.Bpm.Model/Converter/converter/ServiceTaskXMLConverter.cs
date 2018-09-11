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
    public class ServiceTaskXMLConverter : BaseBpmnXMLConverter
    {

        public override Type BpmnElementType
        {
            get
            {
                return typeof(ServiceTask);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_TASK_SERVICE;
            }
        }
        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ServiceTask serviceTask = new ServiceTask();
            BpmnXMLUtil.addXMLLocation(serviceTask, xtr);
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
                serviceTask.Implementation = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS);

            }
            else if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION;
                serviceTask.Implementation = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION);

            }
            else if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION;
                serviceTask.Implementation = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION);

            }
            else if ("##WebService".Equals(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE;
                serviceTask.OperationRef = parseOperationRef(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_OPERATION_REF), model);
            }
            else
            {
                serviceTask.Implementation = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION);
            }

            serviceTask.ResultVariableName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE);
            if (string.IsNullOrWhiteSpace(serviceTask.ResultVariableName))
            {
                serviceTask.ResultVariableName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, "resultVariable");
            }

            serviceTask.Type = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TYPE);
            serviceTask.ExtensionId = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID);

            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION)))
            {
                serviceTask.SkipExpression = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION);
            }
            parseChildElements(XMLElementName, serviceTask, model, xtr);

            return serviceTask;
        }
        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

            ServiceTask serviceTask = (ServiceTask)element;

            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS, serviceTask.Implementation, xtw);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION, serviceTask.Implementation, xtw);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION, serviceTask.Implementation, xtw);
            }

            if (!string.IsNullOrWhiteSpace(serviceTask.ResultVariableName))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE, serviceTask.ResultVariableName, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TYPE, serviceTask.Type, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.ExtensionId))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID, serviceTask.ExtensionId, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.SkipExpression))
            {
                writeQualifiedAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION, serviceTask.SkipExpression, xtw);
            }
        }
        protected internal override bool writeExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            ServiceTask serviceTask = (ServiceTask)element;

            if (serviceTask.CustomProperties.Count > 0)
            {
                foreach (CustomProperty customProperty in serviceTask.CustomProperties)
                {

                    if (string.IsNullOrWhiteSpace(customProperty.SimpleValue))
                    {
                        continue;
                    }

                    if (!didWriteExtensionStartElement)
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_EXTENSIONS);
                        didWriteExtensionStartElement = true;
                    }
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_NAME, customProperty.Name);
                    if ((customProperty.SimpleValue.Contains("${") || customProperty.SimpleValue.Contains("#{")) && customProperty.SimpleValue.Contains("}"))
                    {

                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    }
                    else
                    {
                        xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_FIELD_STRING, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    }
                    xtw.writeCharacters(customProperty.SimpleValue);
                    xtw.writeEndElement();
                    xtw.writeEndElement();
                }
            }
            else
            {
                didWriteExtensionStartElement = FieldExtensionExport.writeFieldExtensions(serviceTask.FieldExtensions, didWriteExtensionStartElement, xtw);
            }

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