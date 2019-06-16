using Sys.Workflow.bpmn.constants;
using Sys.Workflow.engine.@delegate;
using Sys.Workflow.engine.impl.bpmn.listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.engine.impl.persistence.entity
{
    /// <summary>
    /// 添加一个分配用户时的侦听器，用来从系统中获取执行人信息
    /// </summary>
    class DeployUserTaskAssigneeListener : IDeployUserTaskAssigneeListener
    {
        /// <summary>
        /// 添加一个分配用户时的侦听器，用来从系统中获取执行人信息
        /// </summary>
        /// <param name="userTasks"></param>
        /// <returns></returns>
        public bool AddUserTaskAssignmentListener(IEnumerable<XElement> userTasks)
        {
            bool changed = false;

            foreach (XElement task in userTasks)
            {
                XElement extElem = task.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

                if (extElem == null)
                {
                    extElem = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

                    task.Add(extElem);
                }

                Type taskListenerType = typeof(UserTaskAssignmentListener);
                XElement startListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_TASK_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                          where
                     BaseTaskListenerFields.EVENTNAME_ASSIGNMENT.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
                     (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(taskListenerType.FullName)).GetValueOrDefault()
                                          select x).FirstOrDefault();

                if (startListener != null)
                {
                    continue;
                }

                startListener = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_TASK_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{taskListenerType.FullName},{taskListenerType.Assembly.GetName().Name}"),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BaseTaskListenerFields.EVENTNAME_ASSIGNMENT));
                extElem.Add(startListener);

                changed = true;
            }

            return changed;
        }
    }
}
