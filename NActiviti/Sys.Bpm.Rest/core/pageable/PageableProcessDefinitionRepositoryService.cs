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
using Sys.Workflow.Engine.Repository;
using Sys.Workflow;

namespace Sys.Workflow.Cloud.Services.Core.Pageables
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableProcessDefinitionRepositoryService
    {
        private readonly ILogger logger = null;

        private readonly IRepositoryService repositoryService;

        private readonly PageRetriever pageRetriever;

        private readonly ProcessDefinitionConverter processDefinitionConverter;

        private readonly ProcessDefinitionSortApplier sortApplier;

        private readonly SecurityPoliciesApplicationService securityService;

        /// <summary>
        /// 
        /// </summary>
        public PageableProcessDefinitionRepositoryService(IRepositoryService repositoryService,
            PageRetriever pageRetriever,
            ProcessDefinitionConverter processDefinitionConverter,
            ProcessDefinitionSortApplier sortApplier,
            SecurityPoliciesApplicationService securityPolicyApplicationService,
            ILoggerFactory loggerFactory)
        {
            this.repositoryService = repositoryService;
            this.pageRetriever = pageRetriever;
            this.processDefinitionConverter = processDefinitionConverter;
            this.sortApplier = sortApplier;
            this.securityService = securityPolicyApplicationService;
            logger = loggerFactory.CreateLogger<PageableProcessDefinitionRepositoryService>();
        }

        /// <summary>
        /// 
        /// </summary>
        public SecurityPoliciesApplicationService SecurityService => securityService;

        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionConverter ProcessDefinitionConverter => processDefinitionConverter;

        /// <summary>
        /// 
        /// </summary>
        public PageRetriever PageRetriever => pageRetriever;

        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionSortApplier SortApplier => sortApplier;

        /// <summary>
        /// 
        /// </summary>

        public virtual IPage<ProcessDefinition> GetLatestProcessDefinitions(Pageable pageable)
        {
            IProcessDefinitionQuery query = repositoryService.CreateProcessDefinitionQuery()
                .SetLatestVersion();

            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);

            sortApplier.ApplySort(query, pageable);
            return pageRetriever.LoadPage(query, pageable, processDefinitionConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessDefinition> GetProcessDefinitions(Pageable pageable)
        {
            IProcessDefinitionQuery query = repositoryService.CreateProcessDefinitionQuery();
            logger.LogWarning("Securite read not implementation");
            //query = securityService.restrictProcessDefQuery(query, SecurityPolicy.READ);

            sortApplier.ApplySort(query, pageable);
            return pageRetriever.LoadPage(query, pageable, processDefinitionConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessDefinition> GetAllProcessDefinitions(Pageable pageable)
        {
            IProcessDefinitionQuery query = repositoryService.CreateProcessDefinitionQuery();

            sortApplier.ApplySort(query, pageable);
            return pageRetriever.LoadPage(query, pageable, processDefinitionConverter);
        }
    }
}