using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Rest.Client;
using Sys.Workflown.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Sys.Workflow.Client.Tests.Rest.Client
{
    //[Order(1)]
    public class ProcessDefinitionClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionClientTest(ITestOutputHelper testOutputHelper)
        {

        }

        [Fact]
        public async void LatestProcessDefinitions_最终流程定义列表()
        {
            Exception ex = await Record.ExceptionAsync(async () =>
            {
                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                ProcessDefinitionQuery query = new ProcessDefinitionQuery();
                query.TenantId = ctx.TenantId;
                query.Pageable = new Pageable
                {
                    PageNo = 1,
                    PageSize = int.MaxValue,
                    Sort = new Sort(new Sort.Order[]
                    {
                        new Sort.Order
                        {
                            Property = "name",
                            Direction = Sort.Direction.ASC
                        }
                    })
                };

                Resources<ProcessDefinition> list = await client.LatestProcessDefinitions(query).ConfigureAwait(false);

                Assert.True(list.TotalCount == list.List.Count());
                Assert.True(list.TotalCount == list.RecordCount);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Fact]
        public async void ProcessDefinitions_分页获取所有部署流程()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                int offset = 1;
                Resources<ProcessDefinition> list = null;
                while (true)
                {
                    ProcessDefinitionQuery query = new ProcessDefinitionQuery();
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

                    list = await client.LatestProcessDefinitions(query).ConfigureAwait(false);
                    if (list.List.Count() < 10)
                    {
                        break;
                    }

                    offset = offset + 1;
                }

                Assert.True(offset == 1 && list.TotalCount <= 0 ? true : list.TotalCount / 10 + 1 == offset);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public async void GetProcessDefinition_查找流程定义(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment dep = await ctx.DeployAsync(bpmnFile, bpmnName).ConfigureAwait(false);

                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                Resources<ProcessDefinition> defs = await client.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    DeploymentId = dep.Id
                }).ConfigureAwait(false);

                ProcessDefinition def = await client.GetProcessDefinition(defs.List.First().Id).ConfigureAwait(false);

                Assert.NotNull(def);
                Assert.True(def.Id == defs.List.First().Id);

                await ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient().Remove(dep.Id);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public async void GetProcessModel_返回流程定义BPMN_XML结构字符串(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment dep = await ctx.DeployAsync(bpmnFile, bpmnName).ConfigureAwait(false);

                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                Resources<ProcessDefinition> defs = await client.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    DeploymentId = dep.Id
                }).ConfigureAwait(false);

                string xml = await client.GetProcessModel(defs.List.First().Id).ConfigureAwait(false);

                Assert.False(string.IsNullOrWhiteSpace(xml));

                await ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient().Remove(dep.Id).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public async void GetProcessModel_根据名称_租户id_查询流程定义BPMN_XML结构字符串(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment dep = await ctx.DeployAsync(bpmnFile, bpmnName).ConfigureAwait(false);

                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                Resources<ProcessDefinition> defs = await client.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    DeploymentId = dep.Id
                }).ConfigureAwait(false);

                var p = defs.List.First();

                string xml = await client.GetProcessModel(new ProcessDefinitionQuery
                {
                    Name = p.Name,
                    TenantId = p.TenantId 
                }).ConfigureAwait(false);

                Assert.False(string.IsNullOrWhiteSpace(xml));
            }).ConfigureAwait(false);

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public async void GetBpmnModel_返回流程定义对象模型(string bpmnFile)
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                string bpmnName = string.Join("", ctx.Guid2IntString(Guid.NewGuid()));
                Deployment dep = await ctx.DeployAsync(bpmnFile, bpmnName).ConfigureAwait(false);

                IProcessDefinitionController client = ctx.CreateWorkflowHttpProxy().GetProcessDefinitionClient();

                Resources<ProcessDefinition> defs = await client.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    DeploymentId = dep.Id
                }).ConfigureAwait(false);

                ActionResult<BpmnModel> model = await client.GetBpmnModel(defs.List.First().Id).ConfigureAwait(false);

                Assert.NotNull(model.Result);
                Assert.IsType<ObjectResult>(model.Result);
                Assert.IsType<BpmnModel>(((ObjectResult)model.Result).Value);

                await ctx.CreateWorkflowHttpProxy().GetDefinitionDeployerClient().Remove(dep.Id).ConfigureAwait(false);
            });

            Assert.NotNull(ex);
        }

        public Task<string> GetProcessDiagram(string id)
        {
            throw new NotImplementedException();
        }
    }
}
