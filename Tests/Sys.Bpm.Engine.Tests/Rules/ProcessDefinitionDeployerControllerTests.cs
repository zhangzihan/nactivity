using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using Sys.Workflow.Engine.Exceptions;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Rules;
using Sys.Workflow.Validation;
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
    public class ProcessDefinitionDeployerControllerTests : IDisposable
    {
        private readonly PropertyInfo _serviceProviderProperty;
        private readonly IServiceProvider? _originalServiceProvider;

        /// <summary>
        /// 初始化测试实例并隔离静态服务提供器。
        /// </summary>
        public ProcessDefinitionDeployerControllerTests()
        {
            _serviceProviderProperty = typeof(ProcessEngineServiceProvider).GetProperty(
                "ServiceProvider",
                BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("未找到 ProcessEngineServiceProvider.ServiceProvider 属性。");
            _originalServiceProvider = (IServiceProvider?)_serviceProviderProperty.GetValue(null);

            var services = new ServiceCollection();
            services.AddLogging();
            _serviceProviderProperty.SetValue(null, services.BuildServiceProvider());
        }

        /// <summary>
        /// 还原测试前的静态服务提供器。
        /// </summary>
        public void Dispose()
        {
            _serviceProviderProperty.SetValue(null, _originalServiceProvider);
        }

        /// <summary>
        /// 验证控制器会通过应用服务返回指定决策的流程引用。
        /// </summary>
        [Fact]
        public async Task QueryDecisionReferences_Should_DelegateToApplicationService()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var decisionReferenceIndexAppServiceMock = new Mock<IDecisionProcessReferenceIndexApplicationService>(MockBehavior.Strict);

            decisionReferenceIndexAppServiceMock
                .Setup(service => service.QueryDecisionReferencesAsync(
                    "expense.approval",
                    "tenant-a",
                    true,
                    default))
                .ReturnsAsync(new List<DecisionProcessReferenceRecord>
                {
                    new DecisionProcessReferenceRecord
                    {
                        DeploymentId = "dep-draft",
                        DeploymentName = "Leave Draft",
                        ProcessDefinitionKey = "leaveApproval",
                        ProcessDefinitionName = "请假审批",
                        TenantId = "tenant-a",
                        IsDraft = true,
                        DeployTime = new DateTime(2026, 7, 7, 11, 0, 0, DateTimeKind.Utc),
                        ReferencedTasks = new List<DecisionProcessReferenceTaskRecord>
                        {
                            new DecisionProcessReferenceTaskRecord
                            {
                                TaskId = "task_rule",
                                DecisionKey = "expense.approval",
                                VersionTag = "draft-v2"
                            }
                        }
                    }
                });

            var controller = CreateController(
                processEngineMock.Object,
                decisionProcessReferenceIndexApplicationService: decisionReferenceIndexAppServiceMock.Object);
            var result = await controller.QueryDecisionReferences("expense.approval", "tenant-a").ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("dep-draft", result[0].DeploymentId);
            Assert.True(result[0].IsDraft);
            Assert.Equal("draft-v2", result[0].ReferencedTasks[0].VersionTag);
            decisionReferenceIndexAppServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证当流程引用未发布决策版本时，发布接口会返回统一的 Http400 业务异常。
        /// </summary>
        [Fact]
        public async Task Deploy_Should_ThrowHttp400Exception_WhenDecisionVersionIsNotPublished()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var deploymentBuilderMock = new Mock<IDeploymentBuilder>(MockBehavior.Strict);

            repositoryServiceMock
                .Setup(service => service.CreateDeployment())
                .Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.DisableSchemaValidation()).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.EnableDuplicateFiltering()).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Name("Leave Approval")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Category(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Key("leaveApproval")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.TenantId("tenant-a")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.BusinessKey(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.BusinessPath(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.StartForm(null, It.IsAny<string>())).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.AddString("Leave Approval.bpmn", It.IsAny<string>())).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock
                .Setup(builder => builder.Deploy())
                .Throws(new ActivitiValidationException(new List<ValidationError>
                {
                    new ValidationError
                    {
                        ActivityName = "task_rule",
                        DefaultDescription = "规则任务 task_rule 引用了未发布的决策版本 expense.approval:draft-v2。"
                    }
                }));

            var controller = CreateController(processEngineMock.Object);

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
            repositoryServiceMock.VerifyAll();
            deploymentBuilderMock.VerifyAll();
        }

        /// <summary>
        /// 验证部署成功后会触发决策引用索引同步。
        /// </summary>
        [Fact]
        public async Task Deploy_Should_SyncDecisionReferenceIndex_WhenDeploymentSucceeds()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var deploymentBuilderMock = new Mock<IDeploymentBuilder>(MockBehavior.Strict);
            var deploymentMock = new Mock<IDeployment>(MockBehavior.Strict);
            var decisionReferenceIndexAppServiceMock = new Mock<IDecisionProcessReferenceIndexApplicationService>(MockBehavior.Strict);

            deploymentMock.SetupGet(item => item.Id).Returns("dep-1");
            deploymentMock.SetupGet(item => item.Name).Returns("Leave Approval");
            deploymentMock.SetupGet(item => item.Category).Returns(string.Empty);
            deploymentMock.SetupGet(item => item.TenantId).Returns("tenant-a");
            deploymentMock.SetupGet(item => item.BusinessKey).Returns(string.Empty);
            deploymentMock.SetupGet(item => item.DeploymentTime).Returns(new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));

            repositoryServiceMock
                .Setup(service => service.CreateDeployment())
                .Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.DisableSchemaValidation()).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.EnableDuplicateFiltering()).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Name("Leave Approval")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Category(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Key("leaveApproval")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.TenantId("tenant-a")).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.BusinessKey(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.BusinessPath(null)).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.StartForm(null, It.IsAny<string>())).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.AddString("Leave Approval.bpmn", It.IsAny<string>())).Returns(deploymentBuilderMock.Object);
            deploymentBuilderMock.Setup(builder => builder.Deploy()).Returns(deploymentMock.Object);
            decisionReferenceIndexAppServiceMock
                .Setup(service => service.TrySyncDeploymentReferencesAsync(
                    deploymentMock.Object,
                    It.Is<string>(xml => xml.Contains("expense.approval")),
                    false,
                    default))
                .Returns(Task.CompletedTask);

            var controller = CreateController(
                processEngineMock.Object,
                decisionProcessReferenceIndexApplicationService: decisionReferenceIndexAppServiceMock.Object);
            var result = await controller.Deploy(new ProcessDefinitionDeployer
            {
                Name = "Leave Approval",
                Key = "leaveApproval",
                TenantId = "tenant-a",
                BpmnXML = CreateBusinessRuleTaskBpmnXml("expense.approval", "v1.0.0")
            }).ConfigureAwait(false);

            Assert.Equal("dep-1", result.Id);
            repositoryServiceMock.VerifyAll();
            deploymentBuilderMock.VerifyAll();
            decisionReferenceIndexAppServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证删除部署前会尝试清理决策引用索引。
        /// </summary>
        [Fact]
        public async Task Remove_Should_DeleteDecisionReferenceIndex_WhenDeploymentExists()
        {
            var repositoryServiceMock = new Mock<IRepositoryService>(MockBehavior.Strict);
            var processEngineMock = CreateProcessEngineMock(repositoryServiceMock.Object);
            var decisionReferenceIndexAppServiceMock = new Mock<IDecisionProcessReferenceIndexApplicationService>(MockBehavior.Strict);

            decisionReferenceIndexAppServiceMock
                .Setup(service => service.TryDeleteDeploymentReferencesAsync("dep-1", default))
                .Returns(Task.CompletedTask);
            repositoryServiceMock
                .Setup(service => service.DeleteDeployment("dep-1"));

            var controller = CreateController(
                processEngineMock.Object,
                decisionProcessReferenceIndexApplicationService: decisionReferenceIndexAppServiceMock.Object);
            var result = await controller.Remove("dep-1").ConfigureAwait(false);

            Assert.True(result);
            repositoryServiceMock.VerifyAll();
            decisionReferenceIndexAppServiceMock.VerifyAll();
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
        /// <param name="decisionProcessReferenceIndexApplicationService">决策引用索引应用服务。</param>
        /// <param name="dataSource">工作流数据源。</param>
        /// <param name="databaseReader">数据库元数据读取器。</param>
        private static ProcessDefinitionDeployerController CreateController(
            IProcessEngine processEngine,
            IDecisionProcessReferenceIndexApplicationService decisionProcessReferenceIndexApplicationService = null,
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
                decisionProcessReferenceIndexApplicationService,
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
