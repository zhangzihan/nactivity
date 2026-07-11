using Moq;
using SmartSql.Abstractions;
using Sys.Workflow.Rules.Core.Repositories;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 定义通用规则仓储写入行为的回归测试。
    /// </summary>
    public class DecisionRepositoriesTests
    {
        /// <summary>
        /// 验证决策定义不存在时会走插入路径。
        /// </summary>
        [Fact]
        public async Task SaveDefinitionAsync_Should_Insert_WhenDefinitionDoesNotExist()
        {
            var mapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionDefinitionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionDefinitionRepository" &&
                    context.SqlId == "GetByDecisionKey")))
                .ReturnsAsync(Array.Empty<DecisionDefinitionRecord>());
            mapperMock
                .Setup(mapper => mapper.ExecuteAsync(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionDefinitionRepository" &&
                    context.SqlId == "InsertDefinition" &&
                    context.DataSourceChoice == DataSourceChoice.Write)))
                .ReturnsAsync(1);

            var repository = new DecisionDefinitionRepository(mapperMock.Object);
            var record = new DecisionDefinitionRecord
            {
                DecisionKey = "expense.approval",
                Name = "费用审批",
                Tags = new List<string> { "expense", "approval" },
                CreatedBy = "tester"
            };

            var savedRecord = await repository.SaveAsync(record).ConfigureAwait(false);

            Assert.False(string.IsNullOrWhiteSpace(savedRecord.DecisionId));
            Assert.Equal("DMN", savedRecord.SourceFormat);
            Assert.Equal("Draft", savedRecord.Status);
            Assert.Equal(savedRecord.CreatedTime, record.CreatedTime);
            Assert.True(savedRecord.ModifiedTime >= savedRecord.CreatedTime);
            mapperMock.VerifyAll();
        }

        /// <summary>
        /// 验证已存在的决策定义会走更新路径并保留创建元数据。
        /// </summary>
        [Fact]
        public async Task SaveDefinitionAsync_Should_Update_WhenDefinitionAlreadyExists()
        {
            var existingRecord = new DecisionDefinitionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                Name = "费用审批",
                CreatedBy = "seed-user",
                CreatedTime = new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero)
            };

            var mapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionDefinitionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionDefinitionRepository" &&
                    context.SqlId == "GetByDecisionKey")))
                .ReturnsAsync(new[] { existingRecord });
            mapperMock
                .Setup(mapper => mapper.ExecuteAsync(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionDefinitionRepository" &&
                    context.SqlId == "UpdateDefinition" &&
                    context.DataSourceChoice == DataSourceChoice.Write)))
                .ReturnsAsync(1);

            var repository = new DecisionDefinitionRepository(mapperMock.Object);
            var record = new DecisionDefinitionRecord
            {
                DecisionKey = "expense.approval",
                Name = "费用审批-更新",
                ModifiedBy = "editor"
            };

            var savedRecord = await repository.SaveAsync(record).ConfigureAwait(false);

            Assert.Equal(existingRecord.DecisionId, savedRecord.DecisionId);
            Assert.Equal(existingRecord.CreatedBy, savedRecord.CreatedBy);
            Assert.Equal(existingRecord.CreatedTime, savedRecord.CreatedTime);
            Assert.Equal("editor", savedRecord.ModifiedBy);
            mapperMock.VerifyAll();
        }

        /// <summary>
        /// 验证新增草稿版本时会清理旧草稿标记并分配下一个版本号。
        /// </summary>
        [Fact]
        public async Task SaveVersionAsync_Should_InsertDraftVersion_AndClearPreviousDraftFlags()
        {
            var executedSqlIds = new List<string>();
            var mapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionVersionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository" &&
                    context.SqlId == "GetByVersionTag")))
                .ReturnsAsync(Array.Empty<DecisionVersionRecord>());
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionVersionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository" &&
                    context.SqlId == "GetDraftVersion")))
                .ReturnsAsync(Array.Empty<DecisionVersionRecord>());
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionVersionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository" &&
                    context.SqlId == "QueryByDecisionKey")))
                .ReturnsAsync(new[]
                {
                    new DecisionVersionRecord { VersionNumber = 2 },
                    new DecisionVersionRecord { VersionNumber = 1 }
                });
            mapperMock
                .Setup(mapper => mapper.ExecuteAsync(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository")))
                .Callback<RequestContext>(context => executedSqlIds.Add(context.SqlId))
                .ReturnsAsync(1);

            var repository = new DecisionVersionRepository(mapperMock.Object);
            var record = new DecisionVersionRecord
            {
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                VersionTag = "v3.0.0-draft",
                IsDraft = true,
                DefinitionContent = "<dmn />",
                CreatedBy = "tester"
            };

            var savedRecord = await repository.SaveAsync(record).ConfigureAwait(false);

            Assert.False(string.IsNullOrWhiteSpace(savedRecord.VersionId));
            Assert.Equal(3, savedRecord.VersionNumber);
            Assert.Contains("ClearDraftFlags", executedSqlIds);
            Assert.Contains("InsertVersion", executedSqlIds);
            mapperMock.VerifyAll();
        }

        /// <summary>
        /// 验证更新已发布版本时会清理其他已发布标记并补齐发布时间。
        /// </summary>
        [Fact]
        public async Task SaveVersionAsync_Should_UpdatePublishedVersion_AndClearPreviousPublishedFlags()
        {
            var existingRecord = new DecisionVersionRecord
            {
                VersionId = Guid.NewGuid().ToString("D"),
                DecisionId = Guid.NewGuid().ToString("D"),
                DecisionKey = "expense.approval",
                VersionNumber = 5,
                VersionTag = "v5.0.0",
                CreatedBy = "seed-user",
                CreatedTime = new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero)
            };

            var executedSqlIds = new List<string>();
            var mapperMock = new Mock<ISmartSqlMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(mapper => mapper.QueryAsync<DecisionVersionRecord>(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository" &&
                    context.SqlId == "GetByVersionId")))
                .ReturnsAsync(new[] { existingRecord });
            mapperMock
                .Setup(mapper => mapper.ExecuteAsync(It.Is<RequestContext>(context =>
                    context.Scope == "Sys.Workflow.Rules.Core.Repositories.DecisionVersionRepository")))
                .Callback<RequestContext>(context => executedSqlIds.Add(context.SqlId))
                .ReturnsAsync(1);

            var repository = new DecisionVersionRepository(mapperMock.Object);
            var record = new DecisionVersionRecord
            {
                VersionId = existingRecord.VersionId,
                DecisionId = existingRecord.DecisionId,
                DecisionKey = existingRecord.DecisionKey,
                VersionTag = existingRecord.VersionTag,
                VersionNumber = existingRecord.VersionNumber,
                IsPublished = true,
                DefinitionContent = "<dmn version=\"5\" />",
                PublishedBy = "publisher"
            };

            var savedRecord = await repository.SaveAsync(record).ConfigureAwait(false);

            Assert.Equal(existingRecord.CreatedBy, savedRecord.CreatedBy);
            Assert.Equal(existingRecord.CreatedTime, savedRecord.CreatedTime);
            Assert.NotNull(savedRecord.PublishedTime);
            Assert.Equal("publisher", savedRecord.PublishedBy);
            Assert.True(executedSqlIds.SequenceEqual(new[] { "ClearPublishedFlags", "UpdateVersion" }));
            mapperMock.VerifyAll();
        }
    }
}
