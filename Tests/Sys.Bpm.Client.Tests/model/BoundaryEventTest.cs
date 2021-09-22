using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;

namespace Sys.Workflow.Client.Tests.Models
{
    public class BoundaryEventTest
    {
        private readonly InProcessWorkflowEngine context;
        private readonly IProcessEngine processEngine;

        public BoundaryEventTest()
        {
            context = new InProcessWorkflowEngine();
            context.Create();
            processEngine = ProcessEngineFactory.CreateProcessEngine("InProcess");
        }

        [Theory]
        [InlineData("立即执行TIMER.bpmn")]
        public void 立即执行TIMER(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

                ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

                Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
                {
                    Id = "评审员",
                    FullName = "评审员",
                    TenantId = context.TenantId
                };

                IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
                    .Name(Path.GetFileNameWithoutExtension(bpmnFile))
                    .TenantId(context.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(context.TenantId);

                IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

                IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
                    .SetDeploymentId(deploy.Id)
                    .SetProcessDefinitionTenantId(context.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                string uid = Guid.NewGuid().ToString();
                string utask = Guid.NewGuid().ToString();

                WorkflowVariable variables = new WorkflowVariable();
                variables.AddAssignee("users", uid).AddAssignee("tasks", utask);

                IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, variables));

                IList<ITask> tasks = commandExecutor.Execute(new GetMyTasksCmd(utask));
                Assert.NotEmpty(tasks);

                tasks = commandExecutor.Execute(new GetMyTasksCmd(uid));
                Assert.NotEmpty(tasks);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("边界信号事件.bpmn")]
        public void 读取信号边界事件处理(string bpmnFile)
        {
            string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

            ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

            Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
            {
                Id = "评审员",
                FullName = "评审员",
                TenantId = context.TenantId
            };

            IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
                .Name(Path.GetFileNameWithoutExtension(bpmnFile))
                .TenantId(context.TenantId)
                .AddString(bpmnFile, xml)
                .EnableDuplicateFiltering()
                .TenantId(context.TenantId);

            IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

            IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
                .SetDeploymentId(deploy.Id)
                .SetProcessDefinitionTenantId(context.TenantId)
                .SetLatestVersion()
                .SingleResult();

            IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, null));

            IList<ITask> tasks = processEngine.TaskService.GetMyTasks("用户1");

            ITaskEntity task = tasks[0] as ITaskEntity;
            IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

            //Process process = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).DeploymentManager.ResolveProcessDefinition(definition).Process;
            //FlowElement activity = process.GetFlowElement(execution.CurrentActivityId, true);
            //string taskName = task.Name;

            //commandExecutor.Execute(new TerminateTaskCmd(task.Id, "测试终止", true));

            processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
            {
                { "流程变量", "变量值" }
            });

            tasks = processEngine.TaskService.GetMyTasks("用户2");
            task = tasks[0] as ITaskEntity;
            processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
            {
                { "流程变量", "变量值" }
            });

            var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

            //commandExecutor.Execute(new ReturnToActivityCmd(task.Id, execution.CurrentActivityId, "回退到", null));

            //task = tasks[0] as ITaskEntity;
            //execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

            //commandExecutor.Execute(new TerminateProcessInstanceCmd(processInstance.Id, "流程已被取消", null));

            //IProcessInstance activityInstance = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor.Execute(new StartProcessInstanceByActivityCmd(definition.Id, execution.BusinessKey, activity.Id, null, execution.TenantId, null, true, execution.Name));

            //try
            //{
            //    //var events = processEngine.RuntimeService.CreateExecutionQuery()
            //    //    .SetParentId(tasks[0].ExecutionId)
            //    //    .SignalEventSubscriptionName("自动回退")
            //    //    .List();

            //    int count = 0;
            //    while (count < 3)
            //    {
            //        processEngine.ManagementService.ExecuteCommand(new SignalEventReceivedCmd("自动回退", null, false, context.TenantId));
            //        count += 1;
            //    }
            //}
            //catch { }
        }

        [Theory]
        [InlineData("边界节点循环流程.bpmn")]
        public void 边界节点循环事件处理(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string xml = IntegrationTestContext.ReadBpmn(bpmnFile);

                ICommandExecutor commandExecutor = (processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;

                Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
                {
                    Id = "评审员",
                    FullName = "评审员",
                    TenantId = context.TenantId
                };

                IDeploymentBuilder builder = processEngine.RepositoryService.CreateDeployment()
                    .Name(Path.GetFileNameWithoutExtension(bpmnFile))
                    .TenantId(context.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(context.TenantId);

                IDeployment deploy = commandExecutor.Execute(new DeployCmd(builder));

                IProcessDefinition definition = processEngine.RepositoryService.CreateProcessDefinitionQuery()
                    .SetDeploymentId(deploy.Id)
                    .SetProcessDefinitionTenantId(context.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                string admin = Guid.NewGuid().ToString();
                string node = Guid.NewGuid().ToString();
                string next = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee(nameof(admin), admin)
                .AddAssignee(nameof(node), node)
                .AddAssignee(nameof(next), next);

                IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, variables));

                Thread.Sleep(10000);

                IList<ITask> tasks = processEngine.TaskService.GetMyTasks(node);

                Assert.True(tasks.Count == 0);

                IList<ITask> adminTasks = processEngine.TaskService.GetMyTasks(admin);

                Assert.True(adminTasks.Count == 1);

                IList<ITask> nextTasks = processEngine.TaskService.GetMyTasks(next);

                Assert.True(nextTasks.Count == 0);

                processEngine.TaskService.Complete(adminTasks[0].Id);

                tasks = processEngine.TaskService.GetMyTasks(node);

                Assert.True(tasks.Count == 1);

                processEngine.TaskService.Complete(tasks[0].Id);

                nextTasks = processEngine.TaskService.GetMyTasks(next);

                Assert.True(nextTasks.Count == 1);

                processEngine.TaskService.Complete(nextTasks[0].Id);

                tasks = processEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.True(tasks.Count == 0);

                IHistoricProcessInstance hisProcess = processEngine.HistoryService.CreateHistoricProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.NotNull(hisProcess);
            });

            Assert.Null(ex);
        }
    }
}
