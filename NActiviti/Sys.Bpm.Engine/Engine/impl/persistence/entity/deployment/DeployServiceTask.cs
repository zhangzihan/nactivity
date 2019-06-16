using Sys.Workflow.bpmn.constants;
using Sys.Workflow.bpmn.model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.engine.impl.persistence.entity
{
    /// <summary>
    /// 添加服务节点的默认实现
    /// </summary>
    public class DeployServiceTask : IDeployServiceTask
    {
        /// <summary>
        /// 添加服务节点的默认实现
        /// </summary>
        public bool AddDefaultImplementation(IEnumerable<XElement> tasks)
        {
            bool changed = false;
            foreach (XElement task in tasks ?? new XElement[0])
            {
                var impl = task.Attribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));
                if (impl == null)
                {
                    task.Add(new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_CLASS, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), ImplementationType.IMPLEMENTATION_TASK_SERVICE_DEFAULT));
                    changed = true;
                }
            }

            return changed;
        }
    }
}
