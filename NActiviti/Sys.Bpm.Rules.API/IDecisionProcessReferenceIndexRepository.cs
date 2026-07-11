using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义流程对决策引用索引的持久化仓储接口。
    /// </summary>
    public interface IDecisionProcessReferenceIndexRepository
    {
        /// <summary>
        /// 以部署为粒度重建决策引用索引快照。
        /// </summary>
        /// <param name="snapshot">部署引用快照。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task ReplaceDeploymentReferencesAsync(
            DecisionProcessReferenceIndexSnapshot snapshot,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除指定部署对应的全部引用索引。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task DeleteByDeploymentIdAsync(string deploymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定部署集合中已完成索引的部署标识。
        /// </summary>
        /// <param name="deploymentIds">部署标识集合。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已存在索引快照的部署标识列表。</returns>
        Task<IReadOnlyList<string>> GetIndexedDeploymentIdsAsync(
            IReadOnlyCollection<string> deploymentIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 按决策键查询引用索引明细行。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的索引明细行。</returns>
        Task<IReadOnlyList<DecisionProcessReferenceIndexEntryRecord>> QueryEntriesAsync(
            DecisionProcessReferenceIndexQuery query,
            CancellationToken cancellationToken = default);
    }
}
