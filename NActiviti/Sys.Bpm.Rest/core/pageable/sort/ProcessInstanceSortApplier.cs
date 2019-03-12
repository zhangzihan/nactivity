using org.activiti.api.runtime.shared.query;
using org.activiti.engine.impl;
using org.activiti.engine.query;
using org.activiti.engine.runtime;
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

namespace org.activiti.cloud.services.core.pageable.sort
{
    public class ProcessInstanceSortApplier : BaseSortApplier<IProcessInstanceQuery, IProcessInstance>
    {

        private IDictionary<string, IQueryProperty> orderByProperties = new Dictionary<string, IQueryProperty>(StringComparer.OrdinalIgnoreCase);

        public ProcessInstanceSortApplier()
        {
            orderByProperties["id"] = ProcessInstanceQueryProperty.PROCESS_INSTANCE_ID;
            orderByProperties["processDefinitionId"] = ProcessInstanceQueryProperty.PROCESS_DEFINITION_ID;
            orderByProperties["startDate"] = ProcessInstanceQueryProperty.PROCESS_EXECUTION_STARTDATE;
            orderByProperties["name"] = ProcessInstanceQueryProperty.PROCESS_EXECUTION_NAME;
        }

        protected internal override void applyDefaultSort(IProcessInstanceQuery query)
        {
            query.orderByProcessInstanceId().asc();
        }

        protected internal override IQueryProperty getOrderByProperty(Sort.Order order)
        {
            orderByProperties.TryGetValue(order.Property, out IQueryProperty qp);

            return qp;
        }
    }

}