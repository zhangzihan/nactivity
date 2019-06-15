using Microsoft.AspNetCore.Mvc;
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
using org.activiti.engine.impl.identity;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using Sys.Bpm.Exceptions;
using Sys.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Sys.Bpmn.Test.rest.controller
{
    [Order(3)]
    public class ProcessInstanceControllerTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

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
        [Theory]
        [InlineData("用户1", "用户2")]
        public void Start_启动一个流程实例(params string[] users)
        {
            Exception ex = Record.Exception(() =>
            {
                ctx.HttpDeployProcess();

                StartProcess(users);
            });

            Assert.Null(ex);
        }

        private void StartProcess(string[] users)
        {
            IProcessEngine pe = ctx.Resolve<IProcessEngine>();
            IProcessDefinition def = pe.RepositoryService.createProcessDefinitionQuery()
                .processDefinitionName("简单顺序流")
                .processDefinitionTenantId(ctx.TenantId)
                .latestVersion()
                .singleResult();

            Assert.NotNull(def);

            IList<StartProcessInstanceCmd> cmds = new List<StartProcessInstanceCmd>();

            foreach (var usr in users)
            {
                cmds.Add(
                new StartProcessInstanceCmd(def.Id,
                    new Dictionary<string, object>
                    {
                        { "name", new string[]{ usr }}
                    })
                {
                    TenantId = ctx.TenantId
                });
            };

            HttpResponseMessage res = AsyncHelper.RunSync<HttpResponseMessage>(() => ctx.CreateHttpClientProxy().PostAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_INS_ROUTER_V1}/start", cmds.ToArray()));

            Assert.True(res.StatusCode == HttpStatusCode.OK);

            ProcessInstance[] pi = JsonConvert.DeserializeObject<ProcessInstance[]>(res.Content.ReadAsStringAsync().Result);

            Assert.NotNull(pi);
            Assert.True(pi.Length == users.Length);
        }

        [Theory]
        [InlineData("新用户")]
        public void Suspend_挂起一个流程(params string[] users)
        {
            Exception ex = Record.Exception(() =>
            {
                ProcessInstance[] instances = GetProcessInstanceList();

                Assert.NotNull(instances);

                if (instances.Length == 0)
                {
                    StartProcess(users);

                    instances = GetProcessInstanceList();
                }

                ProcessInstance pi = instances[0];

                HttpResponseMessage res = AsyncHelper.RunSync<HttpResponseMessage>(() => ctx.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_INS_ROUTER_V1}/{pi.Id}/suspend"));

                Assert.True(res.StatusCode == HttpStatusCode.OK);
            });

            Assert.Null(ex);
        }

        private ProcessInstance[] GetProcessInstanceList()
        {
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

            HttpResponseMessage res = AsyncHelper.RunSync<HttpResponseMessage>(() => ctx.PostAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_INS_ROUTER_V1}", piq));

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
                IProcessEngine pe = ctx.Resolve<IProcessEngine>();
                IProcessDefinition def = pe.RepositoryService.createProcessDefinitionQuery()
                    .processDefinitionName("简单顺序流")
                    .processDefinitionTenantId(ctx.TenantId)
                    .latestVersion()
                    .singleResult();

                Assert.NotNull(def);

                string uid = Guid.NewGuid().ToString();

                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd(def.Id,
                        new Dictionary<string, object>
                        {
                            { "name", new string[]{ uid }}
                        })
                {
                    TenantId = ctx.TenantId
                };

                IHttpClientProxy clientProxy = ctx.CreateHttpClientProxy(new UserInfo
                {
                    Id = uid,
                    Name = uid,
                    TenantId = ctx.TenantId
                });

                //启动流程
                ProcessInstance res = AsyncHelper.RunSync<ProcessInstance[]>(() => clientProxy.PostAsync<ProcessInstance[]>($"{WorkflowConstants.PROC_INS_ROUTER_V1}/start", new StartProcessInstanceCmd[] { cmd }))[0];

                Assert.NotNull(res);

                //查找我的任务--教师注册
                Resources<TaskModel> tasks = AsyncHelper.RunSync<Resources<TaskModel>>(() =>
                    clientProxy.GetAsync<Resources<TaskModel>>($"{WorkflowConstants.TASK_ROUTER_V1}/{uid}/mytasks"));

                Assert.NotNull(tasks);
                Assert.True(tasks.List.Count() > 0);

                TaskModel task = tasks.List.FirstOrDefault(x => x.ProcessInstanceId == res.Id);

                //完成我的任务--教师注册
                HttpResponseMessage complete = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                    clientProxy.PostAsync<HttpResponseMessage>($"{WorkflowConstants.TASK_ROUTER_V1}/complete", new CompleteTaskCmd(task.Id, null, null)));

                Assert.True(complete.StatusCode == HttpStatusCode.OK);

                //查找我的任务--学生注册
                tasks = AsyncHelper.RunSync<Resources<TaskModel>>(() =>
                    clientProxy.GetAsync<Resources<TaskModel>>($"{WorkflowConstants.TASK_ROUTER_V1}/{uid}/mytasks"));

                Assert.NotNull(tasks);
                Assert.True(tasks.List.Count() > 0);

                task = tasks.List.FirstOrDefault(x => x.ProcessInstanceId == res.Id);

                //完成我的任务--学生注册
                complete = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                    clientProxy.PostAsync<HttpResponseMessage>($"{WorkflowConstants.TASK_ROUTER_V1}/complete", new CompleteTaskCmd(task.Id, null, null)));

                Assert.True(complete.StatusCode == HttpStatusCode.OK);

                //查找我的任务--支付
                tasks = AsyncHelper.RunSync<Resources<TaskModel>>(() =>
                    clientProxy.GetAsync<Resources<TaskModel>>($"{WorkflowConstants.TASK_ROUTER_V1}/{uid}/mytasks"));

                Assert.NotNull(tasks);
                Assert.True(tasks.List.Count() > 0);

                task = tasks.List.FirstOrDefault(x => x.ProcessInstanceId == res.Id);

                //完成我的任务--支付
                complete = AsyncHelper.RunSync<HttpResponseMessage>(() =>
                    clientProxy.PostAsync<HttpResponseMessage>($"{WorkflowConstants.TASK_ROUTER_V1}/complete", new CompleteTaskCmd(task.Id, null, null)));

                Assert.True(complete.StatusCode == HttpStatusCode.OK);

                //查找流程实例是否存在
                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => clientProxy.GetAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_INS_ROUTER_V1}/{res.Id}"));
            });

            Assert.NotNull(ex);
        }
    }
}
