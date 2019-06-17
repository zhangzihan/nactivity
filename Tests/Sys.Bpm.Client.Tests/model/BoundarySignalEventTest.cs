using Microsoft.Xunit.Performance;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflown.Test;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Sys.Workflow.Client.Tests.Models
{
    public class BoundarySignalEventTest
    {
        private readonly InProcessWorkflowEngine context;
        private readonly IProcessEngine processEngine;

        public BoundarySignalEventTest()
        {
            context = new InProcessWorkflowEngine();
            context.Create();
            processEngine = ProcessEngineFactory.Instance.GetProcessEngine("InProcess");
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
        [InlineData("会签节点回退.bpmn")]
        public void 会签节点回退时到任意已完成节点终止当前节点(string bpmnFile)
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

            string[] users = new string[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, new Dictionary<string, object>
            {
                { "users", users }
            }));

            try
            {
                IList<ITask> tasks = processEngine.TaskService.GetMyTasks("节点1");

                ITaskEntity task = tasks[0] as ITaskEntity;
                IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

                processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
                {
                    { "流程变量", "变量值" }
                });

                tasks = processEngine.TaskService.GetMyTasks("节点2");
                task = tasks[0] as ITaskEntity;
                processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
                {
                    { "流程变量", "变量值" }
                });

                var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

                tasks = processEngine.TaskService.GetMyTasks(users[0]);
                task = tasks[0] as ITaskEntity;

                commandExecutor.Execute(new ReturnToActivityCmd(task.Id, list[0].Id, $"回退测试到{list[0].Name}"));
            }
            catch
            {

            }

            System.Diagnostics.Debugger.Break();
        }


        [Theory]
        [InlineData("回退到任意已完成节点.bpmn")]
        public void 回退到任务已完成节点(string bpmnFile)
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

            try
            {
                IList<ITask> tasks = processEngine.TaskService.GetMyTasks("节点1");

                ITaskEntity task = tasks[0] as ITaskEntity;
                IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

                processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
                {
                    { "流程变量", "变量值" }
                });

                tasks = processEngine.TaskService.GetMyTasks("节点2");
                task = tasks[0] as ITaskEntity;
                processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
                {
                    { "流程变量", "变量值" }
                });

                var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

                tasks = processEngine.TaskService.GetMyTasks("节点3");
                task = tasks[0] as ITaskEntity;

                commandExecutor.Execute(new ReturnToActivityCmd(task.Id, list[0].Id, $"回退测试到{list[0].Name}"));
            }
            catch
            {

            }

            System.Diagnostics.Debugger.Break();
        }

        [Theory]
        [InlineData("边界信号事件.bpmn")]
        public void 获取已完成流程节点(string bpmnFile)
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

            BpmnModel model = commandExecutor.Execute(new GetBpmnModelCmd(definition.Id));

            IProcessInstance processInstance = commandExecutor.Execute(new StartProcessInstanceCmd(definition.Id, null));

            IList<ITask> tasks = processEngine.TaskService.GetMyTasks("用户1");

            ITaskEntity task = tasks[0] as ITaskEntity;
            IExecutionEntity execution = commandExecutor.Execute(new GetExecutionByIdCmd(task.ExecutionId));

            processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
            {
                { "流程变量", "变量值" }
            });

            var list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

            Assert.Contains(model.MainProcess.FlowElements, x => list.Any(y => x.Id == y.Id));

            tasks = processEngine.TaskService.GetMyTasks("用户2");
            task = tasks[0] as ITaskEntity;
            processEngine.TaskService.Complete(task.Id, new Dictionary<string, object>
            {
                { "流程变量", "变量值" }
            });

            list = commandExecutor.Execute(new GetCompletedTaskModelsCmd(task.ProcessInstanceId, true));

            Assert.Contains(model.MainProcess.FlowElements, x => list.Any(y => x.Id == y.Id));
        }
    }
}
