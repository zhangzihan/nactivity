using System;

namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    public class SignalAndMessageDefinitionExport : IBpmnXMLConstants
    {
        public static void writeSignalsAndMessages(BpmnModel model, XMLStreamWriter xtw)
        {

            foreach (Process process in model.Processes)
            {
                foreach (FlowElement flowElement in process.findFlowElementsOfType<Event>())
                {
                    Event @event = (Event)flowElement;
                    if (@event.EventDefinitions.Count > 0)
                    {
                        EventDefinition eventDefinition = @event.EventDefinitions[0];
                        if (eventDefinition is SignalEventDefinition)
                        {
                            SignalEventDefinition signalEvent = (SignalEventDefinition)eventDefinition;
                            if (!string.IsNullOrWhiteSpace(signalEvent.SignalRef))
                            {
                                if (!model.containsSignalId(signalEvent.SignalRef))
                                {
                                    Signal signal = new Signal(signalEvent.SignalRef, signalEvent.SignalRef);
                                    model.addSignal(signal);
                                }
                            }
                        }
                        else if (eventDefinition is MessageEventDefinition)
                        {
                            MessageEventDefinition messageEvent = (MessageEventDefinition)eventDefinition;
                            if (!string.IsNullOrWhiteSpace(messageEvent.MessageRef))
                            {
                                if (!model.containsMessageId(messageEvent.MessageRef))
                                {
                                    Message message = new Message(messageEvent.MessageRef, messageEvent.MessageRef, null);
                                    model.addMessage(message);
                                }
                            }
                        }
                    }
                }
            }

            foreach (Signal signal in model.Signals)
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_SIGNAL);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, signal.Id);
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, signal.Name);
                if (signal.Scope != null)
                {
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_SCOPE, signal.Scope);
                }
                xtw.writeEndElement();
            }

            foreach (Message message in model.Messages)
            {
                xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MESSAGE);
                string messageId = message.Id;
                // remove the namespace from the message id if set
                if (model.TargetNamespace != null && messageId.StartsWith(model.TargetNamespace, StringComparison.Ordinal))
                {
                    messageId = messageId.Replace(model.TargetNamespace, "");
                    messageId = messageId.replaceFirst(":", "");
                }
                else
                {
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.getNamespace(prefix);
                        if (messageId.StartsWith(@namespace, StringComparison.Ordinal))
                        {
                            messageId = messageId.Replace(model.TargetNamespace, "");
                            messageId = prefix + messageId;
                        }
                    }
                }
                xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID, messageId);
                if (!string.IsNullOrWhiteSpace(message.Name))
                {
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME, message.Name);
                }
                if (!string.IsNullOrWhiteSpace(message.ItemRef))
                {
                    // replace the namespace by the right prefix
                    string itemRef = message.ItemRef;
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.getNamespace(prefix);
                        if (itemRef.StartsWith(@namespace, StringComparison.Ordinal))
                        {
                            if (prefix.Length == 0)
                            {
                                itemRef = itemRef.Replace(@namespace + ":", "");
                            }
                            else
                            {
                                itemRef = itemRef.Replace(@namespace, prefix);
                            }
                            break;
                        }
                    }
                    xtw.writeAttribute(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ITEM_REF, itemRef);
                }
                xtw.writeEndElement();
            }
        }
    }

}