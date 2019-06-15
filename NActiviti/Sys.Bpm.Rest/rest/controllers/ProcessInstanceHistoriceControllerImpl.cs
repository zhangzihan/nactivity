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
using org.activiti.engine.history;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_HIS_INST_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceHistoriceControllerImpl : ControllerBase, IProcessInstanceHistoriceController
    {
        private readonly ProcessEngineWrapper processEngine;
        private readonly IRepositoryService repositoryService;
        private readonly IRuntimeService runtimeService;
        private readonly IHistoryService historyService;
        private readonly SecurityPoliciesApplicationService securityService;
        private readonly PageableProcessHistoryRepositoryService pageableProcessHistoryService;

        /// <inheritdoc />
        public ProcessInstanceHistoriceControllerImpl(ProcessEngineWrapper processEngine,
            PageableProcessHistoryRepositoryService pageableProcessHistoryService,
            IProcessEngine engine,
            SecurityPoliciesApplicationService securityPoliciesApplicationService)
        {
            this.processEngine = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.historyService = engine.HistoryService;
            this.securityService = securityPoliciesApplicationService;
            this.pageableProcessHistoryService = pageableProcessHistoryService;
        }

        /// <inheritdoc />
        [HttpPost]
        public virtual Task<Resources<HistoricInstance>> ProcessInstances(HistoricInstanceQuery query)
        {
            IPage<HistoricInstance> historices = new QueryProcessHistoriecsCmd()
                .LoadPage(historyService, pageableProcessHistoryService, query);

            return Task.FromResult<Resources<HistoricInstance>>(new Resources<HistoricInstance>(historices.GetContent(), historices.GetTotalItems(), query.Pageable.PageNo, query.Pageable.PageSize));
        }

        /// <inheritdoc />
        [HttpGet("{processInstanceId}")]
        public Task<HistoricInstance> GetProcessInstanceById(string processInstanceId)
        {
            HistoricInstance processInstance = processEngine.GetHistoryProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + processInstanceId + "'");
            }
            return Task.FromResult<HistoricInstance>(processInstance);
        }

        /// <inheritdoc />
        [HttpGet("{processInstanceId}/variables/{taskId?}")]
        public Task<Resources<HistoricVariableInstance>> GetVariables(string processInstanceId, string taskId)
        {
            return GetVariables(new ProcessVariablesQuery
            {
                ProcessInstanceId = processInstanceId,
                TaskId = taskId
            });
        }

        /// <inheritdoc />
        [HttpPost("variables")]
        public Task<Resources<HistoricVariableInstance>> GetVariables(ProcessVariablesQuery query)
        {
            IList<HistoricVariableInstance> resourcesList = processEngine.GetHistoricVariables(query);

            Resources<HistoricVariableInstance> resources = new Resources<HistoricVariableInstance>(resourcesList);

            return Task.FromResult(resources);
        }
    }
}