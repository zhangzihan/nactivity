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
using System.Xml;

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

        public virtual void ConvertToBpmnModel(XMLStreamReader xtr, BpmnModel model, Process activeProcess, IList<SubProcess> activeSubProcessList)
        {

            string elementId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            string elementName = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
            bool async = ParseAsync(xtr);
            bool notExclusive = ParseNotExclusive(xtr);
            string defaultFlow = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_DEFAULT);
            bool isForCompensation = ParseForCompensation(xtr);

            BaseElement parsedElement = ConvertXMLToElement(xtr, model);

            if (parsedElement is Artifact currentArtifact)
            {
                currentArtifact.Id = elementId;

                if (activeSubProcessList.Count > 0)
                {
                    activeSubProcessList[activeSubProcessList.Count - 1].AddArtifact(currentArtifact);

                }
                else
                {
                    activeProcess.AddArtifact(currentArtifact);
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
                    subProcess.AddFlowElement(currentFlowElement);

                }
                else
                {
                    activeProcess.AddFlowElement(currentFlowElement);
                }
            }
        }

        public virtual void ConvertToXML(XMLStreamWriter xtw, BaseElement baseElement, BpmnModel model)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, XMLElementName, BpmnXMLConstants.BPMN2_NAMESPACE);
            bool didWriteExtensionStartElement = false;
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ID, baseElement.Id, xtw);
            if (baseElement is FlowElement)
            {
                WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, ((FlowElement)baseElement).Name, xtw);
            }

            if (baseElement is FlowNode flowNode)
            {
                if (flowNode.Asynchronous)
                {
                    WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                    if (flowNode.NotExclusive)
                    {
                        WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                    }
                }

                if (baseElement is Activity activity)
                {
                    if (activity.ForCompensation)
                    {
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                    }
                    if (!string.IsNullOrWhiteSpace(activity.DefaultFlow))
                    {
                        FlowElement defaultFlowElement = model.GetFlowElement(activity.DefaultFlow);
                        if (defaultFlowElement is SequenceFlow)
                        {
                            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_DEFAULT, activity.DefaultFlow, xtw);
                        }
                    }
                }

                if (baseElement is Gateway gateway)
                {
                    if (!string.IsNullOrWhiteSpace(gateway.DefaultFlow))
                    {
                        FlowElement defaultFlowElement = model.GetFlowElement(gateway.DefaultFlow);
                        if (defaultFlowElement is SequenceFlow)
                        {
                            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_DEFAULT, gateway.DefaultFlow, xtw);
                        }
                    }
                }
            }

            WriteAdditionalAttributes(baseElement, model, xtw);

            if (baseElement is FlowElement flowElement)
            {
                if (!string.IsNullOrWhiteSpace(flowElement.Documentation))
                {

                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DOCUMENTATION, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteCharacters(flowElement.Documentation);
                    xtw.WriteEndElement();
                }
            }

            didWriteExtensionStartElement = WriteExtensionChildElements(baseElement, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = WriteListeners(baseElement, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(baseElement, didWriteExtensionStartElement, model.Namespaces, xtw);
            if (baseElement is Activity)
            {
                FailedJobRetryCountExport.WriteFailedJobRetryCount(baseElement as Activity, xtw);
            }

            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }

            if (baseElement is Activity)
            {
                MultiInstanceExport.WriteMultiInstance(baseElement as Activity, xtw);
            }

            WriteAdditionalChildElements(baseElement, model, xtw);

            xtw.WriteEndElement();
        }

        public abstract Type BpmnElementType { get; }

        protected internal abstract BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model);

        public abstract string XMLElementName { get; }

        protected internal abstract void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw);

        protected internal virtual bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return didWriteExtensionStartElement;
        }

        protected internal abstract void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw);

        // To BpmnModel converter convenience methods
        protected internal virtual void ParseChildElements(string elementName, BaseElement parentElement, BpmnModel model, XMLStreamReader xtr)
        {
            ParseChildElements(elementName, parentElement, null, model, xtr);
        }

        protected internal virtual void ParseChildElements(string elementName, BaseElement parentElement, IDictionary<string, BaseChildElementParser> additionalParsers, BpmnModel model, XMLStreamReader xtr)
        {

            IDictionary<string, BaseChildElementParser> childParsers = new Dictionary<string, BaseChildElementParser>();
            if (additionalParsers != null)
            {
                childParsers.PutAll(additionalParsers);
            }
            BpmnXMLUtil.ParseChildElements(elementName, parentElement, xtr, childParsers, model);
        }

        protected internal virtual ExtensionElement ParseExtensionElement(XMLStreamReader xtr)
        {
            ExtensionElement extensionElement = new ExtensionElement
            {
                Name = xtr.LocalName
            };
            if (!string.IsNullOrWhiteSpace(xtr.NamespaceURI))
            {
                extensionElement.Namespace = xtr.NamespaceURI;
            }
            if (!string.IsNullOrWhiteSpace(xtr.Prefix))
            {
                extensionElement.NamespacePrefix = xtr.Prefix;
            }

            BpmnXMLUtil.AddCustomAttributes(xtr, extensionElement, defaultElementAttributes);

            bool readyWithExtensionElement = false;
            while (!readyWithExtensionElement && xtr.HasNext())
            {
                //xtr.next();

                if (xtr.NodeType == XmlNodeType.CDATA)
                {
                    if (!string.IsNullOrWhiteSpace(xtr.Value?.Trim()))
                    {
                        extensionElement.ElementText = xtr.Value?.Trim();
                    }
                }
                else if (xtr.IsStartElement())
                {
                    ExtensionElement childExtensionElement = ParseExtensionElement(xtr);
                    extensionElement.AddChildElement(childExtensionElement);
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

        protected internal virtual bool ParseAsync(XMLStreamReader xtr)
        {
            bool async = false;
            string asyncString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(asyncString, StringComparison.CurrentCultureIgnoreCase))
            {
                async = true;
            }
            return async;
        }

        protected internal virtual bool ParseNotExclusive(XMLStreamReader xtr)
        {
            bool notExclusive = false;
            string exclusiveString = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE.Equals(exclusiveString, StringComparison.CurrentCultureIgnoreCase))
            {
                notExclusive = true;
            }
            return notExclusive;
        }

        protected internal virtual bool ParseForCompensation(XMLStreamReader xtr)
        {
            bool isForCompensation = false;
            string compensationString = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ISFORCOMPENSATION);
            if (BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE.Equals(compensationString, StringComparison.CurrentCultureIgnoreCase))
            {
                isForCompensation = true;
            }
            return isForCompensation;
        }

        protected internal virtual IList<string> ParseDelimitedList(string expression)
        {
            return BpmnXMLUtil.ParseDelimitedList(expression);
        }

        // To XML converter convenience methods

        protected internal virtual string ConvertToDelimitedString(IList<string> stringList)
        {
            return BpmnXMLUtil.ConvertToDelimitedString(stringList);
        }

        protected internal virtual bool WriteFormProperties(FlowElement flowElement, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
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
                            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                            didWriteExtensionStartElement = true;
                        }

                        xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_FORMPROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_ID, property.Id, xtw);

                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_NAME, property.Name, xtw);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_TYPE, property.Type, xtw);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_EXPRESSION, property.Expression, xtw);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_VARIABLE, property.Variable, xtw);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_DEFAULT, property.DefaultExpression, xtw);
                        WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_DATEPATTERN, property.DatePattern, xtw);
                        if (!property.Readable)
                        {
                            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_READABLE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                        if (!property.Writeable)
                        {
                            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_WRITABLE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                        if (property.Required)
                        {
                            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_REQUIRED, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                        }

                        foreach (FormValue formValue in property.FormValues)
                        {
                            if (!string.IsNullOrWhiteSpace(formValue.Id))
                            {
                                xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_VALUE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, formValue.Id);
                                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, formValue.Name);
                                xtw.WriteEndElement();
                            }
                        }

                        xtw.WriteEndElement();
                    }
                }
            }

            return didWriteExtensionStartElement;
        }

        protected internal virtual bool WriteListeners(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return ActivitiListenerExport.WriteListeners(element, didWriteExtensionStartElement, xtw);
        }

        protected internal virtual void WriteEventDefinitions(Event parentEvent, IList<EventDefinition> eventDefinitions, BpmnModel model, XMLStreamWriter xtw)
        {
            foreach (EventDefinition eventDefinition in eventDefinitions)
            {
                if (eventDefinition is TimerEventDefinition)
                {
                    WriteTimerDefinition(parentEvent, (TimerEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is SignalEventDefinition)
                {
                    WriteSignalDefinition(parentEvent, (SignalEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is MessageEventDefinition)
                {
                    WriteMessageDefinition(parentEvent, (MessageEventDefinition)eventDefinition, model, xtw);
                }
                else if (eventDefinition is ErrorEventDefinition)
                {
                    WriteErrorDefinition(parentEvent, (ErrorEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is TerminateEventDefinition)
                {
                    WriteTerminateDefinition(parentEvent, (TerminateEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is CancelEventDefinition)
                {
                    WriteCancelDefinition(parentEvent, (CancelEventDefinition)eventDefinition, xtw);
                }
                else if (eventDefinition is CompensateEventDefinition)
                {
                    WriteCompensateDefinition(parentEvent, (CompensateEventDefinition)eventDefinition, xtw);
                }
            }
        }


        protected internal virtual void WriteTimerDefinition(Event parentEvent, TimerEventDefinition timerDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_TIMERDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);
            if (!string.IsNullOrWhiteSpace(timerDefinition.CalendarName))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_CALENDAR_NAME, timerDefinition.CalendarName, xtw);
            }
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(timerDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            if (!string.IsNullOrWhiteSpace(timerDefinition.TimeDate))
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ATTRIBUTE_TIMER_DATE, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteCharacters(timerDefinition.TimeDate);
                xtw.WriteEndElement();

            }
            else if (!string.IsNullOrWhiteSpace(timerDefinition.TimeCycle))
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX,  BpmnXMLConstants.ATTRIBUTE_TIMER_CYCLE, BpmnXMLConstants.BPMN2_NAMESPACE);

                if (!string.IsNullOrWhiteSpace(timerDefinition.EndDate))
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_END_DATE, timerDefinition.EndDate);
                }

                xtw.WriteCharacters(timerDefinition.TimeCycle);
                xtw.WriteEndElement();

            }
            else if (!string.IsNullOrWhiteSpace(timerDefinition.TimeDuration))
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ATTRIBUTE_TIMER_DURATION, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteCharacters(timerDefinition.TimeDuration);
                xtw.WriteEndElement();
            }

            xtw.WriteEndElement();
        }

        protected internal virtual void WriteSignalDefinition(Event parentEvent, SignalEventDefinition signalDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_SIGNALDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_SIGNAL_REF, signalDefinition.SignalRef, xtw);
            if (parentEvent is ThrowEvent && signalDefinition.Async)
            {
                BpmnXMLUtil.WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, "true", xtw);
            }
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(signalDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteCancelDefinition(Event parentEvent, CancelEventDefinition cancelEventDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_CANCELDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(cancelEventDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteCompensateDefinition(Event parentEvent, CompensateEventDefinition compensateEventDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_COMPENSATEDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_COMPENSATE_ACTIVITYREF, compensateEventDefinition.ActivityRef, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(compensateEventDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteMessageDefinition(Event parentEvent, MessageEventDefinition messageDefinition, BpmnModel model, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_MESSAGEDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);

            string messageRef = messageDefinition.MessageRef;
            if (!string.IsNullOrWhiteSpace(messageRef))
            {
                // remove the namespace from the message id if set
                if (messageRef.StartsWith(model.TargetNamespace, StringComparison.Ordinal))
                {
                    messageRef = messageRef.Replace(model.TargetNamespace, "");
                    messageRef = messageRef.ReplaceFirst(":", "");
                }
                else
                {
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.GetNamespace(prefix);
                        if (messageRef.StartsWith(@namespace, StringComparison.Ordinal))
                        {
                            messageRef = messageRef.Replace(model.TargetNamespace, "");
                            messageRef = prefix + messageRef;
                        }
                    }
                }
            }
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_MESSAGE_REF, messageRef, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(messageDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteErrorDefinition(Event parentEvent, ErrorEventDefinition errorDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_ERRORDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ERROR_REF, errorDefinition.ErrorCode, xtw);
            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(errorDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteTerminateDefinition(Event parentEvent, TerminateEventDefinition terminateDefinition, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_TERMINATEDEFINITION, BpmnXMLConstants.BPMN2_NAMESPACE);

            if (terminateDefinition.TerminateAll)
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TERMINATE_ALL, "true", xtw);
            }

            if (terminateDefinition.TerminateMultiInstance)
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TERMINATE_MULTI_INSTANCE, "true", xtw);
            }

            bool didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(terminateDefinition, false, xtw);
            if (didWriteExtensionStartElement)
            {
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();
        }

        protected internal virtual void WriteDefaultAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            BpmnXMLUtil.WriteDefaultAttribute(attributeName, value, xtw);
        }

        protected internal virtual void WriteQualifiedAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            BpmnXMLUtil.WriteQualifiedAttribute(attributeName, value, xtw);
        }
    }

}