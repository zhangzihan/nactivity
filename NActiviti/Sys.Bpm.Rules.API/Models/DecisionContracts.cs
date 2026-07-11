using System;
using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述一个可执行的决策定义。
    /// </summary>
    public sealed class DecisionDefinitionDescriptor
    {
        /// <summary>
        /// 决策主键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 决策名称。
        /// </summary>
        public string? DecisionName { get; set; }

        /// <summary>
        /// 版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 定义来源标识。
        /// </summary>
        public string? DefinitionSource { get; set; }
    }

    /// <summary>
    /// 描述一次决策执行请求。
    /// </summary>
    public sealed class DecisionExecutionRequest
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
        /// 调用方传入的输入参数。
        /// </summary>
        public IDictionary<string, object?> Inputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 调用链追踪编号。
        /// </summary>
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// 描述一次决策执行结果。
    /// </summary>
    public sealed class DecisionExecutionResult
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
        /// 决策输出参数。
        /// </summary>
        public IDictionary<string, object?> Outputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 命中的规则标识集合。
        /// </summary>
        public IList<string> MatchedRules { get; set; } = new List<string>();

        /// <summary>
        /// 执行解释信息。
        /// </summary>
        public string? Explanation { get; set; }

        /// <summary>
        /// 失败时的错误消息。
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 描述一次决策解释请求。
    /// </summary>
    public sealed class DecisionExplanationRequest
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
        /// 已有执行记录标识；存在时优先尝试读取审计记录。
        /// </summary>
        public string? ExecutionId { get; set; }

        /// <summary>
        /// 解释时使用的输入参数。
        /// </summary>
        public IDictionary<string, object?> Inputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 决策类型。
        /// </summary>
        public string? DecisionType { get; set; }
    }

    /// <summary>
    /// 描述一次决策解释结果。
    /// </summary>
    public sealed class DecisionExplanationResult
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
        /// 实际命中的版本标签。
        /// </summary>
        public string? ResolvedVersionTag { get; set; }

        /// <summary>
        /// 规则路径。
        /// </summary>
        public IList<string> RulePath { get; set; } = new List<string>();

        /// <summary>
        /// 命中条件集合。
        /// </summary>
        public IList<string> MatchedConditions { get; set; } = new List<string>();

        /// <summary>
        /// 未命中条件集合。
        /// </summary>
        public IList<string> RejectedConditions { get; set; } = new List<string>();

        /// <summary>
        /// 评估步骤集合。
        /// </summary>
        public IList<string> EvaluationSteps { get; set; } = new List<string>();

        /// <summary>
        /// 解释文本。
        /// </summary>
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// 描述一次决策测试请求。
    /// </summary>
    public sealed class DecisionTestRequest
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
        /// 绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 测试用例集合。
        /// </summary>
        public IList<DecisionTestCase> TestCases { get; set; } = new List<DecisionTestCase>();
    }

    /// <summary>
    /// 描述单条决策测试用例。
    /// </summary>
    public sealed class DecisionTestCase
    {
        /// <summary>
        /// 用例名称。
        /// </summary>
        public string CaseName { get; set; } = string.Empty;

        /// <summary>
        /// 输入参数。
        /// </summary>
        public IDictionary<string, object?> Inputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 期望输出。
        /// </summary>
        public IDictionary<string, object?> ExpectedOutputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 描述单条决策测试用例结果。
    /// </summary>
    public sealed class DecisionTestCaseResult
    {
        /// <summary>
        /// 用例名称。
        /// </summary>
        public string CaseName { get; set; } = string.Empty;

        /// <summary>
        /// 是否通过。
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// 执行记录标识。
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// 实际输出。
        /// </summary>
        public IDictionary<string, object?> ActualOutputs { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 失败详情集合。
        /// </summary>
        public IList<string> FailureMessages { get; set; } = new List<string>();
    }

    /// <summary>
    /// 描述一次决策测试结果。
    /// </summary>
    public sealed class DecisionTestResult
    {
        /// <summary>
        /// 是否全部通过。
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// 总用例数。
        /// </summary>
        public int TotalCases { get; set; }

        /// <summary>
        /// 通过用例数。
        /// </summary>
        public int PassedCases { get; set; }

        /// <summary>
        /// 失败用例数。
        /// </summary>
        public int FailedCases { get; set; }

        /// <summary>
        /// 用例结果集合。
        /// </summary>
        public IList<DecisionTestCaseResult> CaseResults { get; set; } = new List<DecisionTestCaseResult>();
    }
}
