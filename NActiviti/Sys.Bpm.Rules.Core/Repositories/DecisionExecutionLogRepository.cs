using SmartSql.Abstractions;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Rules.Core.Repositories
{
    /// <summary>
    /// 定义跨数据库通用的决策执行日志仓储实现。
    /// </summary>
    public sealed class DecisionExecutionLogRepository : IDecisionExecutionLogRepository
    {
        private const string Scope = "Sys.Workflow.Rules.Core.Repositories.DecisionExecutionLogRepository";
        private readonly ISmartSqlMapper _sqlMapper;

        /// <summary>
        /// 初始化执行日志仓储。
        /// </summary>
        /// <param name="sqlMapper">SmartSql 映射器。</param>
        public DecisionExecutionLogRepository(ISmartSqlMapper sqlMapper)
        {
            _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
        }

        /// <summary>
        /// 按执行记录标识加载日志明细。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志记录。</returns>
        public async Task<DecisionExecutionLogRecord?> GetByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var records = await _sqlMapper.QueryAsync<DecisionExecutionLogRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetByExecutionId",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    executionId
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.FirstOrDefault();
        }

        /// <summary>
        /// 按条件查询执行日志列表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志列表。</returns>
        public async Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(query);

            var records = await _sqlMapper.QueryAsync<DecisionExecutionLogRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "QueryLogs",
                DataSourceChoice = DataSourceChoice.Read,
                Request = BuildQueryRequest(query)
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.ToList();
        }

        /// <summary>
        /// 统计执行日志条数。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志总数。</returns>
        public async Task<long> CountAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(query);

            var totalCount = await _sqlMapper.ExecuteScalarAsync<long>(new RequestContext
            {
                Scope = Scope,
                SqlId = "CountLogs",
                DataSourceChoice = DataSourceChoice.Read,
                Request = BuildQueryRequest(query)
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return totalCount;
        }

        /// <summary>
        /// 保存执行日志记录。
        /// </summary>
        /// <param name="record">待保存的执行日志记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的执行日志记录。</returns>
        public async Task<DecisionExecutionLogRecord> SaveAsync(DecisionExecutionLogRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(record);

            if (string.IsNullOrWhiteSpace(record.ExecutionId))
            {
                throw new ArgumentException("执行记录标识不能为空。", nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(record));
            }

            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = "InsertLog",
                DataSourceChoice = DataSourceChoice.Write,
                Request = record
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return record;
        }

        /// <summary>
        /// 构造 SmartSql 查询参数对象。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <returns>SmartSql 请求对象。</returns>
        private static object BuildQueryRequest(DecisionExecutionLogQuery query)
        {
            return new
            {
                decisionKey = query.DecisionKey,
                requestedVersionTag = query.RequestedVersionTag,
                resolvedVersionTag = query.ResolvedVersionTag,
                bindingMode = query.BindingMode,
                traceId = query.TraceId,
                executor = query.Executor,
                succeeded = query.Succeeded,
                startExecutedTime = query.StartExecutedTime,
                endExecutedTime = query.EndExecutedTime,
                offset = query.PageIndex > 0 ? (query.PageIndex - 1) * query.PageSize : 0,
                limit = query.PageSize
            };
        }
    }
}
