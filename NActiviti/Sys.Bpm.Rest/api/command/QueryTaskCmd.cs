using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.utils;
using org.activiti.cloud.services.core.pageable;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.commands
{
    public class QueryTaskCmd : ICommand
    {
        public string Id => "queryTaskCmd";

        public IPage<TaskModel> loadPage(ITaskService taskService,
            PageableTaskRepositoryService pageableRepositoryService,
            TaskQuery qo)
        {
            TaskQueryImpl query = taskService.createTaskQuery() as TaskQueryImpl;

            FastCopy.Copy<TaskQuery, TaskQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.applySort(query, qo.Pageable);

            IPage<TaskModel> defs = pageableRepositoryService.PageRetriever.loadPage(query, qo.Pageable, pageableRepositoryService.TaskConverter);

            return defs;
        }
    }
}
