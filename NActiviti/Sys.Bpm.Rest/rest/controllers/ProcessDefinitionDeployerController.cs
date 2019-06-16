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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Sys.Workflow.Rest.Controllers
{
    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_DEP_ROUTER_V1)]
    [ApiController]
    public class ProcessDefinitionDeployerController : ControllerBase, IProcessDefinitionDeployerController
    {
        private readonly IRepositoryService repositoryService;

        private readonly DeploymentConverter deploymentConverter;

        private readonly PageableDeploymentRespositoryService pageableRepositoryService;

        private readonly SecurityPoliciesApplicationService securityPoliciesApplicationService;

        private ILogger logger = null;

        /// <inheritdoc />
        public ProcessDefinitionDeployerController(IProcessEngine engine,
            DeploymentConverter deploymentConverter,
            PageableDeploymentRespositoryService pageableRepositoryService,
            SecurityPoliciesApplicationService securityPoliciesApplicationService,
            ILoggerFactory loggerFactory)
        {
            this.repositoryService = engine.RepositoryService;
            this.deploymentConverter = deploymentConverter;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;

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
        public Task<Deployment> Deploy([FromBody]ProcessDefinitionDeployer deployer)
        {
            try
            {
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

                return Task.FromResult(deploymentConverter.From(dep));
            }
            catch (ActivitiValidationException ex)
            {
                var http400 = new Http400()
                {
                    Code = "activitiValidationException",
                    Message = "流程定义验证失败",
                    Target = this.GetType().Name,
                };

                http400.Details = new List<HttpException>();
                foreach (var err in ex.ValidationErrors ?? new List<ValidationError>())
                {
                    http400.Details.Add(new Http400
                    {
                        Code = "activitiValidationException",
                        OriginError = err,
                        Message = err.ToString()
                    });
                }

                throw new Http400Exception(http400, ex);
            }
        }

        /// <inheritdoc />
        [HttpPost("save")]
        public Task<Deployment> Save(ProcessDefinitionDeployer deployer)
        {
            try
            {
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

                return Task.FromResult<Deployment>(deploymentConverter.From(dep));
            }
            catch (StartFormUniqueException ex)
            {
                throw new Http400Exception(new Http400
                {
                    Code = "startFormUniqueException",
                    Message = ex.Message
                }, ex);
            }
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
        public Task<ActionResult> Remove(string deployId)
        {
            try
            {
                this.repositoryService.DeleteDeployment(deployId);

                return Task.FromResult<ActionResult>(Ok());
            }
            catch (ActivitiObjectNotFoundException ex)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug($"remove deployment failed.{ex.Message}");
                }

                return Task.FromResult<ActionResult>(Ok());
            }
            catch (ExistsProcessInstanceException ex)
            {
                throw new Http400Exception(new Http400
                {
                    Code = "existsProcessInstance",
                    Message = ex.Message,
                }, ex);
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
            if (queryObj == null)
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
    }
}
