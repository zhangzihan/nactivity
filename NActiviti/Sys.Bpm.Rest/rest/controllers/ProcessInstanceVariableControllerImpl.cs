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
    [Route("/v1/process-instances/{processInstanceId}/variables")]
    [ApiController]
    public class ProcessInstanceVariableControllerImpl : ControllerBase, IProcessInstanceVariableController
    {

        private readonly IRuntimeService runtimeService;
        private readonly ProcessInstanceVariableResourceAssembler variableResourceBuilder;
        private readonly SecurityPoliciesApplicationService securityPoliciesApplicationService;
        private readonly ProcessEngineWrapper processEngine;

        public virtual string handleAppException(ActivitiForbiddenException ex)
        {
            return ex.Message;
        }

        public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        {
            return ex.Message;
        }

        public ProcessInstanceVariableControllerImpl(IRuntimeService runtimeService, ProcessInstanceVariableResourceAssembler variableResourceBuilder, SecurityPoliciesApplicationService securityPoliciesApplicationService, ProcessEngineWrapper processEngine)
        {
            this.runtimeService = runtimeService;
            this.variableResourceBuilder = variableResourceBuilder;
            this.securityPoliciesApplicationService = securityPoliciesApplicationService;
            this.processEngine = processEngine;
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override public org.springframework.hateoas.Resources<org.activiti.cloud.services.rest.api.resources.ProcessVariableResource> getVariables(@PathVariable String processInstanceId)
        public virtual Resources<ProcessVariableResource> getVariables(string processInstanceId)
        {
            IList<IVariableInstance> variableInstances = runtimeService.getVariableInstancesByExecutionIds(new HashSet<string> { processInstanceId });

            IList<ProcessVariableResource> resourcesList = new List<ProcessVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.toResource(new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId)));
            }

            Resources<ProcessVariableResource> resources = new Resources<ProcessVariableResource>(resourcesList);
            return resources;
        }

        public virtual Resources<ProcessVariableResource> getVariablesLocal(string processInstanceId)
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
            return resources;
        }


        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override public org.springframework.http.ResponseEntity<Void> setVariables(@PathVariable String processInstanceId, @RequestBody org.activiti.cloud.services.api.commands.SetProcessVariablesCmd setProcessVariablesCmd)
        public virtual IActionResult setVariables(string processInstanceId, SetProcessVariablesCmd setProcessVariablesCmd)
        {
            processEngine.ProcessVariables = setProcessVariablesCmd;

            return Ok();
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override public org.springframework.http.ResponseEntity<Void> removeVariables(@PathVariable String processInstanceId, @RequestBody org.activiti.cloud.services.api.commands.RemoveProcessVariablesCmd removeProcessVariablesCmd)
        public virtual IActionResult removeVariables(string processInstanceId, RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            this.processEngine.removeProcessVariables(removeProcessVariablesCmd);

            return Ok();
        }
    }

}