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
    public class QueryProcessInstanceCmd : ICommand
    {
        public string Id => "queryProcessDefinitionCmd";

        public IPage<ProcessInstance> loadPage(IRuntimeService runtimeService,
            PageableProcessInstanceRepositoryService pageableRepositoryService, ProcessInstanceQuery qo)
        {
            ProcessInstanceQueryImpl query = runtimeService.createProcessInstanceQuery() as ProcessInstanceQueryImpl;
            
            FastCopy.Copy<ProcessInstanceQuery, ProcessInstanceQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.applySort(query, qo.Pageable);

            IPage<ProcessInstance> defs = pageableRepositoryService.PageRetriever.loadPage(query, qo.Pageable, pageableRepositoryService.ProcessDefinitionConverter);

            return defs;
        }
    }
}
