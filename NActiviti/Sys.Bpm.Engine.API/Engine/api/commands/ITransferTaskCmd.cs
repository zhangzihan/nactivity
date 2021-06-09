using System;
using System.Collections.Generic;

namespace Sys.Workflow.Services.Api.Commands
{
    /// <summary>
    /// 任务转派数据传输对象
    /// </summary>
    public interface ITransferTaskCmd
    {
        /// <summary>
        /// 转派人列表
        /// </summary>
        string[] Assignees { get; set; }

        /// <summary>
        /// 转派原因描述
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 转派任务过期时间
        /// </summary>
        DateTime? DueDate { get; set; }

        /// <summary>
        /// 转派任务名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 转派任务优先级
        /// </summary>
        int? Priority { get; set; }

        /// <summary>
        /// 被转派任务id
        /// </summary>
        string TaskId { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        string TenantId { get; set; }

        /// <summary>
        /// 流程变量
        /// </summary>
        WorkflowVariable Variables { get; set; }
        IDictionary<string, object> TransientVariables { get; set; }
    }
}