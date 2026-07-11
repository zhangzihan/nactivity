using Sys.Workflow.Rules.Models;
using Sys.Workflow.Hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
{
    /// <summary>
    /// 规则决策治理与审计 Rest API。
    /// </summary>
    public interface IDecisionGovernanceController
    {
        /// <summary>
        /// 保存决策草稿。
        /// </summary>
        /// <param name="request">草稿保存请求。</param>
        /// <returns>草稿保存结果。</returns>
        Task<DecisionDraftSaveResult> SaveDraft(DecisionDraftSaveRequest request);

        /// <summary>
        /// 发布决策版本。
        /// </summary>
        /// <param name="request">版本发布请求。</param>
        /// <returns>发布结果。</returns>
        Task<DecisionVersionPublishResult> PublishVersion(PublishDecisionVersionRequest request);

        /// <summary>
        /// 回滚到指定决策版本。
        /// </summary>
        /// <param name="request">版本回滚请求。</param>
        /// <returns>回滚结果。</returns>
        Task<DecisionVersionPublishResult> RollbackVersion(RollbackDecisionVersionRequest request);

        /// <summary>
        /// 查询决策执行日志。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <returns>执行日志分页结果。</returns>
        Task<Resources<DecisionExecutionLogRecord>> QueryExecutionLogs(DecisionExecutionLogQuery query);

        /// <summary>
        /// 查询决策定义目录。
        /// </summary>
        /// <param name="query">定义查询条件。</param>
        /// <returns>命中的决策定义集合。</returns>
        Task<IReadOnlyList<DecisionDefinitionRecord>> QueryDefinitions(DecisionDefinitionRepositoryQuery query);

        /// <summary>
        /// 获取指定决策的版本列表。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>命中的版本集合。</returns>
        Task<IReadOnlyList<DecisionVersionRecord>> QueryDecisionVersions(string decisionKey);

        /// <summary>
        /// 获取指定决策定义详情。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>命中的决策定义。</returns>
        Task<DecisionDefinitionRecord?> GetDefinition(string decisionKey);

        /// <summary>
        /// 导入决策定义。
        /// </summary>
        /// <param name="request">导入请求。</param>
        /// <returns>导入结果。</returns>
        Task<DecisionImportResult> Import(DecisionImportRequest request);

        /// <summary>
        /// 导出决策定义。
        /// </summary>
        /// <param name="request">导出请求。</param>
        /// <returns>导出结果。</returns>
        Task<DecisionExportResult> Export(DecisionExportRequest request);

        /// <summary>
        /// 获取决策解释结果。
        /// </summary>
        /// <param name="request">解释请求。</param>
        /// <returns>解释结果。</returns>
        Task<DecisionExplanationResult> Explain(DecisionExplanationRequest request);

        /// <summary>
        /// 执行决策测试。
        /// </summary>
        /// <param name="request">测试请求。</param>
        /// <returns>测试结果。</returns>
        Task<DecisionTestResult> Test(DecisionTestRequest request);

        /// <summary>
        /// 获取指定执行日志详情。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <returns>执行日志明细。</returns>
        Task<DecisionExecutionLogRecord?> GetExecutionLog(string executionId);

        /// <summary>
        /// 获取指定执行记录的输入输出快照。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <returns>执行快照。</returns>
        Task<DecisionExecutionSnapshot?> GetExecutionSnapshot(string executionId);

        /// <summary>
        /// 使指定决策缓存失效。
        /// </summary>
        /// <param name="request">缓存失效请求。</param>
        Task InvalidateCache(DecisionCacheInvalidationRequest request);

        /// <summary>
        /// 查询缓存状态。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <returns>缓存状态。</returns>
        Task<DecisionCacheStatusDescriptor> GetCacheStatus(string? decisionKey);

        /// <summary>
        /// 获取规则决策与宿主设计器之间的协同能力描述。
        /// </summary>
        /// <returns>协同能力描述。</returns>
        Task<DecisionCollaborationDescriptor> GetCollaborationDescriptor();
    }
}
