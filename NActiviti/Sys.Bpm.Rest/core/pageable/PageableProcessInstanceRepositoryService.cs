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
using org.activiti.engine.impl;
using org.activiti.engine.runtime;
using Sys.Workflow;

namespace org.activiti.cloud.services.core.pageable
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableProcessInstanceRepositoryService
    {
        private readonly ILogger logger = null;

        private readonly PageRetriever pageRetriever;

        private readonly IRuntimeService runtimeService;

        private readonly ProcessInstanceSortApplier sortApplier;

        private readonly ProcessInstanceConverter processInstanceConverter;

        //private readonly SecurityPoliciesApplicationService securityService;


        /// <summary>
        /// 
        /// </summary>
        public PageableProcessInstanceRepositoryService(PageRetriever pageRetriever,
            IRuntimeService runtimeService,
            ProcessInstanceSortApplier sortApplier,
            ProcessInstanceConverter processInstanceConverter,
            ILoggerFactory loggerFactory)//, SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.pageRetriever = pageRetriever;
            this.runtimeService = runtimeService;
            this.sortApplier = sortApplier;
            this.processInstanceConverter = processInstanceConverter;
            //this.securityService = securityPolicyApplicationService;
            logger = loggerFactory.CreateLogger<PageableProcessInstanceRepositoryService>();
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
        public virtual IPage<ProcessInstance> GetProcessInstances(Pageable pageable)
        {
            IProcessInstanceQuery query = runtimeService.CreateProcessInstanceQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

            sortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(runtimeService as ServiceImpl, query, pageable, processInstanceConverter, (q, firstResult, pageSize) =>
            {
                return new engine.impl.cmd.GetProcessInstancesCmd(q, firstResult, pageSize);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> GetAllProcessInstances(Pageable pageable)
        {
            IProcessInstanceQuery query = runtimeService.CreateProcessInstanceQuery();

            sortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(runtimeService as ServiceImpl, query, pageable, processInstanceConverter, (q, firstResult, pageSize) =>
            {
                return new engine.impl.cmd.GetProcessInstancesCmd(q, firstResult, pageSize);
            });
        }
    }
}