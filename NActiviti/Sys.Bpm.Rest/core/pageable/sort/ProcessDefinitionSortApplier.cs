using org.activiti.engine.impl;
using org.activiti.engine.query;
using org.activiti.engine.repository;
using org.springframework.data.domain;
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

namespace org.activiti.cloud.services.core.pageable.sort
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class ProcessDefinitionSortApplier extends BaseSortApplier<org.activiti.engine.repository.ProcessDefinitionQuery>
    public class ProcessDefinitionSortApplier : BaseSortApplier<IProcessDefinitionQuery, IProcessDefinition>
    {

        private IDictionary<string, IQueryProperty> orderByProperties = new Dictionary<string, IQueryProperty>();

        public ProcessDefinitionSortApplier()
        {
            orderByProperties["id"] = ProcessDefinitionQueryProperty.PROCESS_DEFINITION_ID;
            orderByProperties["deploymentId"] = ProcessDefinitionQueryProperty.DEPLOYMENT_ID;
            orderByProperties["name"] = ProcessDefinitionQueryProperty.PROCESS_DEFINITION_NAME;
        }

        protected internal override void applyDefaultSort(IProcessDefinitionQuery query)
        {
            query.orderByProcessDefinitionId().asc();
        }

        protected internal override IQueryProperty getOrderByProperty(Sort.Order order)
        {
            return orderByProperties[order.Property];
        }
    }
}