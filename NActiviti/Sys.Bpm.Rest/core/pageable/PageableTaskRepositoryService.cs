using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.engine;
using org.activiti.engine.history;
using org.activiti.engine.task;
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

namespace org.activiti.cloud.services.core.pageable
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
        public virtual IPage<TaskModel> getTasks(Pageable pageable)
        {
            string userId = authenticationWrapper.AuthenticatedUser.Id;
            ITaskQuery query = taskService.createTaskQuery();
            if (!string.ReferenceEquals(userId, null))
            {
                IList<string> groups = null;
                if (userGroupLookupProxy != null)
                {
                    groups = userGroupLookupProxy.getGroupsForCandidateUser(userId);
                }
                query = query.taskCandidateOrAssigned(userId, groups);
            }
            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage<ITask, TaskModel, ITaskQuery>(query, pageable, taskConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> getAllTasks(Pageable pageable)
        {
            ITaskQuery query = taskService.createTaskQuery();
            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, taskConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> getTasks(string processInstanceId, Pageable pageable)
        {
            ITaskQuery query = taskService.createTaskQuery().processInstanceId(processInstanceId);
            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, taskConverter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> getHistoryTasks(string processInstanceId, Pageable pageable)
        {
            IHistoricTaskInstanceQuery query = historyService.createHistoricTaskInstanceQuery().processInstanceId(processInstanceId);
            historicSortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, historicTaskInstanceConverter);
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