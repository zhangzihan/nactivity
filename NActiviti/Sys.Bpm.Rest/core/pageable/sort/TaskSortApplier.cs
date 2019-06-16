using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.engine.impl;
using Sys.Workflow.engine.query;
using Sys.Workflow.engine.task;
using System;
using System.Collections.Generic;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.cloud.services.core.pageable.sort
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskSortApplier : BaseSortApplier<ITaskQuery, ITask>
    {
        private IDictionary<string, IQueryProperty> orderByProperties = new Dictionary<string, IQueryProperty>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        public TaskSortApplier()
        {
            orderByProperties["id"] = TaskQueryProperty.TASK_ID;
            orderByProperties["name"] = TaskQueryProperty.NAME;
            orderByProperties["assignee"] = TaskQueryProperty.ASSIGNEE;
            orderByProperties["createTime"] = TaskQueryProperty.CREATE_TIME;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void ApplyDefaultSort(ITaskQuery query)
        {
            query.OrderByTaskId().Asc();
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