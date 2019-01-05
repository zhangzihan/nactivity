using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable.sort;
using org.activiti.engine;
using org.activiti.engine.task;
using org.springframework.data.domain;
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
    public class PageableTaskService
    {

        private readonly ITaskService taskService;
        private readonly TaskConverter taskConverter;
        private readonly PageRetriever pageRetriever;
        private readonly TaskSortApplier sortApplier;
        private IUserGroupLookupProxy userGroupLookupProxy;
        private AuthenticationWrapper authenticationWrapper = new AuthenticationWrapper();

        public PageableTaskService(ITaskService taskService, TaskConverter taskConverter, PageRetriever pageRetriever, TaskSortApplier sortApplier)
        {
            this.taskService = taskService;
            this.taskConverter = taskConverter;
            this.pageRetriever = pageRetriever;
            this.sortApplier = sortApplier;
        }

        public virtual Page<Task> getTasks(Pageable pageable)
        {

            string userId = authenticationWrapper.AuthenticatedUserId;
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

            return pageRetriever.loadPage<ITask, Task, ITaskQuery>(query, pageable, taskConverter);
        }

        public virtual Page<Task> getAllTasks(Pageable pageable)
        {
            ITaskQuery query = taskService.createTaskQuery();
            sortApplier.applySort(query, pageable);

            return pageRetriever.loadPage(query, pageable, taskConverter);
        }

        public virtual Page<Task> getTasks(string processInstanceId, Pageable pageable)
        {
            ITaskQuery query = taskService.createTaskQuery().processInstanceId(processInstanceId);
            sortApplier.applySort(query, pageable);
            return pageRetriever.loadPage(query, pageable, taskConverter);
        }

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