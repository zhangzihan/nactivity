using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Query;
using Sys.Workflow.Engine.Tasks;
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

namespace Sys.Workflow.Cloud.Services.Core.Pageables.Sorts
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