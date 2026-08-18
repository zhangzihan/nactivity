using Microsoft.Extensions.Logging;
using Moq;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 定义决策流程引用应用服务的隔离测试。
    /// </summary>
    public class DecisionProcessReferenceIndexApplicationServiceTests
    {
        /// <summary>
        /// 验证索引覆盖完整时优先返回目录服务结果。
        /// </summary>
        [Fact]
        public async Task QueryDecisionReferencesAsync_Should_ReturnIndexedReferences_WhenCoverageComplete()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var latestQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var draftQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var latestDeploymentMock = CreateDeploymentMock("dep-published", "Leave Published", "tenant-a", new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));
            var draftDeploymentMock = CreateDeploymentMock("dep-draft", "Leave Draft", "tenant-a", new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc));
            var decisionCatalogServiceMock = new Mock<IDecisionCatalogService>(MockBehavior.Strict);

            latestQueryMock.Setup(query => query.SetLatestDeployment()).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.SetDeploymentTenantId("tenant-a")).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.List()).Returns(new List<IDeployment> { latestDeploymentMock.Object });
            draftQueryMock.Setup(query => query.SetDeploymentTenantId("tenant-a")).Returns(draftQueryMock.Object);
            draftQueryMock.Setup(query => query.FindDrafts()).Returns(new List<IDeployment> { draftDeploymentMock.Object });

            repositoryServiceMock
                .SetupSequence(service => service.CreateDeploymentQuery())
                .Returns(latestQueryMock.Object)
                .Returns(draftQueryMock.Object);
            decisionCatalogServiceMock
                .Setup(service => service.TryQueryDecisionReferencesAsync(
                    It.Is<DecisionProcessReferenceCatalogQuery>(query =>
                        query.DecisionKey == "expense.approval" &&
                        query.IncludeDrafts &&
                        query.DeploymentIds.Count == 2),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DecisionProcessReferenceDescriptor>
                {
                    new DecisionProcessReferenceDescriptor
                    {
                        DeploymentId = "dep-draft",
                        DeploymentName = "Leave Draft",
                        ProcessDefinitionKey = "leaveApproval",
                        ProcessDefinitionName = "请假审批",
                        TenantId = "tenant-a",
                        IsDraft = true,
                        DeployTime = new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc),
                        ReferencedTasks = new List<DecisionProcessReferenceTaskDescriptor>
                        {
                            new DecisionProcessReferenceTaskDescriptor
                            {
                                TaskId = "task_rule",
                                TaskName = "规则校验",
                                DecisionKey = "expense.approval",
                                BindingMode = "FixedVersion",
                                VersionTag = "draft-v2"
                            }
                        }
                    }
                });

            var service = CreateService(repositoryServiceMock.Object, decisionCatalogServiceMock.Object);
            var result = await service.QueryDecisionReferencesAsync("expense.approval", "tenant-a").ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("dep-draft", result[0].DeploymentId);
            Assert.True(result[0].IsDraft);
            Assert.Equal("draft-v2", result[0].ReferencedTasks[0].VersionTag);
            repositoryServiceMock.VerifyAll();
            decisionCatalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证索引不可用时会回退扫描 BPMN 资源。
        /// </summary>
        [Fact]
        public async Task QueryDecisionReferencesAsync_Should_FallbackToBpmnScanning_WhenIndexIsUnavailable()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var latestQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var draftQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var latestDeploymentMock = CreateDeploymentMock("dep-published", "Leave Published", "tenant-a", new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));
            var draftDeploymentMock = CreateDeploymentMock("dep-draft", "Leave Draft", "tenant-a", new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc));
            var decisionCatalogServiceMock = new Mock<IDecisionCatalogService>(MockBehavior.Strict);

            latestQueryMock.Setup(query => query.SetLatestDeployment()).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.SetDeploymentTenantId("tenant-a")).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.List()).Returns(new List<IDeployment> { latestDeploymentMock.Object });
            draftQueryMock.Setup(query => query.SetDeploymentTenantId("tenant-a")).Returns(draftQueryMock.Object);
            draftQueryMock.Setup(query => query.FindDrafts()).Returns(new List<IDeployment> { draftDeploymentMock.Object });

            repositoryServiceMock
                .SetupSequence(service => service.CreateDeploymentQuery())
                .Returns(latestQueryMock.Object)
                .Returns(draftQueryMock.Object);
            decisionCatalogServiceMock
                .Setup(service => service.TryQueryDecisionReferencesAsync(
                    It.IsAny<DecisionProcessReferenceCatalogQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<DecisionProcessReferenceDescriptor>?)null);
            repositoryServiceMock
                .Setup(service => service.GetDeploymentResourceNames("dep-published"))
                .Returns(new List<string> { "leave-published.bpmn" });
            repositoryServiceMock
                .Setup(service => service.GetDeploymentResourceNames("dep-draft"))
                .Returns(new List<string> { "leave-draft.bpmn" });
            repositoryServiceMock
                .Setup(service => service.GetResourceAsStream("dep-published", "leave-published.bpmn"))
                .Returns(() => CreateBpmnStream("expense.approval", "v1.0.0"));
            repositoryServiceMock
                .Setup(service => service.GetResourceAsStream("dep-draft", "leave-draft.bpmn"))
                .Returns(() => CreateBpmnStream("expense.approval", "draft-v2"));

            var service = CreateService(repositoryServiceMock.Object, decisionCatalogServiceMock.Object);
            var result = await service.QueryDecisionReferencesAsync("expense.approval", "tenant-a").ConfigureAwait(false);

            Assert.Equal(2, result.Count);
            Assert.Equal("dep-draft", result[0].DeploymentId);
            Assert.Equal("draft-v2", result[0].ReferencedTasks[0].VersionTag);
            Assert.Equal("dep-published", result[1].DeploymentId);
            Assert.Equal("v1.0.0", result[1].ReferencedTasks[0].VersionTag);
            repositoryServiceMock.VerifyAll();
            decisionCatalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证同步索引时会按 BPMN 规则任务生成快照。
        /// </summary>
        [Fact]
        public async Task TrySyncDeploymentReferencesAsync_Should_BuildSnapshotFromBpmnXml()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var indexRepositoryMock = new Mock<IDecisionProcessReferenceIndexRepository>(MockBehavior.Strict);
            var deploymentMock = CreateDeploymentMock("dep-1", "Leave Approval", "tenant-a", new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));

            indexRepositoryMock
                .Setup(repository => repository.ReplaceDeploymentReferencesAsync(
                    It.Is<DecisionProcessReferenceIndexSnapshot>(snapshot =>
                        snapshot.DeploymentId == "dep-1" &&
                        snapshot.Processes.Count == 1 &&
                        snapshot.Processes[0].ReferencedTasks.Count == 1 &&
                        snapshot.Processes[0].ReferencedTasks[0].DecisionKey == "expense.approval" &&
                        snapshot.Processes[0].ReferencedTasks[0].VersionTag == "v1.0.0"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(
                repositoryServiceMock.Object,
                decisionCatalogService: null,
                decisionProcessReferenceIndexRepository: indexRepositoryMock.Object);
            await service.TrySyncDeploymentReferencesAsync(
                deploymentMock.Object,
                CreateBusinessRuleTaskBpmnXml("expense.approval", "v1.0.0"),
                isDraft: false).ConfigureAwait(false);

            indexRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 创建决策流程引用应用服务实例。
        /// </summary>
        /// <param name="repositoryService">流程仓储服务。</param>
        /// <param name="decisionCatalogService">决策目录服务。</param>
        /// <param name="decisionProcessReferenceIndexRepository">决策引用索引仓储。</param>
        /// <returns>应用服务实例。</returns>
        private static DecisionProcessReferenceIndexApplicationService CreateService(
            IRepositoryService repositoryService,
            IDecisionCatalogService? decisionCatalogService,
            IDecisionProcessReferenceIndexRepository? decisionProcessReferenceIndexRepository = null)
        {
            var processEngineMock = new Mock<IProcessEngine>(MockBehavior.Strict);
            processEngineMock.SetupGet(engine => engine.RepositoryService).Returns(repositoryService);

            return new DecisionProcessReferenceIndexApplicationService(
                processEngineMock.Object,
                decisionCatalogService,
                decisionProcessReferenceIndexRepository,
                LoggerFactory.Create(_ => { }));
        }

        /// <summary>
        /// 创建部署记录 Mock。
        /// </summary>
        /// <param name="id">部署标识。</param>
        /// <param name="name">部署名称。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="deploymentTime">部署时间。</param>
        /// <returns>部署记录 Mock。</returns>
        private static Mock<IDeployment> CreateDeploymentMock(string id, string name, string tenantId, DateTime deploymentTime)
        {
            var deploymentMock = new Mock<IDeployment>(MockBehavior.Strict);
            deploymentMock.SetupGet(item => item.Id).Returns(id);
            deploymentMock.SetupGet(item => item.Name).Returns(name);
            deploymentMock.SetupGet(item => item.TenantId).Returns(tenantId);
            deploymentMock.SetupGet(item => item.DeploymentTime).Returns(deploymentTime);
            return deploymentMock;
        }

        /// <summary>
        /// 为测试创建 BPMN XML 资源流。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>BPMN 资源流。</returns>
        private static Stream CreateBpmnStream(string decisionKey, string versionTag)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(CreateBusinessRuleTaskBpmnXml(decisionKey, versionTag)));
        }

        /// <summary>
        /// 创建带有结构化决策绑定的 BPMN XML。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>BPMN XML 文本。</returns>
        private static string CreateBusinessRuleTaskBpmnXml(string decisionKey, string versionTag)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<definitions xmlns=""http://www.omg.org/spec/BPMN/20100524/MODEL""
             xmlns:bpmndi=""http://www.omg.org/spec/BPMN/20100524/DI""
             xmlns:dc=""http://www.omg.org/spec/DD/20100524/DC""
             xmlns:di=""http://www.omg.org/spec/DD/20100524/DI""
             xmlns:qh=""http://activiti.org/bpmn""
             targetNamespace=""http://sys.workflow/tests"">
  <process id=""leaveApproval"" name=""请假审批"" isExecutable=""true"">
    <startEvent id=""start"" />
    <businessRuleTask id=""task_rule"" name=""规则校验"">
      <extensionElements>
        <qh:decisionBinding decisionKey=""{decisionKey}"" bindingMode=""FixedVersion"" versionTag=""{versionTag}"" />
      </extensionElements>
    </businessRuleTask>
    <endEvent id=""end"" />
    <sequenceFlow id=""flow_1"" sourceRef=""start"" targetRef=""task_rule"" />
    <sequenceFlow id=""flow_2"" sourceRef=""task_rule"" targetRef=""end"" />
  </process>
</definitions>";
        }
    }
}
