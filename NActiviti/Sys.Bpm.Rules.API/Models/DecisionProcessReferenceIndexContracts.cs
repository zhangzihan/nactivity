using System;
using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述单个规则任务对决策的引用索引记录。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexTaskRecord
    {
        /// <summary>
        /// 规则任务标识。
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// 规则任务名称。
        /// </summary>
        public string? TaskName { get; set; }

        /// <summary>
        /// 决策键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }
    }

    /// <summary>
    /// 描述单个流程定义的决策引用索引快照。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexProcessRecord
    {
        /// <summary>
        /// 流程定义键。
        /// </summary>
        public string ProcessDefinitionKey { get; set; } = string.Empty;

        /// <summary>
        /// 流程定义名称。
        /// </summary>
        public string? ProcessDefinitionName { get; set; }

        /// <summary>
        /// 该流程下命中的规则任务列表。
        /// </summary>
        public IList<DecisionProcessReferenceIndexTaskRecord> ReferencedTasks { get; set; } =
            new List<DecisionProcessReferenceIndexTaskRecord>();
    }

    /// <summary>
    /// 描述单个部署的决策引用索引快照。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexSnapshot
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
        /// 租户标识。
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// 是否为草稿部署。
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 部署时间。
        /// </summary>
        public DateTimeOffset DeployTime { get; set; }

        /// <summary>
        /// 当前部署下各流程的引用快照。
        /// </summary>
        public IList<DecisionProcessReferenceIndexProcessRecord> Processes { get; set; } =
            new List<DecisionProcessReferenceIndexProcessRecord>();
    }

    /// <summary>
    /// 描述引用索引查询条件。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexQuery
    {
        /// <summary>
        /// 决策键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 租户标识。
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// 是否包含草稿部署。
        /// </summary>
        public bool IncludeDrafts { get; set; } = true;

        /// <summary>
        /// 限定的部署标识集合。
        /// </summary>
        public IList<string> DeploymentIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// 描述引用索引表中的单行记录。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexEntryRecord
    {
        /// <summary>
        /// 索引行标识。
        /// </summary>
        public string ReferenceId { get; set; } = string.Empty;

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
        public string? ProcessDefinitionKey { get; set; }

        /// <summary>
        /// 流程定义名称。
        /// </summary>
        public string? ProcessDefinitionName { get; set; }

        /// <summary>
        /// 租户标识。
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// 是否为草稿部署。
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 部署时间。
        /// </summary>
        public DateTimeOffset DeployTime { get; set; }

        /// <summary>
        /// 决策键；哨兵行允许为空。
        /// </summary>
        public string? DecisionKey { get; set; }

        /// <summary>
        /// 规则任务标识；哨兵行允许为空。
        /// </summary>
        public string? TaskId { get; set; }

        /// <summary>
        /// 规则任务名称。
        /// </summary>
        public string? TaskName { get; set; }

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 索引写入时间。
        /// </summary>
        public DateTimeOffset IndexedTime { get; set; }
    }
}
