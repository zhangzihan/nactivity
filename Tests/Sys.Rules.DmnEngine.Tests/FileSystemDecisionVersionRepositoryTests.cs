using System;
using System.IO;
using System.Threading.Tasks;
using Sys.Workflow.Rules.Models;
using Xunit;

namespace Sys.Rules.DmnEngine.Tests
{
    /// <summary>
    /// 定义文件系统版本仓储的回归测试。
    /// </summary>
    public class FileSystemDecisionVersionRepositoryTests
    {
        /// <summary>
        /// 创建带样例目录的文件系统版本仓储。
        /// </summary>
        /// <returns>文件系统版本仓储。</returns>
        private static FileSystemDecisionVersionRepository CreateRepository()
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            return new FileSystemDecisionVersionRepository(new[] { sampleDirectory });
        }

        /// <summary>
        /// 验证 LatestPublished 解析会命中最高版本。
        /// </summary>
        [Fact]
        public async Task ResolveAsync_Should_ReturnLatestPublishedVersion_WhenBindingModeIsLatestPublished()
        {
            var repository = CreateRepository();

            var result = await repository.ResolveAsync(new DecisionVersionResolutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished"
            }).ConfigureAwait(false);

            Assert.Equal("single-root-expression", result.DecisionKey);
            Assert.Equal("v2.0.0", result.VersionTag);
            Assert.Contains("Age+20", result.DefinitionContent);
        }

        /// <summary>
        /// 验证按版本标签加载时会返回指定版本内容。
        /// </summary>
        [Fact]
        public async Task GetByVersionTagAsync_Should_ReturnSpecifiedVersionRecord_WhenVersionExists()
        {
            var repository = CreateRepository();

            var record = await repository.GetByVersionTagAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);

            Assert.NotNull(record);
            Assert.Equal("v1.0.0", record!.VersionTag);
            Assert.True(record.IsPublished);
            Assert.Contains("Age+10", record.DefinitionContent);
        }

        /// <summary>
        /// 验证不指定绑定模式时会回落到裸文件定义。
        /// </summary>
        [Fact]
        public async Task ResolveAsync_Should_ReturnDraftDefinition_WhenNoBindingModeOrVersionTagProvided()
        {
            var repository = CreateRepository();

            var result = await repository.ResolveAsync(new DecisionVersionResolutionRequest
            {
                DecisionKey = "single-root-expression"
            }).ConfigureAwait(false);

            Assert.Null(result.VersionTag);
            Assert.Contains("Age+10", result.DefinitionContent);
            Assert.EndsWith("single-root-expression.dmn", result.DefinitionSource);
        }
    }
}
