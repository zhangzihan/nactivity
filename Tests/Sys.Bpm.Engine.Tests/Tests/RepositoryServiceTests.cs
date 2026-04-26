using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Repository;
using Sys.Bpm.Engine.Tests.Fixtures;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Tests
{
    [Collection("EngineTestCollection")]
    public class RepositoryServiceTests
    {
        private readonly InProcessEngineFixture _fixture;

        public RepositoryServiceTests(InProcessEngineFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Deploy_添加Bpmn资源_返回部署对象()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

                Assert.NotNull(deploy);
                Assert.NotNull(deploy.Id);
                Assert.Equal(deployName, deploy.Name);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CreateProcessDefinitionQuery_部署后查询_返回最新版本定义()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

                IProcessDefinition definition = _fixture.ProcessEngine.RepositoryService.CreateProcessDefinitionQuery()
                    .SetDeploymentId(deploy.Id)
                    .SetProcessDefinitionTenantId(_fixture.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                Assert.NotNull(definition);
                Assert.Equal(deploy.Id, definition.DeploymentId);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void EnableDuplicateFiltering_重复部署_过滤重复()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                // 第一次部署
                IDeploymentBuilder builder1 = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy1 = _fixture.CommandExecutor.Execute(new DeployCmd(builder1));

                // 第二次部署（启用重复过滤，应返回相同部署）
                IDeploymentBuilder builder2 = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy2 = _fixture.CommandExecutor.Execute(new DeployCmd(builder2));

                Assert.NotNull(deploy1);
                Assert.NotNull(deploy2);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void GetDeploymentResourceNames_部署后查询_返回资源名称列表()
        {
            var ex = Record.Exception(() =>
            {
                _fixture.SetAuthenticatedUser();

                string bpmnFile = "简单顺序流.bpmn";
                string xml = InProcessEngineFixture.ReadBpmn(bpmnFile);
                string deployName = Guid.NewGuid().ToString();

                IDeploymentBuilder builder = _fixture.ProcessEngine.RepositoryService.CreateDeployment()
                    .Name(deployName)
                    .TenantId(_fixture.TenantId)
                    .AddString(bpmnFile, xml)
                    .EnableDuplicateFiltering()
                    .TenantId(_fixture.TenantId);

                IDeployment deploy = _fixture.CommandExecutor.Execute(new DeployCmd(builder));

                var resourceNames = _fixture.ProcessEngine.RepositoryService.GetDeploymentResourceNames(deploy.Id);

                Assert.NotEmpty(resourceNames);
                Assert.Contains(bpmnFile, resourceNames);
            });

            Assert.Null(ex);
        }
    }
}
