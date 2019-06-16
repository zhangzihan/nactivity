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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_ADMIN_DEF_ROUTER_V1)]
    [ApiController]
    public class ProcessDefinitionAdminControllerImpl : ControllerBase, IProcessDefinitionAdminController
    {
        private readonly ProcessDefinitionResourceAssembler resourceAssembler;

        private readonly PageableProcessDefinitionRepositoryService pageableRepositoryService;

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        /// <inheritdoc />

        public ProcessDefinitionAdminControllerImpl(ProcessDefinitionResourceAssembler resourceAssembler, PageableProcessDefinitionRepositoryService pageableRepositoryService)
        {
            this.resourceAssembler = resourceAssembler;
            this.pageableRepositoryService = pageableRepositoryService;
        }

        /// <inheritdoc />

        [HttpGet]
        public virtual Resources<ProcessDefinition> GetAllProcessDefinitions(
            [FromQuery][BindingBehavior(BindingBehavior.Optional)]Pageable pageable)
        {
            IPage<ProcessDefinition> page = pageableRepositoryService.GetAllProcessDefinitions(pageable);
            //return pagedResourcesAssembler.toResource(pageable, page, resourceAssembler);
            //return new PagedResources<PagedResources<ProcessDefinitionResource>>();

            return null;
        }

    }

}