using org.activiti.bpmn.constants;
using org.activiti.engine.@delegate;
using org.activiti.engine.impl.bpmn.listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace org.activiti.engine.impl.persistence.entity
{
    /// <inheritdoc />
    class DeployRuntimeAssigneeExecutionListener : IDeployRuntimeAssigneeExecutionListener
    {
        /// <inheritdoc />
        public bool AddRuntimeAssigneeExcutionListener(IEnumerable<XElement> usertTasks)
        {
            /*<bpmn2:extensionElements>
             * <camunda:properties>
                  <camunda:property name="runtimeAssignee" value="true" />
                  <camunda:property name="assigneeVariable" value="dynamicUsers" />
                </camunda:properties>
                <camunda:executionListener class="org.activiti.engine.impl.bpmn.listener.RuntimeAssigneeExecutionListener,Sys.Bpm.Engine" event="start" />
                <camunda:executionListener class="org.activiti.engine.impl.bpmn.listener.RuntimeAssigneeExecutionEndedListener,Sys.Bpm.Engine" event="end" />
              </bpmn2:extensionElements>
           */
            bool changed = false;

            foreach (XElement task in usertTasks ?? new XElement[0])
            {
                XElement extElem = task.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

                if (extElem != null)
                {
                    changed = AddStartListener(extElem) | AddEndedListener(extElem);
                }
            }

            return changed;
        }

        private bool AddEndedListener(XElement extElem)
        {
            Type endListenerType = typeof(RuntimeAssigneeExecutionEndedListener);
            XElement endListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                    where
               BaseExecutionListenerFields.EVENTNAME_END.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
               (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(endListenerType.FullName)).GetValueOrDefault()
                                    select x).FirstOrDefault();

            if (endListener != null)
            {
                return false;
            }

            IEnumerable<XElement> eProps = extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTIES, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

            XElement assignee = (from x in eProps.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                 where BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_DATA_NAME)?.Value, StringComparison.OrdinalIgnoreCase)
                                 select x).FirstOrDefault();

            if (assignee != null && bool.TryParse(assignee.Attribute(BpmnXMLConstants.ELEMENT_DATA_VALUE)?.Value, out bool isRuntime) && isRuntime)
            {
                endListener = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{endListenerType.FullName},{endListenerType.Assembly.GetName().Name}"),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BaseExecutionListenerFields.EVENTNAME_END));
                extElem.Add(endListener);

                return true;
            }

            return false;
        }

        private bool AddStartListener(XElement extElem)
        {
            Type runListenerType = typeof(RuntimeAssigneeExecutionListener);
            XElement startListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                      where
                 BaseExecutionListenerFields.EVENTNAME_START.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
                 (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(runListenerType.FullName)).GetValueOrDefault()
                                      select x).FirstOrDefault();

            if (startListener != null)
            {
                return false;
            }

            IEnumerable<XElement> eProps = extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTIES, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

            XElement assignee = (from x in eProps.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                 where BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_DATA_NAME)?.Value, StringComparison.OrdinalIgnoreCase)
                                 select x).FirstOrDefault();

            if (assignee != null && bool.TryParse(assignee.Attribute(BpmnXMLConstants.ELEMENT_DATA_VALUE)?.Value, out bool isRuntime) && isRuntime)
            {
                startListener = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{runListenerType.FullName},{runListenerType.Assembly.GetName().Name}"),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BaseExecutionListenerFields.EVENTNAME_START));
                extElem.Add(startListener);

                return true;
            }

            return false;
        }

    }
}
