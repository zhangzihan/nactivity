using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Utils;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Api.Commands
{
    /// <summary>
    /// 流程定义查询命令
    /// </summary>
    public class QueryProcessDefinitionCmd : ICommand
    {
        /// <inheritdoc />
        public string Id => "queryProcessDefinitionCmd";

        /// <summary>
        /// 读取分页记录
        /// </summary>
        /// <param name="repositoryService">仓储服务</param>
        /// <param name="pageableRepositoryService">分页仓储服务</param>
        /// <param name="qo">查询对象</param>
        /// <returns></returns>
        public IPage<ProcessDefinition> LoadPage(IRepositoryService repositoryService, PageableProcessDefinitionRepositoryService pageableRepositoryService, ProcessDefinitionQuery qo)
        {
            ProcessDefinitionQueryImpl query = repositoryService.CreateProcessDefinitionQuery() as ProcessDefinitionQueryImpl;

            FastCopy.Copy<ProcessDefinitionQuery, ProcessDefinitionQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.ApplySort(query, qo.Pageable);

            IPage<ProcessDefinition> defs = pageableRepositoryService.PageRetriever.LoadPage(query, qo.Pageable, pageableRepositoryService.ProcessDefinitionConverter);

            return defs;
        }
    }
}
