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
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.engine;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_ADMIN_INST_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceAdminControllerImpl : ControllerBase, IProcessInstanceAdminController
    {
        private readonly ProcessEngineWrapper processEngine;
        private readonly IRepositoryService repositoryService;
        private readonly IRuntimeService runtimeService;
        private readonly IHistoryService historyService;
        private readonly ProcessInstanceResourceAssembler resourceAssembler;
        private readonly SecurityPoliciesApplicationService securityService;
        private readonly PageableProcessHistoryRepositoryService pageableProcessHistoryService;
        private readonly PageableProcessInstanceRepositoryService pageableProcessInstanceService;

        /// <inheritdoc />
        public ProcessInstanceAdminControllerImpl(ProcessEngineWrapper processEngine,
            ProcessInstanceResourceAssembler resourceAssembler,
            PageableProcessInstanceRepositoryService pageableProcessInstanceService,
            PageableProcessHistoryRepositoryService pageableProcessHistoryService,
            IProcessEngine engine,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.processEngine = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.historyService = engine.HistoryService;
            this.resourceAssembler = resourceAssembler;
            this.securityService = securityPoliciesApplicationService;
            this.pageableProcessHistoryService = pageableProcessHistoryService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
        }

        /// <inheritdoc />
        [HttpPost]
        public virtual Task<Resources<ProcessInstance>> GetAllProcessInstances(ProcessInstanceQuery query)
        {
            IPage<ProcessInstance> instances = new QueryProcessInstanceCmd().LoadPage(this.runtimeService, this.pageableProcessInstanceService, query);

            IList<ProcessInstanceResource> resources = resourceAssembler.ToResources(instances.GetContent());

            return Task.FromResult<Resources<ProcessInstance>>(new Resources<ProcessInstance>(resources.Select(x => x.Content), instances.GetTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize));
        }

        /// <inheritdoc />
        [HttpPost("historices")]
        public Task<Resources<HistoricInstance>> GetAllProcessHistoriecs(HistoricInstanceQuery query)
        {
            IPage<HistoricInstance> historices = new QueryProcessHistoriecsCmd().LoadPage(historyService, pageableProcessHistoryService, query);

            return Task.FromResult<Resources<HistoricInstance>>(new Resources<HistoricInstance>(historices.GetContent(), historices.GetTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize));
        }
    }
}