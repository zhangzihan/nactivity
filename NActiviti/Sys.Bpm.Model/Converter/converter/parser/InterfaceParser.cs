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
namespace org.activiti.bpmn.converter.parser
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using Sys.Bpm;

    /// 
    public class InterfaceParser : IBpmnXMLConstants
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<InterfaceParser>();

        public virtual void parse(XMLStreamReader xtr, BpmnModel model)
        {
            Interface interfaceObject = new Interface();
            BpmnXMLUtil.addXMLLocation(interfaceObject, xtr);
            interfaceObject.Id = model.TargetNamespace + ":" + xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            interfaceObject.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
            interfaceObject.ImplementationRef = parseMessageRef(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_IMPLEMENTATION_REF), model);

            bool readyWithInterface = false;
            Operation operation = null;
            try
            {
                while (!readyWithInterface && xtr.hasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_OPERATION.Equals(xtr.LocalName))
                    {
                        operation = new Operation();
                        BpmnXMLUtil.addXMLLocation(operation, xtr);
                        operation.Id = model.TargetNamespace + ":" + xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        operation.Name = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                        operation.ImplementationRef = parseMessageRef(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_IMPLEMENTATION_REF), model);

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_IN_MESSAGE.Equals(xtr.LocalName))
                    {
                        string inMessageRef = xtr.ElementText;
                        if (operation != null && !string.IsNullOrWhiteSpace(inMessageRef))
                        {
                            operation.InMessageRef = parseMessageRef(inMessageRef.Trim(), model);
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_OUT_MESSAGE.Equals(xtr.LocalName))
                    {
                        string outMessageRef = xtr.ElementText;
                        if (operation != null && !string.IsNullOrWhiteSpace(outMessageRef))
                        {
                            operation.OutMessageRef = parseMessageRef(outMessageRef.Trim(), model);
                        }

                    }
                    else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_OPERATION.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (operation != null && !string.IsNullOrWhiteSpace(operation.ImplementationRef))
                        {
                            interfaceObject.Operations.Add(operation);
                        }

                    }
                    else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_INTERFACE.Equals(xtr.LocalName))
                    {
                        readyWithInterface = true;
                    }

                    if (xtr.IsEmptyElement && BpmnXMLConstants.ELEMENT_INTERFACE.Equals(xtr.LocalName))
                    {
                        readyWithInterface = true;
                    }
                }
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Error parsing interface child elements");
            }

            model.Interfaces.Add(interfaceObject);
        }

        protected internal virtual string parseMessageRef(string messageRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(messageRef))
            {
                int indexOfP = messageRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = messageRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.getNamespace(prefix);
                    messageRef = messageRef.Substring(indexOfP + 1);

                    if (string.ReferenceEquals(resolvedNamespace, null))
                    {
                        // if it's an invalid prefix will consider this is not a
                        // namespace prefix so will be used as part of the
                        // stringReference
                        messageRef = prefix + ":" + messageRef;
                    }
                    else if (!resolvedNamespace.Equals(model.TargetNamespace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // if it's a valid namespace prefix but it's not the
                        // targetNamespace then we'll use it as a valid namespace
                        // (even out editor does not support defining namespaces it
                        // is still a valid xml file)
                        messageRef = resolvedNamespace + ":" + messageRef;
                    }
                }
                result = messageRef;
            }
            return result;
        }
    }

}