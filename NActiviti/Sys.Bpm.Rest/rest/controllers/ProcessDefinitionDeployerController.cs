using Microsoft.AspNetCore.Mvc;
using org.activiti.engine;
using org.activiti.engine.repository;
using Sys.Bpm.api.model;
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
        private readonly IRepositoryService repository;

        public ProcessDefinitionDeployerController(IProcessEngine engine)
        {
            this.repository = engine.RepositoryService;
        }

        [HttpPost]
        public Task<IActionResult> Deploy([FromBody]ProcessDefinitionDeployer deployer)
        {
            IDeploymentBuilder deployment = this.repository.createDeployment();

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

            deployment.name(deployer.Name)
                .category(deployer.Category)
                .key(deployer.Key)
                .tenantId(deployer.TenantId)
                .addString(resourceName, deployer.BpmnXML)                
                .deploy();

            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
