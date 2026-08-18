using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Bpmn.Helper;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// 定义基于 DMN 规则运行时的业务规则任务行为。
    /// </summary>
    [Serializable]
    public class DmnBusinessRuleTaskActivityBehavior : TaskActivityBehavior, IBusinessRuleTaskDelegate
    {
        private const string ExplicitDecisionNameInputKey = "__decisionName";
        private const string UseDefaultOutputOptionPrefix = "defaultOutput.";
        private const string ManualInterventionVariableNameOption = "manualIntervention.variableName";
        private const string ManualInterventionErrorCodeOption = "manualIntervention.errorCode";
        private const string ManualInterventionErrorMessageOption = "manualIntervention.errorMessage";
        private const string DefaultManualInterventionVariableName = "decisionManualIntervention";
        private const string DefaultManualInterventionErrorCode = "DMN_MANUAL_INTERVENTION";
        private static readonly ILogger<DmnBusinessRuleTaskActivityBehavior> Logger = ProcessEngineServiceProvider.LoggerService<DmnBusinessRuleTaskActivityBehavior>();
        private readonly IList<IExpression> _ruleVariableInputIdExpressions = new List<IExpression>();
        private readonly IList<IExpression> _ruleIdExpressions = new List<IExpression>();

        /// <summary>
        /// 获取或设置是否排除规则。
        /// </summary>
        public bool Exclude { private get; set; }

        /// <summary>
        /// 获取或设置结果变量名。
        /// </summary>
        public string ResultVariable { private get; set; } = "Sys.Workflow.Engine.rules.OUTPUT";

        /// <summary>
        /// 添加业务规则任务输入变量表达式。
        /// </summary>
        /// <param name="inputId">输入变量表达式。</param>
        public virtual void AddRuleVariableInputIdExpression(IExpression inputId)
        {
            ArgumentNullException.ThrowIfNull(inputId);
            _ruleVariableInputIdExpressions.Add(inputId);
        }

        /// <summary>
        /// 添加业务规则任务规则标识表达式。
        /// </summary>
        /// <param name="inputId">规则标识表达式。</param>
        public virtual void AddRuleIdExpression(IExpression inputId)
        {
            ArgumentNullException.ThrowIfNull(inputId);
            _ruleIdExpressions.Add(inputId);
        }

        /// <summary>
        /// 执行业务规则任务并将结果回写到流程变量。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        public override void Execute(IExecutionEntity execution)
        {
            ArgumentNullException.ThrowIfNull(execution);
            DecisionBinding? decisionBinding = null;

            try
            {
                var decisionExecutionService = ResolveDecisionExecutionService();
                decisionBinding = ResolveDecisionBinding(execution);
                var inputs = CollectDecisionInputs(execution, decisionBinding);

                if (!string.IsNullOrWhiteSpace(decisionBinding.DecisionName))
                {
                    inputs[ExplicitDecisionNameInputKey] = decisionBinding.DecisionName;
                }

                var result = decisionExecutionService.ExecuteAsync(new DecisionExecutionRequest
                {
                    DecisionKey = decisionBinding.DecisionKey,
                    DecisionType = decisionBinding.DecisionType,
                    BindingMode = decisionBinding.BindingMode,
                    ExecutionMode = decisionBinding.ExecutionMode,
                    TraceEnabled = decisionBinding.TraceEnabled,
                    TimeoutMs = decisionBinding.TimeoutMs,
                    VersionTag = decisionBinding.VersionTag,
                    Inputs = inputs,
                    TraceId = execution.Id
                }).GetAwaiter().GetResult();

                if (!result.Succeeded)
                {
                    throw new ActivitiException($"业务规则任务执行失败：{result.ErrorMessage ?? result.Explanation ?? "未提供错误详情。"}");
                }

                ApplyDecisionOutputs(execution, result, decisionBinding);
                Leave(execution);
            }
            catch (Exception exc)
            {
                if (HandleFallbackOnFailure(execution, decisionBinding, exc))
                {
                    return;
                }

                HandleExecutionException(execution, exc);
            }
        }

        /// <summary>
        /// 解析决策执行服务。
        /// </summary>
        /// <returns>决策执行服务实例。</returns>
        protected virtual IDecisionExecutionService ResolveDecisionExecutionService()
        {
            var decisionExecutionService = ProcessEngineServiceProvider.Resolve<IDecisionExecutionService>();
            if (decisionExecutionService == null)
            {
                throw new ActivitiException("未注册 IDecisionExecutionService，无法执行 BusinessRuleTask 的 DMN 决策。");
            }

            return decisionExecutionService;
        }

        /// <summary>
        /// 解析当前任务绑定的 DMN 决策信息。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <returns>决策绑定信息。</returns>
        protected virtual DecisionBinding ResolveDecisionBinding(IExecutionEntity execution)
        {
            var extensionBinding = execution.CurrentFlowElement.GetDecisionBinding();
            if (extensionBinding != null)
            {
                return new DecisionBinding(
                    extensionBinding.DecisionKey,
                    extensionBinding.DecisionName,
                    extensionBinding.DecisionType,
                    extensionBinding.BindingMode,
                    extensionBinding.ExecutionMode,
                    extensionBinding.TraceEnabled,
                    extensionBinding.FallbackPolicy,
                    extensionBinding.TimeoutMs,
                    extensionBinding.VersionTag,
                    extensionBinding.InputMappings,
                    extensionBinding.OutputMappings,
                    extensionBinding.Options);
            }

            if (_ruleIdExpressions.Count == 0)
            {
                throw new ActivitiException("BusinessRuleTask 未配置规则标识，无法解析 DMN 决策。");
            }

            var configuredRuleId = ResolveConfiguredString(_ruleIdExpressions[0], execution);
            if (string.IsNullOrWhiteSpace(configuredRuleId))
            {
                throw new ActivitiException("BusinessRuleTask 规则标识为空，无法解析 DMN 决策。");
            }

            var fragments = configuredRuleId.Split(new[] { '#' }, 2, StringSplitOptions.None);
            var decisionKey = fragments[0].Trim();
            var decisionName = fragments.Length > 1 ? fragments[1].Trim() : null;

            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ActivitiException("BusinessRuleTask 规则标识未包含有效的 decisionKey。");
            }

            return new DecisionBinding(
                decisionKey,
                decisionName,
                decisionType: null,
                bindingMode: null,
                executionMode: null,
                traceEnabled: null,
                fallbackPolicy: null,
                timeoutMs: null,
                versionTag: null,
                inputMappings: Array.Empty<BusinessRuleTaskDecisionInputMapping>(),
                outputMappings: Array.Empty<BusinessRuleTaskDecisionOutputMapping>(),
                options: Array.Empty<BusinessRuleTaskDecisionOption>());
        }

        /// <summary>
        /// 收集决策执行输入。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <returns>决策输入字典。</returns>
        protected virtual IDictionary<string, object?> CollectDecisionInputs(IExecutionEntity execution, DecisionBinding decisionBinding)
        {
            var inputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (decisionBinding.InputMappings.Count > 0)
            {
                foreach (var inputMapping in decisionBinding.InputMappings)
                {
                    inputs[inputMapping.InputName] = ResolveInputMappingValue(execution, inputMapping);
                }

                return inputs;
            }

            if (_ruleVariableInputIdExpressions.Count == 0)
            {
                foreach (var variable in execution.Variables)
                {
                    inputs[variable.Key] = variable.Value;
                }

                return inputs;
            }

            foreach (var inputExpression in _ruleVariableInputIdExpressions)
            {
                var variableName = ResolveConfiguredString(inputExpression, execution);
                if (string.IsNullOrWhiteSpace(variableName))
                {
                    continue;
                }

                inputs[variableName] = execution.GetVariable(variableName, false);
            }

            return inputs;
        }

        /// <summary>
        /// 将决策结果写回流程变量。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="result">决策执行结果。</param>
        /// <param name="decisionBinding">决策绑定配置。</param>
        protected virtual void ApplyDecisionOutputs(
            IExecutionEntity execution,
            DecisionExecutionResult result,
            DecisionBinding decisionBinding,
            bool fallbackApplied = false,
            string? fallbackPolicy = null,
            string? fallbackReason = null,
            IDictionary<string, object?>? fallbackContext = null)
        {
            if (decisionBinding.OutputMappings.Count > 0)
            {
                foreach (var outputMapping in decisionBinding.OutputMappings)
                {
                    ApplyOutputMapping(execution, result, outputMapping);
                }
            }
            else
            {
                foreach (var output in result.Outputs)
                {
                    execution.SetVariable(output.Key, output.Value, false);
                }
            }

            WriteResultVariable(execution, result, fallbackApplied, fallbackPolicy, fallbackReason, fallbackContext);
        }

        /// <summary>
        /// 判断失败时是否允许按跳过策略继续流程。
        /// </summary>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <returns>是否跳过当前规则任务。</returns>
        protected virtual bool CanSkipTaskOnFailure(DecisionBinding? decisionBinding)
        {
            return decisionBinding != null &&
                   string.Equals(decisionBinding.FallbackPolicy, "SkipTask", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 处理失败回退策略。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="exception">执行异常。</param>
        /// <returns>若已处理完成则返回 true。</returns>
        protected virtual bool HandleFallbackOnFailure(IExecutionEntity execution, DecisionBinding? decisionBinding, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(execution);
            ArgumentNullException.ThrowIfNull(exception);

            if (decisionBinding == null || string.IsNullOrWhiteSpace(decisionBinding.FallbackPolicy))
            {
                return false;
            }

            if (CanSkipTaskOnFailure(decisionBinding))
            {
                Logger.LogWarning(exception, "BusinessRuleTask 执行失败，按 SkipTask 策略跳过当前规则任务。ExecutionId={ExecutionId}", execution.Id);
                Leave(execution);
                return true;
            }

            if (string.Equals(decisionBinding.FallbackPolicy, "UseDefault", StringComparison.OrdinalIgnoreCase))
            {
                ApplyUseDefaultFallback(execution, decisionBinding, exception);
                return true;
            }

            if (string.Equals(decisionBinding.FallbackPolicy, "ManualIntervention", StringComparison.OrdinalIgnoreCase))
            {
                ApplyManualInterventionFallback(execution, decisionBinding, exception);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 解析结构化输入映射的实际值。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="inputMapping">输入映射配置。</param>
        /// <returns>映射后的输入值。</returns>
        protected virtual object? ResolveInputMappingValue(IExecutionEntity execution, BusinessRuleTaskDecisionInputMapping inputMapping)
        {
            ArgumentNullException.ThrowIfNull(execution);
            ArgumentNullException.ThrowIfNull(inputMapping);

            if (string.Equals(inputMapping.SourceType, "processVariable", StringComparison.OrdinalIgnoreCase))
            {
                return execution.GetVariable(inputMapping.SourceValue, false);
            }

            if (string.Equals(inputMapping.SourceType, "constant", StringComparison.OrdinalIgnoreCase))
            {
                return inputMapping.SourceValue;
            }

            if (string.Equals(inputMapping.SourceType, "contextPath", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveContextPathValue(execution, inputMapping.SourceValue);
            }

            throw new ActivitiException($"暂不支持的 BusinessRuleTask 输入映射类型：{inputMapping.SourceType}。");
        }

        /// <summary>
        /// 按点路径从流程变量上下文中读取嵌套值。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="contextPath">上下文路径，格式如 initiator.deptId。</param>
        /// <returns>路径对应的值；若路径不存在则返回空。</returns>
        protected virtual object? ResolveContextPathValue(IExecutionEntity execution, string? contextPath)
        {
            ArgumentNullException.ThrowIfNull(execution);

            if (string.IsNullOrWhiteSpace(contextPath))
            {
                return null;
            }

            var pathSegments = contextPath.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length == 0)
            {
                return null;
            }

            var rootValue = execution.GetVariable(pathSegments[0], false);
            if (rootValue == null || pathSegments.Length == 1)
            {
                return rootValue;
            }

            var token = rootValue as JToken;
            if (token == null)
            {
                token = JToken.FromObject(rootValue);
            }

            var selectedToken = token.SelectToken(pathSegments[1], false);
            return ConvertJTokenValue(selectedToken);
        }

        /// <summary>
        /// 将 Json Token 转换为普通 CLR 值。
        /// </summary>
        /// <param name="token">待转换的 Json Token。</param>
        /// <returns>转换后的 CLR 值。</returns>
        protected virtual object? ConvertJTokenValue(JToken? token)
        {
            if (token == null)
            {
                return null;
            }

            if (token is JValue valueToken)
            {
                return valueToken.Value;
            }

            return token.ToObject<object>();
        }

        /// <summary>
        /// 将决策结果负载写入结果变量。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="result">决策执行结果。</param>
        /// <param name="fallbackApplied">是否应用了失败回退。</param>
        /// <param name="fallbackPolicy">实际生效的失败策略。</param>
        /// <param name="fallbackReason">失败回退原因。</param>
        /// <param name="fallbackContext">附加上下文。</param>
        protected virtual void WriteResultVariable(
            IExecutionEntity execution,
            DecisionExecutionResult result,
            bool fallbackApplied,
            string? fallbackPolicy,
            string? fallbackReason,
            IDictionary<string, object?>? fallbackContext)
        {
            if (string.IsNullOrWhiteSpace(ResultVariable))
            {
                return;
            }

            var resultPayload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["executionId"] = result.ExecutionId ?? string.Empty,
                ["decisionKey"] = result.DecisionKey,
                ["decisionType"] = result.DecisionType ?? string.Empty,
                ["resolvedVersionTag"] = result.ResolvedVersionTag ?? string.Empty,
                ["succeeded"] = result.Succeeded,
                ["traceEnabled"] = result.TraceEnabled,
                ["outputs"] = result.Outputs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
                ["matchedRules"] = result.MatchedRules.ToList(),
                ["explanation"] = result.Explanation ?? string.Empty,
                ["errorMessage"] = result.ErrorMessage ?? string.Empty,
                ["traceEntries"] = result.TraceEntries.ToList(),
                ["fallbackApplied"] = fallbackApplied,
                ["fallbackPolicy"] = fallbackPolicy ?? string.Empty,
                ["fallbackReason"] = fallbackReason ?? string.Empty
            };

            if (fallbackContext != null && fallbackContext.Count > 0)
            {
                resultPayload["fallbackContext"] = fallbackContext.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            }

            execution.SetVariable(ResultVariable, resultPayload, false);
        }

        /// <summary>
        /// 在 UseDefault 策略下应用默认输出并继续流程。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="exception">原始执行异常。</param>
        protected virtual void ApplyUseDefaultFallback(IExecutionEntity execution, DecisionBinding decisionBinding, Exception exception)
        {
            var defaultOutputs = ResolveDefaultOutputs(decisionBinding);
            if (defaultOutputs.Count == 0)
            {
                throw new ActivitiException("fallbackPolicy 为 UseDefault 时，必须至少配置一个 defaultOutput.* 选项。", exception);
            }

            Logger.LogWarning(exception, "BusinessRuleTask 执行失败，按 UseDefault 策略写入默认输出。ExecutionId={ExecutionId}", execution.Id);

            var fallbackResult = CreateFallbackDecisionResult(
                decisionBinding,
                defaultOutputs,
                $"决策执行失败，已按 UseDefault 策略应用默认输出：{exception.Message}",
                exception.Message);

            ApplyDecisionOutputs(
                execution,
                fallbackResult,
                decisionBinding,
                fallbackApplied: true,
                fallbackPolicy: decisionBinding.FallbackPolicy,
                fallbackReason: exception.Message);

            Leave(execution);
        }

        /// <summary>
        /// 在 ManualIntervention 策略下写入人工介入上下文并传播 BPMN 错误。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="exception">原始执行异常。</param>
        protected virtual void ApplyManualInterventionFallback(IExecutionEntity execution, DecisionBinding decisionBinding, Exception exception)
        {
            var payload = BuildManualInterventionPayload(execution, decisionBinding, exception);
            var variableName = GetDecisionOptionValue(decisionBinding, ManualInterventionVariableNameOption);
            if (string.IsNullOrWhiteSpace(variableName))
            {
                variableName = DefaultManualInterventionVariableName;
            }

            execution.SetVariable(variableName, payload, false);

            var fallbackResult = CreateFallbackDecisionResult(
                decisionBinding,
                new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase),
                $"决策执行失败，已转入人工介入：{exception.Message}",
                exception.Message);

            WriteResultVariable(
                execution,
                fallbackResult,
                fallbackApplied: true,
                fallbackPolicy: decisionBinding.FallbackPolicy,
                fallbackReason: exception.Message,
                fallbackContext: payload);

            var errorCode = GetDecisionOptionValue(decisionBinding, ManualInterventionErrorCodeOption);
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                errorCode = execution.CurrentFlowElement?.GetExtensionElementAttributeValue("errorCode");
            }

            if (string.IsNullOrWhiteSpace(errorCode))
            {
                errorCode = DefaultManualInterventionErrorCode;
            }

            var errorMessage = GetDecisionOptionValue(decisionBinding, ManualInterventionErrorMessageOption);
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = $"业务规则任务触发人工介入：{exception.Message}";
            }

            Logger.LogWarning(exception, "BusinessRuleTask 执行失败，按 ManualIntervention 策略转入人工介入。ExecutionId={ExecutionId}", execution.Id);
            PropagateBpmnError(execution, new BpmnError(errorCode, errorMessage));
        }

        /// <summary>
        /// 创建失败回退场景下的结果对象。
        /// </summary>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="outputs">回退输出。</param>
        /// <param name="explanation">结果说明。</param>
        /// <param name="errorMessage">错误消息。</param>
        /// <returns>回退结果对象。</returns>
        protected virtual DecisionExecutionResult CreateFallbackDecisionResult(
            DecisionBinding decisionBinding,
            IDictionary<string, object?> outputs,
            string explanation,
            string errorMessage)
        {
            return new DecisionExecutionResult
            {
                DecisionKey = decisionBinding.DecisionKey,
                DecisionType = decisionBinding.DecisionType,
                ResolvedVersionTag = decisionBinding.VersionTag,
                Succeeded = false,
                TraceEnabled = decisionBinding.TraceEnabled == true,
                Outputs = outputs,
                MatchedRules = new List<string>(),
                Explanation = explanation,
                ErrorMessage = errorMessage,
                TraceEntries = new List<string>()
            };
        }

        /// <summary>
        /// 构建人工介入流程变量负载。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="exception">原始执行异常。</param>
        /// <returns>人工介入上下文字典。</returns>
        protected virtual IDictionary<string, object?> BuildManualInterventionPayload(IExecutionEntity execution, DecisionBinding decisionBinding, Exception exception)
        {
            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["decisionKey"] = decisionBinding.DecisionKey,
                ["decisionName"] = decisionBinding.DecisionName ?? string.Empty,
                ["decisionType"] = decisionBinding.DecisionType ?? string.Empty,
                ["fallbackPolicy"] = decisionBinding.FallbackPolicy ?? string.Empty,
                ["executionId"] = execution.Id ?? string.Empty,
                ["errorType"] = exception.GetType().FullName ?? exception.GetType().Name,
                ["errorMessage"] = exception.Message,
                ["occurredAtUtc"] = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };

            if (decisionBinding.Options.Count > 0)
            {
                payload["options"] = decisionBinding.Options
                    .ToDictionary(option => option.Name, option => (object?)option.Value, StringComparer.OrdinalIgnoreCase);
            }

            return payload;
        }

        /// <summary>
        /// 解析 UseDefault 失败策略所需的默认输出集合。
        /// </summary>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <returns>默认输出集合。</returns>
        protected virtual IDictionary<string, object?> ResolveDefaultOutputs(DecisionBinding decisionBinding)
        {
            var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var option in decisionBinding.Options)
            {
                if (string.IsNullOrWhiteSpace(option.Name) ||
                    !option.Name.StartsWith(UseDefaultOutputOptionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var outputName = option.Name.Substring(UseDefaultOutputOptionPrefix.Length).Trim();
                if (string.IsNullOrWhiteSpace(outputName))
                {
                    continue;
                }

                outputs[outputName] = ParseDecisionOptionValue(option.Value);
            }

            return outputs;
        }

        /// <summary>
        /// 解析决策选项值。
        /// </summary>
        /// <param name="rawValue">原始字符串值。</param>
        /// <returns>转换后的 CLR 值。</returns>
        protected virtual object? ParseDecisionOptionValue(string? rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            var trimmedValue = rawValue.Trim();
            if (trimmedValue.Length == 0)
            {
                return string.Empty;
            }

            if (string.Equals(trimmedValue, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.Equals(trimmedValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(trimmedValue, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if ((trimmedValue.StartsWith("{", StringComparison.Ordinal) && trimmedValue.EndsWith("}", StringComparison.Ordinal)) ||
                (trimmedValue.StartsWith("[", StringComparison.Ordinal) && trimmedValue.EndsWith("]", StringComparison.Ordinal)) ||
                (trimmedValue.StartsWith("\"", StringComparison.Ordinal) && trimmedValue.EndsWith("\"", StringComparison.Ordinal)))
            {
                try
                {
                    return ConvertJTokenValue(JToken.Parse(trimmedValue));
                }
                catch
                {
                    return rawValue;
                }
            }

            if (long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
            {
                return longValue;
            }

            if (decimal.TryParse(trimmedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return decimalValue;
            }

            return rawValue;
        }

        /// <summary>
        /// 根据名称读取决策扩展选项值。
        /// </summary>
        /// <param name="decisionBinding">决策绑定配置。</param>
        /// <param name="optionName">选项名。</param>
        /// <returns>选项值。</returns>
        protected virtual string? GetDecisionOptionValue(DecisionBinding decisionBinding, string optionName)
        {
            var option = decisionBinding.Options.FirstOrDefault(item =>
                string.Equals(item.Name, optionName, StringComparison.OrdinalIgnoreCase));
            return option?.Value;
        }

        /// <summary>
        /// 将结构化输出映射写回流程变量。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="result">决策执行结果。</param>
        /// <param name="outputMapping">输出映射配置。</param>
        protected virtual void ApplyOutputMapping(IExecutionEntity execution, DecisionExecutionResult result, BusinessRuleTaskDecisionOutputMapping outputMapping)
        {
            ArgumentNullException.ThrowIfNull(execution);
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(outputMapping);

            if (!result.Outputs.TryGetValue(outputMapping.OutputName, out var outputValue))
            {
                return;
            }

            if (string.Equals(outputMapping.TargetType, "processVariable", StringComparison.OrdinalIgnoreCase))
            {
                var targetVariableName = !string.IsNullOrWhiteSpace(outputMapping.TargetValue)
                    ? outputMapping.TargetValue
                    : outputMapping.OutputName;
                execution.SetVariable(targetVariableName, outputValue, false);
                return;
            }

            throw new ActivitiException($"暂不支持的 BusinessRuleTask 输出映射类型：{outputMapping.TargetType}。");
        }

        /// <summary>
        /// 解析表达式配置字符串。
        /// </summary>
        /// <param name="expression">待解析表达式。</param>
        /// <param name="variableScope">变量作用域。</param>
        /// <returns>解析结果字符串。</returns>
        protected virtual string ResolveConfiguredString(IExpression expression, IVariableScope variableScope)
        {
            ArgumentNullException.ThrowIfNull(expression);
            ArgumentNullException.ThrowIfNull(variableScope);

            var expressionText = expression.ExpressionText?.Trim();
            if (string.IsNullOrWhiteSpace(expressionText))
            {
                return string.Empty;
            }

            if (!LooksLikeDelegateExpression(expressionText))
            {
                return expressionText;
            }

            var value = expression.GetValue(variableScope);
            return Convert.ToString(value)?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// 判断是否为可求值表达式。
        /// </summary>
        /// <param name="expressionText">表达式文本。</param>
        /// <returns>是否为表达式。</returns>
        protected virtual bool LooksLikeDelegateExpression(string expressionText)
        {
            return expressionText.StartsWith("${", StringComparison.Ordinal) ||
                   expressionText.StartsWith("#{", StringComparison.Ordinal);
        }

        /// <summary>
        /// 统一处理业务规则任务执行异常。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="exception">捕获到的异常。</param>
        protected virtual void HandleExecutionException(IExecutionEntity execution, Exception exception)
        {
            Logger.LogError(exception, "执行 BusinessRuleTask 的 DMN 决策失败。ExecutionId={ExecutionId}", execution.Id);

            Exception cause = exception;
            BpmnError error = null;
            while (cause != null)
            {
                if (cause is BpmnError bpmnError)
                {
                    error = bpmnError;
                    break;
                }

                cause = cause.InnerException;
            }

            if (error != null)
            {
                PropagateBpmnError(execution, error);
                return;
            }

            var errorCode = execution.CurrentFlowElement?.GetExtensionElementAttributeValue("errorCode");
            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                PropagateBpmnError(execution, new BpmnError(errorCode, exception.Message));
                return;
            }

            throw new ActivitiException("执行 BusinessRuleTask 的 DMN 决策时发生异常。", exception);
        }

        /// <summary>
        /// 向流程实例传播 BPMN 业务错误。
        /// </summary>
        /// <param name="execution">当前执行上下文。</param>
        /// <param name="error">待传播的 BPMN 错误。</param>
        protected virtual void PropagateBpmnError(IExecutionEntity execution, BpmnError error)
        {
            ErrorPropagation.PropagateError(error, execution);
        }

        /// <summary>
        /// 定义决策绑定信息。
        /// </summary>
        protected sealed class DecisionBinding
        {
            /// <summary>
            /// 初始化决策绑定信息。
            /// </summary>
            /// <param name="decisionKey">决策主键。</param>
            /// <param name="decisionName">决策名称。</param>
            /// <param name="decisionType">决策类型。</param>
            /// <param name="bindingMode">绑定模式。</param>
            /// <param name="executionMode">执行模式。</param>
            /// <param name="traceEnabled">是否启用解释追踪。</param>
            /// <param name="fallbackPolicy">失败回退策略。</param>
            /// <param name="timeoutMs">超时时间（毫秒）。</param>
            /// <param name="versionTag">版本标签。</param>
            /// <param name="inputMappings">输入映射集合。</param>
            /// <param name="outputMappings">输出映射集合。</param>
            /// <param name="options">扩展选项集合。</param>
            public DecisionBinding(
                string decisionKey,
                string? decisionName,
                string? decisionType,
                string? bindingMode,
                string? executionMode,
                bool? traceEnabled,
                string? fallbackPolicy,
                int? timeoutMs,
                string? versionTag,
                IEnumerable<BusinessRuleTaskDecisionInputMapping> inputMappings,
                IEnumerable<BusinessRuleTaskDecisionOutputMapping> outputMappings,
                IEnumerable<BusinessRuleTaskDecisionOption> options)
            {
                DecisionKey = decisionKey;
                DecisionName = decisionName;
                DecisionType = decisionType;
                BindingMode = bindingMode;
                ExecutionMode = executionMode;
                TraceEnabled = traceEnabled;
                FallbackPolicy = fallbackPolicy;
                TimeoutMs = timeoutMs;
                VersionTag = versionTag;
                InputMappings = inputMappings?.ToList() ?? new List<BusinessRuleTaskDecisionInputMapping>();
                OutputMappings = outputMappings?.ToList() ?? new List<BusinessRuleTaskDecisionOutputMapping>();
                Options = options?.ToList() ?? new List<BusinessRuleTaskDecisionOption>();
            }

            /// <summary>
            /// 决策主键。
            /// </summary>
            public string DecisionKey { get; }

            /// <summary>
            /// 决策名称。
            /// </summary>
            public string? DecisionName { get; }

            /// <summary>
            /// 决策类型。
            /// </summary>
            public string? DecisionType { get; }

            /// <summary>
            /// 绑定模式。
            /// </summary>
            public string? BindingMode { get; }

            /// <summary>
            /// 执行模式。
            /// </summary>
            public string? ExecutionMode { get; }

            /// <summary>
            /// 是否启用解释追踪。
            /// </summary>
            public bool? TraceEnabled { get; }

            /// <summary>
            /// 失败回退策略。
            /// </summary>
            public string? FallbackPolicy { get; }

            /// <summary>
            /// 超时时间（毫秒）。
            /// </summary>
            public int? TimeoutMs { get; }

            /// <summary>
            /// 版本标签。
            /// </summary>
            public string? VersionTag { get; }

            /// <summary>
            /// 输入映射集合。
            /// </summary>
            public IList<BusinessRuleTaskDecisionInputMapping> InputMappings { get; }

            /// <summary>
            /// 输出映射集合。
            /// </summary>
            public IList<BusinessRuleTaskDecisionOutputMapping> OutputMappings { get; }

            /// <summary>
            /// 扩展选项集合。
            /// </summary>
            public IList<BusinessRuleTaskDecisionOption> Options { get; }
        }
    }
}
