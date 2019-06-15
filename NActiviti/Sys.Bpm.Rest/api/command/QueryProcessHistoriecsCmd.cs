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
        public IPage<HistoricInstance> LoadPage(IHistoryService historyService,
            PageableProcessHistoryRepositoryService pageableRepositoryService, HistoricInstanceQuery qo)
        {
            HistoricProcessInstanceQueryImpl query = historyService.CreateHistoricProcessInstanceQuery() as HistoricProcessInstanceQueryImpl;

            FastCopy.Copy<HistoricInstanceQuery, HistoricProcessInstanceQueryImpl>(qo, query);

            if (qo.IsTerminated.HasValue)
            {
                if (qo.IsTerminated.Value)
                {
                    query.SetDeleted();
                }
                else
                {
                    query.SetNotDeleted();
                }
            }

            pageableRepositoryService.SortApplier.ApplySort(query, qo.Pageable);

            IPage<HistoricInstance> defs = pageableRepositoryService.PageRetriever.LoadPage(historyService as ServiceImpl, query, qo.Pageable, pageableRepositoryService.ProcessDefinitionConverter, (q, firstResult, pageSize) =>
            {
                return new engine.impl.cmd.GetHistoricProcessInstancesCmd(q, firstResult, pageSize);
            });

            return defs;
        }
    }
}
