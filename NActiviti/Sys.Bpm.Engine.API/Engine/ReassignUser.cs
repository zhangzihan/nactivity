using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Services.Api.Commands
{
    /// <summary>
    /// 重新分配任务处理人
    /// </summary>
    public class ReassignUser
    {
        /// <summary>
        /// 任务id
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 从哪个人
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 分配到哪个人
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 重分配原因
        /// </summary>
        public string Reason { get; set; }
    }
}
