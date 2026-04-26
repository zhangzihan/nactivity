using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
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
    public class HistoryServiceTests
    {
        private readonly InProcessEngineFixture _fixture;

        public HistoryServiceTests(InProcessEngineFixture fixture)
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
        public void CreateHistoricProcessInstanceQuery_已完成流程_可查到历史实例()
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

                // 查询历史流程实例
                IHistoricProcessInstance hisInstance = _fixture.ProcessEngine.HistoryService.CreateHistoricProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.NotNull(hisInstance);
                Assert.Equal(processInstance.Id, hisInstance.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CreateHistoricTaskInstanceQuery_完成任务_可查到历史任务()
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
                _fixture.ProcessEngine.TaskService.Complete(task.Id, new Dictionary<string, object>());

                // 查询历史任务实例
                var historicTasks = _fixture.ProcessEngine.HistoryService.CreateHistoricTaskInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.NotEmpty(historicTasks);
                Assert.Contains(historicTasks, t => t.Id == task.Id);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CreateHistoricVariableInstanceQuery_设置变量_可查到历史变量()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);
                variables.Add("测试变量", "变量值");

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                // 查询历史变量实例
                var historicVariables = _fixture.ProcessEngine.HistoryService.CreateHistoricVariableInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .List();

                Assert.NotEmpty(historicVariables);
            });

            Assert.Null(ex);
        }
    }
}
