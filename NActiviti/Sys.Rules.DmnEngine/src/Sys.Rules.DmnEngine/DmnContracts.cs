using System;
using System.Collections.Generic;

namespace Sys.Rules.DmnEngine
{
    /// <summary>
    /// 描述内核加载后的决策定义。
    /// </summary>
    public sealed class DmnDefinitionDescriptor
    {
        /// <summary>
        /// 决策主键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 解析后的决策名称。
        /// </summary>
        public string? DecisionName { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 来源位置。
        /// </summary>
        public string? DefinitionSource { get; set; }
    }

    /// <summary>
    /// 描述内核执行请求。
    /// </summary>
    public sealed class DmnExecutionRequest
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
        /// 决策类型约束。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 执行模式。
        /// </summary>
        public string? ExecutionMode { get; set; }

        /// <summary>
        /// 是否启用解释追踪。
        /// </summary>
        public bool? TraceEnabled { get; set; }

        /// <summary>
        /// 执行超时时间（毫秒）。
        /// </summary>
        public int? TimeoutMs { get; set; }

        /// <summary>
        /// 输入参数。
        /// </summary>
        public IDictionary<string, object?> Inputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 跟踪编号。
        /// </summary>
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// 描述内核执行结果。
    /// </summary>
    public sealed class DmnExecutionResult
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
        /// 是否执行成功。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 决策类型。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 实际命中的版本标签。
        /// </summary>
        public string? ResolvedVersionTag { get; set; }

        /// <summary>
        /// 是否启用了解释追踪。
        /// </summary>
        public bool TraceEnabled { get; set; }

        /// <summary>
        /// 执行跟踪明细。
        /// </summary>
        public IList<string> TraceEntries { get; set; } = new List<string>();

        /// <summary>
        /// 输出参数。
        /// </summary>
        public IDictionary<string, object?> Outputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 命中的规则集合。
        /// </summary>
        public IList<string> MatchedRules { get; set; } = new List<string>();

        /// <summary>
        /// 执行解释。
        /// </summary>
        public string? Explanation { get; set; }

        /// <summary>
        /// 错误消息。
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
