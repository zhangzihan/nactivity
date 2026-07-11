using SmartSql.Abstractions;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Rules.Core.Repositories
{
    /// <summary>
    /// 定义跨数据库通用的决策版本仓储实现。
    /// </summary>
    public sealed class DecisionVersionRepository : IDecisionVersionRepository
    {
        private const string Scope = "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository";
        private readonly ISmartSqlMapper _sqlMapper;

        /// <summary>
        /// 初始化决策版本仓储。
        /// </summary>
        /// <param name="sqlMapper">SmartSql 映射器。</param>
        public DecisionVersionRepository(ISmartSqlMapper sqlMapper)
        {
            _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
        }

        /// <summary>
        /// 按决策键与版本标签加载版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的版本记录。</returns>
        public async Task<DecisionVersionRecord?> GetByVersionTagAsync(string decisionKey, string versionTag, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionVersionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetByVersionTag",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionKey,
                    versionTag
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 加载指定决策的最新已发布版本。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>最新已发布版本记录。</returns>
        public async Task<DecisionVersionRecord?> GetLatestPublishedAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionVersionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetLatestPublished",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionKey
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 查询指定决策的全部版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录集合。</returns>
        public async Task<IReadOnlyList<DecisionVersionRecord>> QueryByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionVersionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "QueryByDecisionKey",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionKey
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.ToList();
        }

        /// <summary>
        /// 保存决策版本记录。
        /// </summary>
        /// <param name="record">待保存的版本记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的版本记录。</returns>
        public async Task<DecisionVersionRecord> SaveAsync(DecisionVersionRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(record);

            if (string.IsNullOrWhiteSpace(record.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.DefinitionContent))
            {
                throw new ArgumentException("决策定义内容不能为空。", nameof(record));
            }

            var now = DateTimeOffset.UtcNow;
            var existingRecord = await ResolveExistingRecordAsync(record, cancellationToken).ConfigureAwait(false);
            var persistedRecord = await PrepareRecordForSaveAsync(record, existingRecord, now, cancellationToken).ConfigureAwait(false);

            if (persistedRecord.IsDraft)
            {
                await ClearDraftFlagsAsync(persistedRecord.DecisionKey, persistedRecord.VersionId, cancellationToken).ConfigureAwait(false);
            }

            if (persistedRecord.IsPublished)
            {
                await ClearPublishedFlagsAsync(persistedRecord.DecisionKey, persistedRecord.VersionId, cancellationToken).ConfigureAwait(false);
            }

            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = existingRecord == null ? "InsertVersion" : "UpdateVersion",
                DataSourceChoice = DataSourceChoice.Write,
                Request = persistedRecord
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return persistedRecord;
        }

        /// <summary>
        /// 按绑定模式解析运行时应命中的版本记录。
        /// </summary>
        /// <param name="request">版本解析请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>运行时版本解析结果。</returns>
        public async Task<DecisionVersionResolutionResult> ResolveAsync(DecisionVersionResolutionRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            DecisionVersionRecord? resolvedRecord;
            if (string.Equals(request.BindingMode, "LatestPublished", StringComparison.OrdinalIgnoreCase))
            {
                resolvedRecord = await GetLatestPublishedAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false);
                if (resolvedRecord == null)
                {
                    throw new InvalidOperationException($"决策 `{request.DecisionKey}` 不存在已发布版本。");
                }

                return MapToResolutionResult(resolvedRecord);
            }

            if (!string.IsNullOrWhiteSpace(request.VersionTag))
            {
                resolvedRecord = await GetByVersionTagAsync(request.DecisionKey, request.VersionTag, cancellationToken).ConfigureAwait(false);
                if (resolvedRecord == null)
                {
                    throw new InvalidOperationException($"未找到决策 `{request.DecisionKey}` 的版本 `{request.VersionTag}`。");
                }

                return MapToResolutionResult(resolvedRecord);
            }

            resolvedRecord = await GetDraftAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false)
                ?? await GetLatestPublishedAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false);

            if (resolvedRecord == null)
            {
                throw new InvalidOperationException($"未找到决策 `{request.DecisionKey}` 可执行的版本记录。");
            }

            return MapToResolutionResult(resolvedRecord);
        }

        /// <summary>
        /// 加载指定决策的草稿版本。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>草稿版本记录。</returns>
        private async Task<DecisionVersionRecord?> GetDraftAsync(string decisionKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionVersionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetDraftVersion",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionKey
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 按版本标识加载版本记录。
        /// </summary>
        /// <param name="versionId">版本标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的版本记录。</returns>
        private async Task<DecisionVersionRecord?> GetByVersionIdAsync(string versionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionVersionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetByVersionId",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    versionId
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 解析当前保存动作对应的已存在版本记录。
        /// </summary>
        /// <param name="record">待保存版本记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已存在版本记录；不存在时返回空。</returns>
        private async Task<DecisionVersionRecord?> ResolveExistingRecordAsync(DecisionVersionRecord record, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(record.VersionId))
            {
                var existingById = await GetByVersionIdAsync(record.VersionId, cancellationToken).ConfigureAwait(false);
                if (existingById != null)
                {
                    return existingById;
                }
            }

            if (!string.IsNullOrWhiteSpace(record.VersionTag))
            {
                var existingByVersionTag = await GetByVersionTagAsync(record.DecisionKey, record.VersionTag, cancellationToken).ConfigureAwait(false);
                if (existingByVersionTag != null)
                {
                    return existingByVersionTag;
                }
            }

            if (record.IsDraft)
            {
                return await GetDraftAsync(record.DecisionKey, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// 归一化待保存的版本记录字段。
        /// </summary>
        /// <param name="record">待保存版本记录。</param>
        /// <param name="existingRecord">已存在版本记录。</param>
        /// <param name="now">当前时间。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>归一化后的版本记录。</returns>
        private async Task<DecisionVersionRecord> PrepareRecordForSaveAsync(
            DecisionVersionRecord record,
            DecisionVersionRecord? existingRecord,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            record.VersionId = existingRecord?.VersionId ?? EnsureIdentifier(record.VersionId);
            record.DecisionId = existingRecord?.DecisionId ?? record.DecisionId;

            if (string.IsNullOrWhiteSpace(record.DecisionId))
            {
                throw new ArgumentException("决策定义标识不能为空。", nameof(record));
            }

            record.VersionNumber = existingRecord?.VersionNumber
                ?? (record.VersionNumber > 0
                    ? record.VersionNumber
                    : await GetNextVersionNumberAsync(record.DecisionKey, cancellationToken).ConfigureAwait(false));
            record.CreatedBy = existingRecord?.CreatedBy ?? record.CreatedBy;
            record.CreatedTime = existingRecord?.CreatedTime ?? (record.CreatedTime == default ? now : record.CreatedTime);

            if (record.IsPublished)
            {
                record.PublishedTime ??= now;
            }
            else if (existingRecord == null || !existingRecord.IsPublished)
            {
                record.PublishedTime = null;
                record.PublishedBy = record.IsPublished ? record.PublishedBy : null;
                record.PublishComment = record.IsPublished ? record.PublishComment : null;
            }

            return record;
        }

        /// <summary>
        /// 获取指定决策的下一个版本号。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>下一个版本号。</returns>
        private async Task<long> GetNextVersionNumberAsync(string decisionKey, CancellationToken cancellationToken)
        {
            var versions = await QueryByDecisionKeyAsync(decisionKey, cancellationToken).ConfigureAwait(false);
            return versions.Count == 0 ? 1 : versions.Max(item => item.VersionNumber) + 1;
        }

        /// <summary>
        /// 清理同一决策下其他版本的草稿标记。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="currentVersionId">当前版本标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task ClearDraftFlagsAsync(string decisionKey, string currentVersionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = "ClearDraftFlags",
                DataSourceChoice = DataSourceChoice.Write,
                Request = new
                {
                    decisionKey,
                    currentVersionId
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 清理同一决策下其他版本的已发布标记。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="currentVersionId">当前版本标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task ClearPublishedFlagsAsync(string decisionKey, string currentVersionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = "ClearPublishedFlags",
                DataSourceChoice = DataSourceChoice.Write,
                Request = new
                {
                    decisionKey,
                    currentVersionId
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 将持久化记录转换为运行时解析结果。
        /// </summary>
        /// <param name="record">命中的版本记录。</param>
        /// <returns>运行时版本解析结果。</returns>
        private static DecisionVersionResolutionResult MapToResolutionResult(DecisionVersionRecord record)
        {
            return new DecisionVersionResolutionResult
            {
                DecisionId = record.DecisionId,
                DecisionKey = record.DecisionKey,
                VersionId = record.VersionId,
                VersionTag = record.VersionTag ?? string.Empty,
                VersionNumber = record.VersionNumber,
                DefinitionContent = record.DefinitionContent,
                Checksum = record.Checksum,
                DefinitionSource = $"decision://decision_version/{record.DecisionKey}/{record.VersionTag ?? "draft"}"
            };
        }

        /// <summary>
        /// 确保持久化标识存在，不存在时生成新值。
        /// </summary>
        /// <param name="identifier">现有标识。</param>
        /// <returns>可持久化的标识。</returns>
        private static string EnsureIdentifier(string? identifier)
        {
            return string.IsNullOrWhiteSpace(identifier)
                ? Guid.NewGuid().ToString("D")
                : identifier;
        }
    }
}
