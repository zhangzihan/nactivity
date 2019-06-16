using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Query;
using Sys.Workflow.Engine.Repository;
using System;
using System.Collections.Generic;

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

namespace Sys.Workflow.Cloud.Services.Core.Pageables.Sorts
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessDefinitionSortApplier : BaseSortApplier<IProcessDefinitionQuery, IProcessDefinition>
    {

        private readonly IDictionary<string, IQueryProperty> orderByProperties = new Dictionary<string, IQueryProperty>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionSortApplier()
        {
            orderByProperties["id"] = ProcessDefinitionQueryProperty.PROCESS_DEFINITION_ID;
            orderByProperties["deploymentId"] = ProcessDefinitionQueryProperty.DEPLOYMENT_ID;
            orderByProperties["name"] = ProcessDefinitionQueryProperty.PROCESS_DEFINITION_NAME;
            orderByProperties["version"] = ProcessDefinitionQueryProperty.PROCESS_DEFINITION_VERSION;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void ApplyDefaultSort(IProcessDefinitionQuery query)
        {
            query.OrderByProcessDefinitionId().Asc();
        }

        /// <summary>
        /// 
        /// </summary>

        protected internal override IQueryProperty GetOrderByProperty(Sort.Order order)
        {
            orderByProperties.TryGetValue(order.Property, out IQueryProperty qp);

            return qp;
        }
    }
}