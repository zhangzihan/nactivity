using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.activiti.engine.impl.persistence.entity;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace org.activiti.cloud.services.rest.controllers
{
    [Route(WorkflowConstants.PROC_INS_VAR_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceVariableControllerImpl : ControllerBase, IProcessInstanceVariableController
    {

        private readonly IRuntimeService runtimeService;
        private readonly ProcessInstanceVariableResourceAssembler variableResourceBuilder;
        private readonly SecurityPoliciesApplicationService securityPoliciesApplicationService;
        private readonly ProcessEngineWrapper processEngine;

        //public virtual string handleAppException(ActivitiForbiddenException ex)
        //{
        //    return ex.Message;
        //}

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        public ProcessInstanceVariableControllerImpl(IProcessEngine engine,
            ProcessInstanceVariableResourceAssembler variableResourceBuilder, 
            SecurityPoliciesApplicationService securityPoliciesApplicationService, 
            ProcessEngineWrapper processEngine)
        {
            this.runtimeService = engine.RuntimeService;
            this.variableResourceBuilder = variableResourceBuilder;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;
            this.processEngine = processEngine;
        }

        [HttpGet]
        public virtual Task<Resources<ProcessVariableResource>> getVariables(string processInstanceId)
        {
            IList<IVariableInstance> variableInstances = runtimeService.getVariableInstancesByExecutionIds(new HashSet<string> { processInstanceId });

            IList<ProcessVariableResource> resourcesList = new List<ProcessVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.toResource(new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId)));
            }

            Resources<ProcessVariableResource> resources = new Resources<ProcessVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }

        [HttpGet("local")]
        public virtual Task<Resources<ProcessVariableResource>> getVariablesLocal(string processInstanceId)
        {
            IDictionary<string, IVariableInstance> variableInstancesMap = runtimeService.getVariableInstancesLocal(processInstanceId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<ProcessVariableResource> resourcesList = new List<ProcessVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.toResource(new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId)));
            }

            Resources<ProcessVariableResource> resources = new Resources<ProcessVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }

        [HttpPost]
        public virtual Task<IActionResult> setVariables(string processInstanceId, SetProcessVariablesCmd setProcessVariablesCmd)
        {
            processEngine.ProcessVariables = setProcessVariablesCmd;

            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("remove")]
        public virtual Task<IActionResult> removeVariables(string processInstanceId, RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            this.processEngine.removeProcessVariables(removeProcessVariablesCmd);

            return Task.FromResult<IActionResult>(Ok());
        }
    }

}