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

using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.ADMIN_TASK_ROUTER_V1)]
    [ApiController]
    public class TaskAdminControllerImpl : ControllerBase, ITaskAdminController
    {
        private readonly ProcessEngineWrapper processEngine;

        private readonly TaskResourceAssembler taskResourceAssembler;


        /// <inheritdoc />
        public TaskAdminControllerImpl(ProcessEngineWrapper processEngine, TaskResourceAssembler taskResourceAssembler)
        {
            this.processEngine = processEngine;
            this.taskResourceAssembler = taskResourceAssembler;
        }

        [HttpPost("reassign")]
        public Task<TaskModel[]> ReassignTaskUser(ReassignTaskUserCmd cmd)
        {
            TaskModel[] tasks = processEngine.ReassignTaskUsers(cmd);

            return Task.FromResult(tasks);
        }

        /// <inheritdoc />
        [HttpPost]
        public virtual Task<Resources<TaskModel>> GetAllTasks(Pageable pageable)
        {
            IPage<TaskModel> page = processEngine.GetAllTasks(pageable);

            //return pagedResourcesAssembler.toResource(pageable, page, taskResourceAssembler);

            return null;
        }
    }

}