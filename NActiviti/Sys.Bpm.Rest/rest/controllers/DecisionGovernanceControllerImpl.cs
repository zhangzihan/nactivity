using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{
    /// <summary>
    /// 规则决策治理与审计控制器。
    /// </summary>
    [Route(WorkflowConstants.DECISION_ROUTER_V1)]
    [ApiController]
    public class DecisionGovernanceControllerImpl : WorkflowController, IDecisionGovernanceController
    {
        private readonly IDecisionGovernanceService _decisionGovernanceService;
        private readonly IDecisionExecutionLogRepository _decisionExecutionLogRepository;
        private readonly IDecisionDefinitionRepository? _decisionDefinitionRepository;
        private readonly IDecisionVersionRepository? _decisionVersionRepository;
        private readonly IDecisionRuntimeService? _decisionRuntimeService;

        /// <summary>
        /// 初始化规则决策治理控制器。
        /// </summary>
        /// <param name="decisionGovernanceService">规则治理服务。</param>
        /// <param name="decisionExecutionLogRepository">执行日志仓储。</param>
        /// <param name="decisionDefinitionRepository">定义仓储。</param>
        /// <param name="decisionVersionRepository">版本仓储。</param>
        /// <param name="decisionRuntimeService">规则运行时服务。</param>
        public DecisionGovernanceControllerImpl(
            IDecisionGovernanceService decisionGovernanceService,
            IDecisionExecutionLogRepository decisionExecutionLogRepository,
            IDecisionDefinitionRepository? decisionDefinitionRepository = null,
            IDecisionVersionRepository? decisionVersionRepository = null,
            IDecisionRuntimeService? decisionRuntimeService = null)
        {
            _decisionGovernanceService = decisionGovernanceService ?? throw new ArgumentNullException(nameof(decisionGovernanceService));
            _decisionExecutionLogRepository = decisionExecutionLogRepository ?? throw new ArgumentNullException(nameof(decisionExecutionLogRepository));
            _decisionDefinitionRepository = decisionDefinitionRepository;
            _decisionVersionRepository = decisionVersionRepository;
            _decisionRuntimeService = decisionRuntimeService;
        }

        /// <summary>
        /// 保存决策草稿。
        /// </summary>
        /// <param name="request">草稿保存请求。</param>
        /// <returns>草稿保存结果。</returns>
        [HttpPost("drafts")]
        public async Task<DecisionDraftSaveResult> SaveDraft([FromBody] DecisionDraftSaveRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                request.Operator ??= GetAuthenticatedOperatorId();
                return await _decisionGovernanceService.SaveDraftAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(SaveDraft), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(SaveDraft), ex);
            }
        }

        /// <summary>
        /// 发布决策版本。
        /// </summary>
        /// <param name="request">版本发布请求。</param>
        /// <returns>发布结果。</returns>
        [HttpPost("publish")]
        public async Task<DecisionVersionPublishResult> PublishVersion([FromBody] PublishDecisionVersionRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                request.Operator ??= GetAuthenticatedOperatorId();
                return await _decisionGovernanceService.PublishVersionAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(PublishVersion), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(PublishVersion), ex);
            }
        }

        /// <summary>
        /// 回滚到指定决策版本。
        /// </summary>
        /// <param name="request">版本回滚请求。</param>
        /// <returns>回滚结果。</returns>
        [HttpPost("rollback")]
        public async Task<DecisionVersionPublishResult> RollbackVersion([FromBody] RollbackDecisionVersionRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                request.Operator ??= GetAuthenticatedOperatorId();
                return await _decisionGovernanceService.RollbackVersionAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(RollbackVersion), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(RollbackVersion), ex);
            }
        }

        /// <summary>
        /// 查询决策执行日志。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <returns>执行日志分页结果。</returns>
        [HttpPost("execution-logs/query")]
        public async Task<Resources<DecisionExecutionLogRecord>> QueryExecutionLogs([FromBody] DecisionExecutionLogQuery query)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(query);

                var records = await _decisionExecutionLogRepository.QueryAsync(query).ConfigureAwait(false);
                var totalCount = await _decisionExecutionLogRepository.CountAsync(query).ConfigureAwait(false);

                return new Resources<DecisionExecutionLogRecord>(records, totalCount, query.PageIndex, query.PageSize);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(QueryExecutionLogs), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(QueryExecutionLogs), ex);
            }
        }

        /// <summary>
        /// 查询决策定义目录。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <returns>命中的决策定义集合。</returns>
        [HttpPost("definitions/query")]
        public async Task<IReadOnlyList<DecisionDefinitionRecord>> QueryDefinitions([FromBody] DecisionDefinitionRepositoryQuery query)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(query);
                return await GetDecisionDefinitionRepository().QueryAsync(query).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(QueryDefinitions), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(QueryDefinitions), ex);
            }
        }

        /// <summary>
        /// 查询指定决策的版本目录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>命中的版本集合。</returns>
        [HttpGet("{decisionKey}/versions")]
        public async Task<IReadOnlyList<DecisionVersionRecord>> QueryDecisionVersions(string decisionKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(decisionKey))
                {
                    throw new ArgumentException("decisionKey is required", nameof(decisionKey));
                }

                return await GetDecisionVersionRepository().QueryByDecisionKeyAsync(decisionKey).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(QueryDecisionVersions), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(QueryDecisionVersions), ex);
            }
        }

        /// <summary>
        /// 获取指定决策定义详情。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>命中的决策定义。</returns>
        [HttpGet("definitions/{decisionKey}")]
        public async Task<DecisionDefinitionRecord?> GetDefinition(string decisionKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(decisionKey))
                {
                    throw new ArgumentException("decisionKey is required", nameof(decisionKey));
                }

                return await GetDecisionDefinitionRepository().GetByDecisionKeyAsync(decisionKey).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(GetDefinition), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(GetDefinition), ex);
            }
        }

        /// <summary>
        /// 导入决策定义。
        /// </summary>
        /// <param name="request">导入请求。</param>
        /// <returns>导入结果。</returns>
        [HttpPost("import")]
        public async Task<DecisionImportResult> Import([FromBody] DecisionImportRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                request.Operator ??= GetAuthenticatedOperatorId();
                return await GetDecisionRuntimeService().ImportAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(Import), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(Import), ex);
            }
        }

        /// <summary>
        /// 导出决策定义。
        /// </summary>
        /// <param name="request">导出请求。</param>
        /// <returns>导出结果。</returns>
        [HttpPost("export")]
        public async Task<DecisionExportResult> Export([FromBody] DecisionExportRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                return await GetDecisionRuntimeService().ExportAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(Export), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(Export), ex);
            }
        }

        /// <summary>
        /// 获取决策解释结果。
        /// </summary>
        /// <param name="request">解释请求。</param>
        /// <returns>解释结果。</returns>
        [HttpPost("explain")]
        public async Task<DecisionExplanationResult> Explain([FromBody] DecisionExplanationRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                return await GetDecisionRuntimeService().ExplainAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(Explain), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(Explain), ex);
            }
        }

        /// <summary>
        /// 执行决策测试。
        /// </summary>
        /// <param name="request">测试请求。</param>
        /// <returns>测试结果。</returns>
        [HttpPost("test")]
        public async Task<DecisionTestResult> Test([FromBody] DecisionTestRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                return await GetDecisionRuntimeService().TestAsync(request).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(Test), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(Test), ex);
            }
        }

        /// <summary>
        /// 获取指定执行日志详情。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <returns>执行日志明细。</returns>
        [HttpGet("execution-logs/{executionId}")]
        public Task<DecisionExecutionLogRecord?> GetExecutionLog(string executionId)
        {
            return _decisionExecutionLogRepository.GetByExecutionIdAsync(executionId);
        }

        /// <summary>
        /// 获取指定执行记录的输入输出快照。
        /// </summary>
        /// <param name="executionId">执行记录标识。</param>
        /// <returns>执行快照。</returns>
        [HttpGet("execution-logs/{executionId}/snapshot")]
        public Task<DecisionExecutionSnapshot?> GetExecutionSnapshot(string executionId)
        {
            return GetDecisionRuntimeService().GetExecutionSnapshotAsync(executionId);
        }

        /// <summary>
        /// 使指定决策缓存失效。
        /// </summary>
        /// <param name="request">缓存失效请求。</param>
        [HttpPost("cache/invalidate")]
        public Task InvalidateCache([FromBody] DecisionCacheInvalidationRequest request)
        {
            return GetDecisionRuntimeService().InvalidateCacheAsync(request);
        }

        /// <summary>
        /// 查询缓存状态。
        /// </summary>
        /// <param name="decisionKey">决策主键。</param>
        /// <returns>缓存状态。</returns>
        [HttpGet("cache-status")]
        public Task<DecisionCacheStatusDescriptor> GetCacheStatus([FromQuery] string? decisionKey)
        {
            return GetDecisionRuntimeService().GetCacheStatusAsync(decisionKey);
        }

        /// <summary>
        /// 获取规则决策与宿主设计器之间的协同能力描述。
        /// </summary>
        /// <returns>协同能力描述。</returns>
        [HttpGet("collaboration-descriptor")]
        public Task<DecisionCollaborationDescriptor> GetCollaborationDescriptor()
        {
            var descriptor = new DecisionCollaborationDescriptor
            {
                WorkspaceRoute = "/dmn",
                WorkspacePickerQueryKey = "picker",
                WorkspaceChannelEventName = "qhflow.decision-workspace",
                GovernanceApiBasePath = $"{WorkflowConstants.DECISION_ROUTER_V1}",
                ProcessDeployerApiBasePath = "Api/v1/workflow/process-deployer",
                WorkspaceMessageTypes = new List<string>
                {
                    "qhflow.decision-selected",
                    "qhflow.decision-saved",
                    "qhflow.decision-published",
                    "qhflow.decision-test-completed"
                },
                WorkspaceBusEvents = new List<string>
                {
                    "workflow.decision.workspace.changed",
                    "workflow.decision.test.completed"
                },
                CapabilityFlags = new Dictionary<string, bool>
                {
                    ["workspacePicker"] = true,
                    ["impactAnalysis"] = true,
                    ["publishGate"] = true,
                    ["executionSnapshot"] = true,
                    ["cacheGovernance"] = true
                }
            };

            return Task.FromResult(descriptor);
        }

        /// <summary>
        /// 获取已配置的决策定义仓储。
        /// </summary>
        /// <returns>定义仓储实例。</returns>
        private IDecisionDefinitionRepository GetDecisionDefinitionRepository()
        {
            if (_decisionDefinitionRepository == null)
            {
                throw new InvalidOperationException("当前宿主未配置决策定义仓储，无法查询决策目录。");
            }

            return _decisionDefinitionRepository;
        }

        /// <summary>
        /// 获取已配置的决策版本仓储。
        /// </summary>
        /// <returns>版本仓储实例。</returns>
        private IDecisionVersionRepository GetDecisionVersionRepository()
        {
            if (_decisionVersionRepository == null)
            {
                throw new InvalidOperationException("当前宿主未配置决策版本仓储，无法查询版本目录。");
            }

            return _decisionVersionRepository;
        }

        /// <summary>
        /// 获取已配置的规则运行时服务。
        /// </summary>
        /// <returns>规则运行时服务实例。</returns>
        private IDecisionRuntimeService GetDecisionRuntimeService()
        {
            if (_decisionRuntimeService == null)
            {
                throw new InvalidOperationException("当前宿主未配置规则运行时服务。");
            }

            return _decisionRuntimeService;
        }
    }
}
