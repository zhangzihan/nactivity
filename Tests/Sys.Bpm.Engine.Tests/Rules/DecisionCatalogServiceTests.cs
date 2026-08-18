using Moq;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Core.Services;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 定义决策目录服务的隔离测试。
    /// </summary>
    public class DecisionCatalogServiceTests
    {
        /// <summary>
        /// 验证索引覆盖不完整时返回空以便上层回退扫描。
        /// </summary>
        [Fact]
        public async Task TryQueryDecisionReferencesAsync_Should_ReturnNull_WhenIndexCoverageIsIncomplete()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var indexRepositoryMock = new Mock<IDecisionProcessReferenceIndexRepository>(MockBehavior.Strict);

            indexRepositoryMock
                .Setup(repository => repository.GetIndexedDeploymentIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(items => items.Count == 2),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "dep-1" });

            var service = new DecisionCatalogService(
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object,
                executionLogRepositoryMock.Object,
                indexRepositoryMock.Object);
            var result = await service.TryQueryDecisionReferencesAsync(new DecisionProcessReferenceCatalogQuery
            {
                DecisionKey = "expense.approval",
                DeploymentIds = new[] { "dep-1", "dep-2" }
            }).ConfigureAwait(false);

            Assert.Null(result);
            indexRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证校验未发布版本时会抛出业务异常。
        /// </summary>
        [Fact]
        public async Task ValidateDecisionBindingsAsync_Should_Throw_WhenVersionIsNotPublished()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(new DecisionDefinitionRecord
                {
                    DecisionId = "decision-1",
                    DecisionKey = "expense.approval",
                    Name = "费用审批",
                    PublishedVersionTag = "v1.0.0"
                });
            versionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "draft-v2", default))
                .ReturnsAsync(new DecisionVersionRecord
                {
                    DecisionId = "decision-1",
                    DecisionKey = "expense.approval",
                    VersionTag = "draft-v2",
                    IsPublished = false
                });

            var service = new DecisionCatalogService(
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object,
                executionLogRepositoryMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateDecisionBindingsAsync(
                new DecisionBindingValidationRequest
                {
                    Bindings = new List<DecisionBindingReference>
                    {
                        new DecisionBindingReference
                        {
                            SourceId = "task_rule",
                            DecisionKey = "expense.approval",
                            BindingMode = "FixedVersion",
                            VersionTag = "draft-v2"
                        }
                    }
                })).ConfigureAwait(false);

            Assert.Contains("未发布的决策版本", exception.Message);
            definitionRepositoryMock.VerifyAll();
            versionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// 验证查询执行日志时会返回分页结果。
        /// </summary>
        [Fact]
        public async Task QueryExecutionLogsAsync_Should_ReturnPagedResult()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var executionLogRepositoryMock = new Mock<IDecisionExecutionLogRepository>(MockBehavior.Strict);
            var query = new DecisionExecutionLogQuery
            {
                DecisionKey = "expense.approval",
                PageIndex = 2,
                PageSize = 10
            };

            executionLogRepositoryMock
                .Setup(repository => repository.QueryAsync(query, default))
                .ReturnsAsync(new List<DecisionExecutionLogRecord>
                {
                    new DecisionExecutionLogRecord
                    {
                        ExecutionId = "exec-1",
                        DecisionKey = "expense.approval",
                        ExecutedTime = new DateTimeOffset(2026, 7, 7, 10, 0, 0, TimeSpan.Zero)
                    }
                });
            executionLogRepositoryMock
                .Setup(repository => repository.CountAsync(query, default))
                .ReturnsAsync(21);

            var service = new DecisionCatalogService(
                definitionRepositoryMock.Object,
                versionRepositoryMock.Object,
                executionLogRepositoryMock.Object);
            var result = await service.QueryExecutionLogsAsync(query).ConfigureAwait(false);

            Assert.Single(result.Items);
            Assert.Equal(21, result.TotalCount);
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(10, result.PageSize);
            executionLogRepositoryMock.VerifyAll();
        }
    }
}
