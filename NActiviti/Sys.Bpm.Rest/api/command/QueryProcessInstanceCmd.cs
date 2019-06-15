using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.utils;
using org.activiti.cloud.services.core.pageable;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.impl.cmd;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    /// 流程实例查询命令
    /// </summary>
    public class QueryProcessInstanceCmd : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id => "queryProcessDefinitionCmd";


        /// <summary>
        /// 读取分页记录
        /// </summary>
        /// <param name="runtimeService">运行时仓储服务</param>
        /// <param name="pageableRepositoryService">分页仓储服务</param>
        /// <param name="qo">查询对象</param>
        /// <returns></returns>
        public IPage<ProcessInstance> LoadPage(IRuntimeService runtimeService,
            PageableProcessInstanceRepositoryService pageableRepositoryService, ProcessInstanceQuery qo)
        {
            ProcessInstanceQueryImpl query = runtimeService.CreateProcessInstanceQuery() as ProcessInstanceQueryImpl;

            FastCopy.Copy<ProcessInstanceQuery, ProcessInstanceQueryImpl>(qo, query);
            query.OnlyProcessInstances = true;

            pageableRepositoryService.SortApplier.ApplySort(query, qo.Pageable);

            IPage<ProcessInstance> defs = pageableRepositoryService.PageRetriever.LoadPage(runtimeService as ServiceImpl, query, qo.Pageable, pageableRepositoryService.ProcessDefinitionConverter, (q, firstResult, pageSize) =>
            {
                return new GetProcessInstancesCmd(q, firstResult, pageSize);
            });

            return defs;
        }
    }
}
