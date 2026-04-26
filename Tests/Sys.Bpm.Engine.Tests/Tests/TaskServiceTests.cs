using Newtonsoft.Json;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Bpm.Engine.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Tests
{
    [Collection("EngineTestCollection")]
    public class TaskServiceTests
    {
        private readonly InProcessEngineFixture _fixture;

        public TaskServiceTests(InProcessEngineFixture fixture)
        {
            _fixture = fixture;
        }

        private IProcessDefinition DeployBpmn(string bpmnFile)
        {
            string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
            string deployName = Guid.NewGuid().ToString();

            IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                .Name(deployName)
                .TenantId(_fixture.TenantId)
                .AddString(bpmnFile, xml)
                .EnableDuplicateFiltering()
                .TenantId(_fixture.TenantId);

            IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

            return _fixture.ProcessEngine.RepositoryService.CreateProcessDefinitionQuery()
                .SetDeploymentId(deploy.Id)
                .SetProcessDefinitionTenantId(_fixture.TenantId)
                .SetLatestVersion()
                .SingleResult();
        }

        [Fact]
        public void CreateTaskQuery_启动流程后查询_返回任务列表()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                var tasks = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.NotEmpty(tasks);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void GetMyTasks_按办理人查询_返回对应用户任务()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                IList<ITask> tasks = _fixture.ProcessEngine.TaskService.GetMyTasks(user);

                Assert.NotEmpty(tasks);
                Assert.Equal(user, tasks[0].Assignee);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void Claim_认领任务_办理人变更()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user1 = Guid.NewGuid().ToString();
                string user2 = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user1);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                var tasks = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                ITask task = tasks.First();

                // 先释放任务，再认领给新用户
                _fixture.ProcessEngine.TaskService.Unclaim(task.Id);
                _fixture.ProcessEngine.TaskService.Claim(task.Id, user2);

                var claimedTasks = _fixture.ProcessEngine.TaskService.GetMyTasks(user2);
                Assert.NotEmpty(claimedTasks);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void Complete_完成任务_任务不再存在()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                var tasks = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                ITask task = tasks.First();

                // 完成任务
                _fixture.ProcessEngine.TaskService.Complete(task.Id, new Dictionary<string, object>());

                // 原任务不再存在
                var remaining = _fixture.ProcessEngine.TaskService.CreateTaskQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.DoesNotContain(remaining, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("会签流程.bpmn")]
        public void 会签流程_多人流转_全部完成后进入下一节点(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn(bpmnFile);

                string[] users = new[]
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                };

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, new Dictionary<string, object>
                    {
                        { "countersignUsers", users }
                    }));

                // 每个会签用户应有任务
                foreach (var user in users)
                {
                    IList<ITask> userTasks = _fixture.ProcessEngine.TaskService.GetMyTasks(user);
                    Assert.NotEmpty(userTasks);
                }

                // 完成所有会签任务
                foreach (var user in users)
                {
                    IList<ITask> userTasks = _fixture.ProcessEngine.TaskService.GetMyTasks(user);
                    foreach (var t in userTasks)
                    {
                        _fixture.ProcessEngine.TaskService.Complete(t.Id, new Dictionary<string, object>
                        {
                            { "流程变量", "变量值" }
                        });
                    }
                }
            });

            Assert.Null(ex);
        }
    }
}
