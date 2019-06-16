using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <summary>
    /// 转换用户任务为并行多任务
    /// </summary>
    public interface IDeployUserTaskToParallel
    {
        /// <summary>
        /// 修改用户任务节点为并行实例，解决追加节点操作
        /// </summary>
        bool ConvertUserTaskToParallel(IEnumerable<XElement> userTasks);
    }
}
