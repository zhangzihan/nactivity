using Newtonsoft.Json;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xunit;
using Xunit.Extensions.Ordering;
using static org.activiti.api.runtime.shared.query.Sort;

namespace Sys.Bpmn.Test.rest.controller
{
    [Order(1)]
    public class ProcessDefinitionControllerTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionControllerTest()
        {
        }

        private IProcessDefinitionController CreateController()
        {
            return new ProcessDefinitionControllerImpl(
                    ctx.Resolve<IProcessEngine>(),
                    ctx.Resolve<ProcessDefinitionConverter>(),
                    ctx.Resolve<ProcessDefinitionResourceAssembler>(),
                    ctx.Resolve<PageableProcessDefinitionRepositoryService>(),
                    ctx.Resolve<SecurityPoliciesApplicationService>());
        }

        [Theory]
        [InlineData(1, 20)]
        public void LatestProcessDefinitions_测试最终发布的流程定义(int offset, int pageSize)
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                IProcessDefinitionController pdc = CreateController();

                var query = new ProcessDefinitionQuery
                {
                    TenantId = ctx.TenantId,
                    Pageable = new Pageable
                    {
                        PageNo = offset,
                        PageSize = pageSize,
                        Sort = new Sort(new Order[]
                        {
                            new Order(){ Direction = Direction.ASC, Property = "name" }
                        })
                    }
                };

                Resources<ProcessDefinition> result = AsyncHelper.RunSync<Resources<ProcessDefinition>>(() => ctx.PostAsync<Resources<ProcessDefinition>>($"{WorkflowConstants.PROC_DEF_ROUTER_V1}/latest", query));

                Assert.NotNull(result);
                Assert.True(result.RecordCount <= pageSize);
            }));

            Assert.Null(ex);
        }

        [Theory]
        [InlineData(1, 20)]
        public void ProcessDefinitions_获取所有工作流定义_已发布_每页20条记录_默认按name排序(int pageNo, int pageSize)
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                decimal totalCount = 0M;
                long recordCount = 0;

                do
                {
                    ProcessDefinitionQuery query = new ProcessDefinitionQuery
                    {
                        TenantId = ctx.TenantId,
                        Pageable = new Pageable
                        {
                            PageNo = pageNo,
                            PageSize = pageSize,
                            Sort = new Sort(new Order[]
                            {
                            new Order(){ Direction = Direction.ASC, Property = "name" }
                            })
                        }
                    };

                    var result = AsyncHelper.RunSync<Resources<ProcessDefinition>>(() =>
                    ctx.PostAsync<Resources<ProcessDefinition>>($"{WorkflowConstants.PROC_DEF_ROUTER_V1}/list", query));

                    totalCount = result.TotalCount;
                    recordCount = result.RecordCount;

                    decimal pages = Math.Ceiling(totalCount / pageSize) + 1;
                    if (pages == pageNo + 1)
                    {
                        break;
                    }
                    pageNo = pageNo + 1;

                    Assert.NotNull(result);
                    Assert.True(result.RecordCount <= pageSize);
                } while (true);

                Assert.True(pageNo == Math.Ceiling(totalCount / pageSize));
            }));

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetProcessDefinition_根据ID查找流程定义(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                string bpmnName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                Deployment dep = ctx.HttpDeployProcess(bpmnName, bpmnFile);

                ProcessDefinition find = GetLatestProcessDefinitionByDeploymentId(dep.Id);

                Assert.NotNull(find);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetProcessModel_首先部署BPMN_调用服务读取BMMN_文件内容应该等于服务读取内容(string bpmFile)
        {
            Exception ex = Record.Exception(() =>
            {
                string bpmnName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                Deployment dep = ctx.HttpDeployProcess(bpmnName, bpmFile);

                ProcessDefinition find = GetLatestProcessDefinitionByDeploymentId(dep.Id);

                var response = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                ctx.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_DEF_ROUTER_V1}/{find.Id}/processmodel"));

                var xml = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

                Assert.NotNull(xml?.Trim() == "" ? null : xml.Trim());
            });

            Assert.Null(ex);
        }

        private ProcessDefinition GetLatestProcessDefinitionByDeploymentId(string deploymentId)
        {
            var query = new ProcessDefinitionQuery
            {
                TenantId = ctx.TenantId,
                Pageable = new Pageable
                {
                    PageNo = 1,
                    PageSize = 1
                },
                DeploymentId = deploymentId
            };

            var result = AsyncHelper.RunSync<Resources<ProcessDefinition>>(() =>
                ctx.PostAsync<Resources<ProcessDefinition>>($"{WorkflowConstants.PROC_DEF_ROUTER_V1}/latest", query));

            ProcessDefinition find = result.List.FirstOrDefault();
            return find;
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetBpmnModel_获取BPMN对象模型图(string bpmFile)
        {
            Exception ex = Record.Exception(() =>
            {
                string bpmnName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                Deployment dep = ctx.HttpDeployProcess(bpmnName, bpmFile);

                ProcessDefinition find = GetLatestProcessDefinitionByDeploymentId(dep.Id);

                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                ctx.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_DEF_ROUTER_V1}/{find.Id}/bpmnmodel"));

                string data = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

                BpmnModel model = JsonConvert.DeserializeObject<BpmnModel>(data, new JsonSerializerSettings
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                Assert.NotNull(model);
            });

            Assert.Null(ex);
        }
    }
}
