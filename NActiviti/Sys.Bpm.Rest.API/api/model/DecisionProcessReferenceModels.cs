using System;
using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Api.Model
{
    /// <summary>
    /// 描述按决策键查询流程引用的请求参数。
    /// </summary>
    public sealed class DecisionProcessReferenceQuery
    {
        /// <summary>
        /// 决策键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 租户标识。
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 是否包含草稿流程。
        /// </summary>
        public bool IncludeDrafts { get; set; } = true;
    }

    /// <summary>
    /// 描述单个规则任务对决策的引用详情。
    /// </summary>
    public sealed class DecisionProcessReferenceTaskRecord
    {
        /// <summary>
        /// 规则任务标识。
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// 规则任务名称。
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 决策键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string BindingMode { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string VersionTag { get; set; }
    }

    /// <summary>
    /// 描述某个流程对指定决策的引用记录。
    /// </summary>
    public sealed class DecisionProcessReferenceRecord
    {
        /// <summary>
        /// 部署标识。
        /// </summary>
        public string DeploymentId { get; set; } = string.Empty;

        /// <summary>
        /// 部署名称。
        /// </summary>
        public string DeploymentName { get; set; } = string.Empty;

        /// <summary>
        /// 流程定义键。
        /// </summary>
        public string ProcessDefinitionKey { get; set; } = string.Empty;

        /// <summary>
        /// 流程定义名称。
        /// </summary>
        public string ProcessDefinitionName { get; set; }

        /// <summary>
        /// 租户标识。
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 是否为草稿部署。
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 部署时间。
        /// </summary>
        public DateTime DeployTime { get; set; }

        /// <summary>
        /// 当前流程中的引用任务集合。
        /// </summary>
        public IList<DecisionProcessReferenceTaskRecord> ReferencedTasks { get; set; } =
            new List<DecisionProcessReferenceTaskRecord>();
    }
}
