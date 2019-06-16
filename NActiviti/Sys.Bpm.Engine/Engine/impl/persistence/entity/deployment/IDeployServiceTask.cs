using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <summary>
    /// 添加服务节点的默认实现
    /// </summary>
    public interface IDeployServiceTask
    {
        /// <summary>
        /// 添加服务节点的默认实现
        /// </summary>
        bool AddDefaultImplementation(IEnumerable<XElement> tasks);
    }
}
