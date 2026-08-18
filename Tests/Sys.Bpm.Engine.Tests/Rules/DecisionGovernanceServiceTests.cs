using Moq;
using SmartSql.Abstractions;
using SmartSql.Abstractions.DbSession;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Core.Services;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 定义决策治理服务的隔离测试。
    /// </summary>
    public class DecisionGovernanceServiceTests
    {
        /// <summary>
        /// 最小可解析的 DMN 定义样例。
        /// </summary>
        /// <returns>有效的 DMN XML。</returns>
        private static string CreateValidDmnDefinition()
        {
            return """
                   <?xml version="1.0" encoding="UTF-8"?>
                   <definitions xmlns="http://www.omg.org/spec/DMN/20151101/dmn.xsd" id="definitions_single_root_v1" name="single-root-v1" namespace="https://sys.workflow/rules/tests">
                     <inputData id="input_age" name="Age" />
                     <decision id="decision_age_plus_ten" name="Age +10">
                       <variable id="variable_age10" name="Age10" typeRef="integer" />
                       <informationRequirement>
                         <requiredInput href="#input_age" />
                       </informationRequirement>
                       <literalExpression id="expression_age_plus_ten">
                         <text>Age+10</text>
                       </literalExpression>
                     </decision>
                   </definitions>
                   """;
        }

        /// <summary>
        /// 验证保存草稿时会同步写入定义主记录与草稿版本记录。
        /// </summary>
        [Fact]
        public async Task SaveDraftAsync_Should_SaveDefinitionAndDraftVersion()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync((DecisionDefinitionRecord?)null);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);

            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ReturnsAsync((DecisionVersionRecord record, System.Threading.CancellationToken _) => record);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var result = await service.SaveDraftAsync(new DecisionDraftSaveRequest
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DefinitionContent = "<dmn />",
                VersionTag = "draft-v1",
                DecisionType = "DecisionTable",
                Tags = new List<string> { "expense" },
                Operator = "tester"
            }).ConfigureAwait(false);

            Assert.Equal("Draft", result.Definition.Status);
            Assert.Equal("draft-v1", result.Definition.DraftVersionTag);
            Assert.Equal("draft-v1", result.DraftVersion.VersionTag);
            Assert.True(result.DraftVersion.IsDraft);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionDefinitionRecord>(record =>
                record.DecisionKey == "expense.approval" &&
                record.DraftVersionTag == "draft-v1"), default), Times.AtLeastOnce);
            versionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionVersionRecord>(record =>
                record.DecisionKey == "expense.approval" &&
                record.VersionTag == "draft-v1" &&
                record.IsDraft &&
                !record.IsPublished), default), Times.Once);
        }

        /// <summary>
        /// 验证保存草稿成功时会提交治理事务。
        /// </summary>
        [Fact]
        public async Task SaveDraftAsync_Should_CommitTransaction_WhenSaveSucceeds()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var sqlMapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync((DecisionDefinitionRecord?)null);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);

            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ReturnsAsync((DecisionVersionRecord record, System.Threading.CancellationToken _) => record);

            sqlMapperMock
                .Setup(mapper => mapper.BeginTransaction())
                .Returns(Mock.Of<IDbConnectionSession>());
            sqlMapperMock
                .Setup(mapper => mapper.CommitTransaction());

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object, sqlMapperMock.Object);
            var result = await service.SaveDraftAsync(new DecisionDraftSaveRequest
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DefinitionContent = "<dmn />",
                VersionTag = "draft-v1",
                Operator = "tester"
            }).ConfigureAwait(false);

            Assert.Equal("draft-v1", result.DraftVersion.VersionTag);
            sqlMapperMock.Verify(mapper => mapper.BeginTransaction(), Times.Once);
            sqlMapperMock.Verify(mapper => mapper.CommitTransaction(), Times.Once);
            sqlMapperMock.Verify(mapper => mapper.RollbackTransaction(), Times.Never);
        }

        /// <summary>
        /// 验证保存草稿失败时会回滚治理事务。
        /// </summary>
        [Fact]
        public async Task SaveDraftAsync_Should_RollbackTransaction_WhenVersionSaveFails()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);
            var sqlMapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync((DecisionDefinitionRecord?)null);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);

            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ThrowsAsync(new InvalidOperationException("save failed"));

            sqlMapperMock
                .Setup(mapper => mapper.BeginTransaction())
                .Returns(Mock.Of<IDbConnectionSession>());
            sqlMapperMock
                .Setup(mapper => mapper.RollbackTransaction());

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object, sqlMapperMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveDraftAsync(new DecisionDraftSaveRequest
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DefinitionContent = "<dmn />",
                VersionTag = "draft-v1",
                Operator = "tester"
            })).ConfigureAwait(false);

            Assert.Equal("save failed", exception.Message);
            sqlMapperMock.Verify(mapper => mapper.BeginTransaction(), Times.Once);
            sqlMapperMock.Verify(mapper => mapper.CommitTransaction(), Times.Never);
            sqlMapperMock.Verify(mapper => mapper.RollbackTransaction(), Times.Once);
        }

        /// <summary>
        /// 验证发布当前草稿版本时会清理定义上的草稿标签并更新发布标签。
        /// </summary>
        [Fact]
        public async Task PublishVersionAsync_Should_PublishDraftVersion_AndUpdateDefinition()
        {
            var existingDefinition = new DecisionDefinitionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DraftVersionTag = "draft-v1",
                Status = "Draft"
            };
            var existingVersion = new DecisionVersionRecord
            {
                VersionId = Guid.NewGuid().ToString("D"),
                DecisionId = existingDefinition.DecisionId,
                DecisionKey = existingDefinition.DecisionKey,
                VersionTag = "draft-v1",
                VersionNumber = 1,
                IsDraft = true,
                DefinitionContent = CreateValidDmnDefinition()
            };

            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(existingDefinition);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);

            versionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "draft-v1", default))
                .ReturnsAsync(existingVersion);
            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ReturnsAsync((DecisionVersionRecord record, System.Threading.CancellationToken _) => record);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var result = await service.PublishVersionAsync(new PublishDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                Operator = "publisher",
                PublishComment = "release"
            }).ConfigureAwait(false);

            Assert.Equal("Published", result.Definition.Status);
            Assert.Null(result.Definition.DraftVersionTag);
            Assert.Equal("draft-v1", result.Definition.PublishedVersionTag);
            Assert.True(result.PublishedVersion.IsPublished);
            Assert.False(result.PublishedVersion.IsDraft);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionDefinitionRecord>(record =>
                record.PublishedVersionTag == "draft-v1" &&
                record.DraftVersionTag == null &&
                record.Status == "Published"), default), Times.Once);
        }

        /// <summary>
        /// 验证发布非法 DMN 定义时会在写入前被拦截。
        /// </summary>
        [Fact]
        public async Task PublishVersionAsync_Should_Throw_WhenDmnDefinitionIsInvalid()
        {
            var existingDefinition = new DecisionDefinitionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                Name = "费用审批",
                DraftVersionTag = "draft-v1",
                Status = "Draft",
                SourceFormat = "DMN"
            };
            var existingVersion = new DecisionVersionRecord
            {
                VersionId = Guid.NewGuid().ToString("D"),
                DecisionId = existingDefinition.DecisionId,
                DecisionKey = existingDefinition.DecisionKey,
                VersionTag = "draft-v1",
                VersionNumber = 1,
                IsDraft = true,
                DefinitionContent = "not xml"
            };

            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(existingDefinition);

            versionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "draft-v1", default))
                .ReturnsAsync(existingVersion);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PublishVersionAsync(new PublishDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                Operator = "publisher"
            })).ConfigureAwait(false);

            Assert.Contains("DMN 校验失败", exception.Message);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default), Times.Never);
            versionRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default), Times.Never);
        }

        /// <summary>
        /// 验证回滚指定版本时会重新发布目标版本并更新定义发布标签。
        /// </summary>
        [Fact]
        public async Task RollbackVersionAsync_Should_RepublishTargetVersion()
        {
            var existingDefinition = new DecisionDefinitionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                Name = "费用审批",
                PublishedVersionTag = "v3.0.0",
                Status = "Published"
            };
            var targetVersion = new DecisionVersionRecord
            {
                VersionId = Guid.NewGuid().ToString("D"),
                DecisionId = existingDefinition.DecisionId,
                DecisionKey = existingDefinition.DecisionKey,
                VersionTag = "v2.0.0",
                VersionNumber = 2,
                IsPublished = false,
                DefinitionContent = CreateValidDmnDefinition()
            };

            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(existingDefinition);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);

            versionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "v2.0.0", default))
                .ReturnsAsync(targetVersion);
            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ReturnsAsync((DecisionVersionRecord record, System.Threading.CancellationToken _) => record);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var result = await service.RollbackVersionAsync(new RollbackDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                TargetVersionTag = "v2.0.0",
                Operator = "rollback-user"
            }).ConfigureAwait(false);

            Assert.Equal("Published", result.Definition.Status);
            Assert.Equal("v2.0.0", result.Definition.PublishedVersionTag);
            Assert.Equal("v2.0.0", result.PublishedVersion.VersionTag);
            Assert.True(result.PublishedVersion.IsPublished);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionDefinitionRecord>(record =>
                record.PublishedVersionTag == "v2.0.0" &&
                record.Status == "Published"), default), Times.Once);
        }

        /// <summary>
        /// 验证回滚到非法 DMN 版本时会在重新发布前被拦截。
        /// </summary>
        [Fact]
        public async Task RollbackVersionAsync_Should_Throw_WhenTargetDmnDefinitionIsInvalid()
        {
            var existingDefinition = new DecisionDefinitionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                Name = "费用审批",
                PublishedVersionTag = "v3.0.0",
                Status = "Published",
                SourceFormat = "DMN"
            };
            var targetVersion = new DecisionVersionRecord
            {
                VersionId = Guid.NewGuid().ToString("D"),
                DecisionId = existingDefinition.DecisionId,
                DecisionKey = existingDefinition.DecisionKey,
                VersionTag = "v2.0.0",
                VersionNumber = 2,
                IsPublished = false,
                DefinitionContent = "<definitions />"
            };

            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(existingDefinition);

            versionRepositoryMock
                .Setup(repository => repository.GetByVersionTagAsync("expense.approval", "v2.0.0", default))
                .ReturnsAsync(targetVersion);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RollbackVersionAsync(new RollbackDecisionVersionRequest
            {
                DecisionKey = "expense.approval",
                TargetVersionTag = "v2.0.0",
                Operator = "rollback-user"
            })).ConfigureAwait(false);

            Assert.Contains("DMN 校验失败", exception.Message);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default), Times.Never);
            versionRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default), Times.Never);
        }

        /// <summary>
        /// 验证导入时会保存为草稿并返回导入结果。
        /// </summary>
        [Fact]
        public async Task ImportAsync_Should_SaveDraftAndReturnImportedDecisionKey()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync((DecisionDefinitionRecord?)null);
            definitionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionDefinitionRecord>(), default))
                .ReturnsAsync((DecisionDefinitionRecord record, System.Threading.CancellationToken _) => record);
            versionRepositoryMock
                .Setup(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default))
                .ReturnsAsync((DecisionVersionRecord record, System.Threading.CancellationToken _) => record);

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var result = await service.ImportAsync(new DecisionImportRequest
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                Content = CreateValidDmnDefinition(),
                VersionTag = "draft-v1",
                Operator = "tester"
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.Single(result.ImportedDecisionKeys);
            Assert.Equal("expense.approval", result.ImportedDecisionKeys[0]);
            definitionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionDefinitionRecord>(record =>
                record.DecisionKey == "expense.approval" &&
                record.DraftVersionTag == "draft-v1"), default), Times.AtLeastOnce);
            versionRepositoryMock.Verify(repository => repository.SaveAsync(It.Is<DecisionVersionRecord>(record =>
                record.DecisionKey == "expense.approval" &&
                record.DefinitionContent == CreateValidDmnDefinition()), default), Times.Once);
        }

        /// <summary>
        /// 验证导入已存在定义且未允许覆盖时会抛出业务异常。
        /// </summary>
        [Fact]
        public async Task ImportAsync_Should_Throw_WhenDefinitionExistsAndOverwriteDisabled()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            definitionRepositoryMock
                .Setup(repository => repository.GetByDecisionKeyAsync("expense.approval", default))
                .ReturnsAsync(new DecisionDefinitionRecord
                {
                    DecisionId = "decision-1",
                    DecisionKey = "expense.approval",
                    Name = "费用审批"
                });

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportAsync(new DecisionImportRequest
            {
                DecisionKey = "expense.approval",
                Content = CreateValidDmnDefinition()
            })).ConfigureAwait(false);

            Assert.Contains("已存在", exception.Message);
            definitionRepositoryMock.VerifyAll();
            versionRepositoryMock.Verify(repository => repository.SaveAsync(It.IsAny<DecisionVersionRecord>(), default), Times.Never);
        }

        /// <summary>
        /// 验证未指定版本标签时会导出最新已发布版本。
        /// </summary>
        [Fact]
        public async Task ExportAsync_Should_ReturnLatestPublishedVersion_WhenVersionTagIsEmpty()
        {
            var definitionRepositoryMock = new Mock<IDecisionDefinitionRepository>(MockBehavior.Strict);
            var versionRepositoryMock = new Mock<IDecisionVersionRepository>(MockBehavior.Strict);

            versionRepositoryMock
                .Setup(repository => repository.GetLatestPublishedAsync("expense.approval", default))
                .ReturnsAsync(new DecisionVersionRecord
                {
                    DecisionKey = "expense.approval",
                    VersionTag = "v1.0.0",
                    DefinitionContent = CreateValidDmnDefinition(),
                    Checksum = "abc123"
                });

            var service = new DecisionGovernanceService(definitionRepositoryMock.Object, versionRepositoryMock.Object);
            var result = await service.ExportAsync(new DecisionExportRequest
            {
                DecisionKey = "expense.approval"
            }).ConfigureAwait(false);

            Assert.Equal("expense.approval.v1.0.0.dmn", result.FileName);
            Assert.Equal("application/xml", result.ContentType);
            Assert.Equal("abc123", result.Checksum);
            Assert.Equal(CreateValidDmnDefinition(), result.Content);
            versionRepositoryMock.VerifyAll();
        }
    }
}
