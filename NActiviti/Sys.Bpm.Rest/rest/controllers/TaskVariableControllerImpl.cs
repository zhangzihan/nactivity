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
    [Route(WorkflowConstants.TASK_VAR_ROUTER_V1)]
    [ApiController]
    public class TaskVariableControllerImpl : ControllerBase, ITaskVariableController
    {

        private readonly ProcessEngineWrapper processEngine;

        private readonly ITaskService taskService;

        private readonly TaskVariableResourceAssembler variableResourceBuilder;


        /// <inheritdoc />
        public TaskVariableControllerImpl(ProcessEngineWrapper processEngine,
            IProcessEngine engine,
            TaskVariableResourceAssembler variableResourceBuilder)
        {
            this.processEngine = processEngine;
            this.taskService = engine.TaskService;
            this.variableResourceBuilder = variableResourceBuilder;
        }


        /// <inheritdoc />
        [HttpGet]
        public virtual Task<Resources<TaskVariableResource>> GetVariables([FromQuery]string taskId)
        {
            _ = taskService.GetVariables(taskId);
            IDictionary<string, IVariableInstance> variableInstancesMap = taskService.GetVariableInstances(taskId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<TaskVariableResource> resourcesList = new List<TaskVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.ToResource(new TaskVariable(variableInstance.TaskId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId, TaskVariable.TaskVariableScope.GLOBAL)));
            }

            Resources<TaskVariableResource> resources = new Resources<TaskVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }


        /// <inheritdoc />
        [HttpGet("local")]
        public virtual Task<Resources<TaskVariableResource>> GetVariablesLocal([FromQuery]string taskId)
        {
            _ = taskService.GetVariablesLocal(taskId);
            IDictionary<string, IVariableInstance> variableInstancesMap = taskService.GetVariableInstancesLocal(taskId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<TaskVariableResource> resourcesList = new List<TaskVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.ToResource(new TaskVariable(variableInstance.TaskId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId, TaskVariable.TaskVariableScope.LOCAL)));
            }

            Resources<TaskVariableResource> resources = new Resources<TaskVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }

        /// <inheritdoc />

        [HttpPost]
        public virtual Task<ActionResult> SetVariables(string taskId, [FromBody]SetTaskVariablesCmd setTaskVariablesCmd)
        {
            processEngine.SetTaskVariables(setTaskVariablesCmd);

            return Task.FromResult<ActionResult>(Ok());
        }


        /// <inheritdoc />
        [HttpPost("local")]
        public virtual Task<ActionResult> SetVariablesLocal(string taskId, [FromBody]SetTaskVariablesCmd setTaskVariablesCmd)
        {
            processEngine.SetTaskVariablesLocal(setTaskVariablesCmd);

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <inheritdoc />
        [HttpGet("{variableName}")]
        public Task<TaskVariableResource> GetVariable(string taskId, string variableName)
        {
            _ = taskService.GetVariable(taskId, variableName);
            IVariableInstance variableInstance = taskService.GetVariableInstance(taskId, variableName);

            TaskVariableResource resource = variableResourceBuilder.ToResource(new TaskVariable(variableInstance.TaskId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId, TaskVariable.TaskVariableScope.GLOBAL));

            return Task.FromResult(resource);
        }
    }

}