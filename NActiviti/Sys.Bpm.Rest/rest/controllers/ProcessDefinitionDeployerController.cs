using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using Sys.Bpm.rest.api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.rest.controllers
{
    [Route("workflow/process-deployer")]
    [ApiController]
    public class ProcessDefinitionDeployerController : ControllerBase, IProcessDefinitionDeployerController
    {
        private readonly IRepositoryService repositoryService;

        private readonly DeploymentConverter deploymentConverter;

        private readonly PageableDeploymentRespositoryService pageableRepositoryService;

        private readonly SecurityPoliciesApplicationService securityPoliciesApplicationService;

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

        [HttpPost("latest")]
        public Task<Resources<Deployment>> LatestDeployments(DeploymentQuery queryObj)
        {
            queryObj = queryObj ?? new DeploymentQuery();
            queryObj.LatestDeployment = true;

            return AllDeployments(queryObj);
        }

        [HttpPost("list")]
        public Task<Resources<Deployment>> AllDeployments(DeploymentQuery queryObj)
        {
            IPage<Deployment> defs = new QueryDeploymentCmd().loadPage(this.repositoryService, this.pageableRepositoryService, queryObj);

            Resources<Deployment> list = new Resources<Deployment>(defs.getContent(), defs.getTotalItems(), queryObj.Pageable.Offset, queryObj.Pageable.PageSize);

            return System.Threading.Tasks.Task.FromResult(list);
        }

        [HttpPost]
        public Task<Deployment> Deploy([FromBody]ProcessDefinitionDeployer deployer)
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

            return Task.FromResult<Deployment>(deploymentConverter.from(dep));
        }

        [HttpPost("save")]
        public Task<Deployment> Save(ProcessDefinitionDeployer deployer)
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
                .save();

            return Task.FromResult<Deployment>(deploymentConverter.from(dep));
        }

        [HttpGet("{deployId}/remove")]
        public Task<IActionResult> RemoveDeployment(string deployId)
        {
            this.repositoryService.deleteDeployment(deployId);

            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
