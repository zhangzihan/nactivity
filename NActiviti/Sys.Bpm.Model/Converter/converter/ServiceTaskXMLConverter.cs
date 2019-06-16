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
    using Sys.Workflow.bpmn.converter.export;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

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
                return BpmnXMLConstants.ELEMENT_TASK_SERVICE;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            ServiceTask serviceTask = new ServiceTask();
            BpmnXMLUtil.AddXMLLocation(serviceTask, xtr);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
                serviceTask.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS);

            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION;
                serviceTask.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION);

            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION;
                serviceTask.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION);

            }
            else if ("##WebService".Equals(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION)))
            {
                serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE;
                serviceTask.OperationRef = ParseOperationRef(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_OPERATION_REF), model);
            }
            else
            {
                serviceTask.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_TASK_IMPLEMENTATION);
            }

            serviceTask.ResultVariableName = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE);
            if (string.IsNullOrWhiteSpace(serviceTask.ResultVariableName))
            {
                serviceTask.ResultVariableName = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, "resultVariable");
            }

            serviceTask.Type = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TYPE);
            serviceTask.ExtensionId = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID);

            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION)))
            {
                serviceTask.SkipExpression = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION);
            }
            ParseChildElements(XMLElementName, serviceTask, model, xtr);

            return serviceTask;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {

            ServiceTask serviceTask = (ServiceTask)element;

            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS, serviceTask.Implementation, xtw);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXPRESSION, serviceTask.Implementation, xtw);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_DELEGATEEXPRESSION, serviceTask.Implementation, xtw);
            }

            if (!string.IsNullOrWhiteSpace(serviceTask.ResultVariableName))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_RESULTVARIABLE, serviceTask.ResultVariableName, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TYPE, serviceTask.Type, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.ExtensionId))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID, serviceTask.ExtensionId, xtw);
            }
            if (!string.IsNullOrWhiteSpace(serviceTask.SkipExpression))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_SKIP_EXPRESSION, serviceTask.SkipExpression, xtw);
            }
        }
        protected internal override bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
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
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                        didWriteExtensionStartElement = true;
                    }
                    xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_FIELD, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_FIELD_NAME, customProperty.Name);
                    if ((customProperty.SimpleValue.Contains("${") || customProperty.SimpleValue.Contains("#{")) && customProperty.SimpleValue.Contains("}"))
                    {

                        xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ATTRIBUTE_FIELD_EXPRESSION, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    }
                    else
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_FIELD_STRING, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    }
                    xtw.WriteCharacters(customProperty.SimpleValue);
                    xtw.WriteEndElement();
                    xtw.WriteEndElement();
                }
            }
            else
            {
                didWriteExtensionStartElement = FieldExtensionExport.WriteFieldExtensions(serviceTask.FieldExtensions, didWriteExtensionStartElement, xtw);
            }

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