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

    /// <inheritdoc />
    [Route(WorkflowConstants.TASK_VAR_ROUTER_V1)]
    [ApiController]
    public class TaskVariableControllerImpl : ControllerBase, ITaskVariableController
    {

        private ProcessEngineWrapper processEngine;

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
        public virtual Task<Resources<TaskVariableResource>> getVariables([FromQuery]string taskId)
        {
            IDictionary<string, object> variables = taskService.getVariables(taskId);
            IDictionary<string, IVariableInstance> variableInstancesMap = taskService.getVariableInstances(taskId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<TaskVariableResource> resourcesList = new List<TaskVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.toResource(new TaskVariable(variableInstance.TaskId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId, TaskVariable.TaskVariableScope.GLOBAL)));
            }

            Resources<TaskVariableResource> resources = new Resources<TaskVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }


        /// <inheritdoc />
        [HttpGet("local")]
        public virtual Task<Resources<TaskVariableResource>> getVariablesLocal([FromQuery]string taskId)
        {
            IDictionary<string, object> variables = taskService.getVariablesLocal(taskId);
            IDictionary<string, IVariableInstance> variableInstancesMap = taskService.getVariableInstancesLocal(taskId);
            IList<IVariableInstance> variableInstances = new List<IVariableInstance>();
            if (variableInstancesMap != null)
            {
                ((List<IVariableInstance>)variableInstances).AddRange(variableInstancesMap.Values);
            }
            IList<TaskVariableResource> resourcesList = new List<TaskVariableResource>();
            foreach (IVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(variableResourceBuilder.toResource(new TaskVariable(variableInstance.TaskId, variableInstance.Name, variableInstance.TypeName, variableInstance.Value, variableInstance.ExecutionId, TaskVariable.TaskVariableScope.LOCAL)));
            }

            Resources<TaskVariableResource> resources = new Resources<TaskVariableResource>(resourcesList);

            return Task.FromResult(resources);
        }

        /// <inheritdoc />

        [HttpPost]
        public virtual Task<IActionResult> setVariables(string taskId, [FromBody]SetTaskVariablesCmd setTaskVariablesCmd)
        {

            processEngine.TaskVariables = setTaskVariablesCmd;

            return Task.FromResult<IActionResult>(Ok());
        }


        /// <inheritdoc />
        [HttpPost("local")]
        public virtual Task<IActionResult> setVariablesLocal(string taskId, [FromBody]SetTaskVariablesCmd setTaskVariablesCmd)
        {
            processEngine.TaskVariablesLocal = setTaskVariablesCmd;

            return Task.FromResult<IActionResult>(Ok());
        }
    }

}