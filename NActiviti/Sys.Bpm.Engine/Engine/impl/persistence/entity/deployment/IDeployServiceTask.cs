using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace org.activiti.engine.impl.persistence.entity
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
