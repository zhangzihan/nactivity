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
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.springframework.data.domain;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.controllers
{
    [Route("/workflow/admin/tasks")]
    [ApiController]
    public class TaskAdminControllerImpl : ControllerBase, ITaskAdminController
    {
        private ProcessEngineWrapper processEngine;

        private readonly TaskResourceAssembler taskResourceAssembler;

        public TaskAdminControllerImpl(ProcessEngineWrapper processEngine, TaskResourceAssembler taskResourceAssembler)
        {
            this.processEngine = processEngine;
            this.taskResourceAssembler = taskResourceAssembler;
        }

        [HttpGet]
        public virtual PagedResources<TaskResource> getAllTasks(Pageable pageable)
        {
            Page<Task> page = processEngine.getAllTasks(pageable);

            //return pagedResourcesAssembler.toResource(pageable, page, taskResourceAssembler);

            return null;
        }
    }

}