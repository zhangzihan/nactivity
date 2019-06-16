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


        /// <summary>
        /// 
        /// </summary>
        public PageableProcessHistoryRepositoryService(PageRetriever pageRetriever,
            IHistoryService historyService,
            HistoryInstanceSortApplier sortApplier,
            HistoricInstanceConverter processInstanceConverter,
            SecurityPoliciesApplicationService securityPolicyApplicationService,
            ILoggerFactory loggerFactory)
        {
            this.pageRetriever = pageRetriever;
            this.historyService = historyService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            this.securityService = securityPolicyApplicationService;
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
    }
}