using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策执行日志仓储接口。
    /// </summary>
    public interface IDecisionExecutionLogRepository
    {
        /// <summary>
        /// 按执行记录标识加载日志明细。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志记录。</returns>
        Task<DecisionExecutionLogRecord?> GetByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 按条件查询执行日志列表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志列表。</returns>
        Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 统计执行日志条数。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的执行日志总数。</returns>
        Task<long> CountAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存执行日志记录。
        /// </summary>
        /// <param name="record">待保存的执行日志记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的执行日志记录。</returns>
        Task<DecisionExecutionLogRecord> SaveAsync(DecisionExecutionLogRecord record, CancellationToken cancellationToken = default);
    }
}
