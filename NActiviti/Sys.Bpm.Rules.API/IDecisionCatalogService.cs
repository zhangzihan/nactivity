using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策目录与审计查询服务接口。
    /// </summary>
    public interface IDecisionCatalogService
    {
        /// <summary>
        /// 查询决策执行日志。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>分页日志结果。</returns>
        Task<DecisionCatalogPageResult<DecisionExecutionLogRecord>> QueryExecutionLogsAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定执行日志详情。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>执行日志明细。</returns>
        Task<DecisionExecutionLogRecord?> GetExecutionLogAsync(string executionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询决策定义目录。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义集合。</returns>
        Task<IReadOnlyList<DecisionDefinitionRecord>> QueryDefinitionsAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定决策的版本目录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录集合。</returns>
        Task<IReadOnlyList<DecisionVersionRecord>> QueryDecisionVersionsAsync(string decisionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定决策定义详情。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的定义记录。</returns>
        Task<DecisionDefinitionRecord?> GetDefinitionAsync(string decisionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定执行记录的输入输出快照。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>执行快照。</returns>
        Task<DecisionExecutionSnapshot?> GetExecutionSnapshotAsync(string executionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试基于决策引用索引查询流程引用。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的引用记录；索引不可用或覆盖不完整时返回空。</returns>
        Task<IReadOnlyList<DecisionProcessReferenceDescriptor>?> TryQueryDecisionReferencesAsync(DecisionProcessReferenceCatalogQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 校验一组决策绑定是否满足目录门禁要求。
        /// </summary>
        /// <param name="request">校验请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task ValidateDecisionBindingsAsync(DecisionBindingValidationRequest request, CancellationToken cancellationToken = default);
    }
}
