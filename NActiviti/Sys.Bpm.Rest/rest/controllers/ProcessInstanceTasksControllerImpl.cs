/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Hateoas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_INS_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceTasksControllerImpl : WorkflowController, IProcessInstanceTasksController
    {
        private readonly PageableTaskRepositoryService pageableTaskService;

        private readonly TaskResourceAssembler taskResourceAssembler;

        /// <inheritdoc />
        public ProcessInstanceTasksControllerImpl(PageableTaskRepositoryService pageableTaskService, TaskResourceAssembler taskResourceAssembler)
        {
            this.pageableTaskService = pageableTaskService;
            this.taskResourceAssembler = taskResourceAssembler;
        }

        /// <inheritdoc />
        [HttpPost("tasks")]
        public virtual Task<Resources<TaskModel>> GetTasks([FromBody]ProcessInstanceTaskQuery query)
        {
            if (query is null)
            {
                throw new ArgumentNullException("query");
            }

            IPage<TaskModel> page = pageableTaskService.GetTasks(query.ProcessInstanceId, query.BusinessKey, query.TenantId, query.Pageable);

            List<TaskResource> res = taskResourceAssembler.ToResources(page.GetContent()).ToList();

            IPage<TaskModel> historics = null;

            if (query.IncludeCompleted)
            {
                historics = pageableTaskService.GetHistoryTasks(query.ProcessInstanceId, query.BusinessKey, query.TenantId, query.Pageable, true);

                res.AddRange(taskResourceAssembler.ToResources(historics.GetContent()));
            }

            Resources<TaskModel> tasks = new Resources<TaskModel>(res.Select(x => x.Content), page.GetTotalItems() + (query.IncludeCompleted ? historics.GetTotalItems() : 0), query.Pageable.PageNo, query.Pageable.PageSize);

            return Task.FromResult<Resources<TaskModel>>(tasks);
        }
    }

}