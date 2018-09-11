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
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.springframework.data.domain;
using Sys;

namespace org.activiti.cloud.services.core.pageable
{
    public class PageableRepositoryService
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<PageableRepositoryService>();

        private readonly IRepositoryService repositoryService;

        private readonly PageRetriever pageRetriever;

        private readonly ProcessDefinitionConverter processDefinitionConverter;

        private readonly ProcessDefinitionSortApplier sortApplier;

        private readonly SecurityPoliciesApplicationService securityService;

        public PageableRepositoryService(IRepositoryService repositoryService, PageRetriever pageRetriever, ProcessDefinitionConverter processDefinitionConverter, ProcessDefinitionSortApplier sortApplier, SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.repositoryService = repositoryService;
            this.pageRetriever = pageRetriever;
            this.processDefinitionConverter = processDefinitionConverter;
            this.sortApplier = sortApplier;
            this.securityService = securityPolicyApplicationService;
        }

        public virtual Page<ProcessDefinition> getProcessDefinitions(Pageable pageable)
        {

            IProcessDefinitionQuery query = repositoryService.createProcessDefinitionQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);

            sortApplier.applySort(query, pageable);
            return pageRetriever.loadPage(query, pageable, processDefinitionConverter);
        }

        public virtual Page<ProcessDefinition> getAllProcessDefinitions(Pageable pageable)
        {
            IProcessDefinitionQuery query = repositoryService.createProcessDefinitionQuery();

            sortApplier.applySort(query, pageable);
            return pageRetriever.loadPage(query, pageable, processDefinitionConverter);
        }


    }

}