using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using Sys.Bpm.api.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Sys.Bpmn.Test.rest.controller
{
    public class ProcessInstanceControllerTest
    {
        private readonly UnitTestContext ctx = UnitTestContext.CreateDefaultUnitTestContext();

        public ProcessInstanceControllerTest()
        {

        }

        private IProcessInstanceController CreateController()
        {
            return new ProcessInstanceControllerImpl(ctx.Resolve<ProcessEngineWrapper>(),
                                ctx.Resolve<ProcessInstanceResourceAssembler>(),
                                ctx.Resolve<PageableProcessInstanceRepositoryService>(),
                                ctx.Resolve<IProcessEngine>(),
                                ctx.Resolve<SecurityPoliciesApplicationService>());
        }

        /// <summary>
        /// 简单顺序流的每个任务分配变量名都是name,启动时如果没有name变量名,会抛出异常.
        /// </summary>
        [Fact]
        public void Start_启动一个流程实例()
        {
            Exception ex = Record.Exception(() =>
            {
                IProcessInstanceController processInstance = CreateController();

                IProcessEngine pe = ctx.Resolve<IProcessEngine>();
                IProcessDefinition def = pe.RepositoryService.createProcessDefinitionQuery()
                    .processDefinitionName("简单顺序流")
                    .processDefinitionTenantId(ctx.TenantId)
                    .latestVersion()
                    .singleResult();

                Assert.NotNull(def);

                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd(def.Id,
                    new Dictionary<string, object>
                    {
                        { "name", "新用户"}
                    });

                cmd.ProcessInstanceName = $"{def.Name}";
                cmd.TenantId = ctx.TenantId;

                ProcessInstance pi = processInstance.Start(cmd).Result;
                Assert.NotNull(pi);
            });

            Assert.Null(ex);
        }

        /// <summary>
        /// 简单顺序流的每个任务分配变量名都是name,启动时如果没有name变量名,会抛出异常.
        /// </summary>
        [Fact]
        public void Start_使用URL启动一个流程实例()
        {
            Exception ex = Record.Exception(() =>
            {
                StartProcess();
            });

            Assert.Null(ex);
        }

        private void StartProcess()
        {
            IProcessEngine pe = ctx.Resolve<IProcessEngine>();
            IProcessDefinition def = pe.RepositoryService.createProcessDefinitionQuery()
                .processDefinitionName("简单顺序流")
                .processDefinitionTenantId(ctx.TenantId)
                .latestVersion()
                .singleResult();

            Assert.NotNull(def);

            StartProcessInstanceCmd cmd = new StartProcessInstanceCmd(def.Id,
                new Dictionary<string, object>
                {
                        { "name", "新用户"}
                });

            cmd.ProcessInstanceName = $"{def.Name}";
            cmd.TenantId = ctx.TenantId;

            HttpClient client = ctx.TestServer.CreateClient();

            HttpResponseMessage res = client.PostAsync("workflow/process-instances/start", new JsonContent(cmd)).Result;

            Assert.True(res.StatusCode == HttpStatusCode.OK);

            ProcessInstance pi = JsonConvert.DeserializeObject<ProcessInstance>(res.Content.ReadAsStringAsync().Result);

            Assert.NotNull(pi);
        }

        [Fact]
        public void Suspend_挂起一个流程()
        {
            Exception ex = Record.Exception(() =>
            {
                ProcessInstance[] instances = GetProcessInstanceList();

                Assert.NotNull(instances);

                if (instances.Length == 0)
                {
                    StartProcess();

                    instances = GetProcessInstanceList();
                }

                ProcessInstance pi = instances[0];

                HttpClient client = ctx.TestServer.CreateClient();

                HttpResponseMessage res = client.GetAsync($"workflow/process-instances/{pi.Id}/suspend").Result;

                Assert.True(res.StatusCode == HttpStatusCode.OK);
            });

            Assert.Null(ex);
        }

        private ProcessInstance[] GetProcessInstanceList()
        {
            HttpClient client = ctx.TestServer.CreateClient();
            ProcessInstanceQuery piq = new ProcessInstanceQuery
            {
                OnlyProcessInstances = true,
                SuspensionState = SuspensionStateProvider.ACTIVE,
                Pageable = new Pageable
                {
                    PageNo = 1,
                    PageSize = 1
                }
            };

            HttpResponseMessage res = client.PostAsync("workflow/process-instances", new JsonContent(piq)).Result;

            Assert.True(res.StatusCode == HttpStatusCode.OK);

            string json = res.Content.ReadAsStringAsync().Result;

            ProcessInstance[] instances = JsonConvert.DeserializeObject<Resources<ProcessInstance>>(json).List.ToArray();

            return instances;
        }

        [Fact]
        public void Start_完成简单顺序流程()
        {
            Exception ex = Record.Exception(() =>
            {
                //IProcessInstanceController processInstance = new ProcessInstanceControllerImpl(ctx.Resolve<ProcessEngineWrapper>(),
                //    ctx.Resolve<ProcessInstanceResourceAssembler>(),
                //    ctx.Resolve<PageableProcessInstanceService>(),
                //    ctx.Resolve<IProcessEngine>(),
                //    ctx.Resolve<SecurityPoliciesApplicationService>());

                //IProcessEngine pe = ctx.Resolve<IProcessEngine>();
                //IProcessDefinition def = pe.RepositoryService.createProcessDefinitionQuery()
                //    .processDefinitionName("简单顺序流")
                //    .processDefinitionTenantId(ctx.TenantId)
                //    .latestVersion()
                //    .singleResult();

                //Assert.NotNull(def);

                //StartProcessInstanceCmd cmd = new StartProcessInstanceCmd(def.Id,
                //    new Dictionary<string, object>
                //    {
                //            { "name", "新用户"}
                //    });

                //cmd.ProcessInstanceName = $"{def.Name}";
                //cmd.TenantId = ctx.TenantId;

                //ProcessInstance pi = processInstance.Start(cmd).Result;
                //Assert.NotNull(pi);
            });

            Assert.Null(ex);
        }
    }
}
