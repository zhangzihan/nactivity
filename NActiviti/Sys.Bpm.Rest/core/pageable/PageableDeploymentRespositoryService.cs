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
using Sys.Workflow.cloud.services.api.model.converter;
using Sys.Workflow.cloud.services.core.pageable.sort;
using Sys.Workflow.engine;
using Sys.Workflow;

namespace Sys.Workflow.cloud.services.core.pageable
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableDeploymentRespositoryService
    {
        private readonly PageRetriever pageRetriever;

        private readonly IRuntimeService runtimeService;

        private readonly DeploymentSortApplier sortApplier;

        private readonly DeploymentConverter deploymentConverter;

        private readonly SecurityPoliciesApplicationService securityService;

        //private readonly SecurityPoliciesApplicationService securityService;


        /// <summary>
        /// 
        /// </summary>
        public PageableDeploymentRespositoryService(PageRetriever pageRetriever,
            IRuntimeService runtimeService,
            DeploymentSortApplier sortApplier,
            DeploymentConverter deploymentConverter,
            SecurityPoliciesApplicationService securityPolicyApplicationService)
        {
            this.pageRetriever = pageRetriever;
            this.runtimeService = runtimeService;
            this.sortApplier = sortApplier;
            this.deploymentConverter = deploymentConverter;
            this.securityService = securityPolicyApplicationService;
        }

        /// <summary>
        /// 
        /// </summary>
        public DeploymentConverter DeploymentConverter => deploymentConverter;

        /// <summary>
        /// 
        /// </summary>
        public PageRetriever PageRetriever => pageRetriever;

        /// <summary>
        /// 
        /// </summary>
        public DeploymentSortApplier SortApplier => sortApplier;

        //public virtual IPage<Deployment> getDeployments(Pageable pageable)
        //{
        //    IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();
        //    logger.LogWarning("Securite read not implementation");
        //    //query = securityService.restrictProcessInstQuery(query, SecurityPolicy.READ);

        //    sortApplier.applySort(query, pageable);

        //    return pageRetriever.loadPage(query, pageable, processInstanceConverter);
        //}
    }
}