using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Api.Model
{
    /// <summary>
    /// 流程实例化任务查询对象
    /// </summary>
    public class ProcessInstanceTaskQuery : AbstractQuery
    {
        /// <summary>
        /// 流程实例id
        /// </summary>
        public string ProcessInstanceId { get; set; }

        /// <summary>
        /// 流程启动业务键值
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 包含已完成的实例
        /// </summary>
        public bool IncludeCompleted { get; set; } = true;
    }
}
