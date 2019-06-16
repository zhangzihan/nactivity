using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using Sys.Workflow.engine.impl.persistence.entity;
using org.springframework.hateoas;
using Sys.Bpm.Exceptions;
using Sys.Bpm.Rest.Client;
using Sys.Bpmn.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Sys.Bpm.Client.Tests.rest.client
{
    //[Order(4)]
    public class ProcessInstanceHistoriceClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        public ProcessInstanceHistoriceClientTest()
        {

        }

        [Fact]
        public void ProcessInstances_分页获取流程历史实例列表()
        {
            var ex = Record.Exception(() =>
            {
                IProcessInstanceHistoriceController client = ctx.CreateWorkflowHttpProxy().GetProcessInstanceHistoriceClient();

                int offset = 1;
                Resources<HistoricInstance> list = null;
                while (true)
                {
                    HistoricInstanceQuery query = new HistoricInstanceQuery();
                    query.TenantId = ctx.TenantId;
                    query.IsTerminated = true;
                    query.Pageable = new Pageable
                    {
                        PageNo = offset,
                        PageSize = 10,
                        Sort = new Sort(new Sort.Order[]
                        {
                            new Sort.Order
                            {
                                Property = "name",
                                Direction = Sort.Direction.ASC
                            }
                        })
                    };

                    list = AsyncHelper.RunSync<Resources<HistoricInstance>>(() => client.ProcessInstances(query));
                    if (list.List.Count() < 10)
                    {
                        break;
                    }

                    offset = offset + 1;
                }

                Assert.True(offset == 1 && list.TotalCount <= 0 ? true : list.TotalCount / 10 + 1 == offset);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void GetProcessInstanceById_主键查找历史实例()
        {
            var ex = Record.Exception(() =>
            {
                IProcessInstanceHistoriceController client = ctx.CreateWorkflowHttpProxy().GetProcessInstanceHistoriceClient();

                HistoricInstanceQuery query = new HistoricInstanceQuery();
                query.TenantId = ctx.TenantId;
                query.Pageable = new Pageable
                {
                    PageNo = 1,
                    PageSize = 1
                };

                Resources<HistoricInstance> list = AsyncHelper.RunSync<Resources<HistoricInstance>>(() => client.ProcessInstances(query));
                if (list.TotalCount == 0)
                {
                    return;
                }

                HistoricInstance inst = AsyncHelper.RunSync<HistoricInstance>(() => client.GetProcessInstanceById(list.List.First().Id));

                Assert.NotNull(inst);
            });

            Assert.Null(ex);
        }
    }
}
