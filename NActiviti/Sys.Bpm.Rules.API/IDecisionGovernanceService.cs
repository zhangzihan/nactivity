using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策治理服务接口。
    /// </summary>
    public interface IDecisionGovernanceService
    {
        /// <summary>
        /// 保存决策草稿定义与草稿版本。
        /// </summary>
        /// <param name="request">草稿保存请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>草稿保存结果。</returns>
        Task<DecisionDraftSaveResult> SaveDraftAsync(DecisionDraftSaveRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发布指定决策版本。
        /// </summary>
        /// <param name="request">版本发布请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>发布结果。</returns>
        Task<DecisionVersionPublishResult> PublishVersionAsync(PublishDecisionVersionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将指定版本重新发布为当前生效版本。
        /// </summary>
        /// <param name="request">版本回滚请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>回滚结果。</returns>
        Task<DecisionVersionPublishResult> RollbackVersionAsync(RollbackDecisionVersionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 导入决策定义。
        /// </summary>
        /// <param name="request">导入请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>导入结果。</returns>
        Task<DecisionImportResult> ImportAsync(DecisionImportRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 导出决策定义。
        /// </summary>
        /// <param name="request">导出请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>导出结果。</returns>
        Task<DecisionExportResult> ExportAsync(DecisionExportRequest request, CancellationToken cancellationToken = default);
    }
}
