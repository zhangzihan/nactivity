using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策版本仓储与运行时版本解析接口。
    /// </summary>
    public interface IDecisionVersionRepository
    {
        /// <summary>
        /// 按决策键与版本标签加载版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的版本记录。</returns>
        Task<DecisionVersionRecord?> GetByVersionTagAsync(string decisionKey, string versionTag, CancellationToken cancellationToken = default);

        /// <summary>
        /// 加载指定决策的最新已发布版本。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>最新已发布版本记录。</returns>
        Task<DecisionVersionRecord?> GetLatestPublishedAsync(string decisionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定决策的全部版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录集合。</returns>
        Task<IReadOnlyList<DecisionVersionRecord>> QueryByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存决策版本记录。
        /// </summary>
        /// <param name="record">待保存的版本记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>保存后的版本记录。</returns>
        Task<DecisionVersionRecord> SaveAsync(DecisionVersionRecord record, CancellationToken cancellationToken = default);

        /// <summary>
        /// 按绑定模式解析运行时应命中的决策版本。
        /// </summary>
        /// <param name="request">版本解析请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>解析得到的版本结果。</returns>
        Task<DecisionVersionResolutionResult> ResolveAsync(DecisionVersionResolutionRequest request, CancellationToken cancellationToken = default);
    }
}
