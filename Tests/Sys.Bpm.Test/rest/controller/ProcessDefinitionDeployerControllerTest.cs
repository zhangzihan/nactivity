using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using org.activiti.bpmn.constants;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.engine;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.repository;
using Sys.Bpm.api.http;
using Sys.Bpm.rest.controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Sys.Bpmn.Test.rest.controller
{
    public class ProcessDefinitionDeployerControllerTest
    {
        private readonly UnitTestContext ctx = UnitTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionDeployerControllerTest()
        {
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Deploy_流程部署测试(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

                pdd.BpmnXML = ctx.ReadBpmn(bpmnFile);
                pdd.Name = Path.GetFileNameWithoutExtension(bpmnFile);
                pdd.DisableBpmnValidation = false;
                pdd.TenantId = ctx.TenantId;

                HttpResponseMessage response = ctx.PostAsync($"{WorkflowConstants.PROC_DEP_ROUTER_V1}", pdd).Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Deployment deployment = JsonConvert.DeserializeObject<Deployment>(response.Content.ReadAsStringAsync().Result);

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
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                };

                HttpInvoke[] actions = new HttpInvoke[]
                {
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

                HttpResponseMessage response = ctx.PostAsync($"{WorkflowConstants.PROC_DEP_ROUTER_V1}/save", pdd).Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                string strResult = response.Content.ReadAsStringAsync().Result;

                Deployment deployment = JsonConvert.DeserializeObject<Deployment>(strResult);

                IProcessEngine engine = ctx.Resolve<IProcessEngine>();

                IDeploymentQuery query = engine.RepositoryService.createDeploymentQuery()
                    .deploymentName(deployment.Name)
                    .deploymentTenantId(ctx.TenantId);

                IList<IDeployment> drafts = query.findDrafts();

                Assert.True(drafts?.Count <= 1);
            });

            Assert.Null(ex);
        }
    }
}
