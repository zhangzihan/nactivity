using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Bpm.Rest.Client;
using Sys.Bpmn.Test;
using System;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Client.Tests.rest.client
{
    public class ProcessDefinitionDeployerClientTest
    {
        private readonly UnitTestContext ctx = UnitTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionDeployerClientTest()
        {

        }

        [Fact]
        public void AllDeployments_获取所有部署流程()
        {
            Exception ex = Record.Exception(() =>
            {
                IProcessDefinitionDeployerController client = ctx.CeateWorkflowHttpProxy().GetDefinitionDeployerClient();

                DeploymentQuery query = new DeploymentQuery();
                query.TenantId = ctx.TenantId;
                query.Pageable = new Pageable
                {
                    PageNo = 1,
                    PageSize = 100,
                    Sort = new Sort(new Sort.Order[]
                    {
                        new Sort.Order
                        {
                            Property = "name",
                            Direction = Sort.Direction.ASC
                        }
                    })
                };

                Resources<Deployment> list = client.AllDeployments(query).Result;

                Assert.True(list.List.Count() <= 100);
                Assert.DoesNotContain(list.List, x => x.TenantId != ctx.TenantId);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void AllDeployments_分页获取所有部署流程()
        {
            Exception ex = Record.Exception(() =>
            {
                IProcessDefinitionDeployerController client = ctx.CeateWorkflowHttpProxy().GetDefinitionDeployerClient();

                int offset = 1;
                Resources<Deployment> list = null;
                while (true)
                {
                    DeploymentQuery query = new DeploymentQuery();
                    query.TenantId = ctx.TenantId;
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

                    list = client.AllDeployments(query).Result;
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
    }
}
