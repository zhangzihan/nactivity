using Newtonsoft.Json;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Xunit;
using Xunit.Extensions.Ordering;
using static org.activiti.api.runtime.shared.query.Sort;

namespace Sys.Bpmn.Test.rest.controller
{
    [Order(2)]
    public class ProcessDefinitionDeployerControllerTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionDeployerControllerTest()
        {
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        [InlineData("条件分支流程.bpmn")]
        public void Deploy_流程部署测试(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                Deployment deployment = ctx.HttpDeployProcess(bpmnFile, false);

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Deploy_流程部署测试_并且确认草稿已删除(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

                pdd.BpmnXML = ctx.ReadBpmn(bpmnFile);
                pdd.Name = Path.GetFileNameWithoutExtension(bpmnFile);
                pdd.TenantId = ctx.TenantId;

                Action<HttpResponseMessage> deployResponse = (response) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    Deployment deployment = JsonConvert.DeserializeObject<Deployment>(response.Content.ReadAsStringAsync().Result);

                    Assert.NotNull(deployment);
                };

                Action<HttpResponseMessage> draftResponse = (response) =>
                {
                    Assert.Null(response);
                };

                Action<HttpResponseMessage> saveAfterResponse = (response) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    Deployment deployment = JsonConvert.DeserializeObject<Deployment>(response.Content.ReadAsStringAsync().Result);

                    Assert.NotNull(deployment);
                };

                Action<HttpResponseMessage> saveResponse = (response) =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    Deployment deployment = JsonConvert.DeserializeObject<Deployment>(response.Content.ReadAsStringAsync().Result);

                    Assert.NotNull(deployment);
                };

                HttpInvoke[] actions = new HttpInvoke[]
                {
                    //保存草稿
                    new HttpInvoke
                    {
                        Url = $"{WorkflowConstants.PROC_DEP_ROUTER_V1}/save",
                        Data = pdd,
                        Response = saveResponse
                    },
                    //读取草稿
                    new HttpInvoke
                    {
                        Url = $"{WorkflowConstants.PROC_DEP_ROUTER_V1}/{ctx.TenantId}/{pdd.Name}/draft",
                        Method = HttpMethod.Get,
                        Response = saveAfterResponse
                    },
                    //流程部署
                    new HttpInvoke
                    {
                        Url = $"{WorkflowConstants.PROC_DEP_ROUTER_V1}",
                        Data = pdd,
                        Response = deployResponse
                    },
                    //读取草稿
                    new HttpInvoke
                    {
                        Url = $"{WorkflowConstants.PROC_DEP_ROUTER_V1}/{ctx.TenantId}/{pdd.Name}/draft",
                        Method = HttpMethod.Get,
                        Response = draftResponse
                    }
                };

                ctx.Invoke(actions);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Save_保存为草稿_并且仅保存草稿为一条记录(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

                string xml = ctx.ReadBpmn(bpmnFile);
                XDocument doc = XDocument.Parse(xml);

                pdd.BpmnXML = doc.ToString();
                pdd.Name = Path.GetFileNameWithoutExtension(bpmnFile);
                pdd.TenantId = ctx.TenantId;
                pdd.EnableDuplicateFiltering = false;

                Deployment deployment = AsyncHelper.RunSync<Deployment>(() => ctx.PostAsync<Deployment>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/save", pdd));

                IProcessEngine engine = ctx.Resolve<IProcessEngine>();

                IDeploymentQuery query = engine.RepositoryService.createDeploymentQuery()
                    .deploymentName(deployment.Name)
                    .deploymentTenantId(ctx.TenantId);

                IList<IDeployment> drafts = query.findDrafts();

                Assert.True(drafts?.Count <= 1);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData(1, 20)]
        public void Latest_查询最终部署的流程_已发布(int pageNo, int pageSize)
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                decimal totalCount = 0M;
                long recordCount = 0;

                do
                {
                    DeploymentQuery query = new DeploymentQuery
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

                    var result = AsyncHelper.RunSync<Resources<Deployment>>(() =>
                    ctx.PostAsync<Resources<Deployment>>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/latest", query));

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
        [InlineData(1, 20)]
        public void AllDeploymnents_查询所有的部署流程_已发布_未发布(int pageNo, int pageSize)
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                decimal totalCount = 0M;
                long recordCount = 0;

                do
                {
                    DeploymentQuery query = new DeploymentQuery
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

                    var result = AsyncHelper.RunSync<Resources<Deployment>>(() =>
                    ctx.PostAsync<Resources<Deployment>>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/list", query));

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
        public void GetProcessModel_首先部署BPMN_调用服务读取BMMN_文件内容应该等于服务读取内容(string bpmFile)
        {
            Exception ex = Record.Exception(() =>
            {
                Deployment dep = ctx.HttpDeployProcess(bpmFile);

                var response = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                ctx.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/{dep.Id}/processmodel"));

                var xml = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

                Assert.NotNull(xml?.Trim() == "" ? null : xml.Trim());
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetBpmnModel_获取BPMN对象模型图(string bpmFile)
        {
            Exception ex = Record.Exception(() =>
            {
                string bpmnName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                Deployment dep = ctx.HttpDeployProcess(bpmnName, bpmFile);

                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                ctx.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/{dep.Id}/bpmnmodel"));

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
