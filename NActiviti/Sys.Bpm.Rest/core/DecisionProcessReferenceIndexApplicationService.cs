using Microsoft.Extensions.Logging;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// 定义流程部署与决策引用索引之间的应用服务。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexApplicationService : IDecisionProcessReferenceIndexApplicationService
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IDecisionCatalogService? _decisionCatalogService;
        private readonly IDecisionProcessReferenceIndexRepository? _decisionProcessReferenceIndexRepository;
        private readonly ILogger<DecisionProcessReferenceIndexApplicationService> _logger;

        /// <summary>
        /// 初始化决策引用索引应用服务。
        /// </summary>
        /// <param name="processEngine">流程引擎。</param>
        /// <param name="decisionCatalogService">决策目录服务。</param>
        /// <param name="decisionProcessReferenceIndexRepository">决策引用索引仓储。</param>
        /// <param name="loggerFactory">日志工厂。</param>
        public DecisionProcessReferenceIndexApplicationService(
            IProcessEngine processEngine,
            IDecisionCatalogService? decisionCatalogService,
            IDecisionProcessReferenceIndexRepository? decisionProcessReferenceIndexRepository,
            ILoggerFactory loggerFactory)
        {
            _repositoryService = (processEngine ?? throw new ArgumentNullException(nameof(processEngine))).RepositoryService;
            _decisionCatalogService = decisionCatalogService;
            _decisionProcessReferenceIndexRepository = decisionProcessReferenceIndexRepository;
            _logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)))
                .CreateLogger<DecisionProcessReferenceIndexApplicationService>();
        }

        /// <summary>
        /// 查询指定决策被流程引用的情况，优先走索引，索引不可用时回退扫描 BPMN。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="includeDrafts">是否包含草稿流程。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的流程引用记录。</returns>
        public async Task<IReadOnlyList<DecisionProcessReferenceRecord>> QueryDecisionReferencesAsync(
            string decisionKey,
            string? tenantId = null,
            bool includeDrafts = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                throw new ArgumentException("decisionKey is required", nameof(decisionKey));
            }

            var deployments = LoadReferenceDeployments(tenantId, includeDrafts);
            var indexedRecords = await TryQueryDecisionReferencesFromIndexAsync(
                decisionKey,
                tenantId,
                includeDrafts,
                deployments,
                cancellationToken).ConfigureAwait(false);
            if (indexedRecords != null)
            {
                return indexedRecords;
            }

            return QueryDecisionReferencesByScanning(decisionKey, deployments);
        }

        /// <summary>
        /// 尝试同步指定部署对应的决策引用索引。
        /// </summary>
        /// <param name="deployment">部署记录。</param>
        /// <param name="bpmnXml">BPMN XML 文本。</param>
        /// <param name="isDraft">是否为草稿部署。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task TrySyncDeploymentReferencesAsync(
            IDeployment deployment,
            string bpmnXml,
            bool isDraft,
            CancellationToken cancellationToken = default)
        {
            if (_decisionProcessReferenceIndexRepository == null ||
                deployment == null ||
                string.IsNullOrWhiteSpace(deployment.Id) ||
                string.IsNullOrWhiteSpace(bpmnXml))
            {
                return;
            }

            try
            {
                var model = ReadBpmnModelFromXml(bpmnXml);
                var snapshot = BuildDecisionReferenceIndexSnapshot(model, deployment, isDraft);
                await _decisionProcessReferenceIndexRepository
                    .ReplaceDeploymentReferencesAsync(snapshot, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Sync decision reference index failed. deploymentId={DeploymentId}", deployment.Id);
            }
        }

        /// <summary>
        /// 尝试删除指定部署对应的决策引用索引。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task TryDeleteDeploymentReferencesAsync(string deploymentId, CancellationToken cancellationToken = default)
        {
            if (_decisionProcessReferenceIndexRepository == null || string.IsNullOrWhiteSpace(deploymentId))
            {
                return;
            }

            try
            {
                await _decisionProcessReferenceIndexRepository
                    .DeleteByDeploymentIdAsync(deploymentId, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete decision reference index failed. deploymentId={DeploymentId}", deploymentId);
            }
        }

        /// <summary>
        /// 从 BPMN XML 文本读取 BPMN 模型。
        /// </summary>
        /// <param name="bpmnXml">BPMN XML 文本。</param>
        /// <returns>BPMN 模型。</returns>
        private static BpmnModel ReadBpmnModelFromXml(string bpmnXml)
        {
            var xmlBytes = Encoding.UTF8.GetBytes(bpmnXml ?? string.Empty);
            using var stream = new MemoryStream(xmlBytes, writable: false);
            return new BpmnXMLConverter().ConvertToBpmnModel(new XMLStreamReader(stream));
        }

        /// <summary>
        /// 将 BPMN 模型中的决策绑定转换为索引快照。
        /// </summary>
        /// <param name="model">BPMN 模型。</param>
        /// <param name="deployment">部署记录。</param>
        /// <param name="isDraft">是否为草稿。</param>
        /// <returns>索引快照。</returns>
        private static DecisionProcessReferenceIndexSnapshot BuildDecisionReferenceIndexSnapshot(
            BpmnModel model,
            IDeployment deployment,
            bool isDraft)
        {
            var snapshot = new DecisionProcessReferenceIndexSnapshot
            {
                DeploymentId = deployment.Id,
                DeploymentName = deployment.Name,
                TenantId = deployment.TenantId,
                IsDraft = isDraft,
                DeployTime = NormalizeDeploymentTime(deployment.DeploymentTime)
            };

            foreach (var process in model.Processes)
            {
                var processRecord = new DecisionProcessReferenceIndexProcessRecord
                {
                    ProcessDefinitionKey = process.Id,
                    ProcessDefinitionName = process.Name
                };

                foreach (var businessRuleTask in process.FindFlowElementsOfType<BusinessRuleTask>(true))
                {
                    var decisionBinding = businessRuleTask.GetDecisionBinding();
                    if (decisionBinding == null || string.IsNullOrWhiteSpace(decisionBinding.DecisionKey))
                    {
                        continue;
                    }

                    processRecord.ReferencedTasks.Add(new DecisionProcessReferenceIndexTaskRecord
                    {
                        TaskId = businessRuleTask.Id,
                        TaskName = businessRuleTask.Name,
                        DecisionKey = decisionBinding.DecisionKey,
                        BindingMode = decisionBinding.BindingMode,
                        VersionTag = decisionBinding.VersionTag
                    });
                }

                if (processRecord.ReferencedTasks.Count > 0)
                {
                    snapshot.Processes.Add(processRecord);
                }
            }

            return snapshot;
        }

        /// <summary>
        /// 归一化部署时间，避免默认值写入索引。
        /// </summary>
        /// <param name="deploymentTime">原始部署时间。</param>
        /// <returns>可持久化的部署时间。</returns>
        private static DateTimeOffset NormalizeDeploymentTime(DateTime deploymentTime)
        {
            if (deploymentTime == default)
            {
                return DateTimeOffset.UtcNow;
            }

            if (deploymentTime.Kind == DateTimeKind.Unspecified)
            {
                return new DateTimeOffset(DateTime.SpecifyKind(deploymentTime, DateTimeKind.Utc));
            }

            return new DateTimeOffset(deploymentTime.ToUniversalTime());
        }

        /// <summary>
        /// 优先通过目录服务中的索引视图查询流程引用，索引不可用时返回空以便回退扫描。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="includeDrafts">是否包含草稿流程。</param>
        /// <param name="deployments">候选部署集合。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的引用记录；索引不可用时返回空。</returns>
        private async Task<IReadOnlyList<DecisionProcessReferenceRecord>?> TryQueryDecisionReferencesFromIndexAsync(
            string decisionKey,
            string? tenantId,
            bool includeDrafts,
            IReadOnlyList<(IDeployment Deployment, bool IsDraft)> deployments,
            CancellationToken cancellationToken)
        {
            if (_decisionCatalogService == null)
            {
                return null;
            }

            try
            {
                var deploymentIds = deployments
                    .Select(item => item.Deployment.Id)
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var descriptors = await _decisionCatalogService
                    .TryQueryDecisionReferencesAsync(new DecisionProcessReferenceCatalogQuery
                    {
                        DecisionKey = decisionKey,
                        TenantId = tenantId,
                        IncludeDrafts = includeDrafts,
                        DeploymentIds = deploymentIds
                    }, cancellationToken)
                    .ConfigureAwait(false);
                if (descriptors == null)
                {
                    return null;
                }

                return descriptors.Select(MapDecisionReferenceRecord).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Query decision references from index failed, falling back to BPMN scan. decisionKey={DecisionKey}", decisionKey);
                return null;
            }
        }

        /// <summary>
        /// 通过实时扫描 BPMN XML 回退查询流程对决策的引用。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="deployments">候选部署集合。</param>
        /// <returns>命中的引用记录集合。</returns>
        private IReadOnlyList<DecisionProcessReferenceRecord> QueryDecisionReferencesByScanning(
            string decisionKey,
            IReadOnlyList<(IDeployment Deployment, bool IsDraft)> deployments)
        {
            var records = new List<DecisionProcessReferenceRecord>();
            foreach (var deploymentSource in deployments)
            {
                var model = ReadBpmnModelFromDeployment(deploymentSource.Deployment);
                records.AddRange(BuildDecisionReferenceRecords(
                    model,
                    deploymentSource.Deployment,
                    deploymentSource.IsDraft,
                    decisionKey));
            }

            return OrderDecisionReferenceRecords(records);
        }

        /// <summary>
        /// 读取用于决策引用分析的部署集合。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="includeDrafts">是否包含草稿流程。</param>
        /// <returns>部署与草稿标记集合。</returns>
        private IReadOnlyList<(IDeployment Deployment, bool IsDraft)> LoadReferenceDeployments(string? tenantId, bool includeDrafts)
        {
            var results = new List<(IDeployment Deployment, bool IsDraft)>();

            var latestQuery = _repositoryService.CreateDeploymentQuery().SetLatestDeployment();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                latestQuery.SetDeploymentTenantId(tenantId);
            }

            foreach (var deployment in latestQuery.List())
            {
                results.Add((deployment, false));
            }

            if (!includeDrafts)
            {
                return results;
            }

            var draftQuery = _repositoryService.CreateDeploymentQuery();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                draftQuery.SetDeploymentTenantId(tenantId);
            }

            foreach (var draftDeployment in draftQuery.FindDrafts())
            {
                if (results.Any(item => item.Deployment.Id == draftDeployment.Id))
                {
                    continue;
                }

                results.Add((draftDeployment, true));
            }

            return results;
        }

        /// <summary>
        /// 从部署资源中读取 BPMN 模型。
        /// </summary>
        /// <param name="deployment">部署记录。</param>
        /// <returns>BPMN 模型。</returns>
        private BpmnModel ReadBpmnModelFromDeployment(IDeployment deployment)
        {
            var resourceNames = _repositoryService.GetDeploymentResourceNames(deployment.Id);
            if (resourceNames == null || resourceNames.Count == 0)
            {
                throw new InvalidOperationException($"部署 {deployment.Id} 未包含 BPMN 资源。");
            }

            var resourceName = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".bpmn", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".bpmn20.xml", StringComparison.OrdinalIgnoreCase)) ?? resourceNames[0];
            using var resourceStream = _repositoryService.GetResourceAsStream(deployment.Id, resourceName);
            return new BpmnXMLConverter().ConvertToBpmnModel(new XMLStreamReader(resourceStream));
        }

        /// <summary>
        /// 将 BPMN 模型中的决策绑定转换为流程引用返回模型。
        /// </summary>
        /// <param name="model">BPMN 模型。</param>
        /// <param name="deployment">部署记录。</param>
        /// <param name="isDraft">是否为草稿。</param>
        /// <param name="decisionKeyFilter">可选的决策键过滤条件。</param>
        /// <returns>引用记录集合。</returns>
        private static IReadOnlyList<DecisionProcessReferenceRecord> BuildDecisionReferenceRecords(
            BpmnModel model,
            IDeployment deployment,
            bool isDraft,
            string? decisionKeyFilter = null)
        {
            var records = new List<DecisionProcessReferenceRecord>();
            foreach (var process in model.Processes)
            {
                var referencedTasks = new List<DecisionProcessReferenceTaskRecord>();
                foreach (var businessRuleTask in process.FindFlowElementsOfType<BusinessRuleTask>(true))
                {
                    var decisionBinding = businessRuleTask.GetDecisionBinding();
                    if (decisionBinding == null || string.IsNullOrWhiteSpace(decisionBinding.DecisionKey))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(decisionKeyFilter) &&
                        !string.Equals(decisionBinding.DecisionKey, decisionKeyFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    referencedTasks.Add(new DecisionProcessReferenceTaskRecord
                    {
                        TaskId = businessRuleTask.Id,
                        TaskName = businessRuleTask.Name,
                        DecisionKey = decisionBinding.DecisionKey,
                        BindingMode = decisionBinding.BindingMode,
                        VersionTag = decisionBinding.VersionTag
                    });
                }

                if (referencedTasks.Count == 0)
                {
                    continue;
                }

                records.Add(new DecisionProcessReferenceRecord
                {
                    DeploymentId = deployment.Id,
                    DeploymentName = deployment.Name,
                    ProcessDefinitionKey = process.Id,
                    ProcessDefinitionName = process.Name,
                    TenantId = deployment.TenantId,
                    IsDraft = isDraft,
                    DeployTime = deployment.DeploymentTime,
                    ReferencedTasks = referencedTasks
                });
            }

            return records;
        }

        /// <summary>
        /// 按既有口径排序引用记录。
        /// </summary>
        /// <param name="records">待排序记录集合。</param>
        /// <returns>排序后的引用记录集合。</returns>
        private static IReadOnlyList<DecisionProcessReferenceRecord> OrderDecisionReferenceRecords(
            IEnumerable<DecisionProcessReferenceRecord> records)
        {
            return records
                .OrderByDescending(item => item.IsDraft)
                .ThenByDescending(item => item.DeployTime)
                .ToList();
        }

        /// <summary>
        /// 将目录服务引用结果映射为 Rest 返回模型。
        /// </summary>
        /// <param name="descriptor">目录服务引用结果。</param>
        /// <returns>Rest 返回模型。</returns>
        private static DecisionProcessReferenceRecord MapDecisionReferenceRecord(DecisionProcessReferenceDescriptor descriptor)
        {
            return new DecisionProcessReferenceRecord
            {
                DeploymentId = descriptor.DeploymentId,
                DeploymentName = descriptor.DeploymentName,
                ProcessDefinitionKey = descriptor.ProcessDefinitionKey,
                ProcessDefinitionName = descriptor.ProcessDefinitionName,
                TenantId = descriptor.TenantId,
                IsDraft = descriptor.IsDraft,
                DeployTime = descriptor.DeployTime,
                ReferencedTasks = descriptor.ReferencedTasks
                    .Select(item => new DecisionProcessReferenceTaskRecord
                    {
                        TaskId = item.TaskId,
                        TaskName = item.TaskName,
                        DecisionKey = item.DecisionKey,
                        BindingMode = item.BindingMode,
                        VersionTag = item.VersionTag
                    })
                    .ToList()
            };
        }
    }
}
