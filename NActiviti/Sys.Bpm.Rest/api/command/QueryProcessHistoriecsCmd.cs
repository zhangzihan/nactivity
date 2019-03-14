using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.utils;
using org.activiti.cloud.services.core.pageable;
using org.activiti.engine;
using org.activiti.engine.impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    /// 流程实例查询命令
    /// </summary>
    public class QueryProcessHistoriecsCmd : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id => "queryProcessHistoriecsCmd";


        /// <summary>
        /// 读取分页记录
        /// </summary>
        /// <param name="historyService">历史记录仓储服务</param>
        /// <param name="pageableRepositoryService">分页仓储服务</param>
        /// <param name="qo">查询对象</param>
        /// <returns></returns>
        public IPage<HistoricInstance> loadPage(IHistoryService historyService,
            PageableProcessHistoryRepositoryService pageableRepositoryService, HistoricInstanceQuery qo)
        {
            HistoricProcessInstanceQueryImpl query = historyService.createHistoricProcessInstanceQuery() as HistoricProcessInstanceQueryImpl;
            
            FastCopy.Copy<HistoricInstanceQuery, HistoricProcessInstanceQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.applySort(query, qo.Pageable);

            IPage<HistoricInstance> defs = pageableRepositoryService.PageRetriever.loadPage(query, qo.Pageable, pageableRepositoryService.ProcessDefinitionConverter);

            return defs;
        }
    }
}
