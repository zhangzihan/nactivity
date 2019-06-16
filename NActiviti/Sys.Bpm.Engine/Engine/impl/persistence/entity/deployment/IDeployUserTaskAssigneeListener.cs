using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <summary>
    /// 添加一个分配用户时的侦听器，用来从系统中获取执行人信息
    /// </summary>
    public interface IDeployUserTaskAssigneeListener
    {
        /// <summary>
        /// 添加一个分配用户时的侦听器，用来从系统中获取执行人信息
        /// </summary>
        /// <param name="userTasks"></param>
        /// <returns></returns>
        bool AddUserTaskAssignmentListener(IEnumerable<XElement> userTasks);
    }
}
