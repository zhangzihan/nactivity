using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Sys.Data;
using Sys.Net.Http;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Controllers;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 流程部署控制器中规则决策集成能力的隔离测试。
    /// </summary>
    public class ProcessDefinitionDeployerControllerTests
    {
        /// <summary>
        /// 验证控制器可以按决策键扫描已发布与草稿流程引用。
        /// </summary>
        [Fact]
        public async Task QueryDecisionReferences_Should_ReturnPublishedAndDraftReferences()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var latestQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var draftQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var latestDeploymentMock = new Mock<IDeployment>(MockBehavior.Strict);
            var draftDeploymentMock = new Mock<IDeployment>(MockBehavior.Strict);

            latestDeploymentMock.SetupGet(item => item.Id).Returns("dep-published");
            latestDeploymentMock.SetupGet(item => item.Name).Returns("Leave Published");
            latestDeploymentMock.SetupGet(item => item.TenantId).Returns("tenant-a");
            latestDeploymentMock.SetupGet(item => item.DeploymentTime).Returns(new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));
            draftDeploymentMock.SetupGet(item => item.Id).Returns("dep-draft");
            draftDeploymentMock.SetupGet(item => item.Name).Returns("Leave Draft");
            draftDeploymentMock.SetupGet(item => item.TenantId).Returns("tenant-a");
            draftDeploymentMock.SetupGet(item => item.DeploymentTime).Returns(new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc));

            latestQueryMock.Setup(query => query.SetLatestDeployment()).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.List()).Returns(new List<IDeployment> { latestDeploymentMock.Object });
            draftQueryMock.Setup(query => query.FindDrafts()).Returns(new List<IDeployment> { draftDeploymentMock.Object });

            repositoryServiceMock
                .SetupSequence(service => service.CreateDeploymentQuery())
                .Returns(latestQueryMock.Object)
                .Returns(draftQueryMock.Object);
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

            var controller = CreateController(processEngineMock.Object);
            var result = await controller.QueryDecisionReferences("expense.approval").ConfigureAwait(false);

            Assert.Equal(2, result.Count);
            Assert.Equal("dep-draft", result[0].DeploymentId);
            Assert.True(result[0].IsDraft);
            Assert.Equal("leaveApproval", result[0].ProcessDefinitionKey);
            Assert.Single(result[0].ReferencedTasks);
            Assert.Equal("task_rule", result[0].ReferencedTasks[0].TaskId);
            Assert.Equal("draft-v2", result[0].ReferencedTasks[0].VersionTag);
            Assert.Equal("dep-published", result[1].DeploymentId);
            Assert.False(result[1].IsDraft);
            Assert.Equal("v1.0.0", result[1].ReferencedTasks[0].VersionTag);

            repositoryServiceMock.VerifyAll();
            processEngineMock.VerifyAll();
        }

        /// <summary>
        /// 验证当全部候选部署均已建立索引时，控制器优先从索引返回流程引用。
        /// </summary>
        [Fact]
        public async Task QueryDecisionReferences_Should_ReturnIndexedReferences_WhenCoverageComplete()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var latestQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var draftQueryMock = new Mock<IDeploymentQuery>(MockBehavior.Strict);
            var latestDeploymentMock = new Mock<IDeployment>(MockBehavior.Strict);
            var draftDeploymentMock = new Mock<IDeployment>(MockBehavior.Strict);
            var referenceIndexRepositoryMock = new Mock<IDecisionProcessReferenceIndexRepository>(MockBehavior.Strict);

            latestDeploymentMock.SetupGet(item => item.Id).Returns("dep-published");
            latestDeploymentMock.SetupGet(item => item.Name).Returns("Leave Published");
            latestDeploymentMock.SetupGet(item => item.TenantId).Returns("tenant-a");
            latestDeploymentMock.SetupGet(item => item.DeploymentTime).Returns(new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));
            draftDeploymentMock.SetupGet(item => item.Id).Returns("dep-draft");
            draftDeploymentMock.SetupGet(item => item.Name).Returns("Leave Draft");
            draftDeploymentMock.SetupGet(item => item.TenantId).Returns("tenant-a");
            draftDeploymentMock.SetupGet(item => item.DeploymentTime).Returns(new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc));

            latestQueryMock.Setup(query => query.SetLatestDeployment()).Returns(latestQueryMock.Object);
            latestQueryMock.Setup(query => query.List()).Returns(new List<IDeployment> { latestDeploymentMock.Object });
            draftQueryMock.Setup(query => query.FindDrafts()).Returns(new List<IDeployment> { draftDeploymentMock.Object });

            repositoryServiceMock
                .SetupSequence(service => service.CreateDeploymentQuery())
                .Returns(latestQueryMock.Object)
                .Returns(draftQueryMock.Object);

            referenceIndexRepositoryMock
                .Setup(repository => repository.GetIndexedDeploymentIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(items => items.Count == 2),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "dep-published", "dep-draft" });
            referenceIndexRepositoryMock
                .Setup(repository => repository.QueryEntriesAsync(
                    It.Is<DecisionProcessReferenceIndexQuery>(query =>
                        query.DecisionKey == "expense.approval" &&
                        query.IncludeDrafts &&
                        query.DeploymentIds.Count == 2),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DecisionProcessReferenceIndexEntryRecord>
                {
                    new DecisionProcessReferenceIndexEntryRecord
                    {
                        ReferenceId = "row-1",
                        DeploymentId = "dep-draft",
                        DeploymentName = "Leave Draft",
                        ProcessDefinitionKey = "leaveApproval",
                        ProcessDefinitionName = "请假审批",
                        TenantId = "tenant-a",
                        IsDraft = true,
                        DeployTime = new DateTimeOffset(new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc)),
                        DecisionKey = "expense.approval",
                        TaskId = "task_rule",
                        TaskName = "规则校验",
                        BindingMode = "FixedVersion",
                        VersionTag = "draft-v2",
                        IndexedTime = DateTimeOffset.UtcNow
                    },
                    new DecisionProcessReferenceIndexEntryRecord
                    {
                        ReferenceId = "row-2",
                        DeploymentId = "dep-published",
                        DeploymentName = "Leave Published",
                        ProcessDefinitionKey = "leaveApproval",
                        ProcessDefinitionName = "请假审批",
                        TenantId = "tenant-a",
                        IsDraft = false,
                        DeployTime = new DateTimeOffset(new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc)),
                        DecisionKey = "expense.approval",
                        TaskId = "task_rule",
                        TaskName = "规则校验",
                        BindingMode = "FixedVersion",
                        VersionTag = "v1.0.0",
                        IndexedTime = DateTimeOffset.UtcNow
                    }
                });

            var controller = CreateController(
                processEngineMock.Object,
                decisionProcessReferenceIndexRepository: referenceIndexRepositoryMock.Object);
            var result = await controller.QueryDecisionReferences("expense.approval").ConfigureAwait(false);

            Assert.Equal(2, result.Count);
            Assert.Equal("dep-draft", result[0].DeploymentId);
            Assert.True(result[0].IsDraft);
            Assert.Equal("draft-v2", result[0].ReferencedTasks[0].VersionTag);
            Assert.Equal("dep-published", result[1].DeploymentId);
            Assert.False(result[1].IsDraft);

            repositoryServiceMock.VerifyAll();
            processEngineMock.VerifyAll();
            referenceIndexRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证当流程引用未发布决策版本时，发布接口会返回统一的 Http400 业务异常。
        /// </summary>
        [Fact]
        public async Task Deploy_Should_ThrowHttp400Exception_WhenDecisionVersionIsNotPublished()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var decisionDefinitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var decisionVersionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            decisionDefinitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DecisionDefinitionRecord
                {
                    DecisionId = "decision-1",
                    DecisionKey = "expense.approval",
                    Name = "费用审批",
                    PublishedVersionTag = "v1.0.0"
                });
            decisionVersionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "draft-v2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DecisionVersionRecord
                {
                    VersionId = "version-2",
                    DecisionId = "decision-1",
                    DecisionKey = "expense.approval",
                    VersionTag = "draft-v2",
                    IsPublished = false,
                    IsDraft = true
                });

            var controller = CreateController(
                processEngineMock.Object,
                decisionDefinitionRepository: decisionDefinitionRepositoryMock.Object,
                decisionVersionRepository: decisionVersionRepositoryMock.Object);

            var exception = await Assert.ThrowsAsync<Http400Exception>(() => controller.Deploy(new ProcessDefinitionDeployer
            {
                Name = "Leave Approval",
                Key = "leaveApproval",
                TenantId = "tenant-a",
                BpmnXML = CreateBusinessRuleTaskBpmnXml("expense.approval", "draft-v2")
            })).ConfigureAwait(false);

            Assert.Equal("decision-governance-invalid-state", exception.Http400.Code);
            Assert.Equal(nameof(ProcessDefinitionDeployerController.Deploy), exception.Http400.Target);
            Assert.Contains("未发布的决策版本", exception.Http400.Message);

            processEngineMock.VerifyAll();
            decisionDefinitionRepositoryMock.VerifyAll();
            decisionVersionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证不支持的数据库类型场景会直接返回跳过状态。
        /// </summary>
        [Fact]
        public async Task GetDecisionSchemaStatus_Should_ReturnSkipped_WhenDatabaseTypeIsNotSupported()
        {
            var controller = CreateSchemaStatusController("oracle", ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE);

            var result = await controller.GetDecisionSchemaStatus().ConfigureAwait(false);

            Assert.Equal("Skipped", result.Status);
            Assert.False(result.IsPostgreSql);
            Assert.False(result.IsSqlServer);
            Assert.False(result.DecisionSchemaChecked);
            Assert.False(result.UpgradeRequired);
        }

        /// <summary>
        /// 验证 PostgreSQL 且 schema 完整时返回健康状态。
        /// </summary>
        [Fact]
        public async Task GetDecisionSchemaStatus_Should_ReturnHealthy_WhenSchemaIsComplete()
        {
            var dataSource = CreateDataSource("7.0.0.1");
            var databaseReaderMock = new Mock<IDatabaseReader>(MockBehavior.Strict);
            databaseReaderMock
                .Setup(reader => reader.TableList())
                .Returns(new List<DatabaseTable>
                {
                    new DatabaseTable { Name = "decision_definition" },
                    new DatabaseTable { Name = "decision_version" },
                    new DatabaseTable { Name = "decision_execution_log" },
                    new DatabaseTable { Name = "process_decision_reference" }
                });

            var controller = CreateSchemaStatusController(
                ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES,
                ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE,
                dataSource,
                databaseReaderMock.Object);

            var result = await controller.GetDecisionSchemaStatus().ConfigureAwait(false);

            Assert.Equal("Healthy", result.Status);
            Assert.True(result.IsPostgreSql);
            Assert.True(result.DecisionSchemaChecked);
            Assert.True(result.SchemaVersionMatches);
            Assert.True(result.DecisionSchemaHealthy);
            Assert.False(result.UpgradeRequired);
            Assert.Empty(result.MissingTables);
            databaseReaderMock.VerifyAll();
        }

        /// <summary>
        /// 验证 PostgreSQL 版本落后或扩展表缺失时返回升级要求状态。
        /// </summary>
        [Fact]
        public async Task GetDecisionSchemaStatus_Should_ReturnUpgradeRequired_WhenSchemaIsIncomplete()
        {
            var dataSource = CreateDataSource("7.0.0.0");
            var databaseReaderMock = new Mock<IDatabaseReader>(MockBehavior.Strict);
            databaseReaderMock
                .Setup(reader => reader.TableList())
                .Returns(new List<DatabaseTable>
                {
                    new DatabaseTable { Name = "decision_definition" },
                    new DatabaseTable { Name = "decision_version" }
                });

            var controller = CreateSchemaStatusController(
                ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES,
                ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE,
                dataSource,
                databaseReaderMock.Object);

            var result = await controller.GetDecisionSchemaStatus().ConfigureAwait(false);

            Assert.Equal("UpgradeRequired", result.Status);
            Assert.True(result.IsPostgreSql);
            Assert.True(result.DecisionSchemaChecked);
            Assert.False(result.SchemaVersionMatches);
            Assert.False(result.DecisionSchemaHealthy);
            Assert.True(result.UpgradeRequired);
            Assert.Equal(2, result.MissingTables.Count);
            Assert.Contains("decision_execution_log", result.MissingTables);
            Assert.Contains("process_decision_reference", result.MissingTables);
            databaseReaderMock.VerifyAll();
        }

        /// <summary>
        /// 验证检查过程抛错时返回检查失败状态而不是向上抛异常。
        /// </summary>
        [Fact]
        public async Task GetDecisionSchemaStatus_Should_ReturnInspectionFailed_WhenInspectionThrows()
        {
            var controller = CreateSchemaStatusController(
                ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES,
                ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE);

            var result = await controller.GetDecisionSchemaStatus().ConfigureAwait(false);

            Assert.Equal("InspectionFailed", result.Status);
            Assert.True(result.IsPostgreSql);
            Assert.False(result.DecisionSchemaChecked);
            Assert.Contains("数据源", result.InspectionError);
        }

        /// <summary>
        /// 验证 SQL Server 且 schema 完整时返回健康状态。
        /// </summary>
        [Fact]
        public async Task GetDecisionSchemaStatus_Should_ReturnHealthy_WhenSqlServerSchemaIsComplete()
        {
            var dataSource = CreateDataSource("7.0.0.1");
            var databaseReaderMock = new Mock<IDatabaseReader>(MockBehavior.Strict);
            databaseReaderMock
                .Setup(reader => reader.TableList())
                .Returns(new List<DatabaseTable>
                {
                    new DatabaseTable { Name = "decision_definition" },
                    new DatabaseTable { Name = "decision_version" },
                    new DatabaseTable { Name = "decision_execution_log" },
                    new DatabaseTable { Name = "process_decision_reference" }
                });

            var controller = CreateSchemaStatusController(
                "mssql",
                ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE,
                dataSource,
                databaseReaderMock.Object);

            var result = await controller.GetDecisionSchemaStatus().ConfigureAwait(false);

            Assert.Equal("Healthy", result.Status);
            Assert.False(result.IsPostgreSql);
            Assert.True(result.IsSqlServer);
            Assert.True(result.DecisionSchemaChecked);
            Assert.True(result.SchemaVersionMatches);
            Assert.True(result.DecisionSchemaHealthy);
            Assert.False(result.UpgradeRequired);
            Assert.Empty(result.MissingTables);
            databaseReaderMock.VerifyAll();
        }

        /// <summary>
        /// 创建流程部署控制器测试实例。
        /// </summary>
        /// <param name="processEngine">流程引擎。</param>
        /// <param name="decisionDefinitionRepository">决策定义仓储。</param>
        /// <param name="decisionVersionRepository">决策版本仓储。</param>
        /// <param name="decisionProcessReferenceIndexRepository">决策引用索引仓储。</param>
        /// <param name="dataSource">工作流数据源。</param>
        /// <param name="databaseReader">数据库元数据读取器。</param>
        private static ProcessDefinitionDeployerController CreateController(
            IProcessEngine processEngine,
            IDecisionDefinitionRepository decisionDefinitionRepository = null,
            IDecisionVersionRepository decisionVersionRepository = null,
            IDecisionProcessReferenceIndexRepository decisionProcessReferenceIndexRepository = null,
            IDataSource dataSource = null,
            IDatabaseReader databaseReader = null)
        {
            var deploymentConverter = new DeploymentConverter(new ListConverter());
            var pageableRepositoryService = new PageableDeploymentRespositoryService(
                null,
                null,
                null,
                deploymentConverter,
                null);
            return new ProcessDefinitionDeployerController(
                processEngine,
                deploymentConverter,
                pageableRepositoryService,
                null,
                LoggerFactory.Create(_ => { }),
                decisionDefinitionRepository,
                decisionVersionRepository,
                decisionProcessReferenceIndexRepository,
                dataSource,
                databaseReader);
        }

        /// <summary>
        /// 创建带真实流程配置对象的流程引擎 Mock，避免控制器构造函数读取配置时触发严格 Mock 异常。
        /// </summary>
        /// <param name="repositoryService">流程仓储服务。</param>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <returns>已完成基础 Setup 的流程引擎 Mock。</returns>
        private static Mock<IProcessEngine> CreateProcessEngineMock(
            IRepositoryService repositoryService,
            string databaseType = ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES,
            string databaseSchemaUpdate = ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE)
        {
            var processEngineMock = new Mock<IProcessEngine>(MockBehavior.Strict);
            processEngineMock
                .SetupGet(engine => engine.RepositoryService)
                .Returns(repositoryService);
            processEngineMock
                .SetupGet(engine => engine.ProcessEngineConfiguration)
                .Returns(CreateProcessEngineConfiguration(repositoryService, databaseType, databaseSchemaUpdate));
            return processEngineMock;
        }

        /// <summary>
        /// 创建可供控制器读取数据库类型与 schema 策略的真实流程引擎配置对象。
        /// </summary>
        /// <param name="repositoryService">流程仓储服务。</param>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <returns>流程引擎配置实例。</returns>
        private static ProcessEngineConfigurationImpl CreateProcessEngineConfiguration(
            IRepositoryService repositoryService,
            string databaseType,
            string databaseSchemaUpdate)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            var processEngineConfiguration = new StandaloneProcessEngineConfiguration(
                new Mock<IHistoryService>(MockBehavior.Loose).Object,
                new Mock<ITaskService>(MockBehavior.Loose).Object,
                new Mock<IDynamicBpmnService>(MockBehavior.Loose).Object,
                repositoryService,
                new Mock<IRuntimeService>(MockBehavior.Loose).Object,
                new Mock<IManagementService>(MockBehavior.Loose).Object,
                new Mock<Sys.Workflow.Engine.Impl.Asyncexecutor.IAsyncExecutor>(MockBehavior.Loose).Object,
                configuration);
            typeof(ProcessEngineConfiguration)
                .GetField("databaseType", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(processEngineConfiguration, databaseType);
            processEngineConfiguration.DatabaseSchemaUpdate = databaseSchemaUpdate;
            return processEngineConfiguration;
        }

        /// <summary>
        /// 创建用于 schema 状态查询测试的控制器实例。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <param name="dataSource">工作流数据源。</param>
        /// <param name="databaseReader">数据库元数据读取器。</param>
        /// <returns>流程部署控制器。</returns>
        private static ProcessDefinitionDeployerController CreateSchemaStatusController(
            string databaseType,
            string databaseSchemaUpdate,
            IDataSource dataSource = null,
            IDatabaseReader databaseReader = null)
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Loose);
            var processEngineMock = CreateProcessEngineMock(
                repositoryServiceMock.Object,
                databaseType,
                databaseSchemaUpdate);

            return CreateController(
                processEngineMock.Object,
                dataSource: dataSource,
                databaseReader: databaseReader);
        }

        /// <summary>
        /// 创建返回固定 schema.version 的测试数据源。
        /// </summary>
        /// <param name="schemaVersion">数据库中存储的 schema 版本。</param>
        /// <returns>测试数据源。</returns>
        private static IDataSource CreateDataSource(string schemaVersion)
        {
            var parameterCollectionMock = new Mock<DbParameterCollection>(MockBehavior.Loose);
            var parameterMock = new Mock<DbParameter>(MockBehavior.Loose);
            var commandMock = new Mock<DbCommand>(MockBehavior.Loose);
            var connectionMock = new Mock<DbConnection>(MockBehavior.Loose);
            var dataSourceMock = new Mock<IDataSource>(MockBehavior.Strict);

            parameterCollectionMock
                .Setup(collection => collection.Add(It.IsAny<object>()))
                .Returns(0);
            commandMock
                .Protected()
                .Setup<DbParameterCollection>("DbParameterCollection")
                .Returns(parameterCollectionMock.Object);
            commandMock
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(parameterMock.Object);
            commandMock
                .Setup(command => command.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(schemaVersion);
            connectionMock
                .Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(commandMock.Object);
            connectionMock
                .Setup(connection => connection.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            dataSourceMock
                .Setup(source => source.GetConnection())
                .Returns(connectionMock.Object);

            return dataSourceMock.Object;
        }

        /// <summary>
        /// 为测试创建 BPMN XML 资源流。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        private static Stream CreateBpmnStream(string decisionKey, string versionTag)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(CreateBusinessRuleTaskBpmnXml(decisionKey, versionTag)));
        }

        /// <summary>
        /// 创建带有结构化决策绑定的 BPMN XML。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
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
