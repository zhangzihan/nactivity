using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Impl
{
    /// <summary>
    /// 运行时动态获取用户变量
    /// </summary>
    public class RuntimeAssigneeUser
    {
        /// <summary>
        /// 用户指定的办理人列表
        /// </summary>
        public string[] Users { get; set; }

        /// <summary>
        /// 流程变量字段
        /// </summary>
        public string Field { get; set; } = "runtimeUsers";
    }
}
