using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Exceptions;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Validation;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;
using DatabaseSchemaReader;
using IDataSource = Sys.Data.IDataSource;
using Sys.Workflow.Cloud.Services.Rest.Controllers;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.DB;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{
    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_DEP_ROUTER_V1)]
    [ApiController]
    public class ProcessDefinitionDeployerController : WorkflowController, IProcessDefinitionDeployerController
    {
        private readonly IRepositoryService repositoryService;

        private readonly DeploymentConverter deploymentConverter;

        private readonly PageableDeploymentRespositoryService pageableRepositoryService;

        private readonly SecurityPoliciesApplicationService securityPoliciesApplicationService;

        private readonly IDecisionDefinitionRepository decisionDefinitionRepository;

        private readonly IDecisionVersionRepository decisionVersionRepository;

        private readonly IDecisionProcessReferenceIndexRepository decisionProcessReferenceIndexRepository;

        private readonly ProcessEngineConfigurationImpl processEngineConfiguration;

        private readonly IDataSource dataSource;

        private readonly IDatabaseReader databaseReader;

        private ILogger logger = null;

        /// <inheritdoc />
        public ProcessDefinitionDeployerController(IProcessEngine engine,
            DeploymentConverter deploymentConverter,
            PageableDeploymentRespositoryService pageableRepositoryService,
            SecurityPoliciesApplicationService securityPoliciesApplicationService,
            ILoggerFactory loggerFactory,
            IDecisionDefinitionRepository decisionDefinitionRepository = null,
            IDecisionVersionRepository decisionVersionRepository = null,
            IDecisionProcessReferenceIndexRepository decisionProcessReferenceIndexRepository = null,
            IDataSource dataSource = null,
            IDatabaseReader databaseReader = null)
        {
            this.repositoryService = engine.RepositoryService;
            this.deploymentConverter = deploymentConverter;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;
            this.decisionDefinitionRepository = decisionDefinitionRepository;
            this.decisionVersionRepository = decisionVersionRepository;
            this.decisionProcessReferenceIndexRepository = decisionProcessReferenceIndexRepository;
            this.processEngineConfiguration = engine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl;
            this.dataSource = dataSource;
            this.databaseReader = databaseReader;

            logger = loggerFactory.CreateLogger<ProcessDefinitionDeployerController>();
        }

        /// <inheritdoc />
        [HttpPost("latest")]
        public Task<Resources<Deployment>> Latest(DeploymentQuery queryObj)
        {
            queryObj = queryObj ?? new DeploymentQuery();
            queryObj.LatestDeployment = true;

            return AllDeployments(queryObj);
        }

        /// <inheritdoc />

        [HttpPost("list")]
        public Task<Resources<Deployment>> AllDeployments(DeploymentQuery queryObj)
        {
            IPage<Deployment> defs = new QueryDeploymentCmd().LoadPage(this.repositoryService, this.pageableRepositoryService, queryObj);

            Resources<Deployment> list = new Resources<Deployment>(defs.GetContent(), defs.GetTotalItems(), queryObj.Pageable.PageNo, queryObj.Pageable.PageSize);

            return Task.FromResult(list);
        }

        /// <inheritdoc />
        [HttpPost]
        public async Task<Deployment> Deploy([FromBody]ProcessDefinitionDeployer deployer)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(deployer);
                await ValidateDecisionBindingsAsync(deployer).ConfigureAwait(false);

                IDeploymentBuilder deployment = this.repositoryService.CreateDeployment();

                if (deployer.DisableSchemaValidation)
                {
                    deployment.DisableSchemaValidation();
                }

                if (deployer.EnableDuplicateFiltering)
                {
                    deployment.EnableDuplicateFiltering();
                }

                string resourceName = deployer.Name.EndsWith("bpmn", StringComparison.OrdinalIgnoreCase) ? deployer.Name : $"{deployer.Name}.bpmn";

                IDeployment dep = deployment.Name(deployer.Name)
                    .Category(deployer.Category)
                    .Key(deployer.Key)
                    .TenantId(deployer.TenantId)
                    .BusinessKey(deployer.BusinessKey)
                    .BusinessPath(deployer.BusinessPath)
                    .StartForm(deployer.StartForm, deployer.BpmnXML)
                    .AddString(resourceName, deployer.BpmnXML)
                    .Deploy();

                await TrySyncDecisionReferenceIndexAsync(dep, deployer.BpmnXML, isDraft: false).ConfigureAwait(false);
                return deploymentConverter.From(dep);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(Deploy), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(Deploy), ex);
            }
        }

        /// <inheritdoc />
        [HttpPost("save")]
        public async Task<Deployment> Save(ProcessDefinitionDeployer deployer)
        {
            ArgumentNullException.ThrowIfNull(deployer);

            IDeploymentBuilder deployment = this.repositoryService
            .CreateDeployment()
            .DisableDuplicateStartForm();

            deployment.DisableBpmnValidation();
            deployment.DisableSchemaValidation();
            deployment.EnableDuplicateFiltering();

            string resourceName = deployer.Name.EndsWith("bpmn", StringComparison.OrdinalIgnoreCase) ? deployer.Name : $"{deployer.Name}.bpmn";

            IDeployment dep = deployment.Name(deployer.Name)
                .Category(deployer.Category)
                .Key(deployer.Key)
                .TenantId(deployer.TenantId)
                .BusinessKey(deployer.BusinessKey)
                .BusinessPath(deployer.BusinessPath)
                .StartForm(deployer.StartForm, deployer.BpmnXML)
                .AddString(resourceName, deployer.BpmnXML)
                .Save();

            await TrySyncDecisionReferenceIndexAsync(dep, deployer.BpmnXML, isDraft: true).ConfigureAwait(false);
            return deploymentConverter.From(dep);
        }

        /// <inheritdoc />
        [HttpGet("{tenantId}/{name}/draft")]
        public Task<Deployment> Draft(string tenantId, string name)
        {
            IDeploymentQuery query = this.repositoryService.CreateDeploymentQuery();
            IList<IDeployment> defs = query.SetDeploymentName(name)
                .SetDeploymentTenantId(tenantId)
                .FindDrafts();

            if (defs.Count > 1)
            {
                throw new Exception();
            }

            return Task.FromResult<Deployment>(defs?.Count == 0 ? null : deploymentConverter.From(defs[0]));
        }

        /// <inheritdoc />
        [HttpGet("{deployId}/remove")]
        public async Task<bool> Remove(string deployId)
        {
            try
            {
                await TryDeleteDecisionReferenceIndexAsync(deployId).ConfigureAwait(false);
                this.repositoryService.DeleteDeployment(deployId);

                return true;
            }
            catch (ActivitiObjectNotFoundException ex)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug($"remove deployment failed.{ex.Message}");
                }

                return true;
            }
        }

        /// <inheritdoc />
        [HttpGet("{id}/processmodel")]
        public Task<string> GetProcessModel(string id)
        {
            IList<string> names = repositoryService.GetDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.GetResourceAsStream(id, names[0]);

            resourceStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[resourceStream.Length];
            resourceStream.Read(data, 0, data.Length);

            string xml = Encoding.UTF8.GetString(data);

            return Task.FromResult<string>(xml);
        }

        /// <inheritdoc />
        [HttpPost("processmodel")]
        public Task<string> GetProcessModel(DeploymentQuery queryObj)
        {
            if (queryObj is null)
            {
                throw new ArgumentNullException("queryObj");
            }

            IDeploymentQuery query = this.repositoryService.CreateDeploymentQuery();
            IList<IDeployment> defs = query.SetDeploymentName(queryObj.Name)
                .SetDeploymentTenantId(queryObj.TenantId)
                .FindDrafts();

            if (defs.Count != 1)
            {
                throw new ActivitiException("找到不止一个草稿");
            }

            return GetProcessModel(defs[0].Id);
        }

        /// <inheritdoc />
        [HttpGet("{id}/bpmnmodel")]
        public Task<ActionResult<BpmnModel>> GetBpmnModel(string id)
        {
            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();

            IList<string> names = repositoryService.GetDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.GetResourceAsStream(id, names[0]);

            BpmnModel model = bpmnXMLConverter.ConvertToBpmnModel(new XMLStreamReader(resourceStream));

            return Task.FromResult<ActionResult<BpmnModel>>(new JsonResult(model, new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }

        /// <inheritdoc />
        [HttpGet("decision-references/{decisionKey}")]
        public async Task<IReadOnlyList<DecisionProcessReferenceRecord>> QueryDecisionReferences(
            string decisionKey,
            [FromQuery] string tenantId = null,
            [FromQuery] bool includeDrafts = true)
        {
            try
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
                    deployments).ConfigureAwait(false);
                if (indexedRecords != null)
                {
                    return indexedRecords;
                }

                return QueryDecisionReferencesByScanning(decisionKey, deployments);
            }
            catch (ArgumentException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_REQUEST, ex.Message, nameof(QueryDecisionReferences), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw CreateHttp400Exception(WorkflowConstants.DECISION_GOVERNANCE_INVALID_STATE, ex.Message, nameof(QueryDecisionReferences), ex);
            }
        }

        /// <inheritdoc />
        [HttpGet("schema-status")]
        public async Task<DecisionSchemaStatusRecord> GetDecisionSchemaStatus()
        {
            string databaseType = processEngineConfiguration?.DatabaseType ?? string.Empty;
            string databaseSchemaUpdate = processEngineConfiguration?.DatabaseSchemaUpdate ?? string.Empty;

            if (!DecisionSchemaWarningAdvisor.IsSupportedDatabaseType(databaseType))
            {
                return DecisionSchemaStatusBuilder.BuildSkippedStatus(databaseType, databaseSchemaUpdate);
            }

            try
            {
                string actualSchemaVersion = await QueryDatabaseSchemaVersionAsync().ConfigureAwait(false);
                IReadOnlyList<string> missingTables = QueryMissingDecisionSchemaTables();
                return DecisionSchemaStatusBuilder.BuildCheckedStatus(
                    databaseType,
                    databaseSchemaUpdate,
                    actualSchemaVersion,
                    missingTables);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Inspect decision schema status failed. databaseType={DatabaseType}",
                    databaseType);
                return DecisionSchemaStatusBuilder.BuildInspectionFailedStatus(
                    databaseType,
                    databaseSchemaUpdate,
                    ex.Message);
            }
        }

        /// <summary>
        /// 优先通过引用索引查询流程对决策的引用，索引不完整时返回空以便回退扫描。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="includeDrafts">是否包含草稿部署。</param>
        /// <param name="deployments">候选部署集合。</param>
        /// <returns>命中的引用记录；索引不可用或不完整时返回空。</returns>
        private async Task<IReadOnlyList<DecisionProcessReferenceRecord>?> TryQueryDecisionReferencesFromIndexAsync(
            string decisionKey,
            string tenantId,
            bool includeDrafts,
            IReadOnlyList<(IDeployment Deployment, bool IsDraft)> deployments)
        {
            if (decisionProcessReferenceIndexRepository == null)
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

                var indexedDeploymentIds = await decisionProcessReferenceIndexRepository
                    .GetIndexedDeploymentIdsAsync(deploymentIds)
                    .ConfigureAwait(false);
                if (deploymentIds.Count != indexedDeploymentIds.Count)
                {
                    return null;
                }

                var entries = await decisionProcessReferenceIndexRepository
                    .QueryEntriesAsync(new DecisionProcessReferenceIndexQuery
                    {
                        DecisionKey = decisionKey,
                        TenantId = tenantId,
                        IncludeDrafts = includeDrafts,
                        DeploymentIds = deploymentIds
                    })
                    .ConfigureAwait(false);
                return GroupDecisionReferenceIndexEntries(entries);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Query decision references from index failed, falling back to BPMN scan. decisionKey={DecisionKey}", decisionKey);
                return null;
            }
        }

        /// <summary>
        /// 查询数据库当前记录的 workflow schema 版本。
        /// </summary>
        /// <returns>数据库中的 schema.version 值。</returns>
        private async Task<string> QueryDatabaseSchemaVersionAsync()
        {
            if (dataSource == null)
            {
                throw new InvalidOperationException("当前宿主未注入工作流数据源。");
            }

            await using var connection = dataSource.GetConnection() as DbConnection
                ?? throw new InvalidOperationException("当前工作流数据源未提供可异步访问的 DbConnection。");
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "select value_ from act_ge_property where name_ = @name";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@name";
            parameter.Value = "schema.version";
            command.Parameters.Add(parameter);

            object result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 查询当前 PostgreSQL 规则扩展 schema 缺失的表集合。
        /// </summary>
        /// <returns>缺失表名集合。</returns>
        private IReadOnlyList<string> QueryMissingDecisionSchemaTables()
        {
            if (databaseReader == null)
            {
                throw new InvalidOperationException("当前宿主未注入数据库元数据读取器。");
            }

            var existingTables = new HashSet<string>(
                databaseReader.TableList()
                    .Select(item => item.Name)
                    .Where(item => !string.IsNullOrWhiteSpace(item)),
                StringComparer.OrdinalIgnoreCase);

            return DecisionSchemaWarningAdvisor.GetMissingTables(existingTables.Contains);
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
        /// <param name="includeDrafts">是否包含草稿。</param>
        /// <returns>部署与草稿标记集合。</returns>
        private IReadOnlyList<(IDeployment Deployment, bool IsDraft)> LoadReferenceDeployments(string tenantId, bool includeDrafts)
        {
            var results = new List<(IDeployment Deployment, bool IsDraft)>();

            var latestQuery = repositoryService.CreateDeploymentQuery().SetLatestDeployment();
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

            var draftQuery = repositoryService.CreateDeploymentQuery();
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
            var resourceNames = repositoryService.GetDeploymentResourceNames(deployment.Id);
            if (resourceNames == null || resourceNames.Count == 0)
            {
                throw new InvalidOperationException($"部署 {deployment.Id} 未包含 BPMN 资源。");
            }

            var resourceName = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".bpmn", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".bpmn20.xml", StringComparison.OrdinalIgnoreCase)) ?? resourceNames[0];
            using var resourceStream = repositoryService.GetResourceAsStream(deployment.Id, resourceName);
            return new BpmnXMLConverter().ConvertToBpmnModel(new XMLStreamReader(resourceStream));
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
            string decisionKeyFilter = null)
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
        /// 将索引明细行分组为流程引用返回模型。
        /// </summary>
        /// <param name="entries">索引明细行。</param>
        /// <returns>分组后的流程引用集合。</returns>
        private static IReadOnlyList<DecisionProcessReferenceRecord> GroupDecisionReferenceIndexEntries(
            IReadOnlyList<DecisionProcessReferenceIndexEntryRecord> entries)
        {
            var records = entries
                .Where(item => !string.IsNullOrWhiteSpace(item.DecisionKey) && !string.IsNullOrWhiteSpace(item.TaskId))
                .GroupBy(item => new
                {
                    item.DeploymentId,
                    item.DeploymentName,
                    item.ProcessDefinitionKey,
                    item.ProcessDefinitionName,
                    item.TenantId,
                    item.IsDraft,
                    item.DeployTime
                })
                .Select(group => new DecisionProcessReferenceRecord
                {
                    DeploymentId = group.Key.DeploymentId,
                    DeploymentName = group.Key.DeploymentName,
                    ProcessDefinitionKey = group.Key.ProcessDefinitionKey ?? string.Empty,
                    ProcessDefinitionName = group.Key.ProcessDefinitionName,
                    TenantId = group.Key.TenantId,
                    IsDraft = group.Key.IsDraft,
                    DeployTime = group.Key.DeployTime.UtcDateTime,
                    ReferencedTasks = group
                        .OrderBy(item => item.TaskId)
                        .Select(item => new DecisionProcessReferenceTaskRecord
                        {
                            TaskId = item.TaskId ?? string.Empty,
                            TaskName = item.TaskName,
                            DecisionKey = item.DecisionKey ?? string.Empty,
                            BindingMode = item.BindingMode,
                            VersionTag = item.VersionTag
                        })
                        .ToList()
                });

            return OrderDecisionReferenceRecords(records);
        }

        /// <summary>
        /// 尝试将指定部署的决策引用写入索引，不影响主流程保存与部署结果。
        /// </summary>
        /// <param name="deployment">部署记录。</param>
        /// <param name="bpmnXml">BPMN XML 文本。</param>
        /// <param name="isDraft">是否为草稿部署。</param>
        private async Task TrySyncDecisionReferenceIndexAsync(
            IDeployment deployment,
            string bpmnXml,
            bool isDraft)
        {
            if (decisionProcessReferenceIndexRepository == null ||
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
                await decisionProcessReferenceIndexRepository
                    .ReplaceDeploymentReferencesAsync(snapshot)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Sync decision reference index failed. deploymentId={DeploymentId}", deployment.Id);
            }
        }

        /// <summary>
        /// 尝试删除指定部署的决策引用索引，不影响主流程删除结果。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        private async Task TryDeleteDecisionReferenceIndexAsync(string deploymentId)
        {
            if (decisionProcessReferenceIndexRepository == null || string.IsNullOrWhiteSpace(deploymentId))
            {
                return;
            }

            try
            {
                await decisionProcessReferenceIndexRepository
                    .DeleteByDeploymentIdAsync(deploymentId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Delete decision reference index failed. deploymentId={DeploymentId}", deploymentId);
            }
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
        /// 对流程发布请求中的决策绑定执行门禁校验。
        /// </summary>
        /// <param name="deployer">发布请求。</param>
        private async Task ValidateDecisionBindingsAsync(ProcessDefinitionDeployer deployer)
        {
            if (string.IsNullOrWhiteSpace(deployer.BpmnXML))
            {
                throw new ArgumentException("BpmnXML is required", nameof(deployer));
            }

            var xmlBytes = Encoding.UTF8.GetBytes(deployer.BpmnXML);
            using var stream = new MemoryStream(xmlBytes, writable: false);
            var model = new BpmnXMLConverter().ConvertToBpmnModel(new XMLStreamReader(stream));
            await ValidateDecisionBindingsAsync(model).ConfigureAwait(false);
        }

        /// <summary>
        /// 对 BPMN 模型中的规则任务决策绑定执行门禁校验。
        /// </summary>
        /// <param name="model">BPMN 模型。</param>
        private async Task ValidateDecisionBindingsAsync(BpmnModel model)
        {
            foreach (var process in model.Processes)
            {
                foreach (var businessRuleTask in process.FindFlowElementsOfType<BusinessRuleTask>(true))
                {
                    var decisionBinding = businessRuleTask.GetDecisionBinding();
                    if (decisionBinding == null || string.IsNullOrWhiteSpace(decisionBinding.DecisionKey))
                    {
                        throw new ArgumentException($"规则任务 {businessRuleTask.Id} 缺少有效的 decisionKey。", nameof(model));
                    }

                    if ((string.Equals(decisionBinding.BindingMode, "FixedVersion", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(decisionBinding.BindingMode, "VersionTagPreferred", StringComparison.OrdinalIgnoreCase)) &&
                        string.IsNullOrWhiteSpace(decisionBinding.VersionTag))
                    {
                        throw new ArgumentException($"规则任务 {businessRuleTask.Id} 在 {decisionBinding.BindingMode} 模式下必须指定 versionTag。", nameof(model));
                    }

                    if (decisionDefinitionRepository == null || decisionVersionRepository == null)
                    {
                        continue;
                    }

                    var definitionRecord = await decisionDefinitionRepository
                        .GetByDecisionKeyAsync(decisionBinding.DecisionKey)
                        .ConfigureAwait(false);
                    if (definitionRecord == null)
                    {
                        throw new InvalidOperationException($"规则任务 {businessRuleTask.Id} 引用了不存在的决策 {decisionBinding.DecisionKey}。");
                    }

                    if (!string.IsNullOrWhiteSpace(decisionBinding.VersionTag))
                    {
                        var versionRecord = await decisionVersionRepository
                            .GetByVersionTagAsync(decisionBinding.DecisionKey, decisionBinding.VersionTag)
                            .ConfigureAwait(false);
                        if (versionRecord == null)
                        {
                            throw new InvalidOperationException($"规则任务 {businessRuleTask.Id} 引用了不存在的决策版本 {decisionBinding.DecisionKey}:{decisionBinding.VersionTag}。");
                        }

                        if (!versionRecord.IsPublished)
                        {
                            throw new InvalidOperationException($"规则任务 {businessRuleTask.Id} 引用了未发布的决策版本 {decisionBinding.DecisionKey}:{decisionBinding.VersionTag}。");
                        }

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(definitionRecord.PublishedVersionTag))
                    {
                        throw new InvalidOperationException($"规则任务 {businessRuleTask.Id} 引用的决策 {decisionBinding.DecisionKey} 尚无已发布版本。");
                    }
                }
            }
        }
    }
}
