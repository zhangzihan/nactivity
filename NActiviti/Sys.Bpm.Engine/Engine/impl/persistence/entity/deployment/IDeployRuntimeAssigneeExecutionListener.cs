using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.engine.impl.persistence.entity
{
    /// <summary>
    /// 添加运行时分配人员事件侦听
    /// </summary>
    public interface IDeployRuntimeAssigneeExecutionListener
    {
        /// <summary>
        /// 添加运行时分配人员事件侦听
        /// </summary>
        /// <param name="userTasks"></param>
        /// <returns></returns>
        bool AddRuntimeAssigneeExcutionListener(IEnumerable<XElement> userTasks);
    }
}
