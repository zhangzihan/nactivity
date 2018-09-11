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

using org.activiti.bpmn.constants;
using org.activiti.bpmn.converter.child;
using org.activiti.bpmn.converter.export;
using org.activiti.bpmn.converter.util;
using org.activiti.bpmn.model;
using Sys.Bpm;
using System;
using System.Collections.Generic;

namespace org.activiti.bpmn.converter
{
    public abstract class BaseBpmnXMLConverter : IBpmnXMLConstants
    {
        protected internal static readonly IList<ExtensionAttribute> defaultElementAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_ID),
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_NAME)
        };

        protected internal static readonly IList<ExtensionAttribute> defaultActivityAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE),
            new ExtensionAttribute(BpmnXMLConstants.ATTRIBUTE_DEFAULT),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION)
        };

        public virtual void convertToBpmnModel(XMLStreamReader xtr, BpmnModel model, Process activeProcess, IList<SubProcess> activeSubProcessList)
        {

            string elementId = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            string elementName = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
            bool async = parseAsync(xtr);
            bool notExclusive = parseNotExclusive(xtr);
            string defaultFlow = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_DEFAULT);
            bool isForCompensation = parseForCompensation(xtr);

            BaseElement parsedElement = convertXMLToElement(xtr, model);

            if (parsedElement is Artifact currentArtifact)
            {
                currentArtifact.Id = elementId;

                if (activeSubProcessList.Count > 0)
                {
                    activeSubProcessList[activeSubProcessList.Count - 1].addArtifact(currentArtifact);

                }
                else
                {
                    activeProcess.addArtifact(currentArtifact);
                }
            }

            if (parsedElement is FlowElement currentFlowElement)
            {
                currentFlowElement.Id = elementId;
                currentFlowElement.Name = elementName;

                if (currentFlowElement is FlowNode flowNode)
                {
                    flowNode.Asynchronous = async;
                    flowNode.NotExclusive = notExclusive;

                    if (currentFlowElement is Activity activity)
                    {
                        activity.ForCompensation = isForCompensation;
                        if (!string.IsNullOrWhiteSpace(defaultFlow))
                        {
                            activity.DefaultFlow = defaultFlow;
                        }
                    }

                    if (currentFlowElement is Gateway gateway)
                    {
                        if (!string.IsNullOrWhiteSpace(defaultFlow))
                        {
                            gateway.DefaultFlow = defaultFlow;
                        }
                    }
                }

                if (currentFlowElement is DataObject)
                {
                    if (activeSubProcessList.Count > 0)
                    {
                        SubProcess subProcess = activeSubProcessList[activeSubProcessList.Count - 1];
                        subProcess.DataObjects.Add((ValuedDataObject)parsedElement);
                    }
                    else
                    {
                        activeProcess.DataObjects.Add((ValuedDataObject)parsedElement);
                    }
                }

                if (activeSubProcessList.Count > 0)
                {

                    SubProcess subProcess = activeSubProcessList[activeSubProcessList.Count - 1];
                    subProcess.addFlowElement(currentFlowElement);

                }
                else
                {
                    activeProcess.addFlowElement(currentFlowElement);
                }
            }
        }

        public virtual void convertToXML(XMLStreamWriter xtw, BaseElement baseElement, BpmnModel model)
        {
            xtw.writeStartElement(XMLElementName);
            bool didWriteExtensionStartElement = false;
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ID, baseElement.Id, xtw);
            if (baseElement is FlowElement)
            {
                writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, ((FlowElement)baseElement).Name, xtw);
            }

            if (baseElement is FlowNode flowNode)
            {
                if (flowNode.Asynchronous)
                {
                    writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                    if (flowNode.NotExclusive)
                    {
                        writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                    }
                }

                if (baseElement is Activity activity)
                {
                    if (activity.ForCompensation)
                    {
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(activity.DefaultFlow))
                    {
                        FlowElement defaultFlowElement = model.getFlowElement(activity.DefaultFlow);
                        if (defaultFlowElement is SequenceFlow)
                        {
                            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_DEFAULT, activity.DefaultFlow, xtw);
                        }
                    }
                }

                if (baseElement is Gateway gateway)
                {
                    if (!string.IsNullOrWhiteSpace(gateway.DefaultFlow))
                    {
                        FlowElement defaultFlowElement = model.getFlowElement(gateway.DefaultFlow);
                        if (defaultFlowElement is SequenceFlow)
                        {
                            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_DEFAULT, gateway.DefaultFlow, xtw);
                        }
                    }
                }
            }

            writeAdditionalAttributes(baseElement, model, xtw);

            if (baseElement is FlowElement flowElement)
            {
                if (!string.IsNullOrWhiteSpace(flowElement.Documentation))
                {

                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_DOCUMENTATION);
                    xtw.writeCharacters(flowElement.Documentation);
                    xtw.writeEndElement();
                }
            }

            didWriteExtensionStartElement = writeExtensionChildElements(baseElement, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = writeListeners(baseElement, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(baseElement, didWriteExtensionStartElement, model.Namespaces, xtw);
            if (baseElement is Activity)
            {
                FailedJobRetryCountExport.writeFailedJobRetryCount(baseElement as Activity, xtw);
            }

            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }

            if (baseElement is Activity)
            {
                MultiInstanceExport.writeMultiInstance(baseElement as Activity, xtw);

            }

            writeAdditionalChildElements(baseElement, model, xtw);

            xtw.writeEndElement();
        }

        public abstract Type BpmnElementType { get; }

        protected internal abstract BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model);

        public abstract string XMLElementName { get; }

        protected internal abstract void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw);

        protected internal virtual bool writeExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return didWriteExtensionStartElement;
        }

        protected internal abstract void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw);

        // To BpmnModel converter convenience methods
        protected internal virtual void parseChildElements(string elementName, BaseElement parentElement, BpmnModel model, XMLStreamReader xtr)
        {
            parseChildElements(elementName, parentElement, null, model, xtr);
        }

        protected internal virtual void parseChildElements(string elementName, BaseElement parentElement, IDictionary<string, BaseChildElementParser> additionalParsers, BpmnModel model, XMLStreamReader xtr)
        {

            IDictionary<string, BaseChildElementParser> childParsers = new Dictionary<string, BaseChildElementParser>();
            if (additionalParsers != null)
            {
                childParsers.putAll(additionalParsers);
            }
            BpmnXMLUtil.parseChildElements(elementName, parentElement, xtr, childParsers, model);
        }

        protected internal virtual ExtensionElement parseExtensionElement(XMLStreamReader xtr)
        {
            ExtensionElement extensionElement = new ExtensionElement();
            extensionElement.Name = xtr.LocalName;
            if (!string.IsNullOrWhiteSpace(xtr.NamespaceURI))
            {
                extensionElement.Namespace = xtr.NamespaceURI;
            }
            if (!string.IsNullOrWhiteSpace(xtr.Prefix))
            {
                extensionElement.NamespacePrefix = xtr.Prefix;
            }

            BpmnXMLUtil.addCustomAttributes(xtr, extensionElement, defaultElementAttributes);

            bool readyWithExtensionElement = false;
            while (!readyWithExtensionElement && xtr.hasNext())
            {
                //xtr.next();

                if (xtr.NodeType == System.Xml.XmlNodeType.CDATA)
                {
                    if (!string.IsNullOrWhiteSpace(xtr.Value?.Trim()))
                    {
                        extensionElement.ElementText = xtr.Value?.Trim();
                    }
                }
                else if (xtr.StartElement)
                {
                    ExtensionElement childExtensionElement = parseExtensionElement(xtr);
                    extensionElement.addChildElement(childExtensionElement);
                }
                else if (xtr.EndElement && string.Compare(extensionElement.Name, xtr.LocalName, true) == 0)
                {
                    readyWithExtensionElement = true;
                }

                if (xtr.IsEmptyElement && string.Compare(extensionElement.Name, xtr.LocalName, true) == 0)
                {
                    readyWithExtensionElement = true;
                }
            }
            return extensionElement;
        }

        protected internal virtual bool parseAsync(XMLStreamReader xtr)
        {
            bool async = false;
            string asyncString = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(asyncString, StringComparison.CurrentCultureIgnoreCase))
            {
                async = true;
            }
            return async;
        }

        protected internal virtual bool parseNotExclusive(XMLStreamReader xtr)
        {
            bool notExclusive = false;
            string exclusiveString = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(exclusiveString, StringComparison.CurrentCultureIgnoreCase))
            {
                notExclusive = true;
            }
            return notExclusive;
        }

        protected internal virtual bool parseForCompensation(XMLStreamReader xtr)
        {
            bool isForCompensation = false;
            string compensationString = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(compensationString, StringComparison.CurrentCultureIgnoreCase))
            {
                isForCompensation = true;
            }
            return isForCompensation;
        }

        protected internal virtual IList<string> parseDelimitedList(string expression)
        {
            return BpmnXMLUtil.parseDelimitedList(expression);
        }

        // To XML converter convenience methods

        protected internal virtual string convertToDelimitedString(IList<string> stringList)
        {
            return BpmnXMLUtil.convertToDelimitedString(stringList);
        }

        protected internal virtual bool writeFormProperties(FlowElement flowElement, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {

            IList<FormProperty> propertyList = null;
            if (flowElement is UserTask)
            {
                propertyList = ((UserTask)flowElement).FormProperties;
            }
            else if (flowElement is StartEvent)
            {
                propertyList = ((StartEvent)flowElement).FormProperties;
            }

            if (propertyList != null)
            {

                foreach (FormProperty property in propertyList)
                {

                    if (!string.IsNullOrWhiteSpace(property.Id))
                    {

                        if (!didWriteExtensionStartElement)
                        {
                            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EXTENSIONS);
                            didWriteExtensionStartElement = true;
                        }

                        xtw.writeStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_FORMPROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_ID, property.Id, xtw);

                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_NAME, property.Name, xtw);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_TYPE, property.Type, xtw);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_EXPRESSION, property.Expression, xtw);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_VARIABLE, property.Variable, xtw);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_DEFAULT, property.DefaultExpression, xtw);
                        writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_DATEPATTERN, property.DatePattern, xtw);
                        if (!property.Readable)
                        {
                            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                        if (!property.Writeable)
                        {
                            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                        if (property.Required)
                        {
                            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                        }

                        foreach (FormValue formValue in property.FormValues)
                        {
                            if (!string.IsNullOrWhiteSpace(formValue.Id))
                            {
                                xtw.writeStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_VALUE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                                xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_ID, formValue.Id);
                                xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, formValue.Name);
                                xtw.writeEndElement();
                            }
                        }

                        xtw.writeEndElement();
                    }
                }
            }

            return didWriteExtensionStartElement;
        }

        protected internal virtual bool writeListeners(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return ActivitiListenerExport.writeListeners(element, didWriteExtensionStartElement, xtw);
        }

        protected internal virtual void writeEventDefinitions(Event parentEvent, IList<EventDefinition> eventDefinitions, BpmnModel model, XMLStreamWriter xtw)
        {
            foreach (EventDefinition eventDefinition in eventDefinitions)
            {
                if (eventDefinition is TimerEventDefinition)
                {
                    writeTimerDefinition(parentEvent, (TimerEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is SignalEventDefinition)
                {
                    writeSignalDefinition(parentEvent, (SignalEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is MessageEventDefinition)
                {
                    writeMessageDefinition(parentEvent, (MessageEventDefinition)eventDefinition, model, xtw);
                }
                else if (eventDefinition is ErrorEventDefinition)
                {
                    writeErrorDefinition(parentEvent, (ErrorEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is TerminateEventDefinition)
                {
                    writeTerminateDefinition(parentEvent, (TerminateEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is CancelEventDefinition)
                {
                    writeCancelDefinition(parentEvent, (CancelEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is CompensateEventDefinition)
                {
                    writeCompensateDefinition(parentEvent, (CompensateEventDefinition)eventDefinition, xtw);
                }
            }
        }


        protected internal virtual void writeTimerDefinition(Event parentEvent, TimerEventDefinition timerDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_TIMERDEFINITION);
            if (!string.IsNullOrWhiteSpace(timerDefinition.CalendarName))
            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_CALENDAR_NAME, timerDefinition.CalendarName, xtw);
            }
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(timerDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            if (!string.IsNullOrWhiteSpace(timerDefinition.TimeDate))
            {
                xtw.writeStartElement(BpmnXMLConstants.ATTRIBUTE_TIMER_DATE);
                xtw.writeCharacters(timerDefinition.TimeDate);
                xtw.writeEndElement();

            }
            else if (!string.IsNullOrWhiteSpace(timerDefinition.TimeCycle))
            {
                xtw.writeStartElement(BpmnXMLConstants.ATTRIBUTE_TIMER_CYCLE);

                if (!string.IsNullOrWhiteSpace(timerDefinition.EndDate))
                {
                    xtw.writeAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_END_DATE, timerDefinition.EndDate);
                }

                xtw.writeCharacters(timerDefinition.TimeCycle);
                xtw.writeEndElement();

            }
            else if (!string.IsNullOrWhiteSpace(timerDefinition.TimeDuration))
            {
                xtw.writeStartElement(BpmnXMLConstants.ATTRIBUTE_TIMER_DURATION);
                xtw.writeCharacters(timerDefinition.TimeDuration);
                xtw.writeEndElement();
            }

            xtw.writeEndElement();
        }

        protected internal virtual void writeSignalDefinition(Event parentEvent, SignalEventDefinition signalDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION);
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_SIGNAL_REF, signalDefinition.SignalRef, xtw);
            if (parentEvent is ThrowEvent && signalDefinition.Async)
            {
                BpmnXMLUtil.writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, "true", xtw);
            }
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(signalDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeCancelDefinition(Event parentEvent, CancelEventDefinition cancelEventDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_CANCELDEFINITION);
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(cancelEventDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeCompensateDefinition(Event parentEvent, CompensateEventDefinition compensateEventDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_COMPENSATEDEFINITION);
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_COMPENSATE_ACTIVITYREF, compensateEventDefinition.ActivityRef, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(compensateEventDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeMessageDefinition(Event parentEvent, MessageEventDefinition messageDefinition, BpmnModel model, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_MESSAGEDEFINITION);

            string messageRef = messageDefinition.MessageRef;
            if (!string.IsNullOrWhiteSpace(messageRef))
            {
                // remove the namespace from the message id if set
                if (messageRef.StartsWith(model.TargetNamespace, StringComparison.Ordinal))
                {
                    messageRef = messageRef.Replace(model.TargetNamespace, "");
                    messageRef = messageRef.replaceFirst(":", "");
                }
                else
                {
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.getNamespace(prefix);
                        if (messageRef.StartsWith(@namespace, StringComparison.Ordinal))
                        {
                            messageRef = messageRef.Replace(model.TargetNamespace, "");
                            messageRef = prefix + messageRef;
                        }
                    }
                }
            }
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_MESSAGE_REF, messageRef, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(messageDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeErrorDefinition(Event parentEvent, ErrorEventDefinition errorDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_ERRORDEFINITION);
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ERROR_REF, errorDefinition.ErrorCode, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(errorDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeTerminateDefinition(Event parentEvent, TerminateEventDefinition terminateDefinition, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EVENT_TERMINATEDEFINITION);

            if (terminateDefinition.TerminateAll)
            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TERMINATE_ALL, "true", xtw);
            }

            if (terminateDefinition.TerminateMultiInstance)
            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TERMINATE_MULTI_INSTANCE, "true", xtw);
            }

            bool didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(terminateDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.writeEndElement();
            }
            xtw.writeEndElement();
        }

        protected internal virtual void writeDefaultAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            BpmnXMLUtil.writeDefaultAttribute(attributeName, value, xtw);
        }

        protected internal virtual void writeQualifiedAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            BpmnXMLUtil.writeQualifiedAttribute(attributeName, value, xtw);
        }
    }

}