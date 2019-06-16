using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Utils;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Api.Commands
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
