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
    /// 定义跨数据库通用的决策定义仓储实现。
    /// </summary>
    public sealed class DecisionDefinitionRepository : IDecisionDefinitionRepository
    {
        private const string Scope = "Sys.Workflow.Rules.Core.Repositories.DecisionDefinitionRepository";
        private readonly ISmartSqlMapper _sqlMapper;

        /// <summary>
        /// 初始化决策定义仓储。
        /// </summary>
        /// <param name="sqlMapper">SmartSql 映射器。</param>
        public DecisionDefinitionRepository(ISmartSqlMapper sqlMapper)
        {
            _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
        }

        /// <summary>
        /// 按决策定义标识加载主记录。
        /// </summary>
        /// <param name="decisionId">决策定义标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录。</returns>
        public async Task<DecisionDefinitionRecord?> GetByDecisionIdAsync(string decisionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionDefinitionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetByDecisionId",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionId
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 按决策键加载主记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录。</returns>
        public async Task<DecisionDefinitionRecord?> GetByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionDefinitionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetByDecisionKey",
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
        /// 查询决策定义主记录列表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录集合。</returns>
        public async Task<IReadOnlyList<DecisionDefinitionRecord>> QueryAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(query);

            var records = await _sqlMapper.QueryAsync<DecisionDefinitionRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "QueryDefinitions",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    keyword = query.Keyword,
                    category = query.Category,
                    decisionType = query.DecisionType,
                    status = query.Status,
                    offset = query.PageIndex > 0 ? (query.PageIndex - 1) * query.PageSize : 0,
                    limit = query.PageSize
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.ToList();
        }

        /// <summary>
        /// 保存决策定义主记录。
        /// </summary>
        /// <param name="record">待保存的主记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的主记录。</returns>
        public async Task<DecisionDefinitionRecord> SaveAsync(DecisionDefinitionRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(record);

            if (string.IsNullOrWhiteSpace(record.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.Name))
            {
                throw new ArgumentException("决策名称不能为空。", nameof(record));
            }

            var now = DateTimeOffset.UtcNow;
            var existingRecord = await ResolveExistingRecordAsync(record, cancellationToken).ConfigureAwait(false);
            var persistedRecord = PrepareRecordForSave(record, existingRecord, now);

            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = existingRecord == null ? "InsertDefinition" : "UpdateDefinition",
                DataSourceChoice = DataSourceChoice.Write,
                Request = persistedRecord
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return persistedRecord;
        }

        /// <summary>
        /// 解析当前保存动作对应的已存在主记录。
        /// </summary>
        /// <param name="record">待保存记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已存在的主记录；不存在时返回空。</returns>
        private async Task<DecisionDefinitionRecord?> ResolveExistingRecordAsync(DecisionDefinitionRecord record, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(record.DecisionId))
            {
                var existingById = await GetByDecisionIdAsync(record.DecisionId, cancellationToken).ConfigureAwait(false);
                if (existingById != null)
                {
                    return existingById;
                }
            }

            return await GetByDecisionKeyAsync(record.DecisionKey, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 归一化待保存的主记录字段。
        /// </summary>
        /// <param name="record">待保存记录。</param>
        /// <param name="existingRecord">已存在主记录。</param>
        /// <param name="now">当前时间。</param>
        /// <returns>归一化后的主记录。</returns>
        private static DecisionDefinitionRecord PrepareRecordForSave(
            DecisionDefinitionRecord record,
            DecisionDefinitionRecord? existingRecord,
            DateTimeOffset now)
        {
            record.DecisionId = existingRecord?.DecisionId ?? EnsureIdentifier(record.DecisionId);
            record.SourceFormat = string.IsNullOrWhiteSpace(record.SourceFormat) ? "DMN" : record.SourceFormat;
            record.Status = string.IsNullOrWhiteSpace(record.Status) ? "Draft" : record.Status;
            record.Tags = record.Tags?.ToList() ?? new List<string>();
            record.CreatedBy = existingRecord?.CreatedBy ?? record.CreatedBy;
            record.CreatedTime = existingRecord?.CreatedTime ?? (record.CreatedTime == default ? now : record.CreatedTime);
            record.ModifiedBy ??= record.CreatedBy;
            record.ModifiedTime = now;

            return record;
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
