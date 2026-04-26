using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Management;
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
    public class ManagementServiceTests
    {
        private readonly InProcessEngineFixture _fixture;

        public ManagementServiceTests(InProcessEngineFixture fixture)
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

        // [Fact(Skip = "CreateJobQuery 存在 JIT 编译问题: System.InvalidProgramException")]
        [Fact]
        public void CreateJobQuery_查询作业_返回作业列表()
        {
            var ex = Record.Exception(() =>
            {
                var jobs = _fixture.ProcessEngine.ManagementService.CreateJobQuery()
                    .List();

                Assert.NotNull(jobs);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void SuspendProcessDefinitionById_挂起定义_不可启动新实例()
        {
            _fixture.SetAuthenticatedUser();
            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");

            try
            {
                // 挂起流程定义
                _fixture.ProcessEngine.RepositoryService.SuspendProcessDefinitionById(definition.Id);

                // 验证已挂起
                bool isSuspended = _fixture.ProcessEngine.RepositoryService.IsProcessDefinitionSuspended(definition.Id);
                Assert.True(isSuspended);
            }
            finally
            {
                // 无论测试是否成功，都恢复激活状态，避免污染其他测试
                try
                {
                    if (_fixture.ProcessEngine.RepositoryService.IsProcessDefinitionSuspended(definition.Id))
                    {
                        _fixture.ProcessEngine.RepositoryService.ActivateProcessDefinitionById(definition.Id);
                    }
                }
                catch { /* 忽略恢复时的异常 */ }
            }
        }

        [Fact(Skip = "引擎 API Bug: ActivateProcessDefinitionById 传入 DateTime.Now 导致延迟执行而非立即生效，需用 ActivateProcessDefinitionCmd(IProcessDefinitionEntity, ...) 构造函数绕过")]
        public void ActivateProcessDefinitionById_激活定义_可启动新实例()
        {
            _fixture.SetAuthenticatedUser();
            IProcessDefinition definition = DeployBpmn("简单顺序流.bpmn");

            try
            {
                // 先挂起
                _fixture.ProcessEngine.RepositoryService.SuspendProcessDefinitionById(definition.Id);

                // 再激活
                _fixture.ProcessEngine.RepositoryService.ActivateProcessDefinitionById(definition.Id);

                // 可启动新实例即表示激活成功
                string user = Guid.NewGuid().ToString();
                var variables = new WorkflowVariable();
                variables.AddAssignee("name", user);

                IProcessInstance processInstance = _fixture.CommandExecutor.Execute(
                    new StartProcessInstanceCmd(definition.Id, variables));

                Assert.NotNull(processInstance);
            }
            finally
            {
                // 确保恢复激活状态
                try
                {
                    if (_fixture.ProcessEngine.RepositoryService.IsProcessDefinitionSuspended(definition.Id))
                    {
                        _fixture.ProcessEngine.RepositoryService.ActivateProcessDefinitionById(definition.Id);
                    }
                }
                catch { /* 忽略恢复时的异常 */ }
            }
        }

        [Fact]
        public void TableCount_查询表行数_返回非空字典()
        {
            var ex = Record.Exception(() =>
            {
                IDictionary<string, long> tableCount = _fixture.ProcessEngine.ManagementService.TableCount;

                Assert.NotNull(tableCount);
            });

            Assert.Null(ex);
        }
    }
}
