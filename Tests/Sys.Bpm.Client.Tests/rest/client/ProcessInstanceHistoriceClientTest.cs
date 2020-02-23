using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Rest.Client;
using Sys.Workflow.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Sys.Workflow.Client.Tests.Rest.Client
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

                    list = client.ProcessInstances(query).GetAwaiter().GetResult();
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

                Resources<HistoricInstance> list = client.ProcessInstances(query).GetAwaiter().GetResult();
                if (list.TotalCount == 0)
                {
                    return;
                }

                HistoricInstance inst = client.GetProcessInstanceById(list.List.First().Id).GetAwaiter().GetResult();

                Assert.NotNull(inst);
            });

            Assert.Null(ex);
        }
    }
}
