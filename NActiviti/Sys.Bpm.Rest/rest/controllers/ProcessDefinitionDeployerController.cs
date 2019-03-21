using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.converter;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.exceptions;
using org.activiti.engine.impl.bpmn.parser;
using org.activiti.engine.repository;
using org.activiti.validation;
using org.springframework.hateoas;
using Sys.Bpm.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Sys.Bpm.rest.controllers
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

        /// <inheritdoc />
        public ProcessDefinitionDeployerController(IProcessEngine engine,
            DeploymentConverter deploymentConverter,
            PageableDeploymentRespositoryService pageableRepositoryService,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.repositoryService = engine.RepositoryService;
            this.deploymentConverter = deploymentConverter;
            this.pageableRepositoryService = pageableRepositoryService;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;
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
            IPage<Deployment> defs = new QueryDeploymentCmd().loadPage(this.repositoryService, this.pageableRepositoryService, queryObj);

            Resources<Deployment> list = new Resources<Deployment>(defs.getContent(), defs.getTotalItems(), queryObj.Pageable.PageNo, queryObj.Pageable.PageSize);

            return Task.FromResult(list);
        }

        /// <inheritdoc />
        [HttpPost]
        public Task<Deployment> Deploy([FromBody]ProcessDefinitionDeployer deployer)
        {
            try
            {
                IDeploymentBuilder deployment = this.repositoryService.createDeployment();

                if (deployer.DisableBpmnValidation)
                {
                    deployment.disableBpmnValidation();
                }

                if (deployer.DisableSchemaValidation)
                {
                    deployment.disableSchemaValidation();
                }

                if (deployer.EnableDuplicateFiltering)
                {
                    deployment.enableDuplicateFiltering();
                }

                string resourceName = deployer.Name.EndsWith("bpmn", StringComparison.OrdinalIgnoreCase) ? deployer.Name : $"{deployer.Name}.bpmn";

                IDeployment dep = deployment.name(deployer.Name)
                    .category(deployer.Category)
                    .key(deployer.Key)
                    .tenantId(deployer.TenantId)
                    .businessKey(deployer.BusinessKey)
                    .businessPath(deployer.BusinessPath)
                    .startForm(deployer.StartForm, deployer.BpmnXML)
                    .addString(resourceName, deployer.BpmnXML)
                    .deploy();

                return Task.FromResult(deploymentConverter.from(dep));
            }
            catch (ActivitiValidationException ex)
            {
                var http400 = new Http400()
                {
                    Code = "activitiValidationException",
                    Message = "流程定义验证失败",
                    Target = this.GetType().Name,
                };

                http400.Details = new List<Http400>();
                foreach (var err in ex.ValidationErrors ?? new List<ValidationError>())
                {
                    http400.Details.Add(new Http400
                    {
                        Code = "activitiValidationException",
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
            IDeploymentBuilder deployment = this.repositoryService
                .createDeployment()
                .disableDuplicateStartForm();

            deployment.disableBpmnValidation();
            deployment.disableSchemaValidation();
            deployment.enableDuplicateFiltering();

            string resourceName = deployer.Name.EndsWith("bpmn", StringComparison.OrdinalIgnoreCase) ? deployer.Name : $"{deployer.Name}.bpmn";

            IDeployment dep = deployment.name(deployer.Name)
                .category(deployer.Category)
                .key(deployer.Key)
                .tenantId(deployer.TenantId)
                .businessKey(deployer.BusinessKey)
                .businessPath(deployer.BusinessPath)
                .startForm(deployer.StartForm, deployer.BpmnXML)
                .addString(resourceName, deployer.BpmnXML)
                .save();

            return Task.FromResult<Deployment>(deploymentConverter.from(dep));
        }

        /// <inheritdoc />
        [HttpGet("{tenantId}/{name}/draft")]
        public Task<Deployment> Draft(string tenantId, string name)
        {
            IDeploymentQuery query = this.repositoryService.createDeploymentQuery();
            IList<IDeployment> defs = query.deploymentName(name)
                .deploymentTenantId(tenantId)
                .findDrafts();

            if (defs.Count > 1)
            {
                throw new Exception();
            }

            return Task.FromResult<Deployment>(defs?.Count == 0 ? null : deploymentConverter.from(defs[0]));
        }

        /// <inheritdoc />
        [HttpGet("{deployId}/remove")]
        public Task<IActionResult> Remove(string deployId)
        {
            this.repositoryService.deleteDeployment(deployId);

            return Task.FromResult<IActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpGet("{id}/processmodel")]
        public Task<string> GetProcessModel(string id)
        {
            IList<string> names = repositoryService.getDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.getResourceAsStream(id, names[0]);

            resourceStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[resourceStream.Length];
            resourceStream.Read(data, 0, data.Length);

            string xml = Encoding.UTF8.GetString(data);

            return Task.FromResult<string>(xml);
        }

        /// <inheritdoc />
        [HttpGet("{id}/bpmnmodel")]
        public Task<BpmnModel> GetBpmnModel(string id)
        {
            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();

            IList<string> names = repositoryService.getDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.getResourceAsStream(id, names[0]);

            BpmnModel model = bpmnXMLConverter.convertToBpmnModel(new XMLStreamReader(resourceStream));

            return Task.FromResult<BpmnModel>(model);
        }
    }
}
