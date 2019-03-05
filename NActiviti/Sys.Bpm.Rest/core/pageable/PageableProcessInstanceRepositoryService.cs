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
    public class PageableProcessInstanceRepositoryService
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<PageableProcessInstanceRepositoryService>();

        private PageRetriever pageRetriever;

        private IRuntimeService runtimeService;

        private ProcessInstanceSortApplier sortApplier;

        private ProcessInstanceConverter processInstanceConverter;

        //private readonly SecurityPoliciesApplicationService securityService;


        public PageableProcessInstanceRepositoryService(PageRetriever pageRetriever, IRuntimeService runtimeService, ProcessInstanceSortApplier sortApplier, ProcessInstanceConverter processInstanceConverter)//, SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.pageRetriever = pageRetriever;
            this.runtimeService = runtimeService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            //this.securityService = securityPolicyApplicationService;
        }

        public ProcessInstanceConverter ProcessDefinitionConverter => processInstanceConverter;

        public PageRetriever PageRetriever => pageRetriever;

        public ProcessInstanceSortApplier SortApplier => sortApplier;

        public virtual IPage<ProcessInstance> getProcessInstances(Pageable pageable)
        {
            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }

        public virtual IPage<ProcessInstance> getAllProcessInstances(Pageable pageable)
        {

            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();

            sortApplier.applySort(query, pageable);
            return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        }
    }
}