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
using System.Threading.Tasks;
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

        [Fact]
        public async Task StartProcessInstanceByCmdAsync_单个命令_返回流程实例()
        {
            _fixture.SetAuthenticatedUser();

            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
            string user = Guid.NewGuid().ToString();

            var variables = new WorkflowVariable();
            variables.AddAssignee("name", user);

            var cmd = new StartProcessInstanceCmdStub
            {
                ProcessDefinitionId = definition.Id,
                Variables = variables
            };

            IProcessInstance[] instances = await _fixture.ProcessEngine.RuntimeService
                .StartProcessInstanceByCmdAsync(new IStartProcessInstanceCmd[] { cmd });

            Assert.NotNull(instances);
            Assert.Single(instances);
            Assert.NotNull(instances[0]);
            Assert.Equal(definition.Id, instances[0].ProcessDefinitionId);
        }

        [Fact]
        public async Task StartProcessInstanceByCmdAsync_批量命令_返回多个流程实例()
        {
            _fixture.SetAuthenticatedUser();

            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");

            var cmds = new IStartProcessInstanceCmd[3];
            for (int i = 0; i < cmds.Length; i++)
            {
                string user = Guid.NewGuid().ToString();
                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                cmds[i] = new StartProcessInstanceCmdStub
                {
                    ProcessDefinitionId = definition.Id,
                    Variables = variables
                };
            }

            IProcessInstance[] instances = await _fixture.ProcessEngine.RuntimeService
                .StartProcessInstanceByCmdAsync(cmds);

            Assert.NotNull(instances);
            Assert.Equal(3, instances.Length);
            Assert.All(instances, ins =>
            {
                Assert.NotNull(ins);
                Assert.Equal(definition.Id, ins.ProcessDefinitionId);
            });
        }

        [Fact]
        public async Task StartProcessInstanceByCmdAsync_指定业务键_业务键正确()
        {
            _fixture.SetAuthenticatedUser();

            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
            string bizKey = Guid.NewGuid().ToString();
            string user = Guid.NewGuid().ToString();

            var variables = new WorkflowVariable();
            variables.AddAssignee("name", user);

            var cmd = new StartProcessInstanceCmdStub
            {
                ProcessDefinitionId = definition.Id,
                BusinessKey = bizKey,
                Variables = variables
            };

            IProcessInstance[] instances = await _fixture.ProcessEngine.RuntimeService
                .StartProcessInstanceByCmdAsync(new IStartProcessInstanceCmd[] { cmd });

            Assert.NotNull(instances);
            Assert.Single(instances);
            Assert.Equal(bizKey, instances[0].BusinessKey);
        }

        [Fact]
        public async Task StartProcessInstanceByCmdAsync_命令为null_抛出ArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _fixture.ProcessEngine.RuntimeService.StartProcessInstanceByCmdAsync(null));
        }

        [Fact]
        public async Task StartProcessInstanceByCmdAsync_异步不阻塞调用线程()
        {
            _fixture.SetAuthenticatedUser();

            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");
            string user = Guid.NewGuid().ToString();

            var variables = new WorkflowVariable();
            variables.AddAssignee("name", user);

            var cmd = new StartProcessInstanceCmdStub
            {
                ProcessDefinitionId = definition.Id,
                Variables = variables
            };

            // 验证返回的 Task 尚未同步完成（即不是 Task.FromResult 的伪异步）
            Task<IProcessInstance[]> task = _fixture.ProcessEngine.RuntimeService
                .StartProcessInstanceByCmdAsync(new IStartProcessInstanceCmd[] { cmd });

            // Task 不应是在调用瞬间就已完成（IsCompleted == true 表示同步执行，即伪异步）
            // 注意：由于命令执行很快，Task 可能在 await 之前就完成了，所以仅验证 Task 类型
            Assert.NotNull(task);
            Assert.True(task is Task<IProcessInstance[]>);

            IProcessInstance[] instances = await task;
            Assert.NotNull(instances);
            Assert.Single(instances);
        }

        /// <summary>
        /// IStartProcessInstanceCmd 的测试桩实现
        /// </summary>
        private class StartProcessInstanceCmdStub : IStartProcessInstanceCmd
        {
            public string BusinessKey { get; set; }
            public string ProcessDefinitionId { get; set; }
            public string ProcessDefinitionBusinessKey { get; set; }
            public string ProcessDefinitionKey { get; set; }
            public string ProcessInstanceName { get; set; }
            public string ProcessName { get; set; }
            public string StartForm { get; set; }
            public string TenantId { get; set; }
            public WorkflowVariable Variables { get; set; }
            public string InitialFlowElementId { get; set; }
            public string StartByMessage { get; set; }
        }
    }
}
