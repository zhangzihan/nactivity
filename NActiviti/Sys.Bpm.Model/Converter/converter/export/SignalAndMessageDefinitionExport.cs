using System;

namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    public class SignalAndMessageDefinitionExport : IBpmnXMLConstants
    {
        public static void WriteSignalsAndMessages(BpmnModel model, XMLStreamWriter xtw)
        {

            foreach (Process process in model.Processes)
            {
                foreach (FlowElement flowElement in process.FindFlowElementsOfType<Event>())
                {
                    Event @event = (Event)flowElement;
                    if (@event.EventDefinitions.Count > 0)
                    {
                        EventDefinition eventDefinition = @event.EventDefinitions[0];
                        if (eventDefinition is SignalEventDefinition signalEvent)
                        {
                            if (!string.IsNullOrWhiteSpace(signalEvent.SignalRef))
                            {
                                if (!model.ContainsSignalId(signalEvent.SignalRef))
                                {
                                    Signal signal = new Signal(signalEvent.SignalRef, signalEvent.SignalRef);
                                    model.AddSignal(signal);
                                }
                            }
                        }
                        else if (eventDefinition is MessageEventDefinition messageEvent)
                        {
                            if (!string.IsNullOrWhiteSpace(messageEvent.MessageRef))
                            {
                                if (!model.ContainsMessageId(messageEvent.MessageRef))
                                {
                                    Message message = new Message(messageEvent.MessageRef, messageEvent.MessageRef, null);
                                    model.AddMessage(message);
                                }
                            }
                        }
                    }
                }
            }

            foreach (Signal signal in model.Signals)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_SIGNAL, BpmnXMLConstants.BPMN2_NAMESPACE);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, signal.Id);
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, signal.Name);
                if (signal.Scope != null)
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_SCOPE, signal.Scope);
                }
                xtw.WriteEndElement();
            }

            foreach (Message message in model.Messages)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_MESSAGE, BpmnXMLConstants.BPMN2_NAMESPACE);
                string messageId = message.Id;
                // remove the namespace from the message id if set
                if (model.TargetNamespace != null && messageId.StartsWith(model.TargetNamespace, StringComparison.Ordinal))
                {
                    messageId = messageId.Replace(model.TargetNamespace, "");
                    messageId = messageId.ReplaceFirst(":", "");
                }
                else
                {
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.GetNamespace(prefix);
                        if (messageId.StartsWith(@namespace, StringComparison.Ordinal))
                        {
                            messageId = messageId.Replace(model.TargetNamespace, "");
                            messageId = prefix + messageId;
                        }
                    }
                }
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, messageId);
                if (!string.IsNullOrWhiteSpace(message.Name))
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, message.Name);
                }
                if (!string.IsNullOrWhiteSpace(message.ItemRef))
                {
                    // replace the namespace by the right prefix
                    string itemRef = message.ItemRef;
                    foreach (string prefix in model.Namespaces.Keys)
                    {
                        string @namespace = model.GetNamespace(prefix);
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
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ITEM_REF, itemRef);
                }
                xtw.WriteEndElement();
            }
        }
    }

}