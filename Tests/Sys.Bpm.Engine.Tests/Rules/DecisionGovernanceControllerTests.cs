using Moq;
using Sys.Net.Http;
using Sys.Workflow.Cloud.Services.Rest.Controllers;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Sys.Workflow.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 规则治理控制器隔离测试。
    /// </summary>
    public class DecisionGovernanceControllerTests
    {
        /// <summary>
        /// 验证保存草稿接口会转发到治理服务。
        /// </summary>
        [Fact]
        public async Task SaveDraft_Should_DelegateToGovernanceService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var expectedResult = new DecisionDraftSaveResult
            {
                Definition = new DecisionDefinitionRecord
                {
                    DecisionKey = "expense.approval",
                    DraftVersionTag = "draft-v1"
                },
                DraftVersion = new DecisionVersionRecord
                {
                    DecisionKey = "expense.approval",
                    VersionTag = "draft-v1",
                    IsDraft = true
                }
            };

            governanceServiceMock
                .Setup(service => service.SaveDraftAsync(It.IsAny<DecisionDraftSaveRequest>(), default))
                .ReturnsAsync(expectedResult);

            var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
            var result = await controller.SaveDraft(new DecisionDraftSaveRequest
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DefinitionContent = "<dmn />"
            }).ConfigureAwait(false);

            Assert.Equal("expense.approval", result.Definition.DecisionKey);
            Assert.Equal("draft-v1", result.DraftVersion.VersionTag);
            governanceServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证保存草稿时若未显式传入操作人，则从认证上下文自动回填。
        /// </summary>
        [Fact]
        public async Task SaveDraft_Should_PopulateOperatorFromAuthentication_WhenOperatorIsEmpty()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);

            governanceServiceMock
                .Setup(service => service.SaveDraftAsync(It.Is<DecisionDraftSaveRequest>(request =>
                    request.DecisionKey == "expense.approval" &&
                    request.Operator == "tester"), default))
                .ReturnsAsync(new DecisionDraftSaveResult
                {
                    Definition = new DecisionDefinitionRecord
                    {
                        DecisionKey = "expense.approval"
                    },
                    DraftVersion = new DecisionVersionRecord
                    {
                        DecisionKey = "expense.approval",
                        VersionTag = "draft-v1"
                    }
                });

            Authentication.AuthenticatedUser = new UserInfo
            {
                Id = "tester"
            };

            try
            {
                var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
                var result = await controller.SaveDraft(new DecisionDraftSaveRequest
                {
                    DecisionKey = "expense.approval",
                    Name = "费用审批",
                    DefinitionContent = "<dmn />"
                }).ConfigureAwait(false);

                Assert.Equal("expense.approval", result.Definition.DecisionKey);
                governanceServiceMock.VerifyAll();
            }
            finally
            {
                Authentication.AuthenticatedUser = null;
            }
        }

        /// <summary>
        /// 验证保存草稿接口遇到参数异常时会包装为统一的 Http400 业务异常。
        /// </summary>
        [Fact]
        public async Task SaveDraft_Should_ThrowHttp400Exception_WhenGovernanceServiceThrowsArgumentException()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);

            governanceServiceMock
                .Setup(service => service.SaveDraftAsync(It.IsAny<DecisionDraftSaveRequest>(), default))
                .ThrowsAsync(new ArgumentException("decisionKey is required"));

            var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
            var exception = await Assert.ThrowsAsync<Http400Exception>(() => controller.SaveDraft(new DecisionDraftSaveRequest
            {
                DecisionKey = string.Empty,
                Name = "费用审批",
                DefinitionContent = "<dmn />"
            })).ConfigureAwait(false);

            Assert.Equal("decision-governance-invalid-request", exception.Http400.Code);
            Assert.Equal(nameof(DecisionGovernanceControllerImpl.SaveDraft), exception.Http400.Target);
            Assert.Equal("ArgumentException", exception.Http400.OriginError);
            Assert.Equal("decisionKey is required", exception.Http400.Message);
            governanceServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证发布接口会转发到治理服务。
        /// </summary>
        [Fact]
        public async Task PublishVersion_Should_DelegateToGovernanceService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var expectedResult = new DecisionVersionPublishResult
            {
                Definition = new DecisionDefinitionRecord
                {
                    DecisionKey = "expense.approval",
                    PublishedVersionTag = "v1.0.0",
                    Status = "Published"
                },
                PublishedVersion = new DecisionVersionRecord
                {
                    DecisionKey = "expense.approval",
                    VersionTag = "v1.0.0",
                    IsPublished = true
                }
            };

            governanceServiceMock
                .Setup(service => service.PublishVersionAsync(It.IsAny<PublishDecisionVersionRequest>(), default))
                .ReturnsAsync(expectedResult);

            var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
            var result = await controller.PublishVersion(new PublishDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                VersionTag = "v1.0.0"
            }).ConfigureAwait(false);

            Assert.Equal("v1.0.0", result.Definition.PublishedVersionTag);
            Assert.True(result.PublishedVersion.IsPublished);
            governanceServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证发布接口遇到非法状态时会包装为统一的 Http400 业务异常。
        /// </summary>
        [Fact]
        public async Task PublishVersion_Should_ThrowHttp400Exception_WhenGovernanceServiceThrowsInvalidOperationException()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);

            governanceServiceMock
                .Setup(service => service.PublishVersionAsync(It.IsAny<PublishDecisionVersionRequest>(), default))
                .ThrowsAsync(new InvalidOperationException("draft version not found"));

            var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
            var exception = await Assert.ThrowsAsync<Http400Exception>(() => controller.PublishVersion(new PublishDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                VersionTag = "v1.0.0"
            })).ConfigureAwait(false);

            Assert.Equal("decision-governance-invalid-state", exception.Http400.Code);
            Assert.Equal(nameof(DecisionGovernanceControllerImpl.PublishVersion), exception.Http400.Target);
            Assert.Equal("InvalidOperationException", exception.Http400.OriginError);
            Assert.Equal("draft version not found", exception.Http400.Message);
            governanceServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证执行日志查询接口会转发到日志仓储并返回分页结果。
        /// </summary>
        [Fact]
        public async Task QueryExecutionLogs_Should_DelegateToLogRepository_AndReturnPagedResources()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var expectedLogs = new List<DecisionExecutionLogRecord>
            {
                new DecisionExecutionLogRecord
                {
                    ExecutionId = Guid.NewGuid().ToString("D"),
                    DecisionKey = "expense.approval",
                    Succeeded = true
                }
            };

            executionLogRepositoryMock
                .Setup(repository => repository.QueryAsync(It.IsAny<DecisionExecutionLogQuery>(), default))
                .ReturnsAsync(expectedLogs);
            executionLogRepositoryMock
                .Setup(repository => repository.CountAsync(It.IsAny<DecisionExecutionLogQuery>(), default))
                .ReturnsAsync(3L);

            var controller = new DecisionGovernanceControllerImpl(governanceServiceMock.Object, executionLogRepositoryMock.Object);
            var result = await controller.QueryExecutionLogs(new DecisionExecutionLogQuery
            {
                DecisionKey = "expense.approval",
                PageIndex = 2,
                PageSize = 1
            }).ConfigureAwait(false);

            Assert.IsType<Resources<DecisionExecutionLogRecord>>(result);
            Assert.Single(result.List);
            Assert.Equal("expense.approval", Assert.Single(result.List).DecisionKey);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.PageNo);
            Assert.Equal(1, result.PageSize);
            executionLogRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证协同能力描述接口会返回稳定的宿主协同约定。
        /// </summary>
        [Fact]
        public async Task GetCollaborationDescriptor_Should_ReturnStableContract()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object);
            var result = await controller.GetCollaborationDescriptor().ConfigureAwait(false);

            Assert.Equal("/dmn", result.WorkspaceRoute);
            Assert.Equal("picker", result.WorkspacePickerQueryKey);
            Assert.Equal("qhflow.decision-workspace", result.WorkspaceChannelEventName);
            Assert.Contains("qhflow.decision-selected", result.WorkspaceMessageTypes);
            Assert.Contains("workflow.decision.workspace.changed", result.WorkspaceBusEvents);
            Assert.True(result.CapabilityFlags["workspacePicker"]);
            Assert.True(result.CapabilityFlags["impactAnalysis"]);
        }

        /// <summary>
        /// 验证决策定义目录查询接口会转发到定义仓储。
        /// </summary>
        [Fact]
        public async Task QueryDefinitions_Should_DelegateToDefinitionRepository()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var expectedDefinitions = new List<DecisionDefinitionRecord>
            {
                new DecisionDefinitionRecord
                {
                    DecisionKey = "expense.approval",
                    Name = "费用审批",
                    Status = "Published"
                }
            };

            definitionRepositoryMock
                .Setup(repository => repository.QueryAsync(It.Is<DecisionDefinitionRepositoryQuery>(query =>
                    query.Keyword == "expense" &&
                    query.PageIndex == 1 &&
                    query.PageSize == 20), default))
                .ReturnsAsync(expectedDefinitions);

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object);
            var result = await controller.QueryDefinitions(new DecisionDefinitionRepositoryQuery
            {
                Keyword = "expense",
                PageIndex = 1,
                PageSize = 20
            }).ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("expense.approval", result[0].DecisionKey);
            definitionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证版本目录查询接口会转发到版本仓储。
        /// </summary>
        [Fact]
        public async Task QueryDecisionVersions_Should_DelegateToVersionRepository()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var expectedVersions = new List<DecisionVersionRecord>
            {
                new DecisionVersionRecord
                {
                    DecisionKey = "expense.approval",
                    VersionTag = "v1.0.0",
                    IsPublished = true
                }
            };

            versionRepositoryMock
                .Setup(repository => repository.QueryByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(expectedVersions);

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object);
            var result = await controller.QueryDecisionVersions("expense.approval").ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("v1.0.0", result[0].VersionTag);
            versionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证定义详情接口会转发到定义仓储。
        /// </summary>
        [Fact]
        public async Task GetDefinition_Should_DelegateToDefinitionRepository()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(new DecisionDefinitionRecord
                {
                    DecisionKey = "expense.approval",
                    Name = "费用审批"
                });

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object);
            var result = await controller.GetDefinition("expense.approval").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("费用审批", result!.Name);
            definitionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证导入接口会转发到规则运行时服务。
        /// </summary>
        [Fact]
        public async Task Import_Should_DelegateToDecisionRuntimeService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var runtimeServiceMock = new Mock<IDecisionRuntimeService>(MockBehavior.Strict);

            runtimeServiceMock
                .Setup(service => service.ImportAsync(It.Is<DecisionImportRequest>(request =>
                    request.FileName == "expense.approval.dmn" &&
                    request.Operator == "tester"), default))
                .ReturnsAsync(new DecisionImportResult
                {
                    Succeeded = true,
                    ImportedDecisionKeys = new List<string> { "expense.approval" }
                });

            Authentication.AuthenticatedUser = new UserInfo { Id = "tester" };
            try
            {
                var controller = new DecisionGovernanceControllerImpl(
                    governanceServiceMock.Object,
                    executionLogRepositoryMock.Object,
                    decisionRuntimeService: runtimeServiceMock.Object);
                var result = await controller.Import(new DecisionImportRequest
                {
                    FileName = "expense.approval.dmn",
                    Content = "<definitions />"
                }).ConfigureAwait(false);

                Assert.True(result.Succeeded);
                Assert.Contains("expense.approval", result.ImportedDecisionKeys);
                runtimeServiceMock.VerifyAll();
            }
            finally
            {
                Authentication.AuthenticatedUser = null;
            }
        }

        /// <summary>
        /// 验证缓存状态接口会转发到规则运行时服务。
        /// </summary>
        [Fact]
        public async Task GetCacheStatus_Should_DelegateToDecisionRuntimeService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var runtimeServiceMock = new Mock<IDecisionRuntimeService>(MockBehavior.Strict);

            runtimeServiceMock
                .Setup(service => service.GetCacheStatusAsync("expense.approval", default))
                .ReturnsAsync(new DecisionCacheStatusDescriptor
                {
                    DecisionKey = "expense.approval",
                    CachedVersions = new List<string> { "v1.0.0" },
                    CacheProvider = "test"
                });

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                decisionRuntimeService: runtimeServiceMock.Object);
            var result = await controller.GetCacheStatus("expense.approval").ConfigureAwait(false);

            Assert.Equal("expense.approval", result.DecisionKey);
            Assert.Contains("v1.0.0", result.CachedVersions);
            runtimeServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证解释接口会转发到规则运行时服务。
        /// </summary>
        [Fact]
        public async Task Explain_Should_DelegateToDecisionRuntimeService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var runtimeServiceMock = new Mock<IDecisionRuntimeService>(MockBehavior.Strict);

            runtimeServiceMock
                .Setup(service => service.ExplainAsync(It.Is<DecisionExplanationRequest>(request =>
                    request.DecisionKey == "expense.approval"), default))
                .ReturnsAsync(new DecisionExplanationResult
                {
                    DecisionKey = "expense.approval",
                    ExecutionId = "exec-1",
                    EvaluationSteps = new List<string> { "step-1" }
                });

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                decisionRuntimeService: runtimeServiceMock.Object);
            var result = await controller.Explain(new DecisionExplanationRequest
            {
                DecisionKey = "expense.approval"
            }).ConfigureAwait(false);

            Assert.Equal("expense.approval", result.DecisionKey);
            Assert.Equal("exec-1", result.ExecutionId);
            runtimeServiceMock.VerifyAll();
        }

        /// <summary>
        /// 验证测试接口会转发到规则运行时服务。
        /// </summary>
        [Fact]
        public async Task Test_Should_DelegateToDecisionRuntimeService()
        {
            var governanceServiceMock = new Mock<IDecisionGovernanceService>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var runtimeServiceMock = new Mock<IDecisionRuntimeService>(MockBehavior.Strict);

            runtimeServiceMock
                .Setup(service => service.TestAsync(It.Is<DecisionTestRequest>(request =>
                    request.DecisionKey == "expense.approval"), default))
                .ReturnsAsync(new DecisionTestResult
                {
                    Passed = true,
                    TotalCases = 1,
                    PassedCases = 1,
                    FailedCases = 0
                });

            var controller = new DecisionGovernanceControllerImpl(
                governanceServiceMock.Object,
                executionLogRepositoryMock.Object,
                decisionRuntimeService: runtimeServiceMock.Object);
            var result = await controller.Test(new DecisionTestRequest
            {
                DecisionKey = "expense.approval"
            }).ConfigureAwait(false);

            Assert.True(result.Passed);
            Assert.Equal(1, result.TotalCases);
            runtimeServiceMock.VerifyAll();
        }
    }
}
