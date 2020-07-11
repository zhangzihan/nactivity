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

using Microsoft.Extensions.Logging;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core.Pageables.Sorts;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
using Sys.Workflow;
using System;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cmd;

namespace Sys.Workflow.Cloud.Services.Core.Pageables
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableProcessHistoryRepositoryService
    {
        private readonly ILogger logger = null;

        private readonly PageRetriever pageRetriever;

        private readonly IHistoryService historyService;

        private readonly HistoryInstanceSortApplier sortApplier;

        private readonly HistoricInstanceConverter processInstanceConverter;

        private readonly SecurityPoliciesApplicationService securityService;
        private readonly HistoryTaskSortApplier historicSortApplier;
        private readonly HistoricTaskInstanceConverter historicTaskInstanceConverter;


        /// <summary>
        /// 
        /// </summary>
        public PageableProcessHistoryRepositoryService(PageRetriever pageRetriever,
            IHistoryService historyService,
            HistoryInstanceSortApplier sortApplier,
            HistoricInstanceConverter processInstanceConverter,
            SecurityPoliciesApplicationService securityPolicyApplicationService,
            ILoggerFactory loggerFactory,
            HistoryTaskSortApplier historicSortApplier,
            HistoricTaskInstanceConverter historicTaskInstanceConverter)
        {
            this.pageRetriever = pageRetriever;
            this.historyService = historyService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            this.securityService = securityPolicyApplicationService;
            this.historicSortApplier = historicSortApplier;
            this.historicTaskInstanceConverter = historicTaskInstanceConverter;
            logger = loggerFactory.CreateLogger<PageableProcessHistoryRepositoryService>();
        }

        /// <summary>
        /// 
        /// </summary>
        public HistoricInstanceConverter ProcessDefinitionConverter => processInstanceConverter;

        /// <summary>
        /// 
        /// </summary>
        public PageRetriever PageRetriever => pageRetriever;

        /// <summary>
        /// 
        /// </summary>
        public HistoryInstanceSortApplier SortApplier => sortApplier;

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<HistoricInstance> GetProcessInstances(Pageable pageable)
        {
            IHistoricProcessInstanceQuery query = historyService.CreateHistoricProcessInstanceQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

            sortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(query, pageable, processInstanceConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<HistoricInstance> GetAllProcessInstances(Pageable pageable)
        {
            IHistoricProcessInstanceQuery query = historyService.CreateHistoricProcessInstanceQuery();

            sortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(query, pageable, processInstanceConverter);
        }

        public virtual IPage<TaskModel> GetHistoryTasks(string processInstanceId, string businessKey, bool? finished = null)
        {
            IHistoricTaskInstanceQuery query = historyService.CreateHistoricTaskInstanceQuery();

            query.SetProcessInstanceId(processInstanceId)
                .SetProcessInstanceBusinessKey(businessKey);

            if (finished.GetValueOrDefault(false))
            {
                query.SetFinished();
            }

            var pageable = new Pageable
            {
                PageNo = 1,
                PageSize = int.MaxValue
            };
            pageable.Sort = new Sort();
            pageable.Sort.Add(new Sort.Order()
            {
                Property = "startTime",
                Direction = Sort.Direction.ASC
            });
            historicSortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(historyService as ServiceImpl, query, pageable, historicTaskInstanceConverter, (q, firstResult, pageSize) =>
            {
                return new GetHistoricInstanceTasksCmd(q, firstResult, pageSize);
            });
        }
    }
}