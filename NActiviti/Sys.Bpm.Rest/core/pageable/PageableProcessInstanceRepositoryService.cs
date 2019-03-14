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
using org.activiti.engine.runtime;
using Sys;

namespace org.activiti.cloud.services.core.pageable
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableProcessInstanceRepositoryService
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<PageableProcessInstanceRepositoryService>();

        private PageRetriever pageRetriever;

        private IRuntimeService runtimeService;

        private ProcessInstanceSortApplier sortApplier;

        private ProcessInstanceConverter processInstanceConverter;

        //private readonly SecurityPoliciesApplicationService securityService;


        /// <summary>
        /// 
        /// </summary>
        public PageableProcessInstanceRepositoryService(PageRetriever pageRetriever, IRuntimeService runtimeService, ProcessInstanceSortApplier sortApplier, ProcessInstanceConverter processInstanceConverter)//, SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.pageRetriever = pageRetriever;
            this.runtimeService = runtimeService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            //this.securityService = securityPolicyApplicationService;
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcessInstanceConverter ProcessDefinitionConverter => processInstanceConverter;

        /// <summary>
        /// 
        /// </summary>
        public PageRetriever PageRetriever => pageRetriever;

        /// <summary>
        /// 
        /// </summary>
        public ProcessInstanceSortApplier SortApplier => sortApplier;

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> getProcessInstances(Pageable pageable)
        {
            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> getAllProcessInstances(Pageable pageable)
        {

            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();

            sortApplier.applySort(query, pageable);
            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }
    }
}