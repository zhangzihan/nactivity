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
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.engine;
using org.activiti.engine.history;
using org.activiti.engine.runtime;
using Sys;

namespace org.activiti.cloud.services.core.pageable
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableProcessHistoryRepositoryService
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<PageableProcessHistoryRepositoryService>();

        private PageRetriever pageRetriever;

        private IHistoryService historyService;

        private HistoryInstanceSortApplier sortApplier;

        private HistoricInstanceConverter processInstanceConverter;

        private readonly SecurityPoliciesApplicationService securityService;


        /// <summary>
        /// 
        /// </summary>
        public PageableProcessHistoryRepositoryService(PageRetriever pageRetriever,
            IHistoryService historyService,
            HistoryInstanceSortApplier sortApplier,
            HistoricInstanceConverter processInstanceConverter,
            SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.pageRetriever = pageRetriever;
            this.historyService = historyService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            this.securityService = securityPolicyApplicationService;
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
        public virtual IPage<HistoricInstance> getProcessInstances(Pageable pageable)
        {
            IHistoricProcessInstanceQuery query = historyService.createHistoricProcessInstanceQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<HistoricInstance> getAllProcessInstances(Pageable pageable)
        {
            IHistoricProcessInstanceQuery query = historyService.createHistoricProcessInstanceQuery();

            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }
    }
}