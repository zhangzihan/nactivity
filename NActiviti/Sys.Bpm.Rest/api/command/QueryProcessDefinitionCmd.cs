using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.api.utils;
using Sys.Workflow.cloud.services.core.pageable;
using Sys.Workflow.engine;
using Sys.Workflow.engine.impl;
using Sys.Workflow.engine.repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sys.Workflow.cloud.services.api.commands
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
