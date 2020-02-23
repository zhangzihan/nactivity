using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Converters;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Sys.Workflow.Client.Tests.Rest.Client
{
    //[Order(2)]
    public class ProcessDefinitionDeployerClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();
        private readonly IProcessDefinitionDeployerController client = null;

        public ProcessDefinitionDeployerClientTest()
        {
            client = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();
        }

        [Fact]
        public void AllDeployments_获取所有部署流程()
        {
            Exception ex = Record.Exception(() =>
            {
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
                if (list.TotalCount > 0)
                {
                    Assert.True(list.List.Count() <= list.TotalCount);
                }
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

        public Deployment Deploy(string bpmnFile)
        {
            string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

            ProcessDefinitionDeployer deployer = new ProcessDefinitionDeployer
            {
                Name = Path.GetFileNameWithoutExtension(bpmnFile),
                BpmnXML = xml,
                TenantId = ctx.TenantId,
                EnableDuplicateFiltering = false
            };

            Deployment deployment = client.Deploy(deployer).GetAwaiter().GetResult();

            return deployment;
        }

        [Theory]
        [InlineData("征文评审.bpmn")]
        public void ToXml(string bpmnFile)
        {
            try
            {
                string root = AppDomain.CurrentDomain.BaseDirectory;

                string docFile = Path.Combine(new string[] { root, "resources", "samples", bpmnFile });

                var bxc = new BpmnXMLConverter();

                BpmnModel model = bxc.ConvertToBpmnModel(docFile);

                byte[] data = bxc.ConvertToXML(model);

                string xml = Encoding.UTF8.GetString(data);

                BpmnModel temp = bxc.ConvertToBpmnModel(new XMLStreamReader(new MemoryStream(data)));
            }
            catch (Exception ex)
            {

            }
        }


        [Theory]
        [InlineData("征文评审.bpmn")]
        public void Deploy_征文评审(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Deployment deployment = Deploy(bpmnFile);

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("WebApi.bpmn")]
        public void Deploy_协同审批(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Deployment deployment = Deploy(bpmnFile);

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("用户任务节点部署.bpmn")]
        public void Deploy_用户任务节点_返回并行任务(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Deployment deployment = Deploy(bpmnFile);

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }



        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Deploy_测试部署(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Deployment deployment = Deploy(bpmnFile);

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Save_保存为草稿(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Deployment deployment = Save(bpmnFile).GetAwaiter().GetResult();

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }

        /// <summary>
        /// 保存为草稿
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <returns></returns>
        private async Task<Deployment> Save(string bpmnFile)
        {
            return await Save(bpmnFile, Path.GetFileNameWithoutExtension(bpmnFile)).ConfigureAwait(false);
        }

        /// <summary>
        /// 保存为草稿
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <param name="bpmnName"></param>
        /// <returns></returns>
        private async Task<Deployment> Save(string bpmnFile, string bpmnName)
        {
            string xml = ctx.ReadBpmn(bpmnFile, bpmnName);

            ProcessDefinitionDeployer deployer = new ProcessDefinitionDeployer
            {
                Name = bpmnName,
                BpmnXML = xml,
                TenantId = ctx.TenantId
            };

            Deployment deployment = await client.Save(deployer).ConfigureAwait(false);
            return deployment;
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Remove_删除部署的流程_先部署一个流程_然后应能正常删除(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment deployment = Save(bpmnFile, bpmnName).GetAwaiter().GetResult();

                client.Remove(deployment.Id).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Remove_删除部署的流程_如果当前流程已存在实例则抛出异常_否则删除(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment deployment = ctx.DeployAsync(bpmnFile, bpmnName).GetAwaiter().GetResult();

                IProcessInstanceController instanceController = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();

                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd
                {
                    ProcessName = bpmnName,
                    TenantId = ctx.TenantId,
                    Variables = new Dictionary<string, object>
                    {
                        { "name", new string[]{ "用户1" } }
                    },
                };
                ProcessInstance[] instances = instanceController.Start(new StartProcessInstanceCmd[] { cmd }).GetAwaiter().GetResult();

                Assert.True(instances.Length > 0);

                IProcessDefinitionDeployerController deployerController = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

                deployerController.Remove(deployment.Id).GetAwaiter().GetResult();
            });

            Assert.NotNull(ex);
        }

        [Fact]
        public void Latest_返回最终部署的流程列表()
        {
            var ex = Record.Exception(() =>
            {
                var list = client.Latest(new DeploymentQuery
                {
                    TenantId = ctx.TenantId
                }).GetAwaiter().GetResult();

                Assert.NotNull(list);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetProcessModel_返回流程定义BPMN_XML结构字符串(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment deployment = ctx.Deploy(bpmnFile, bpmnName);

                string xml = client.GetProcessModel(deployment.Id).GetAwaiter().GetResult();

                Assert.NotNull(xml);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetBpmnModel_返回流程定义对象模型(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment deployment = ctx.Deploy(bpmnFile, bpmnName);

                ActionResult<BpmnModel> model = client.GetBpmnModel(deployment.Id).GetAwaiter().GetResult();

                Assert.NotNull(model.Result);
                Assert.IsType<ObjectResult>(model.Result);
                Assert.IsType<BpmnModel>(((ObjectResult)model.Result).Value);

                client.Remove(deployment.Id).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Draft_读取某租户下流程名的草稿定义(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment deployment = Save(bpmnFile, bpmnName).GetAwaiter().GetResult();

                Deployment draft = client.Draft(ctx.TenantId, bpmnName).GetAwaiter().GetResult();

                Assert.NotNull(draft);
                Assert.True(deployment.Equals(draft));

                client.Remove(deployment.Id).GetAwaiter().GetResult();
            });

            Assert.Null(ex);
        }
    }
}
