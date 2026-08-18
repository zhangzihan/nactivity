using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Engine.Repository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// 定义流程部署与决策引用索引之间的应用服务接口。
    /// </summary>
    public interface IDecisionProcessReferenceIndexApplicationService
    {
        /// <summary>
        /// 查询指定决策被流程引用的情况，优先走索引，索引不可用时回退扫描 BPMN。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="includeDrafts">是否包含草稿流程。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的流程引用记录。</returns>
        Task<IReadOnlyList<DecisionProcessReferenceRecord>> QueryDecisionReferencesAsync(
            string decisionKey,
            string? tenantId = null,
            bool includeDrafts = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试同步指定部署对应的决策引用索引。
        /// </summary>
        /// <param name="deployment">部署记录。</param>
        /// <param name="bpmnXml">BPMN XML 文本。</param>
        /// <param name="isDraft">是否为草稿部署。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task TrySyncDeploymentReferencesAsync(
            IDeployment deployment,
            string bpmnXml,
            bool isDraft,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试删除指定部署对应的决策引用索引。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task TryDeleteDeploymentReferencesAsync(string deploymentId, CancellationToken cancellationToken = default);
    }
}
