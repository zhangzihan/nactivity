using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Hateoas;
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

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
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


        /// <inheritdoc />
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


        /// <inheritdoc />
        [HttpGet]
        public virtual Task<Resources<ProcessInstanceVariable>> GetVariables(string processInstanceId)
        {
            IList<IVariableInstance> variableInstances = runtimeService.GetVariableInstancesByExecutionIds(new string[] { processInstanceId });

            IList<ProcessInstanceVariable> resourcesList = new List<ProcessInstanceVariable>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId));
            }

            Resources<ProcessInstanceVariable> resources = new Resources<ProcessInstanceVariable>(resourcesList);

            return Task.FromResult(resources);
        }

        /// <inheritdoc />
        [HttpGet("local")]
        public virtual Task<Resources<ProcessInstanceVariable>> GetVariablesLocal(string processInstanceId)
        {
            IDictionary<string, IVariableInstance> variableInstancesMap = runtimeService.GetVariableInstancesLocal(processInstanceId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<ProcessInstanceVariable> resourcesList = new List<ProcessInstanceVariable>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId));
            }

            Resources<ProcessInstanceVariable> resources = new Resources<ProcessInstanceVariable>(resourcesList);

            return Task.FromResult(resources);
        }

        /// <inheritdoc />
        [HttpPost]
        public virtual Task<ActionResult> SetVariables(string processInstanceId, SetProcessVariablesCmd setProcessVariablesCmd)
        {
            processEngine.SetProcessVariables(setProcessVariablesCmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpPost("remove")]
        public virtual Task<ActionResult> RemoveVariables(string processInstanceId, RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            this.processEngine.RemoveProcessVariables(removeProcessVariablesCmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpGet("{variableName}")]
        public Task<ProcessInstanceVariable> GetVariable(string processInstanceId, string variableName)
        {
            IVariableInstance variableInstance = runtimeService.GetVariableInstance(processInstanceId, variableName);

            ProcessInstanceVariable variable = new ProcessInstanceVariable(variableInstance.ProcessInstanceId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId);

            return Task.FromResult(variable);
        }
    }

}