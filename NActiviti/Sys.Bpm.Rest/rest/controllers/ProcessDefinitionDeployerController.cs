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

        private readonly IDecisionCatalogService decisionCatalogService;

        private readonly IDecisionProcessReferenceIndexApplicationService decisionProcessReferenceIndexApplicationService;

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
            IDecisionCatalogService decisionCatalogService = null,
            IDecisionProcessReferenceIndexApplicationService decisionProcessReferenceIndexApplicationService = null,
            IDataSource dataSource = null,
            IDatabaseReader databaseReader = null)
        {
            this.repositoryService = engine.RepositoryService;
            this.deploymentConverter = deploymentConverter;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;
            this.decisionCatalogService = decisionCatalogService;
            this.decisionProcessReferenceIndexApplicationService = decisionProcessReferenceIndexApplicationService;
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

                if (decisionProcessReferenceIndexApplicationService == null)
                {
                    throw new InvalidOperationException("当前宿主未配置决策流程引用应用服务。");
                }

                return await decisionProcessReferenceIndexApplicationService
                    .QueryDecisionReferencesAsync(decisionKey, tenantId, includeDrafts)
                    .ConfigureAwait(false);
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
        /// 尝试将指定部署的决策引用写入索引，不影响主流程保存与部署结果。
        /// </summary>
        /// <param name="deployment">部署记录。</param>
        /// <param name="bpmnXml">BPMN XML 文本。</param>
        /// <param name="isDraft">是否为草稿部署。</param>
        private Task TrySyncDecisionReferenceIndexAsync(
            IDeployment deployment,
            string bpmnXml,
            bool isDraft)
        {
            if (decisionProcessReferenceIndexApplicationService == null)
            {
                return Task.CompletedTask;
            }

            return decisionProcessReferenceIndexApplicationService.TrySyncDeploymentReferencesAsync(
                deployment,
                bpmnXml,
                isDraft);
        }

        /// <summary>
        /// 尝试删除指定部署的决策引用索引，不影响主流程删除结果。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        private Task TryDeleteDecisionReferenceIndexAsync(string deploymentId)
        {
            if (decisionProcessReferenceIndexApplicationService == null)
            {
                return Task.CompletedTask;
            }

            return decisionProcessReferenceIndexApplicationService.TryDeleteDeploymentReferencesAsync(deploymentId);
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
            if (decisionCatalogService == null)
            {
                return;
            }

            var validationRequest = new DecisionBindingValidationRequest
            {
                Bindings = model.Processes
                    .SelectMany(process => process.FindFlowElementsOfType<BusinessRuleTask>(true))
                    .Select(businessRuleTask =>
                    {
                        var decisionBinding = businessRuleTask.GetDecisionBinding();
                        return new DecisionBindingReference
                        {
                            SourceId = businessRuleTask.Id,
                            DecisionKey = decisionBinding?.DecisionKey ?? string.Empty,
                            BindingMode = decisionBinding?.BindingMode,
                            VersionTag = decisionBinding?.VersionTag
                        };
                    })
                    .ToList()
            };
            await decisionCatalogService.ValidateDecisionBindingsAsync(validationRequest).ConfigureAwait(false);
        }

    }
}
