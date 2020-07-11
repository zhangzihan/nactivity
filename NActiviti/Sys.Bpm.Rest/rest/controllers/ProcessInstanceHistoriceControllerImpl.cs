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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Hateoas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_HIS_INST_ROUTER_V1)]
    [ApiController]
    public class ProcessInstanceHistoriceControllerImpl : WorkflowController, IProcessInstanceHistoriceController
    {
        private readonly ProcessEngineWrapper processEngine;
        private readonly IRepositoryService repositoryService;
        private readonly IRuntimeService runtimeService;
        private readonly IHistoryService historyService;
        private readonly SecurityPoliciesApplicationService securityService;
        private readonly TaskResourceAssembler taskResourceAssembler;
        private readonly PageableProcessHistoryRepositoryService pageableProcessHistoryService;

        /// <inheritdoc />
        public ProcessInstanceHistoriceControllerImpl(ProcessEngineWrapper processEngine,
            PageableProcessHistoryRepositoryService pageableProcessHistoryService,
            IProcessEngine engine,
            SecurityPoliciesApplicationService securityPoliciesApplicationService,
            TaskResourceAssembler taskResourceAssembler)
        {
            this.processEngine = processEngine;
            this.repositoryService = engine.RepositoryService;
            this.runtimeService = engine.RuntimeService;
            this.historyService = engine.HistoryService;
            this.securityService = securityPoliciesApplicationService;
            this.taskResourceAssembler = taskResourceAssembler;
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

        [HttpGet("{processInstanceId}/tasks/{businessKey}/{finished?}")]
        public virtual Task<Resources<TaskModel>> GetTasks(string processInstanceId, string businessKey, bool? finished)
        {
            IPage<TaskModel> historics = null;

            historics = pageableProcessHistoryService.GetHistoryTasks(processInstanceId, businessKey, finished);

            var res = taskResourceAssembler.ToResources(historics.GetContent());

            long total = historics.GetTotalItems();

            Resources<TaskModel> tasks = new Resources<TaskModel>(res.Select(x => x.Content), total, 1, total);

            return Task.FromResult(tasks);
        }
    }
}