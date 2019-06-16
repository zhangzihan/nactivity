using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.api.utils;
using Sys.Workflow.cloud.services.core.pageable;
using Sys.Workflow.engine;
using Sys.Workflow.engine.impl;
using Sys.Workflow.engine.runtime;
using Sys.Workflow.engine.task;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.cloud.services.api.commands
{
    /// <summary>
    /// 任务查询命令
    /// </summary>
    public class QueryTaskCmd : ICommand
    {
        /// <inheritdoc />
        public string Id => "queryTaskCmd";

        /// <summary>
        /// 读取分页记录
        /// </summary>
        /// <param name="taskService">任务仓储服务</param>
        /// <param name="pageableRepositoryService">分页仓储服务</param>
        /// <param name="qo">查询对象</param>
        /// <returns></returns>
        public IPage<TaskModel> LoadPage(ITaskService taskService,
            PageableTaskRepositoryService pageableRepositoryService,
            TaskQuery qo)
        {
            TaskQueryImpl query = taskService.CreateTaskQuery() as TaskQueryImpl;

            FastCopy.Copy<TaskQuery, TaskQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.ApplySort(query, qo.Pageable);

            IPage<TaskModel> defs = pageableRepositoryService.PageRetriever.LoadPage(query, qo.Pageable, pageableRepositoryService.TaskConverter);

            return defs;
        }
    }
}
