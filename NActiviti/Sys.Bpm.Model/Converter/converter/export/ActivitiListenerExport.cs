using System.Collections.Generic;

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
namespace Sys.Workflow.bpmn.converter.export
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    public class ActivitiListenerExport : IBpmnXMLConstants
    {
        public static bool WriteListeners(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            if (element is IHasExecutionListeners)
            {
                didWriteExtensionStartElement = WriteListeners(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, ((IHasExecutionListeners)element).ExecutionListeners, didWriteExtensionStartElement, xtw);
            }
            // In case of a usertaks, also add task-listeners
            if (element is UserTask)
            {
                didWriteExtensionStartElement = WriteListeners(BpmnXMLConstants.ELEMENT_TASK_LISTENER, ((UserTask)element).TaskListeners, didWriteExtensionStartElement, xtw);
            }

            // In case of a process-element, write the event-listeners
            if (element is Process)
            {
                didWriteExtensionStartElement = WriteEventListeners(((Process)element).EventListeners, didWriteExtensionStartElement, xtw);
            }

            return didWriteExtensionStartElement;
        }
        protected internal static bool WriteEventListeners(IList<EventListener> eventListeners, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {

            if (eventListeners != null && eventListeners.Count > 0)
            {
                foreach (EventListener eventListener in eventListeners)
                {
                    if (!didWriteExtensionStartElement)
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                        didWriteExtensionStartElement = true;
                    }

                    xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_EVENT_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENTS, eventListener.Events, xtw);
                    BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_ENTITY_TYPE, eventListener.EntityType, xtw);

                    if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, eventListener.Implementation, xtw);

                    }
                    else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_DELEGATEEXPRESSION, eventListener.Implementation, xtw);

                    }
                    else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_SIGNAL_EVENT.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_SIGNAL_EVENT_NAME, eventListener.Implementation, xtw);
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE, BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE_SIGNAL, xtw);

                    }
                    else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_GLOBAL_SIGNAL_EVENT.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_SIGNAL_EVENT_NAME, eventListener.Implementation, xtw);
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE, BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE_GLOBAL_SIGNAL, xtw);

                    }
                    else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_MESSAGE_EVENT.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_MESSAGE_EVENT_NAME, eventListener.Implementation, xtw);
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE, BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE_MESSAGE, xtw);

                    }
                    else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_ERROR_EVENT.Equals(eventListener.ImplementationType))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_ERROR_EVENT_CODE, eventListener.Implementation, xtw);
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE, BpmnXMLConstants.ATTRIBUTE_LISTENER_THROW_EVENT_TYPE_ERROR, xtw);
                    }

                    xtw.WriteEndElement();
                }
            }

            return didWriteExtensionStartElement;
        }
        private static bool WriteListeners(string xmlElementName, IList<ActivitiListener> listenerList, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            if (listenerList != null)
            {

                foreach (ActivitiListener listener in listenerList)
                {

                    if (!string.IsNullOrWhiteSpace(listener.Event))
                    {

                        if (!didWriteExtensionStartElement)
                        {
                            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                            didWriteExtensionStartElement = true;
                        }

                        xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, xmlElementName, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, listener.Event, xtw);

                        if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(listener.ImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, listener.Implementation, xtw);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(listener.ImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EXPRESSION, listener.Implementation, xtw);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(listener.ImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_DELEGATEEXPRESSION, listener.Implementation, xtw);
                        }

                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_ON_TRANSACTION, listener.OnTransaction, xtw);

                        if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(listener.CustomPropertiesResolverImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_CLASS, listener.CustomPropertiesResolverImplementation, xtw);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(listener.CustomPropertiesResolverImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_EXPRESSION, listener.CustomPropertiesResolverImplementation, xtw);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(listener.CustomPropertiesResolverImplementationType))
                        {
                            BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_DELEGATEEXPRESSION, listener.CustomPropertiesResolverImplementation, xtw);
                        }

                        FieldExtensionExport.WriteFieldExtensions(listener.FieldExtensions, true, xtw);

                        xtw.WriteEndElement();
                    }
                }
            }
            return didWriteExtensionStartElement;
        }

    }

}