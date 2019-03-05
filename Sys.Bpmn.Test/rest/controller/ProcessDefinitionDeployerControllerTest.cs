using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.constants;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.engine;
using org.activiti.engine.repository;
using Sys.Bpm.rest.api;
using Sys.Bpm.rest.controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                IProcessDefinitionDeployerController pddc = new ProcessDefinitionDeployerController(ctx.Resolve<IProcessEngine>(),
                    ctx.Resolve<DeploymentConverter>(),
                    ctx.Resolve<PageableDeploymentRespositoryService>(),
                    null);

                ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

                pdd.BpmnXML = ctx.ReadBpmn(bpmnFile);
                pdd.Name = Path.GetFileNameWithoutExtension(bpmnFile);
                pdd.TenantId = ctx.TenantId;

                Deployment res = pddc.Deploy(pdd).Result;

                Assert.NotNull(res);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void Deploy_部署非运行状态的流程(string bpmnFile)
        {
            Exception ex = Record.Exception(() =>
            {
                IProcessDefinitionDeployerController pddc = new ProcessDefinitionDeployerController(ctx.Resolve<IProcessEngine>(),
                    ctx.Resolve<DeploymentConverter>(),
                    ctx.Resolve<PageableDeploymentRespositoryService>(),
                    null);

                ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

                string xml = ctx.ReadBpmn(bpmnFile);
                XDocument doc = XDocument.Parse(xml);
                XElement process = doc.Descendants(XName.Get("process", BpmnXMLConstants.BPMN2_NAMESPACE)).First();
                process.Attribute("isExecutable").SetValue(false);

                pdd.BpmnXML = doc.ToString();
                pdd.Name = Path.GetFileNameWithoutExtension(bpmnFile);
                pdd.TenantId = ctx.TenantId;
                pdd.EnableDuplicateFiltering = false;

                Deployment res = pddc.Deploy(pdd).Result;

                Assert.NotNull(res);
            });

            Assert.Null(ex);
        }
    }
}
