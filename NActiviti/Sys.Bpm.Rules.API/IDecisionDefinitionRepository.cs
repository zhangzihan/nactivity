using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策定义主记录仓储接口。
    /// </summary>
    public interface IDecisionDefinitionRepository
    {
        /// <summary>
        /// 按决策定义标识加载主记录。
        /// </summary>
        /// <param name="decisionId">决策定义标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录。</returns>
        Task<DecisionDefinitionRecord?> GetByDecisionIdAsync(string decisionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 按决策键加载主记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录。</returns>
        Task<DecisionDefinitionRecord?> GetByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询决策定义主记录列表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义记录集合。</returns>
        Task<IReadOnlyList<DecisionDefinitionRecord>> QueryAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存决策定义主记录。
        /// </summary>
        /// <param name="record">待保存的主记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的主记录。</returns>
        Task<DecisionDefinitionRecord> SaveAsync(DecisionDefinitionRecord record, CancellationToken cancellationToken = default);
    }
}
