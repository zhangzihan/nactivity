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
    public class RuntimeServiceTests
    {
        private readonly InProcessEngineFixture _fixture;

        public RuntimeServiceTests(InProcessEngineFixture fixture)
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
        public void StartProcessInstanceById_指定定义ID_返回流程实例()
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

                Assert.NotNull(processInstance);
                Assert.Equal(definition.Id, processInstance.ProcessDefinitionId);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void StartProcessInstanceByKey_指定业务键_返回流程实例()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
                string bizKey = Guid.NewGuid().ToString();
                string user = Guid.NewGuid().ToString();

                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(null, definition.Id, bizKey, variables));

                Assert.NotNull(processInstance);
                Assert.Equal(bizKey, processInstance.BusinessKey);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void GetVariables_SetVariable_读写变量正确()
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

                // 设置变量
                _fixture.ProcessEngine.RuntimeService.SetVariable(processInstance.Id, "测试变量", "变量值");

                // 读取变量
                object value = _fixture.ProcessEngine.RuntimeService.GetVariable(processInstance.Id, "测试变量");

                Assert.NotNull(value);
                Assert.Equal("变量值", value.ToString());
            });

            Assert.Null(ex);
        }

        [Fact]
        public void DeleteProcessInstance_终止流程_实例不再可查()
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

                Assert.NotNull(processInstance);

                // 删除流程实例
                _fixture.ProcessEngine.RuntimeService.DeleteProcessInstance(processInstance.Id, "测试删除");

                // 查询应不可见
                IProcessInstance found = _fixture.ProcessEngine.RuntimeService.CreateProcessInstanceQuery()
                    .SetProcessInstanceId(processInstance.Id)
                    .SingleResult();

                Assert.Null(found);
            });

            Assert.Null(ex);
        }
    }
}
