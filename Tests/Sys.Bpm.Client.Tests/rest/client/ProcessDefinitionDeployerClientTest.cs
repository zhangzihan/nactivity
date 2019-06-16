using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.bpmn.converter;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Sys.Bpm.Client.Tests.rest.client
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

            Deployment deployment = AsyncHelper.RunSync<Deployment>(() => client.Deploy(deployer));

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
                Deployment deployment = AsyncHelper.RunSync<Deployment>(() => Save(bpmnFile));

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
                Deployment deployment = AsyncHelper.RunSync<Deployment>(() => Save(bpmnFile, bpmnName));

                AsyncHelper.RunSync(() => client.Remove(deployment.Id));
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
                Deployment deployment = AsyncHelper.RunSync<Deployment>(() => ctx.DeployAsync(bpmnFile, bpmnName));

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
                ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() => instanceController.Start(new StartProcessInstanceCmd[] { cmd }));

                Assert.True(instances.Length > 0);

                IProcessDefinitionDeployerController deployerController = ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

                AsyncHelper.RunSync(() => deployerController.Remove(deployment.Id));
            });

            Assert.NotNull(ex);
            Assert.IsType<Http400Exception>(ex);
        }

        [Fact]
        public void Latest_返回最终部署的流程列表()
        {
            var ex = Record.Exception(() =>
            {
                var list = AsyncHelper.RunSync<Resources<Deployment>>(() => client.Latest(new DeploymentQuery
                {
                    TenantId = ctx.TenantId
                }));

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

                string xml = AsyncHelper.RunSync<string>(() => client.GetProcessModel(deployment.Id));

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

                ActionResult<BpmnModel> model = AsyncHelper.RunSync<ActionResult<BpmnModel>>(() => client.GetBpmnModel(deployment.Id));

                Assert.NotNull(model.Result);
                Assert.IsType<ObjectResult>(model.Result);
                Assert.IsType<BpmnModel>(((ObjectResult)model.Result).Value);

                AsyncHelper.RunSync(() => client.Remove(deployment.Id));
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
                Deployment deployment = AsyncHelper.RunSync<Deployment>(() => Save(bpmnFile, bpmnName));

                Deployment draft = AsyncHelper.RunSync<Deployment>(() => client.Draft(ctx.TenantId, bpmnName));

                Assert.NotNull(draft);
                Assert.True(deployment.Equals(draft));

                AsyncHelper.RunSync(() => client.Remove(deployment.Id));
            });

            Assert.Null(ex);
        }
    }
}
