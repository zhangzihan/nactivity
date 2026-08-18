using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;

namespace Sys.Workflow.Rules
{
    /// <summary>
    /// 定义决策执行服务接口。
    /// </summary>
    public interface IDecisionExecutionService
    {
        /// <summary>
        /// 获取指定决策定义的可执行元数据。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策定义描述。</returns>
        Task<DecisionDefinitionDescriptor> GetDefinitionAsync(string decisionKey, string? versionTag = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行单次决策请求。
        /// </summary>
        /// <param name="request">决策执行请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果。</returns>
        Task<DecisionExecutionResult> ExecuteAsync(DecisionExecutionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量执行决策请求。
        /// </summary>
        /// <param name="requests">决策执行请求集合。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>决策执行结果集合。</returns>
        Task<IReadOnlyList<DecisionExecutionResult>> ExecuteBatchAsync(IReadOnlyCollection<DecisionExecutionRequest> requests, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取决策解释结果。
        /// </summary>
        /// <param name="request">解释请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>解释结果。</returns>
        Task<DecisionExplanationResult> ExplainAsync(DecisionExplanationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行决策测试。
        /// </summary>
        /// <param name="request">测试请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>测试结果。</returns>
        Task<DecisionTestResult> TestAsync(DecisionTestRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使指定决策缓存失效。
        /// </summary>
        /// <param name="request">缓存失效请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task InvalidateCacheAsync(DecisionCacheInvalidationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询当前缓存状态。
        /// </summary>
        /// <param name="decisionKey">可选的决策主键过滤。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>缓存状态。</returns>
        Task<DecisionCacheStatusDescriptor> GetCacheStatusAsync(string? decisionKey = null, CancellationToken cancellationToken = default);
    }
}
