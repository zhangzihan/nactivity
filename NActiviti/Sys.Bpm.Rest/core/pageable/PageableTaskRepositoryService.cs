using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Api.Utils;
using Sys.Workflow.Cloud.Services.Core.Pageables.Sorts;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Tasks;
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

namespace Sys.Workflow.Cloud.Services.Core.Pageables
{
    /// <summary>
    /// 
    /// </summary>
    public class PageableTaskRepositoryService
    {

        private readonly ITaskService taskService;
        private readonly TaskConverter taskConverter;
        private readonly HistoricTaskInstanceConverter historicTaskInstanceConverter;
        private readonly PageRetriever pageRetriever;
        private readonly TaskSortApplier sortApplier;
        private readonly HistoryTaskSortApplier historicSortApplier;
        private IUserGroupLookupProxy userGroupLookupProxy;
        private AuthenticationWrapper authenticationWrapper = new AuthenticationWrapper();
        private readonly IHistoryService historyService;

        /// <summary>
        /// 
        /// </summary>
        public PageableTaskRepositoryService(ITaskService taskService,
            TaskConverter taskConverter,
            HistoricTaskInstanceConverter historicTaskInstanceConverter,
            IHistoryService historyService,
            PageRetriever pageRetriever,
            TaskSortApplier sortApplier,
            HistoryTaskSortApplier historicSortApplier)
        {
            this.taskService = taskService;
            this.historyService = historyService;
            this.taskConverter = taskConverter;
            this.pageRetriever = pageRetriever;
            this.sortApplier = sortApplier;
            this.historicSortApplier = historicSortApplier;
            this.historicTaskInstanceConverter = historicTaskInstanceConverter;
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskSortApplier SortApplier => SortApplier;

        /// <summary>
        /// 
        /// </summary>
        public PageRetriever PageRetriever => pageRetriever;

        /// <summary>
        /// 
        /// </summary>
        public TaskConverter TaskConverter => taskConverter;

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> GetTasks(TaskQuery query)
        {
            string userId = authenticationWrapper.AuthenticatedUser.Id;
            ITaskQuery taskQuery = taskService.CreateTaskQuery();
            if (userId is object)
            {
                IList<string> groups = null;
                if (userGroupLookupProxy != null)
                {
                    groups = userGroupLookupProxy.GetGroupsForCandidateUser(userId);
                }
                taskQuery = taskQuery.SetTaskCandidateOrAssigned(userId, groups);
            }
            if (query.BusinessKey is object)
            {
                taskQuery.SetProcessInstanceBusinessKey(query.BusinessKey);
            }
            sortApplier.ApplySort(taskQuery, query.Pageable);

            return pageRetriever.LoadPage<ITask, TaskModel, ITaskQuery>(taskService as ServiceImpl, taskQuery, query.Pageable, taskConverter, (q, firstResult, pageSize) =>
            {
                return new GetProcessInstanceTasksCmd(q, firstResult, pageSize);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> GetAllTasks(TaskQuery taskQuery)
        {
            TaskQueryImpl query = taskService.CreateTaskQuery() as TaskQueryImpl;

            FastCopy.Copy<TaskQuery, TaskQueryImpl>(taskQuery, query);

            sortApplier.ApplySort(query, taskQuery.Pageable);

            return pageRetriever.LoadPage(taskService as ServiceImpl, query, taskQuery.Pageable, taskConverter, (q, firstResult, pageSize) =>
            {
                return new GetProcessInstanceTasksCmd(q, firstResult, pageSize);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> GetTasks(string processInstanceId, string businessKey, string tenantId, Pageable pageable)
        {
            ITaskQuery query = taskService.CreateTaskQuery();
            if (string.IsNullOrWhiteSpace(processInstanceId) == false)
            {
                query.SetProcessInstanceId(processInstanceId);
            }
            else
            {
                query.SetProcessInstanceBusinessKey(businessKey)
                .SetTaskTenantId(tenantId);
            }
            if (pageable.PageSize == 0)
            {
                pageable.PageNo = 1;
                pageable.PageSize = int.MaxValue;
            }

            sortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(taskService as ServiceImpl, query, pageable, taskConverter, (q, firstResult, pageSize) =>
            {
                return new GetProcessInstanceTasksCmd(q, firstResult, pageSize);
            });
        }

        /// <summary>
        /// 获取流程实例历史记录，默认不启用分页，返回当期流程实例下的所有历史任务.
        /// </summary>
        public virtual IPage<TaskModel> GetHistoryTasks(string processInstanceId, string businessKey, string tenantId, Pageable pageable, bool? finished = null)
        {
            IHistoricTaskInstanceQuery query = historyService.CreateHistoricTaskInstanceQuery();

            if (string.IsNullOrWhiteSpace(processInstanceId) == false)
            {
                query.SetProcessInstanceId(processInstanceId);
            }
            else
            {
                query.SetProcessInstanceBusinessKey(businessKey)
                .SetTaskTenantId(tenantId);
            }

            if (finished.HasValue)
            {
                query.SetFinished();
            }

            if (pageable.PageSize == 0)
            {
                pageable.PageNo = 1;
                pageable.PageSize = int.MaxValue;
            }
            historicSortApplier.ApplySort(query, pageable);

            return pageRetriever.LoadPage(historyService as ServiceImpl, query, pageable, historicTaskInstanceConverter, (q, firstResult, pageSize) =>
            {
                return new GetHistoricInstanceTasksCmd(q, firstResult, pageSize);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IUserGroupLookupProxy UserGroupLookupProxy
        {
            get
            {
                return userGroupLookupProxy;
            }
            set
            {
                this.userGroupLookupProxy = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual AuthenticationWrapper AuthenticationWrapper
        {
            get
            {
                return authenticationWrapper;
            }
            set
            {
                this.authenticationWrapper = value;
            }
        }

    }

}