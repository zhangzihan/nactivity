using System;
using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述决策定义的持久化主记录。
    /// </summary>
    public sealed class DecisionDefinitionRecord
    {
        /// <summary>
        /// 决策定义主键标识。
        /// </summary>
        public string DecisionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 决策名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 决策分类。
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 决策描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 决策类型。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 源格式，如 DMN。
        /// </summary>
        public string SourceFormat { get; set; } = "DMN";

        /// <summary>
        /// 定义状态，如 Draft、Published、Disabled。
        /// </summary>
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// 当前草稿版本标签。
        /// </summary>
        public string? DraftVersionTag { get; set; }

        /// <summary>
        /// 最新已发布版本标签。
        /// </summary>
        public string? PublishedVersionTag { get; set; }

        /// <summary>
        /// 标签集合。
        /// </summary>
        public IList<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 创建人。
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }

        /// <summary>
        /// 最后修改人。
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// 最后修改时间。
        /// </summary>
        public DateTimeOffset ModifiedTime { get; set; }
    }

    /// <summary>
    /// 描述单个决策版本的持久化记录。
    /// </summary>
    public sealed class DecisionVersionRecord
    {
        /// <summary>
        /// 决策版本主键标识。
        /// </summary>
        public string VersionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策定义主键标识。
        /// </summary>
        public string DecisionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 递增版本号。
        /// </summary>
        public long VersionNumber { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 是否为已发布版本。
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// 是否为当前草稿。
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 决策定义内容。
        /// </summary>
        public string DefinitionContent { get; set; } = string.Empty;

        /// <summary>
        /// 决策定义校验摘要。
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// 发布说明。
        /// </summary>
        public string? PublishComment { get; set; }

        /// <summary>
        /// 发布人。
        /// </summary>
        public string? PublishedBy { get; set; }

        /// <summary>
        /// 发布时间。
        /// </summary>
        public DateTimeOffset? PublishedTime { get; set; }

        /// <summary>
        /// 创建人。
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// 描述决策定义仓储查询条件。
    /// </summary>
    public sealed class DecisionDefinitionRepositoryQuery
    {
        /// <summary>
        /// 关键字。
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 分类过滤。
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 决策类型过滤。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 状态过滤。
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 页码。
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小。
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 描述运行时版本解析请求。
    /// </summary>
    public sealed class DecisionVersionResolutionRequest
    {
        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 请求版本标签。
        /// </summary>
        public string? VersionTag { get; set; }
    }

    /// <summary>
    /// 描述运行时版本解析结果。
    /// </summary>
    public sealed class DecisionVersionResolutionResult
    {
        /// <summary>
        /// 决策定义主键标识。
        /// </summary>
        public string DecisionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策版本主键标识。
        /// </summary>
        public string VersionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 实际命中的版本标签。
        /// </summary>
        public string VersionTag { get; set; } = string.Empty;

        /// <summary>
        /// 实际命中的版本号。
        /// </summary>
        public long VersionNumber { get; set; }

        /// <summary>
        /// 定义内容。
        /// </summary>
        public string DefinitionContent { get; set; } = string.Empty;

        /// <summary>
        /// 定义摘要。
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// 定义来源标识。
        /// </summary>
        public string? DefinitionSource { get; set; }
    }

    /// <summary>
    /// 描述决策执行日志持久化记录。
    /// </summary>
    public sealed class DecisionExecutionLogRecord
    {
        /// <summary>
        /// 执行记录主键标识。
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 请求版本标签。
        /// </summary>
        public string? RequestedVersionTag { get; set; }

        /// <summary>
        /// 实际命中版本标签。
        /// </summary>
        public string? ResolvedVersionTag { get; set; }

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 是否执行成功。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 跟踪标识。
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// 执行人。
        /// </summary>
        public string? Executor { get; set; }

        /// <summary>
        /// 错误消息。
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 命中规则集合。
        /// </summary>
        public IList<string> MatchedRules { get; set; } = new List<string>();

        /// <summary>
        /// 输入快照。
        /// </summary>
        public IDictionary<string, object?>? InputSnapshot { get; set; }

        /// <summary>
        /// 输出快照。
        /// </summary>
        public IDictionary<string, object?>? OutputSnapshot { get; set; }

        /// <summary>
        /// 轨迹明细集合。
        /// </summary>
        public IList<string> TraceEntries { get; set; } = new List<string>();

        /// <summary>
        /// 执行时间。
        /// </summary>
        public DateTimeOffset ExecutedTime { get; set; }

        /// <summary>
        /// 耗时毫秒。
        /// </summary>
        public int? ElapsedMs { get; set; }
    }

    /// <summary>
    /// 描述决策执行日志查询条件。
    /// </summary>
    public sealed class DecisionExecutionLogQuery
    {
        /// <summary>
        /// 决策唯一键过滤。
        /// </summary>
        public string? DecisionKey { get; set; }

        /// <summary>
        /// 请求版本标签过滤。
        /// </summary>
        public string? RequestedVersionTag { get; set; }

        /// <summary>
        /// 实际命中版本标签过滤。
        /// </summary>
        public string? ResolvedVersionTag { get; set; }

        /// <summary>
        /// 绑定模式过滤。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 跟踪标识过滤。
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// 执行人过滤。
        /// </summary>
        public string? Executor { get; set; }

        /// <summary>
        /// 是否成功过滤。
        /// </summary>
        public bool? Succeeded { get; set; }

        /// <summary>
        /// 开始执行时间。
        /// </summary>
        public DateTimeOffset? StartExecutedTime { get; set; }

        /// <summary>
        /// 结束执行时间。
        /// </summary>
        public DateTimeOffset? EndExecutedTime { get; set; }

        /// <summary>
        /// 页码。
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小。
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 描述决策导入请求。
    /// </summary>
    public sealed class DecisionImportRequest
    {
        /// <summary>
        /// 导入格式。
        /// </summary>
        public string Format { get; set; } = "DMN";

        /// <summary>
        /// 原始文件名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 定义内容。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 是否覆盖已有定义。
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// 决策主键；为空时尝试从文件名推断。
        /// </summary>
        public string? DecisionKey { get; set; }

        /// <summary>
        /// 决策名称；为空时尝试使用决策主键。
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 导入后的版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 操作人。
        /// </summary>
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 描述决策导入结果。
    /// </summary>
    public sealed class DecisionImportResult
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 已导入决策主键集合。
        /// </summary>
        public IList<string> ImportedDecisionKeys { get; set; } = new List<string>();

        /// <summary>
        /// 告警集合。
        /// </summary>
        public IList<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// 错误集合。
        /// </summary>
        public IList<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// 描述决策导出请求。
    /// </summary>
    public sealed class DecisionExportRequest
    {
        /// <summary>
        /// 决策主键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 导出格式。
        /// </summary>
        public string Format { get; set; } = "DMN";
    }

    /// <summary>
    /// 描述决策导出结果。
    /// </summary>
    public sealed class DecisionExportResult
    {
        /// <summary>
        /// 文件名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 内容类型。
        /// </summary>
        public string ContentType { get; set; } = "application/xml";

        /// <summary>
        /// 导出内容。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 内容校验摘要。
        /// </summary>
        public string? Checksum { get; set; }
    }

    /// <summary>
    /// 描述决策执行输入输出快照。
    /// </summary>
    public sealed class DecisionExecutionSnapshot
    {
        /// <summary>
        /// 执行记录标识。
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// 决策主键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 输入快照。
        /// </summary>
        public IDictionary<string, object?>? InputSnapshot { get; set; }

        /// <summary>
        /// 输出快照。
        /// </summary>
        public IDictionary<string, object?>? OutputSnapshot { get; set; }

        /// <summary>
        /// 执行时间。
        /// </summary>
        public DateTimeOffset ExecutedTime { get; set; }
    }

    /// <summary>
    /// 描述缓存失效请求。
    /// </summary>
    public sealed class DecisionCacheInvalidationRequest
    {
        /// <summary>
        /// 决策主键。
        /// </summary>
        public string? DecisionKey { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 失效范围。
        /// </summary>
        public string? Scope { get; set; }
    }

    /// <summary>
    /// 描述缓存状态。
    /// </summary>
    public sealed class DecisionCacheStatusDescriptor
    {
        /// <summary>
        /// 决策主键。
        /// </summary>
        public string? DecisionKey { get; set; }

        /// <summary>
        /// 已缓存版本集合。
        /// </summary>
        public IList<string> CachedVersions { get; set; } = new List<string>();

        /// <summary>
        /// 最近刷新时间。
        /// </summary>
        public DateTimeOffset? LastRefreshTime { get; set; }

        /// <summary>
        /// 缓存提供方。
        /// </summary>
        public string CacheProvider { get; set; } = string.Empty;
    }
}
