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

using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace org.activiti.cloud.services.rest.controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_INS_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceTasksControllerImpl : ControllerBase, IProcessInstanceTasksController
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
        [HttpPost("{processInstanceId}/tasks")]
        public virtual Task<Resources<TaskModel>> getTasks(string processInstanceId, [FromBody]ProcessInstanceTaskQuery query)
        {
            IPage<TaskModel> page = pageableTaskService.getTasks(processInstanceId, query.Pageable);

            List<TaskResource> res = taskResourceAssembler.toResources(page.getContent()).ToList();

            if (query.IncludeCompleted)
            {
                IPage<TaskModel> historics = pageableTaskService.getHistoryTasks(processInstanceId, query.Pageable);

                res.AddRange(taskResourceAssembler.toResources(historics.getContent()));
            }

            Resources<TaskModel> tasks = new Resources<TaskModel>(res.Select(x => x.Content), page.getTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize);

            return Task.FromResult<Resources<TaskModel>>(tasks);
        }
    }

}