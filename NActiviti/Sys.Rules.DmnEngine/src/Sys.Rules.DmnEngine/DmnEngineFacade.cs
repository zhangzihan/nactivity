using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sys.Rules.DmnEngine.Engine.Decisions.Expression;
using Sys.Rules.DmnEngine.Engine.Decisions.Table;
using Sys.Rules.DmnEngine.Engine.Decisions;
using Sys.Rules.DmnEngine.Engine.Definition;
using Sys.Rules.DmnEngine.Engine.Execution.Context;
using Sys.Rules.DmnEngine.Engine.Execution.Result;
using Sys.Rules.DmnEngine.Parser;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;

namespace Sys.Rules.DmnEngine
{
    /// <summary>
    /// 定义自有 DMN 内核统一入口。
    /// </summary>
    public class DmnEngineFacade
    {
        private const string ExplicitDecisionNameInputKey = "__decisionName";

        /// <summary>
        /// 版本解析结果短 TTL：发布等治理操作存在最长 30 秒生效窗口，换取每次执行省去 1~2 次版本解析 SQL。
        /// </summary>
        private static readonly TimeSpan VersionResolutionCacheTtl = TimeSpan.FromSeconds(30);

        private readonly IReadOnlyList<string> _definitionSearchDirectories;
        private readonly IDecisionVersionRepository _decisionVersionRepository;
        private readonly IDecisionExecutionLogRepository? _decisionExecutionLogRepository;
        private readonly ConcurrentDictionary<string, CachedDefinitionEntry> _definitionCache =
            new ConcurrentDictionary<string, CachedDefinitionEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, CachedVersionResolutionEntry> _versionResolutionCache =
            new ConcurrentDictionary<string, CachedVersionResolutionEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 初始化 DMN 内核入口。
        /// </summary>
        /// <param name="definitionSearchDirectories">DMN 定义搜索目录。</param>
        public DmnEngineFacade(IEnumerable<string>? definitionSearchDirectories = null, IDecisionExecutionLogRepository? decisionExecutionLogRepository = null)
        {
            _definitionSearchDirectories = NormalizeSearchDirectories(definitionSearchDirectories);
            _decisionVersionRepository = new FileSystemDecisionVersionRepository(_definitionSearchDirectories);
            _decisionExecutionLogRepository = decisionExecutionLogRepository;
        }

        /// <summary>
        /// 初始化 DMN 内核入口，并显式指定版本仓储。
        /// </summary>
        /// <param name="decisionVersionRepository">决策版本仓储。</param>
        public DmnEngineFacade(IDecisionVersionRepository decisionVersionRepository, IDecisionExecutionLogRepository? decisionExecutionLogRepository = null)
        {
            _definitionSearchDirectories = Array.Empty<string>();
            _decisionVersionRepository = decisionVersionRepository ?? throw new ArgumentNullException(nameof(decisionVersionRepository));
            _decisionExecutionLogRepository = decisionExecutionLogRepository;
        }

        /// <summary>
        /// 加载指定决策定义的元数据。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策定义描述。</returns>
        public async Task<DmnDefinitionDescriptor> LoadDefinitionAsync(string decisionKey, string? versionTag = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(decisionKey));
            }

            var loadedDefinition = await LoadDefinitionInternalAsync(
                decisionKey,
                bindingMode: string.IsNullOrWhiteSpace(versionTag) ? null : "FixedVersion",
                versionTag,
                cancellationToken).ConfigureAwait(false);
            var selectedDecision = TryResolveDecisionForMetadata(decisionKey, loadedDefinition.Definition);

            return new DmnDefinitionDescriptor
            {
                DecisionKey = decisionKey,
                DecisionName = selectedDecision?.Name,
                VersionTag = loadedDefinition.VersionTag ?? versionTag,
                DefinitionSource = loadedDefinition.FilePath
            };
        }

        /// <summary>
        /// 执行指定的 DMN 决策请求。
        /// </summary>
        /// <param name="request">决策执行请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果。</returns>
        public async Task<DmnExecutionResult> ExecuteAsync(DmnExecutionRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            var executionId = Guid.NewGuid().ToString("D");
            var executedTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            LoadedDmnDefinition? loadedDefinition = null;
            string? decisionType = null;

            try
            {
                ValidateRequestedExecutionOptions(request);
                var explicitDecisionName = TryGetExplicitDecisionName(request.Inputs);
                loadedDefinition = await LoadDefinitionInternalAsync(
                    request.DecisionKey,
                    request.BindingMode,
                    request.VersionTag,
                    cancellationToken).ConfigureAwait(false);
                var selectedDecision = ResolveDecisionForExecution(request.DecisionKey, explicitDecisionName, loadedDefinition.Definition);
                decisionType = DetermineDecisionType(selectedDecision);
                ValidateDecisionType(request.DecisionType, selectedDecision);

                var shouldRecordSnapshots = ShouldRecordSnapshots(request);
                var executionContext = DmnExecutionContextFactory.CreateExecutionContext(
                    loadedDefinition.Definition,
                    options => options.RecordSnapshots = shouldRecordSnapshots);
                ApplyInputParameters(executionContext, request.Inputs);

                var decisionResult = await ExecuteDecisionAsync(
                    executionContext,
                    selectedDecision,
                    request.TimeoutMs,
                    cancellationToken).ConfigureAwait(false);
                var traceEntries = BuildTraceEntries(executionContext, request.ExecutionMode, request.TraceEnabled);
                stopwatch.Stop();

                var executionResult = new DmnExecutionResult
                {
                    ExecutionId = executionId,
                    DecisionKey = request.DecisionKey,
                    DecisionType = decisionType,
                    ResolvedVersionTag = loadedDefinition.VersionTag,
                    Succeeded = true,
                    TraceEnabled = shouldRecordSnapshots,
                    TraceEntries = traceEntries.ToList(),
                    Outputs = MapOutputs(decisionResult),
                    MatchedRules = MapMatchedRules(decisionResult),
                    Explanation = BuildExplanation(loadedDefinition.FilePath, loadedDefinition.VersionTag, selectedDecision, decisionResult, traceEntries)
                };

                await TryWriteExecutionLogAsync(CreateExecutionLogRecord(
                    executionId,
                    request,
                    executionResult,
                    executedTime,
                    stopwatch.ElapsedMilliseconds)).ConfigureAwait(false);

                return executionResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await TryWriteExecutionLogAsync(CreateFailedExecutionLogRecord(
                    executionId,
                    request,
                    loadedDefinition?.VersionTag,
                    executedTime,
                    stopwatch.ElapsedMilliseconds,
                    ex)).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// 使指定决策缓存失效。
        /// </summary>
        /// <param name="request">缓存失效请求。</param>
        public Task InvalidateCacheAsync(DecisionCacheInvalidationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                _definitionCache.Clear();
                _versionResolutionCache.Clear();
                DmnExecutionContext.PurgeExpressionCacheDefinitionScopeAll();
                DmnExecutionContext.PurgeExpressionCacheGlobalScope();
                return Task.CompletedTask;
            }

            var matchingKeys = _definitionCache.Keys
                .Where(key => key.StartsWith($"{request.DecisionKey}||", StringComparison.OrdinalIgnoreCase))
                .Where(key => string.IsNullOrWhiteSpace(request.VersionTag) ||
                    key.Contains($"||{request.VersionTag}||", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var matchingKey in matchingKeys)
            {
                _definitionCache.TryRemove(matchingKey, out _);
            }

            // 同步清除版本解析短 TTL 缓存，保证治理操作（发布/作废/导入）后立即回源。
            var resolutionKeys = _versionResolutionCache.Keys
                .Where(key => key.StartsWith($"{request.DecisionKey}||", StringComparison.OrdinalIgnoreCase))
                .Where(key => string.IsNullOrWhiteSpace(request.VersionTag) ||
                    key.EndsWith($"||{request.VersionTag}", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var resolutionKey in resolutionKeys)
            {
                _versionResolutionCache.TryRemove(resolutionKey, out _);
            }

            if (string.Equals(request.Scope, "Global", StringComparison.OrdinalIgnoreCase))
            {
                DmnExecutionContext.PurgeExpressionCacheGlobalScope();
            }
            else if (string.IsNullOrWhiteSpace(request.VersionTag))
            {
                DmnExecutionContext.PurgeExpressionCacheDefinitionScopeAll();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取当前缓存状态。
        /// </summary>
        /// <param name="decisionKey">可选的决策主键过滤。</param>
        /// <returns>缓存状态描述。</returns>
        public Task<DecisionCacheStatusDescriptor> GetCacheStatusAsync(string? decisionKey = null)
        {
            var matchingEntries = _definitionCache
                .Where(pair => string.IsNullOrWhiteSpace(decisionKey) ||
                    pair.Key.StartsWith($"{decisionKey}||", StringComparison.OrdinalIgnoreCase))
                .Select(pair => pair.Value)
                .ToList();

            return Task.FromResult(new DecisionCacheStatusDescriptor
            {
                DecisionKey = decisionKey,
                CachedVersions = matchingEntries
                    .Select(item => string.IsNullOrWhiteSpace(item.Definition.VersionTag) ? "draft" : item.Definition.VersionTag!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                LastRefreshTime = matchingEntries.Count == 0 ? null : matchingEntries.Max(item => item.LastRefreshTime),
                CacheProvider = "DmnEngineFacade.InMemoryDefinitionCache"
            });
        }

        /// <summary>
        /// 以最佳努力方式写入执行日志，避免审计故障影响主执行语义。
        /// </summary>
        /// <param name="record">待保存的执行日志记录。</param>
        private async Task TryWriteExecutionLogAsync(DecisionExecutionLogRecord record)
        {
            if (_decisionExecutionLogRepository == null)
            {
                return;
            }

            try
            {
                await _decisionExecutionLogRepository.SaveAsync(record, CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // 审计日志不能反向污染决策执行主链路，故此处仅做最佳努力写入。
            }
        }

        /// <summary>
        /// 构造成功执行时的审计日志记录。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="request">执行请求。</param>
        /// <param name="result">执行结果。</param>
        /// <param name="executedTime">执行开始时间。</param>
        /// <param name="elapsedMilliseconds">耗时毫秒。</param>
        /// <returns>执行日志记录。</returns>
        private static DecisionExecutionLogRecord CreateExecutionLogRecord(
            string executionId,
            DmnExecutionRequest request,
            DmnExecutionResult result,
            DateTimeOffset executedTime,
            long elapsedMilliseconds)
        {
            return new DecisionExecutionLogRecord
            {
                ExecutionId = executionId,
                DecisionKey = request.DecisionKey,
                RequestedVersionTag = request.VersionTag,
                ResolvedVersionTag = result.ResolvedVersionTag,
                BindingMode = request.BindingMode,
                Succeeded = result.Succeeded,
                TraceId = request.TraceId,
                ErrorMessage = result.ErrorMessage,
                MatchedRules = result.MatchedRules.ToList(),
                InputSnapshot = CreateSnapshotDictionary(request.Inputs),
                OutputSnapshot = CreateSnapshotDictionary(result.Outputs),
                TraceEntries = result.TraceEntries.ToList(),
                ExecutedTime = executedTime,
                ElapsedMs = ConvertElapsedMilliseconds(elapsedMilliseconds)
            };
        }

        /// <summary>
        /// 构造失败执行时的审计日志记录。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="request">执行请求。</param>
        /// <param name="resolvedVersionTag">已解析出的版本标签。</param>
        /// <param name="executedTime">执行开始时间。</param>
        /// <param name="elapsedMilliseconds">耗时毫秒。</param>
        /// <param name="exception">执行异常。</param>
        /// <returns>执行日志记录。</returns>
        private static DecisionExecutionLogRecord CreateFailedExecutionLogRecord(
            string executionId,
            DmnExecutionRequest request,
            string? resolvedVersionTag,
            DateTimeOffset executedTime,
            long elapsedMilliseconds,
            Exception exception)
        {
            return new DecisionExecutionLogRecord
            {
                ExecutionId = executionId,
                DecisionKey = request.DecisionKey,
                RequestedVersionTag = request.VersionTag,
                ResolvedVersionTag = resolvedVersionTag,
                BindingMode = request.BindingMode,
                Succeeded = false,
                TraceId = request.TraceId,
                ErrorMessage = exception.Message,
                MatchedRules = new List<string>(),
                InputSnapshot = CreateSnapshotDictionary(request.Inputs),
                OutputSnapshot = null,
                TraceEntries = new List<string>(),
                ExecutedTime = executedTime,
                ElapsedMs = ConvertElapsedMilliseconds(elapsedMilliseconds)
            };
        }

        /// <summary>
        /// 复制快照字典，避免后续调用方修改影响审计内容。
        /// </summary>
        /// <param name="source">源字典。</param>
        /// <returns>复制后的快照字典。</returns>
        private static IDictionary<string, object?>? CreateSnapshotDictionary(IDictionary<string, object?>? source)
        {
            if (source == null || source.Count == 0)
            {
                return null;
            }

            return source.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 将耗时毫秒转换为仓储字段可接受的整数。
        /// </summary>
        /// <param name="elapsedMilliseconds">原始耗时毫秒。</param>
        /// <returns>转换后的耗时毫秒。</returns>
        private static int ConvertElapsedMilliseconds(long elapsedMilliseconds)
        {
            if (elapsedMilliseconds <= 0)
            {
                return 0;
            }

            return elapsedMilliseconds > int.MaxValue ? int.MaxValue : (int)elapsedMilliseconds;
        }

        /// <summary>
        /// 执行决策并按需施加超时控制。
        /// </summary>
        /// <param name="executionContext">执行上下文。</param>
        /// <param name="decision">待执行决策。</param>
        /// <param name="timeoutMs">超时时间（毫秒）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果。</returns>
        private static async Task<DmnDecisionResult> ExecuteDecisionAsync(
            DmnExecutionContext executionContext,
            IDmnDecision decision,
            int? timeoutMs,
            CancellationToken cancellationToken)
        {
            if (timeoutMs.HasValue && timeoutMs.Value > 0)
            {
                try
                {
                    return await Task
                        .Run(() => executionContext.ExecuteDecision(decision), cancellationToken)
                        .WaitAsync(TimeSpan.FromMilliseconds(timeoutMs.Value), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TimeoutException ex)
                {
                    throw new DmnEngineException($"DMN 决策执行超时，超时时间为 {timeoutMs.Value}ms。", ex);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return executionContext.ExecuteDecision(decision);
        }

        /// <summary>
        /// 加载 DMN 定义文件并完成解析。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="bindingMode">绑定模式。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已解析的定义上下文。</returns>
        private async Task<LoadedDmnDefinition> LoadDefinitionInternalAsync(
            string decisionKey,
            string? bindingMode,
            string? versionTag,
            CancellationToken cancellationToken)
        {
            var resolutionCacheKey = CreateVersionResolutionCacheKey(decisionKey, bindingMode, versionTag);

            DecisionVersionResolutionResult definitionResolution;

            // 先查短 TTL 版本解析缓存：命中且未过期直接复用，避免每次执行都回源版本仓储（1~2 次 SQL）。
            if (_versionResolutionCache.TryGetValue(resolutionCacheKey, out var cachedResolution) &&
                cachedResolution.ExpiresAt > DateTimeOffset.UtcNow)
            {
                definitionResolution = cachedResolution.Resolution;
            }
            else
            {
                // 过期或未命中才回源；并发下允许多个线程同时回源一次，简单换取无锁读路径。
                definitionResolution = await _decisionVersionRepository.ResolveAsync(new DecisionVersionResolutionRequest
                {
                    DecisionKey = decisionKey,
                    BindingMode = bindingMode,
                    VersionTag = versionTag
                }, cancellationToken).ConfigureAwait(false);

                _versionResolutionCache[resolutionCacheKey] = new CachedVersionResolutionEntry(
                    definitionResolution,
                    DateTimeOffset.UtcNow.Add(VersionResolutionCacheTtl));
            }

            var definitionFilePath = string.IsNullOrWhiteSpace(definitionResolution.DefinitionSource)
                ? $"{decisionKey}.dmn"
                : definitionResolution.DefinitionSource;
            var dmnXml = definitionResolution.DefinitionContent;
            var cacheKey = CreateDefinitionCacheKey(decisionKey, definitionResolution.VersionTag, definitionFilePath);

            if (_definitionCache.TryGetValue(cacheKey, out var cachedEntry))
            {
                return cachedEntry.Definition;
            }

            var detectedVersion = DetectVersion(dmnXml);
            var model = DmnParser.ParseString(dmnXml, detectedVersion);
            var definition = DmnDefinitionFactory.CreateDmnDefinition(model);
            var loadedDefinition = new LoadedDmnDefinition(definitionFilePath, definition, definitionResolution.VersionTag);
            _definitionCache[cacheKey] = new CachedDefinitionEntry(loadedDefinition, DateTimeOffset.UtcNow);

            return loadedDefinition;
        }

        /// <summary>
        /// 解析 DMN 文件版本，并兼容 Camunda 导出的 1.3 扩展语义。
        /// </summary>
        /// <param name="dmnXml">DMN XML 内容。</param>
        /// <returns>识别出的 DMN 版本。</returns>
        private static DmnParser.DmnVersionEnum DetectVersion(string dmnXml)
        {
            var detectedVersion = DmnParser.DetectVersion(dmnXml);
            if (detectedVersion == DmnParser.DmnVersionEnum.V1_3 &&
                dmnXml.Contains("exporter", StringComparison.OrdinalIgnoreCase) &&
                dmnXml.Contains("amunda", StringComparison.OrdinalIgnoreCase))
            {
                return DmnParser.DmnVersionEnum.V1_3ext;
            }

            return detectedVersion;
        }

        /// <summary>
        /// 解析定义文件路径。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="bindingMode">绑定模式。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>DMN 文件解析结果。</returns>
        private DefinitionFileResolution ResolveDefinitionFile(string decisionKey, string? bindingMode, string? versionTag)
        {
            if (string.Equals(bindingMode, "LatestPublished", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveLatestPublishedDefinitionFile(decisionKey);
            }

            if (!string.IsNullOrWhiteSpace(versionTag))
            {
                return ResolveTaggedDefinitionFile(decisionKey, versionTag, bindingMode);
            }

            foreach (var candidatePath in EnumerateCandidateFilePaths(decisionKey))
            {
                if (File.Exists(candidatePath))
                {
                    return new DefinitionFileResolution(Path.GetFullPath(candidatePath), versionTag: null);
                }
            }

            var fileName = Path.GetFileName(decisionKey);
            if (!fileName.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"{fileName}.dmn";
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                if (!Directory.Exists(searchDirectory))
                {
                    continue;
                }

                var matchedPath = Directory
                    .EnumerateFiles(searchDirectory, fileName, SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(matchedPath))
                {
                    return new DefinitionFileResolution(Path.GetFullPath(matchedPath), versionTag: null);
                }
            }

            throw new DmnEngineException($"未找到决策主键 `{decisionKey}` 对应的 DMN 定义文件。");
        }

        /// <summary>
        /// 解析指定版本标签的 DMN 定义文件。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="bindingMode">绑定模式。</param>
        /// <returns>DMN 文件解析结果。</returns>
        private DefinitionFileResolution ResolveTaggedDefinitionFile(string decisionKey, string versionTag, string? bindingMode)
        {
            foreach (var candidatePath in EnumerateVersionTaggedCandidateFilePaths(decisionKey, versionTag))
            {
                if (File.Exists(candidatePath))
                {
                    return new DefinitionFileResolution(Path.GetFullPath(candidatePath), versionTag);
                }
            }

            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            var matchingFiles = EnumerateDefinitionFiles(normalizedDecisionKey)
                .Where(file => string.Equals(
                    TryExtractVersionTagFromFileName(normalizedDecisionKey, file),
                    versionTag,
                    StringComparison.OrdinalIgnoreCase))
                .Select(file => new DefinitionFileResolution(Path.GetFullPath(file), versionTag))
                .ToList();

            if (matchingFiles.Count > 0)
            {
                return matchingFiles[0];
            }

            var modeName = string.IsNullOrWhiteSpace(bindingMode) ? "指定版本" : bindingMode;
            throw new DmnEngineException($"bindingMode 为 {modeName} 时，未找到 decisionKey `{decisionKey}` 对应 versionTag `{versionTag}` 的 DMN 定义文件。");
        }

        /// <summary>
        /// 解析最新已发布版本的 DMN 定义文件。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <returns>DMN 文件解析结果。</returns>
        private DefinitionFileResolution ResolveLatestPublishedDefinitionFile(string decisionKey)
        {
            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            var publishedCandidates = EnumerateDefinitionFiles(normalizedDecisionKey)
                .Select(file => new
                {
                    FilePath = Path.GetFullPath(file),
                    VersionTag = TryExtractVersionTagFromFileName(normalizedDecisionKey, file)
                })
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate.VersionTag))
                .OrderByDescending(candidate => candidate.VersionTag, PublishedVersionTagComparer.Instance)
                .ThenByDescending(candidate => candidate.FilePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (publishedCandidates.Count > 0)
            {
                return new DefinitionFileResolution(
                    publishedCandidates[0].FilePath,
                    publishedCandidates[0].VersionTag);
            }

            foreach (var candidatePath in EnumerateCandidateFilePaths(decisionKey))
            {
                if (File.Exists(candidatePath))
                {
                    return new DefinitionFileResolution(Path.GetFullPath(candidatePath), versionTag: null);
                }
            }

            throw new DmnEngineException($"bindingMode 为 LatestPublished 时，未找到决策主键 `{decisionKey}` 对应的已发布 DMN 定义文件。");
        }

        /// <summary>
        /// 枚举可能的 DMN 文件路径。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <returns>候选路径集合。</returns>
        private IEnumerable<string> EnumerateCandidateFilePaths(string decisionKey)
        {
            var candidateNames = new List<string> { decisionKey };
            if (!decisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase))
            {
                candidateNames.Add($"{decisionKey}.dmn");
            }

            if (Path.IsPathRooted(decisionKey))
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return candidateName;
                }

                yield break;
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return Path.Combine(searchDirectory, candidateName);
                }
            }
        }

        /// <summary>
        /// 枚举带版本标签的候选文件路径。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>候选路径集合。</returns>
        private IEnumerable<string> EnumerateVersionTaggedCandidateFilePaths(string decisionKey, string versionTag)
        {
            var normalizedDecisionKey = decisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase)
                ? decisionKey.Substring(0, decisionKey.Length - 4)
                : decisionKey;
            var candidateNames = new[]
            {
                $"{normalizedDecisionKey}.{versionTag}.dmn",
                $"{normalizedDecisionKey}-{versionTag}.dmn"
            };

            if (Path.IsPathRooted(normalizedDecisionKey))
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return candidateName;
                }

                yield break;
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return Path.Combine(searchDirectory, candidateName);
                }
            }
        }

        /// <summary>
        /// 枚举同一决策键下的所有定义文件。
        /// </summary>
        /// <param name="normalizedDecisionKey">规范化后的决策主键。</param>
        /// <returns>定义文件集合。</returns>
        private IEnumerable<string> EnumerateDefinitionFiles(string normalizedDecisionKey)
        {
            var fileName = $"{normalizedDecisionKey}.dmn";
            var baseFileName = Path.GetFileName(normalizedDecisionKey);

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                if (!Directory.Exists(searchDirectory))
                {
                    continue;
                }

                foreach (var filePath in Directory.EnumerateFiles(searchDirectory, "*.dmn", SearchOption.AllDirectories))
                {
                    var normalizedFilePath = Path.GetFullPath(filePath);
                    if (string.Equals(Path.GetFileName(normalizedFilePath), fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return normalizedFilePath;
                        continue;
                    }

                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedFilePath);
                    if (fileNameWithoutExtension.StartsWith($"{baseFileName}.", StringComparison.OrdinalIgnoreCase) ||
                        fileNameWithoutExtension.StartsWith($"{baseFileName}-", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return normalizedFilePath;
                    }
                }
            }
        }

        /// <summary>
        /// 获取不含扩展名的规范化决策主键。
        /// </summary>
        /// <param name="decisionKey">原始决策主键。</param>
        /// <returns>规范化后的决策主键。</returns>
        private static string GetNormalizedDecisionKey(string decisionKey)
        {
            var normalizedDecisionKey = decisionKey.Trim();
            return normalizedDecisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase)
                ? normalizedDecisionKey.Substring(0, normalizedDecisionKey.Length - 4)
                : normalizedDecisionKey;
        }

        /// <summary>
        /// 从文件名中提取版本标签。
        /// </summary>
        /// <param name="normalizedDecisionKey">规范化后的决策主键。</param>
        /// <param name="filePath">DMN 文件路径。</param>
        /// <returns>提取到的版本标签。</returns>
        private static string? TryExtractVersionTagFromFileName(string normalizedDecisionKey, string filePath)
        {
            var baseFileName = Path.GetFileName(normalizedDecisionKey);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            if (fileNameWithoutExtension.Length <= baseFileName.Length)
            {
                return null;
            }

            if (fileNameWithoutExtension.StartsWith($"{baseFileName}.", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.StartsWith($"{baseFileName}-", StringComparison.OrdinalIgnoreCase))
            {
                return fileNameWithoutExtension.Substring(baseFileName.Length + 1);
            }

            return null;
        }

        /// <summary>
        /// 为元数据查询尝试推断默认决策。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="definition">已解析定义。</param>
        /// <returns>推断出的决策；若存在多根决策则返回空。</returns>
        private static IDmnDecision? TryResolveDecisionForMetadata(string decisionKey, DmnDefinition definition)
        {
            if (TryGetDecisionByName(definition, decisionKey, out var exactDecision))
            {
                return exactDecision;
            }

            var rootDecisions = GetRootDecisions(definition);
            return rootDecisions.Count == 1 ? rootDecisions[0] : null;
        }

        /// <summary>
        /// 为执行请求解析唯一决策。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="explicitDecisionName">显式决策名称。</param>
        /// <param name="definition">已解析定义。</param>
        /// <returns>待执行的决策。</returns>
        private static IDmnDecision ResolveDecisionForExecution(string decisionKey, string? explicitDecisionName, DmnDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(explicitDecisionName))
            {
                if (TryGetDecisionByName(definition, explicitDecisionName, out var explicitDecision))
                {
                    return explicitDecision;
                }

                throw new DmnEngineException($"DMN 文件中不存在名为 `{explicitDecisionName}` 的决策。");
            }

            if (TryGetDecisionByName(definition, decisionKey, out var exactDecision))
            {
                return exactDecision;
            }

            var rootDecisions = GetRootDecisions(definition);
            if (rootDecisions.Count == 1)
            {
                return rootDecisions[0];
            }

            if (rootDecisions.Count > 1)
            {
                var decisionNames = string.Join(", ", rootDecisions.Select(static decision => decision.Name));
                throw new DmnEngineException($"DMN 文件存在多个可执行根决策：{decisionNames}。请通过输入参数 `{ExplicitDecisionNameInputKey}` 显式指定要执行的决策名称。");
            }

            throw new DmnEngineException($"DMN 文件 `{decisionKey}` 中未找到可执行决策。");
        }

        /// <summary>
        /// 校验请求中的执行选项。
        /// </summary>
        /// <param name="request">执行请求。</param>
        private static void ValidateRequestedExecutionOptions(DmnExecutionRequest request)
        {
            if (request.TimeoutMs.HasValue && request.TimeoutMs.Value < 0)
            {
                throw new DmnEngineException("timeoutMs 不能小于 0。");
            }

            if (string.IsNullOrWhiteSpace(request.BindingMode))
            {
                return;
            }

            if (string.Equals(request.BindingMode, "FixedVersion", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(request.VersionTag))
            {
                throw new DmnEngineException("bindingMode 为 FixedVersion 时必须提供 versionTag。");
            }

            if (string.Equals(request.BindingMode, "VersionTagPreferred", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(request.VersionTag))
            {
                throw new DmnEngineException("bindingMode 为 VersionTagPreferred 时必须提供 versionTag。");
            }
        }

        /// <summary>
        /// 校验决策类型与真实决策实现是否匹配。
        /// </summary>
        /// <param name="requestedDecisionType">请求声明的决策类型。</param>
        /// <param name="selectedDecision">实际选中的决策。</param>
        private static void ValidateDecisionType(string? requestedDecisionType, IDmnDecision selectedDecision)
        {
            if (string.IsNullOrWhiteSpace(requestedDecisionType))
            {
                return;
            }

            var actualDecisionType = DetermineDecisionType(selectedDecision);
            if (!string.Equals(requestedDecisionType, actualDecisionType, StringComparison.OrdinalIgnoreCase))
            {
                throw new DmnEngineException($"决策类型不匹配，要求 `{requestedDecisionType}`，实际为 `{actualDecisionType}`。");
            }
        }

        /// <summary>
        /// 判断是否需要记录执行快照。
        /// </summary>
        /// <param name="request">执行请求。</param>
        /// <returns>是否记录执行快照。</returns>
        private static bool ShouldRecordSnapshots(DmnExecutionRequest request)
        {
            if (request.TraceEnabled == true)
            {
                return true;
            }

            return string.Equals(request.ExecutionMode, "Explain", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(request.ExecutionMode, "Debug", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 推断决策实现类型名称。
        /// </summary>
        /// <param name="decision">待识别决策。</param>
        /// <returns>决策类型名称。</returns>
        private static string DetermineDecisionType(IDmnDecision decision)
        {
            return decision switch
            {
                DmnDecisionTable => "DecisionTable",
                DmnExpressionDecision => "LiteralExpression",
                _ => decision.GetType().Name
            };
        }

        /// <summary>
        /// 获取根决策集合。
        /// </summary>
        /// <param name="definition">已解析定义。</param>
        /// <returns>根决策集合。</returns>
        private static IReadOnlyList<IDmnDecision> GetRootDecisions(DmnDefinition definition)
        {
            var dependedDecisionNames = new HashSet<string>(
                definition.Decisions.Values.SelectMany(static decision => decision.RequiredDecisions).Select(static decision => decision.Name),
                StringComparer.OrdinalIgnoreCase);

            var rootDecisions = definition.Decisions.Values
                .Where(decision => !dependedDecisionNames.Contains(decision.Name))
                .OrderBy(static decision => decision.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return rootDecisions.Count > 0
                ? rootDecisions
                : definition.Decisions.Values.OrderBy(static decision => decision.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// 按名称查找决策。
        /// </summary>
        /// <param name="definition">已解析定义。</param>
        /// <param name="decisionName">决策名称。</param>
        /// <param name="decision">命中的决策。</param>
        /// <returns>是否找到决策。</returns>
        private static bool TryGetDecisionByName(DmnDefinition definition, string decisionName, out IDmnDecision? decision)
        {
            if (definition.Decisions.TryGetValue(decisionName, out var exactDecision))
            {
                decision = exactDecision;
                return true;
            }

            decision = definition.Decisions
                .FirstOrDefault(pair => string.Equals(pair.Key, decisionName, StringComparison.OrdinalIgnoreCase))
                .Value;

            return decision != null;
        }

        /// <summary>
        /// 将调用方输入参数写入执行上下文。
        /// </summary>
        /// <param name="executionContext">执行上下文。</param>
        /// <param name="inputs">输入参数。</param>
        private static void ApplyInputParameters(DmnExecutionContext executionContext, IEnumerable<KeyValuePair<string, object?>>? inputs)
        {
            if (inputs == null)
            {
                return;
            }

            foreach (var input in inputs)
            {
                if (input.Key.StartsWith("__", StringComparison.Ordinal))
                {
                    continue;
                }

                executionContext.WithInputParameter(input.Key, input.Value);
            }
        }

        /// <summary>
        /// 解析显式决策名称。
        /// </summary>
        /// <param name="inputs">输入参数。</param>
        /// <returns>显式决策名称。</returns>
        private static string? TryGetExplicitDecisionName(IDictionary<string, object?>? inputs)
        {
            if (inputs == null ||
                !inputs.TryGetValue(ExplicitDecisionNameInputKey, out var configuredDecisionName) ||
                configuredDecisionName == null)
            {
                return null;
            }

            var decisionName = Convert.ToString(configuredDecisionName);
            return string.IsNullOrWhiteSpace(decisionName) ? null : decisionName;
        }

        /// <summary>
        /// 映射执行输出。
        /// </summary>
        /// <param name="decisionResult">上游执行结果。</param>
        /// <returns>外观结果输出。</returns>
        private static IDictionary<string, object?> MapOutputs(DmnDecisionResult decisionResult)
        {
            var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (!decisionResult.HasResult)
            {
                return outputs;
            }

            if (decisionResult.IsSingleResult)
            {
                foreach (var variable in decisionResult.FirstResultVariables)
                {
                    outputs[variable.Name] = variable.Value;
                }

                return outputs;
            }

            foreach (var groupedVariable in decisionResult.Results
                         .SelectMany(static result => result.Variables)
                         .GroupBy(static variable => variable.Name, StringComparer.OrdinalIgnoreCase))
            {
                outputs[groupedVariable.Key] = groupedVariable.Select(static variable => variable.Value).ToList();
            }

            return outputs;
        }

        /// <summary>
        /// 映射命中规则标识。
        /// </summary>
        /// <param name="decisionResult">上游执行结果。</param>
        /// <returns>命中规则集合。</returns>
        private static IList<string> MapMatchedRules(DmnDecisionResult decisionResult)
        {
            return decisionResult.Results
                .SelectMany(static result => result.HitRules)
                .Select(static rule => !string.IsNullOrWhiteSpace(rule.Name) ? rule.Name : $"rule#{rule.Index}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// 构建执行解释信息。
        /// </summary>
        /// <param name="filePath">DMN 文件路径。</param>
        /// <param name="decision">执行的决策。</param>
        /// <param name="decisionResult">上游执行结果。</param>
        /// <returns>解释文本。</returns>
        private static string BuildExplanation(string filePath, string? versionTag, IDmnDecision decision, DmnDecisionResult decisionResult, IReadOnlyList<string> traceEntries)
        {
            var versionText = string.IsNullOrWhiteSpace(versionTag) ? string.Empty : $"（版本 `{versionTag}`）";
            var explanation = $"已执行 DMN 文件 `{Path.GetFileName(filePath)}`{versionText} 中的决策 `{decision.Name}`。";

            if (!decisionResult.HasResult)
            {
                return traceEntries.Count > 0
                    ? $"{explanation} 当前未产生输出结果。执行轨迹：{string.Join(" | ", traceEntries)}"
                    : $"{explanation} 当前未产生输出结果。";
            }

            var resultSummary = decisionResult.IsSingleResult
                ? $"{explanation} 返回单条结果。"
                : $"{explanation} 返回 {decisionResult.Results.Count} 条结果。";

            return traceEntries.Count > 0
                ? $"{resultSummary} 执行轨迹：{string.Join(" | ", traceEntries)}"
                : resultSummary;
        }

        /// <summary>
        /// 构建执行轨迹文本。
        /// </summary>
        /// <param name="executionContext">执行上下文。</param>
        /// <param name="executionMode">执行模式。</param>
        /// <param name="traceEnabled">是否启用追踪。</param>
        /// <returns>轨迹文本集合。</returns>
        private static IReadOnlyList<string> BuildTraceEntries(DmnExecutionContext executionContext, string? executionMode, bool? traceEnabled)
        {
            if (!ShouldIncludeTraceEntries(executionMode, traceEnabled))
            {
                return new List<string>();
            }

            var includeVariables = string.Equals(executionMode, "Debug", StringComparison.OrdinalIgnoreCase);
            var entries = new List<string>();

            foreach (var snapshot in executionContext.Snapshots.Snapshots)
            {
                if (snapshot.Decision == null)
                {
                    entries.Add("step#0: 初始化执行上下文");
                    continue;
                }

                var entry = $"step#{snapshot.Step}: 决策 `{snapshot.DecisionName}` 执行完成";
                if (snapshot.DecisionResult != null)
                {
                    var hitRuleNames = snapshot.DecisionResult.Results
                        .SelectMany(static result => result.HitRules)
                        .Select(static rule => !string.IsNullOrWhiteSpace(rule.Name) ? rule.Name : $"rule#{rule.Index}")
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    if (hitRuleNames.Count > 0)
                    {
                        entry += $"，命中规则：{string.Join(", ", hitRuleNames)}";
                    }
                }

                if (includeVariables)
                {
                    var variableSnapshot = string.Join(
                        ", ",
                        snapshot.Variables.Select(static variable => $"{variable.Name}={variable.Value ?? "null"}"));
                    entry += $"，变量快照：{variableSnapshot}";
                }

                entries.Add(entry);
            }

            return entries;
        }

        /// <summary>
        /// 判断是否应返回轨迹明细。
        /// </summary>
        /// <param name="executionMode">执行模式。</param>
        /// <param name="traceEnabled">是否启用追踪。</param>
        /// <returns>是否返回轨迹明细。</returns>
        private static bool ShouldIncludeTraceEntries(string? executionMode, bool? traceEnabled)
        {
            if (traceEnabled == true)
            {
                return true;
            }

            return string.Equals(executionMode, "Explain", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(executionMode, "Debug", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 生成定义缓存键。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="definitionSource">定义来源。</param>
        /// <returns>缓存键。</returns>
        private static string CreateDefinitionCacheKey(string decisionKey, string? versionTag, string? definitionSource)
        {
            return string.Join("||", decisionKey ?? string.Empty, versionTag ?? string.Empty, definitionSource ?? string.Empty);
        }

        /// <summary>
        /// 生成版本解析缓存键（与治理清理逻辑的前缀/后缀匹配保持一致）。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="bindingMode">绑定模式。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>缓存键。</returns>
        private static string CreateVersionResolutionCacheKey(string decisionKey, string? bindingMode, string? versionTag)
        {
            return string.Join("||", decisionKey ?? string.Empty, bindingMode ?? string.Empty, versionTag ?? string.Empty);
        }

        /// <summary>
        /// 计算内容 SHA256 摘要。
        /// </summary>
        /// <param name="content">原始内容。</param>
        /// <returns>十六进制摘要。</returns>
        public static string ComputeChecksum(string content)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
            return Convert.ToHexString(hash).ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 规范化搜索目录列表。
        /// </summary>
        /// <param name="definitionSearchDirectories">用户提供的搜索目录。</param>
        /// <returns>规范化后的目录集合。</returns>
        private static IReadOnlyList<string> NormalizeSearchDirectories(IEnumerable<string>? definitionSearchDirectories)
        {
            var directories = new List<string>();

            if (definitionSearchDirectories != null)
            {
                foreach (var directory in definitionSearchDirectories)
                {
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        directories.Add(Path.GetFullPath(directory));
                    }
                }
            }

            directories.Add(Path.GetFullPath(AppContext.BaseDirectory));
            directories.Add(Path.GetFullPath(Environment.CurrentDirectory));

            return directories
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// 表示已解析的 DMN 定义。
        /// </summary>
        private sealed class LoadedDmnDefinition
        {
            /// <summary>
            /// 初始化定义载入结果。
            /// </summary>
            /// <param name="filePath">DMN 文件路径。</param>
            /// <param name="definition">已解析定义。</param>
            public LoadedDmnDefinition(string filePath, DmnDefinition definition, string? versionTag)
            {
                FilePath = filePath;
                Definition = definition;
                VersionTag = versionTag;
            }

            /// <summary>
            /// DMN 文件路径。
            /// </summary>
            public string FilePath { get; }

            /// <summary>
            /// 已解析定义。
            /// </summary>
            public DmnDefinition Definition { get; }

            /// <summary>
            /// 解析出的版本标签。
            /// </summary>
            public string? VersionTag { get; }
        }

        /// <summary>
        /// 表示定义缓存项。
        /// </summary>
        private sealed class CachedDefinitionEntry
        {
            /// <summary>
            /// 初始化缓存项。
            /// </summary>
            /// <param name="definition">已解析定义。</param>
            /// <param name="lastRefreshTime">最近刷新时间。</param>
            public CachedDefinitionEntry(LoadedDmnDefinition definition, DateTimeOffset lastRefreshTime)
            {
                Definition = definition;
                LastRefreshTime = lastRefreshTime;
            }

            /// <summary>
            /// 已缓存定义。
            /// </summary>
            public LoadedDmnDefinition Definition { get; }

            /// <summary>
            /// 最近刷新时间。
            /// </summary>
            public DateTimeOffset LastRefreshTime { get; }
        }

        /// <summary>
        /// 表示版本解析结果的短 TTL 缓存项。
        /// </summary>
        private sealed class CachedVersionResolutionEntry
        {
            /// <summary>
            /// 初始化版本解析缓存项。
            /// </summary>
            /// <param name="resolution">已解析的版本结果。</param>
            /// <param name="expiresAt">缓存过期时间。</param>
            public CachedVersionResolutionEntry(DecisionVersionResolutionResult resolution, DateTimeOffset expiresAt)
            {
                Resolution = resolution;
                ExpiresAt = expiresAt;
            }

            /// <summary>
            /// 已缓存的版本解析结果。
            /// </summary>
            public DecisionVersionResolutionResult Resolution { get; }

            /// <summary>
            /// 缓存过期时间。
            /// </summary>
            public DateTimeOffset ExpiresAt { get; }
        }

        /// <summary>
        /// 表示 DMN 定义文件的解析结果。
        /// </summary>
        private sealed class DefinitionFileResolution
        {
            /// <summary>
            /// 初始化 DMN 定义文件解析结果。
            /// </summary>
            /// <param name="filePath">定义文件路径。</param>
            /// <param name="versionTag">版本标签。</param>
            public DefinitionFileResolution(string filePath, string? versionTag)
            {
                FilePath = filePath;
                VersionTag = versionTag;
            }

            /// <summary>
            /// 定义文件路径。
            /// </summary>
            public string FilePath { get; }

            /// <summary>
            /// 版本标签。
            /// </summary>
            public string? VersionTag { get; }
        }

        /// <summary>
        /// 提供已发布版本标签比较逻辑。
        /// </summary>
        private sealed class PublishedVersionTagComparer : IComparer<string>
        {
            /// <summary>
            /// 单例实例。
            /// </summary>
            public static PublishedVersionTagComparer Instance { get; } = new PublishedVersionTagComparer();

            /// <summary>
            /// 比较两个版本标签。
            /// </summary>
            /// <param name="x">左值。</param>
            /// <param name="y">右值。</param>
            /// <returns>比较结果。</returns>
            public int Compare(string? x, string? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                if (TryParseVersion(x, out var parsedX) &&
                    TryParseVersion(y, out var parsedY))
                {
                    return parsedX.CompareTo(parsedY);
                }

                return StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }

            /// <summary>
            /// 尝试将版本标签转换为可比较的 Version 对象。
            /// </summary>
            /// <param name="versionTag">版本标签。</param>
            /// <param name="version">解析结果。</param>
            /// <returns>是否解析成功。</returns>
            private static bool TryParseVersion(string versionTag, out Version version)
            {
                var normalizedVersion = Regex.Replace(versionTag.Trim(), "^[vV]", string.Empty);
                return Version.TryParse(normalizedVersion, out version!);
            }
        }
    }

    /// <summary>
    /// 定义 DMN 内核统一异常。
    /// </summary>
    public class DmnEngineException : Exception
    {
        /// <summary>
        /// 初始化异常实例。
        /// </summary>
        /// <param name="message">异常消息。</param>
        public DmnEngineException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 初始化异常实例，并保留内部异常。
        /// </summary>
        /// <param name="message">异常消息。</param>
        /// <param name="innerException">内部异常。</param>
        public DmnEngineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
