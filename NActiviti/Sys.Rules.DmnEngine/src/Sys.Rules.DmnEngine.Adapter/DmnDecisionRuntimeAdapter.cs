using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sys.Rules.DmnEngine;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;

namespace Sys.Rules.DmnEngine.Adapter
{
    /// <summary>
    /// 定义基于 Sys.Rules.DmnEngine 的决策运行时适配器。
    /// </summary>
    public class DmnDecisionRuntimeAdapter : IDecisionRuntimeService
    {
        private readonly DmnEngineFacade _engineFacade;
        private readonly IDecisionDefinitionRepository? _decisionDefinitionRepository;
        private readonly IDecisionVersionRepository? _decisionVersionRepository;
        private readonly IDecisionExecutionLogRepository? _decisionExecutionLogRepository;
        private readonly IDecisionGovernanceService? _decisionGovernanceService;

        /// <summary>
        /// 初始化适配器实例。
        /// </summary>
        /// <param name="engineFacade">DMN 内核入口。</param>
        /// <param name="decisionDefinitionRepository">决策定义仓储。</param>
        /// <param name="decisionVersionRepository">决策版本仓储。</param>
        /// <param name="decisionExecutionLogRepository">决策执行日志仓储。</param>
        /// <param name="decisionGovernanceService">决策治理服务。</param>
        public DmnDecisionRuntimeAdapter(
            DmnEngineFacade engineFacade,
            IDecisionDefinitionRepository? decisionDefinitionRepository = null,
            IDecisionVersionRepository? decisionVersionRepository = null,
            IDecisionExecutionLogRepository? decisionExecutionLogRepository = null,
            IDecisionGovernanceService? decisionGovernanceService = null)
        {
            _engineFacade = engineFacade ?? throw new ArgumentNullException(nameof(engineFacade));
            _decisionDefinitionRepository = decisionDefinitionRepository;
            _decisionVersionRepository = decisionVersionRepository;
            _decisionExecutionLogRepository = decisionExecutionLogRepository;
            _decisionGovernanceService = decisionGovernanceService;
        }

        /// <summary>
        /// 获取指定决策定义的元数据。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策定义描述。</returns>
        public async Task<DecisionDefinitionDescriptor> GetDefinitionAsync(string decisionKey, string? versionTag = null, CancellationToken cancellationToken = default)
        {
            var descriptor = await _engineFacade.LoadDefinitionAsync(decisionKey, versionTag, cancellationToken).ConfigureAwait(false);

            return new DecisionDefinitionDescriptor
            {
                DecisionKey = descriptor.DecisionKey,
                DecisionName = descriptor.DecisionName,
                VersionTag = descriptor.VersionTag,
                DefinitionSource = descriptor.DefinitionSource
            };
        }

        /// <summary>
        /// 执行单次决策请求。
        /// </summary>
        /// <param name="request">决策执行请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果。</returns>
        public async Task<DecisionExecutionResult> ExecuteAsync(DecisionExecutionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = await _engineFacade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = request.DecisionKey,
                DecisionType = request.DecisionType,
                BindingMode = request.BindingMode,
                ExecutionMode = request.ExecutionMode,
                TraceEnabled = request.TraceEnabled,
                TimeoutMs = request.TimeoutMs,
                VersionTag = request.VersionTag,
                Inputs = request.Inputs,
                TraceId = request.TraceId
            }, cancellationToken).ConfigureAwait(false);

            return new DecisionExecutionResult
            {
                ExecutionId = result.ExecutionId,
                DecisionKey = result.DecisionKey,
                DecisionType = result.DecisionType,
                ResolvedVersionTag = result.ResolvedVersionTag,
                Succeeded = result.Succeeded,
                TraceEnabled = result.TraceEnabled,
                TraceEntries = result.TraceEntries.ToList(),
                Outputs = result.Outputs,
                Explanation = result.Explanation,
                MatchedRules = result.MatchedRules.ToList(),
                ErrorMessage = result.ErrorMessage
            };
        }

        /// <summary>
        /// 批量执行决策请求。
        /// </summary>
        /// <param name="requests">决策执行请求集合。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果集合。</returns>
        public async Task<IReadOnlyList<DecisionExecutionResult>> ExecuteBatchAsync(IReadOnlyCollection<DecisionExecutionRequest> requests, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(requests);

            var results = new List<DecisionExecutionResult>(requests.Count);

            foreach (var request in requests)
            {
                results.Add(await ExecuteAsync(request, cancellationToken).ConfigureAwait(false));
            }

            return results;
        }

        /// <summary>
        /// 查询决策定义目录。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的定义主记录集合。</returns>
        public Task<IReadOnlyList<DecisionDefinitionRecord>> QueryDefinitionsAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            return RequireDecisionDefinitionRepository().QueryAsync(query, cancellationToken);
        }

        /// <summary>
        /// 查询指定决策的版本目录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的版本记录集合。</returns>
        public Task<IReadOnlyList<DecisionVersionRecord>> QueryDecisionVersionsAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(decisionKey));
            }

            return RequireDecisionVersionRepository().QueryByDecisionKeyAsync(decisionKey, cancellationToken);
        }

        /// <summary>
        /// 查询决策执行日志。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志记录集合。</returns>
        public Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryExecutionLogsAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            return RequireDecisionExecutionLogRepository().QueryAsync(query, cancellationToken);
        }

        /// <summary>
        /// 获取指定执行日志详情。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志记录。</returns>
        public Task<DecisionExecutionLogRecord?> GetExecutionLogAsync(string executionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentException("执行记录标识不能为空。", nameof(executionId));
            }

            return RequireDecisionExecutionLogRepository().GetByExecutionIdAsync(executionId, cancellationToken);
        }

        /// <summary>
        /// 获取决策解释结果。
        /// </summary>
        /// <param name="request">解释请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>解释结果。</returns>
        public async Task<DecisionExplanationResult> ExplainAsync(DecisionExplanationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.ExecutionId) && _decisionExecutionLogRepository != null)
            {
                var executionLog = await _decisionExecutionLogRepository
                    .GetByExecutionIdAsync(request.ExecutionId, cancellationToken)
                    .ConfigureAwait(false);
                if (executionLog != null)
                {
                    return CreateExplanationResultFromLog(executionLog);
                }
            }

            var executionResult = await ExecuteAsync(new DecisionExecutionRequest
            {
                DecisionKey = request.DecisionKey,
                VersionTag = request.VersionTag,
                BindingMode = request.BindingMode,
                DecisionType = request.DecisionType,
                ExecutionMode = "Explain",
                TraceEnabled = true,
                Inputs = request.Inputs
            }, cancellationToken).ConfigureAwait(false);

            return CreateExplanationResultFromExecution(executionResult);
        }

        /// <summary>
        /// 执行决策测试。
        /// </summary>
        /// <param name="request">测试请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>测试结果。</returns>
        public async Task<DecisionTestResult> TestAsync(DecisionTestRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            var caseResults = new List<DecisionTestCaseResult>(request.TestCases.Count);
            foreach (var testCase in request.TestCases)
            {
                var executionResult = await ExecuteAsync(new DecisionExecutionRequest
                {
                    DecisionKey = request.DecisionKey,
                    VersionTag = request.VersionTag,
                    BindingMode = request.BindingMode,
                    ExecutionMode = "Debug",
                    TraceEnabled = true,
                    Inputs = testCase.Inputs
                }, cancellationToken).ConfigureAwait(false);

                var failureMessages = CompareExpectedOutputs(testCase.ExpectedOutputs, executionResult.Outputs);
                if (!executionResult.Succeeded)
                {
                    failureMessages.Add(executionResult.ErrorMessage ?? "决策执行失败。");
                }

                caseResults.Add(new DecisionTestCaseResult
                {
                    CaseName = testCase.CaseName,
                    Passed = failureMessages.Count == 0,
                    ExecutionId = executionResult.ExecutionId,
                    ActualOutputs = new Dictionary<string, object?>(executionResult.Outputs, StringComparer.OrdinalIgnoreCase),
                    FailureMessages = failureMessages
                });
            }

            var passedCases = caseResults.Count(item => item.Passed);
            return new DecisionTestResult
            {
                Passed = caseResults.All(item => item.Passed),
                TotalCases = caseResults.Count,
                PassedCases = passedCases,
                FailedCases = caseResults.Count - passedCases,
                CaseResults = caseResults
            };
        }

        /// <summary>
        /// 导入决策定义。
        /// </summary>
        /// <param name="request">导入请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>导入结果。</returns>
        public async Task<DecisionImportResult> ImportAsync(DecisionImportRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException("导入内容不能为空。", nameof(request));
            }

            var decisionKey = ResolveImportedDecisionKey(request);
            var existingDefinition = _decisionDefinitionRepository == null
                ? null
                : await _decisionDefinitionRepository.GetByDecisionKeyAsync(decisionKey, cancellationToken).ConfigureAwait(false);
            if (existingDefinition != null && !request.OverwriteExisting)
            {
                throw new InvalidOperationException($"决策 `{decisionKey}` 已存在，若需覆盖请显式指定 OverwriteExisting=true。");
            }

            await RequireDecisionGovernanceService().SaveDraftAsync(new DecisionDraftSaveRequest
            {
                DecisionId = existingDefinition?.DecisionId,
                DecisionKey = decisionKey,
                Name = string.IsNullOrWhiteSpace(request.Name) ? decisionKey : request.Name!,
                SourceFormat = string.IsNullOrWhiteSpace(request.Format) ? "DMN" : request.Format,
                VersionTag = request.VersionTag,
                DefinitionContent = request.Content,
                Checksum = DmnEngineFacade.ComputeChecksum(request.Content),
                Operator = request.Operator
            }, cancellationToken).ConfigureAwait(false);

            await _engineFacade.InvalidateCacheAsync(new DecisionCacheInvalidationRequest
            {
                DecisionKey = decisionKey
            }).ConfigureAwait(false);

            return new DecisionImportResult
            {
                Succeeded = true,
                ImportedDecisionKeys = new List<string> { decisionKey }
            };
        }

        /// <summary>
        /// 导出决策定义。
        /// </summary>
        /// <param name="request">导出请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>导出结果。</returns>
        public async Task<DecisionExportResult> ExportAsync(DecisionExportRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            var versionRepository = RequireDecisionVersionRepository();
            var versionRecord = string.IsNullOrWhiteSpace(request.VersionTag)
                ? await versionRepository.GetLatestPublishedAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false)
                : await versionRepository.GetByVersionTagAsync(request.DecisionKey, request.VersionTag, cancellationToken).ConfigureAwait(false);
            if (versionRecord == null)
            {
                throw new InvalidOperationException($"未找到决策 `{request.DecisionKey}` 的可导出版本。");
            }

            var fileVersionTag = string.IsNullOrWhiteSpace(versionRecord.VersionTag) ? "draft" : versionRecord.VersionTag;
            return new DecisionExportResult
            {
                FileName = $"{request.DecisionKey}.{fileVersionTag}.dmn",
                ContentType = "application/xml",
                Content = versionRecord.DefinitionContent,
                Checksum = string.IsNullOrWhiteSpace(versionRecord.Checksum)
                    ? DmnEngineFacade.ComputeChecksum(versionRecord.DefinitionContent)
                    : versionRecord.Checksum
            };
        }

        /// <summary>
        /// 获取指定执行记录的输入输出快照。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>执行快照。</returns>
        public async Task<DecisionExecutionSnapshot?> GetExecutionSnapshotAsync(string executionId, CancellationToken cancellationToken = default)
        {
            var executionLog = await GetExecutionLogAsync(executionId, cancellationToken).ConfigureAwait(false);
            if (executionLog == null)
            {
                return null;
            }

            return new DecisionExecutionSnapshot
            {
                ExecutionId = executionLog.ExecutionId,
                DecisionKey = executionLog.DecisionKey,
                InputSnapshot = executionLog.InputSnapshot,
                OutputSnapshot = executionLog.OutputSnapshot,
                ExecutedTime = executionLog.ExecutedTime
            };
        }

        /// <summary>
        /// 使指定决策缓存失效。
        /// </summary>
        /// <param name="request">缓存失效请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public Task InvalidateCacheAsync(DecisionCacheInvalidationRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _engineFacade.InvalidateCacheAsync(request);
        }

        /// <summary>
        /// 查询当前缓存状态。
        /// </summary>
        /// <param name="decisionKey">可选的决策主键过滤。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>缓存状态。</returns>
        public Task<DecisionCacheStatusDescriptor> GetCacheStatusAsync(string? decisionKey = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _engineFacade.GetCacheStatusAsync(decisionKey);
        }

        /// <summary>
        /// 获取已配置的决策定义仓储。
        /// </summary>
        /// <returns>定义仓储。</returns>
        private IDecisionDefinitionRepository RequireDecisionDefinitionRepository()
        {
            if (_decisionDefinitionRepository == null)
            {
                throw new InvalidOperationException("当前运行时未配置决策定义仓储。");
            }

            return _decisionDefinitionRepository;
        }

        /// <summary>
        /// 获取已配置的决策版本仓储。
        /// </summary>
        /// <returns>版本仓储。</returns>
        private IDecisionVersionRepository RequireDecisionVersionRepository()
        {
            if (_decisionVersionRepository == null)
            {
                throw new InvalidOperationException("当前运行时未配置决策版本仓储。");
            }

            return _decisionVersionRepository;
        }

        /// <summary>
        /// 获取已配置的执行日志仓储。
        /// </summary>
        /// <returns>执行日志仓储。</returns>
        private IDecisionExecutionLogRepository RequireDecisionExecutionLogRepository()
        {
            if (_decisionExecutionLogRepository == null)
            {
                throw new InvalidOperationException("当前运行时未配置决策执行日志仓储。");
            }

            return _decisionExecutionLogRepository;
        }

        /// <summary>
        /// 获取已配置的治理服务。
        /// </summary>
        /// <returns>治理服务。</returns>
        private IDecisionGovernanceService RequireDecisionGovernanceService()
        {
            if (_decisionGovernanceService == null)
            {
                throw new InvalidOperationException("当前运行时未配置决策治理服务。");
            }

            return _decisionGovernanceService;
        }

        /// <summary>
        /// 基于执行结果构造解释结果。
        /// </summary>
        /// <param name="executionResult">执行结果。</param>
        /// <returns>解释结果。</returns>
        private static DecisionExplanationResult CreateExplanationResultFromExecution(DecisionExecutionResult executionResult)
        {
            return new DecisionExplanationResult
            {
                ExecutionId = executionResult.ExecutionId,
                DecisionKey = executionResult.DecisionKey,
                ResolvedVersionTag = executionResult.ResolvedVersionTag,
                RulePath = executionResult.MatchedRules.ToList(),
                MatchedConditions = executionResult.MatchedRules.ToList(),
                RejectedConditions = new List<string>(),
                EvaluationSteps = executionResult.TraceEntries.ToList(),
                Explanation = executionResult.Explanation
            };
        }

        /// <summary>
        /// 基于执行日志构造解释结果。
        /// </summary>
        /// <param name="executionLog">执行日志。</param>
        /// <returns>解释结果。</returns>
        private static DecisionExplanationResult CreateExplanationResultFromLog(DecisionExecutionLogRecord executionLog)
        {
            return new DecisionExplanationResult
            {
                ExecutionId = executionLog.ExecutionId,
                DecisionKey = executionLog.DecisionKey,
                ResolvedVersionTag = executionLog.ResolvedVersionTag,
                RulePath = executionLog.MatchedRules?.ToList() ?? new List<string>(),
                MatchedConditions = executionLog.MatchedRules?.ToList() ?? new List<string>(),
                RejectedConditions = new List<string>(),
                EvaluationSteps = executionLog.TraceEntries?.ToList() ?? new List<string>(),
                Explanation = executionLog.ErrorMessage
            };
        }

        /// <summary>
        /// 比较期望输出与实际输出差异。
        /// </summary>
        /// <param name="expectedOutputs">期望输出。</param>
        /// <param name="actualOutputs">实际输出。</param>
        /// <returns>失败消息集合。</returns>
        private static List<string> CompareExpectedOutputs(
            IDictionary<string, object?> expectedOutputs,
            IDictionary<string, object?> actualOutputs)
        {
            var failureMessages = new List<string>();
            foreach (var expectedOutput in expectedOutputs)
            {
                if (!actualOutputs.TryGetValue(expectedOutput.Key, out var actualValue))
                {
                    failureMessages.Add($"缺少输出 `{expectedOutput.Key}`。");
                    continue;
                }

                if (!AreEquivalent(expectedOutput.Value, actualValue))
                {
                    failureMessages.Add(
                        $"输出 `{expectedOutput.Key}` 不匹配，期望 `{expectedOutput.Value ?? "null"}`，实际 `{actualValue ?? "null"}`。");
                }
            }

            return failureMessages;
        }

        /// <summary>
        /// 判断两个值是否语义相等。
        /// </summary>
        /// <param name="expectedValue">期望值。</param>
        /// <param name="actualValue">实际值。</param>
        /// <returns>是否相等。</returns>
        private static bool AreEquivalent(object? expectedValue, object? actualValue)
        {
            if (Equals(expectedValue, actualValue))
            {
                return true;
            }

            return string.Equals(
                Convert.ToString(expectedValue, System.Globalization.CultureInfo.InvariantCulture),
                Convert.ToString(actualValue, System.Globalization.CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析导入时使用的决策主键。
        /// </summary>
        /// <param name="request">导入请求。</param>
        /// <returns>决策主键。</returns>
        private static string ResolveImportedDecisionKey(DecisionImportRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                return request.DecisionKey;
            }

            if (!string.IsNullOrWhiteSpace(request.FileName))
            {
                return Path.GetFileNameWithoutExtension(request.FileName);
            }

            throw new ArgumentException("导入请求缺少 DecisionKey 或 FileName，无法推断决策主键。", nameof(request));
        }
    }
}
