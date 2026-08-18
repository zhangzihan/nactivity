using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Rules.Core.Services
{
    /// <summary>
    /// 定义决策目录与审计查询服务。
    /// </summary>
    public sealed class DecisionCatalogService : IDecisionCatalogService
    {
        private readonly IDecisionDefinitionRepository _decisionDefinitionRepository;
        private readonly IDecisionVersionRepository _decisionVersionRepository;
        private readonly IDecisionExecutionLogRepository _decisionExecutionLogRepository;
        private readonly IDecisionProcessReferenceIndexRepository? _decisionProcessReferenceIndexRepository;

        /// <summary>
        /// 初始化目录服务。
        /// </summary>
        /// <param name="decisionDefinitionRepository">定义仓储。</param>
        /// <param name="decisionVersionRepository">版本仓储。</param>
        /// <param name="decisionExecutionLogRepository">执行日志仓储。</param>
        /// <param name="decisionProcessReferenceIndexRepository">引用索引仓储。</param>
        public DecisionCatalogService(
            IDecisionDefinitionRepository decisionDefinitionRepository,
            IDecisionVersionRepository decisionVersionRepository,
            IDecisionExecutionLogRepository decisionExecutionLogRepository,
            IDecisionProcessReferenceIndexRepository? decisionProcessReferenceIndexRepository = null)
        {
            _decisionDefinitionRepository = decisionDefinitionRepository ?? throw new ArgumentNullException(nameof(decisionDefinitionRepository));
            _decisionVersionRepository = decisionVersionRepository ?? throw new ArgumentNullException(nameof(decisionVersionRepository));
            _decisionExecutionLogRepository = decisionExecutionLogRepository ?? throw new ArgumentNullException(nameof(decisionExecutionLogRepository));
            _decisionProcessReferenceIndexRepository = decisionProcessReferenceIndexRepository;
        }

        /// <summary>
        /// 查询决策执行日志。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>分页日志结果。</returns>
        public async Task<DecisionCatalogPageResult<DecisionExecutionLogRecord>> QueryExecutionLogsAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var records = await _decisionExecutionLogRepository.QueryAsync(query, cancellationToken).ConfigureAwait(false);
            var totalCount = await _decisionExecutionLogRepository.CountAsync(query, cancellationToken).ConfigureAwait(false);
            return new DecisionCatalogPageResult<DecisionExecutionLogRecord>
            {
                Items = records,
                TotalCount = totalCount,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };
        }

        /// <summary>
        /// 获取指定执行日志详情。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>执行日志明细。</returns>
        public Task<DecisionExecutionLogRecord?> GetExecutionLogAsync(string executionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentException("执行记录标识不能为空。", nameof(executionId));
            }

            return _decisionExecutionLogRepository.GetByExecutionIdAsync(executionId, cancellationToken);
        }

        /// <summary>
        /// 查询决策定义目录。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义集合。</returns>
        public Task<IReadOnlyList<DecisionDefinitionRecord>> QueryDefinitionsAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            return _decisionDefinitionRepository.QueryAsync(query, cancellationToken);
        }

        /// <summary>
        /// 查询指定决策的版本目录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录集合。</returns>
        public Task<IReadOnlyList<DecisionVersionRecord>> QueryDecisionVersionsAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(decisionKey));
            }

            return _decisionVersionRepository.QueryByDecisionKeyAsync(decisionKey, cancellationToken);
        }

        /// <summary>
        /// 获取指定决策定义详情。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的定义记录。</returns>
        public Task<DecisionDefinitionRecord?> GetDefinitionAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(decisionKey));
            }

            return _decisionDefinitionRepository.GetByDecisionKeyAsync(decisionKey, cancellationToken);
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
        /// 尝试基于决策引用索引查询流程引用。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的引用记录；索引不可用或覆盖不完整时返回空。</returns>
        public async Task<IReadOnlyList<DecisionProcessReferenceDescriptor>?> TryQueryDecisionReferencesAsync(DecisionProcessReferenceCatalogQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (_decisionProcessReferenceIndexRepository == null)
            {
                return null;
            }

            var deploymentIds = query.DeploymentIds
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var indexedDeploymentIds = await _decisionProcessReferenceIndexRepository
                .GetIndexedDeploymentIdsAsync(deploymentIds, cancellationToken)
                .ConfigureAwait(false);
            if (deploymentIds.Count != indexedDeploymentIds.Count)
            {
                return null;
            }

            var entries = await _decisionProcessReferenceIndexRepository
                .QueryEntriesAsync(new DecisionProcessReferenceIndexQuery
                {
                    DecisionKey = query.DecisionKey,
                    TenantId = query.TenantId,
                    IncludeDrafts = query.IncludeDrafts,
                    DeploymentIds = deploymentIds
                }, cancellationToken)
                .ConfigureAwait(false);

            return GroupDecisionReferenceIndexEntries(entries);
        }

        /// <summary>
        /// 校验一组决策绑定是否满足目录门禁要求。
        /// </summary>
        /// <param name="request">校验请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task ValidateDecisionBindingsAsync(DecisionBindingValidationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            foreach (var binding in request.Bindings)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.DecisionKey))
                {
                    throw new ArgumentException($"规则任务 {binding?.SourceId ?? string.Empty} 缺少有效的 decisionKey。", nameof(request));
                }

                if ((string.Equals(binding.BindingMode, "FixedVersion", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(binding.BindingMode, "VersionTagPreferred", StringComparison.OrdinalIgnoreCase)) &&
                    string.IsNullOrWhiteSpace(binding.VersionTag))
                {
                    throw new ArgumentException($"规则任务 {binding.SourceId} 在 {binding.BindingMode} 模式下必须指定 versionTag。", nameof(request));
                }

                var definitionRecord = await _decisionDefinitionRepository
                    .GetByDecisionKeyAsync(binding.DecisionKey, cancellationToken)
                    .ConfigureAwait(false);
                if (definitionRecord == null)
                {
                    throw new InvalidOperationException($"规则任务 {binding.SourceId} 引用了不存在的决策 {binding.DecisionKey}。");
                }

                if (!string.IsNullOrWhiteSpace(binding.VersionTag))
                {
                    var versionRecord = await _decisionVersionRepository
                        .GetByVersionTagAsync(binding.DecisionKey, binding.VersionTag, cancellationToken)
                        .ConfigureAwait(false);
                    if (versionRecord == null)
                    {
                        throw new InvalidOperationException($"规则任务 {binding.SourceId} 引用了不存在的决策版本 {binding.DecisionKey}:{binding.VersionTag}。");
                    }

                    if (!versionRecord.IsPublished)
                    {
                        throw new InvalidOperationException($"规则任务 {binding.SourceId} 引用了未发布的决策版本 {binding.DecisionKey}:{binding.VersionTag}。");
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(definitionRecord.PublishedVersionTag))
                {
                    throw new InvalidOperationException($"规则任务 {binding.SourceId} 引用的决策 {binding.DecisionKey} 尚无已发布版本。");
                }
            }
        }

        /// <summary>
        /// 将索引明细分组为流程引用结果。
        /// </summary>
        /// <param name="entries">索引明细集合。</param>
        /// <returns>引用记录集合。</returns>
        private static IReadOnlyList<DecisionProcessReferenceDescriptor> GroupDecisionReferenceIndexEntries(
            IReadOnlyList<DecisionProcessReferenceIndexEntryRecord> entries)
        {
            return entries
                .Where(item => !string.IsNullOrWhiteSpace(item.DecisionKey) && !string.IsNullOrWhiteSpace(item.TaskId))
                .GroupBy(item => new
                {
                    item.DeploymentId,
                    item.DeploymentName,
                    item.ProcessDefinitionKey,
                    item.ProcessDefinitionName,
                    item.TenantId,
                    item.IsDraft,
                    item.DeployTime
                })
                .Select(group => new DecisionProcessReferenceDescriptor
                {
                    DeploymentId = group.Key.DeploymentId,
                    DeploymentName = group.Key.DeploymentName,
                    ProcessDefinitionKey = group.Key.ProcessDefinitionKey ?? string.Empty,
                    ProcessDefinitionName = group.Key.ProcessDefinitionName,
                    TenantId = group.Key.TenantId,
                    IsDraft = group.Key.IsDraft,
                    DeployTime = group.Key.DeployTime.UtcDateTime,
                    ReferencedTasks = group
                        .OrderBy(item => item.TaskId)
                        .Select(item => new DecisionProcessReferenceTaskDescriptor
                        {
                            TaskId = item.TaskId ?? string.Empty,
                            TaskName = item.TaskName,
                            DecisionKey = item.DecisionKey ?? string.Empty,
                            BindingMode = item.BindingMode,
                            VersionTag = item.VersionTag
                        })
                        .ToList()
                })
                .OrderByDescending(item => item.IsDraft)
                .ThenByDescending(item => item.DeployTime)
                .ToList();
        }
    }
}
