using org.activiti.bpmn.constants;
using org.activiti.bpmn.model;
using org.activiti.engine.@delegate;
using org.activiti.engine.impl.bpmn.listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace org.activiti.engine.impl.persistence.entity
{
    /// <summary>
    /// 添加运行时分配人员事件侦听
    /// </summary>
    public class DeployMultilInstanceExecutionListener : IDeployMultilInstanceExecutionListener
    {
        /// <summary>
        /// 添加运行时分配人员事件侦听
        /// </summary>
        /// <param name="userTasks"></param>
        /// <returns></returns>
        public bool AddMultilinstanceExcutionListener(IEnumerable<XElement> userTasks)
        {
            bool changed = false;

            foreach (XElement task in userTasks)
            {
                XElement extElem = task.GetOrAddExtensionElements();

                Type taskListenerType = typeof(DelegateCountersignExecutionListener);
                XElement startListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                          where
                     BpmnXMLConstants.ATTRIBUTE_EVENT_START_VALUE.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
                     (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(taskListenerType.FullName)).GetValueOrDefault()
                                          select x).FirstOrDefault();

                if (startListener != null)
                {
                    continue;
                }

                startListener = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE),
                    /*添加 listener attribute */
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{taskListenerType.FullName},{taskListenerType.Assembly.GetName().Name}"),
                    /*添加 listener 事件*/
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BpmnXMLConstants.ATTRIBUTE_EVENT_START_VALUE));
                extElem.Add(startListener);

                changed = true;
            }

            return changed;
        }
    }
}
