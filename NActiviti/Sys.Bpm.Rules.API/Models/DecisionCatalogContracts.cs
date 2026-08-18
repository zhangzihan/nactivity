using System;
using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述目录查询分页结果。
    /// </summary>
    /// <typeparam name="TItem">结果项类型。</typeparam>
    public sealed class DecisionCatalogPageResult<TItem>
    {
        /// <summary>
        /// 当前页结果集合。
        /// </summary>
        public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

        /// <summary>
        /// 总记录数。
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 页码。
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小。
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 描述单个决策绑定引用。
    /// </summary>
    public sealed class DecisionBindingReference
    {
        /// <summary>
        /// 来源标识。
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

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
    /// 描述决策绑定校验请求。
    /// </summary>
    public sealed class DecisionBindingValidationRequest
    {
        /// <summary>
        /// 待校验绑定集合。
        /// </summary>
        public IReadOnlyList<DecisionBindingReference> Bindings { get; set; } = Array.Empty<DecisionBindingReference>();
    }

    /// <summary>
    /// 描述按决策键查询流程引用的目录查询条件。
    /// </summary>
    public sealed class DecisionProcessReferenceCatalogQuery
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
        /// 是否包含草稿流程。
        /// </summary>
        public bool IncludeDrafts { get; set; } = true;

        /// <summary>
        /// 候选部署标识集合。
        /// </summary>
        public IReadOnlyList<string> DeploymentIds { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// 描述规则任务对决策的引用详情。
    /// </summary>
    public sealed class DecisionProcessReferenceTaskDescriptor
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
    /// 描述流程对指定决策的引用记录。
    /// </summary>
    public sealed class DecisionProcessReferenceDescriptor
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
        public DateTime DeployTime { get; set; }

        /// <summary>
        /// 当前流程中的引用任务集合。
        /// </summary>
        public IReadOnlyList<DecisionProcessReferenceTaskDescriptor> ReferencedTasks { get; set; } =
            Array.Empty<DecisionProcessReferenceTaskDescriptor>();
    }
}
