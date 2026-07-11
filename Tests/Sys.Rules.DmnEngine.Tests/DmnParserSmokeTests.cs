using System;
using System.IO;
using Sys.Rules.DmnEngine.Engine.Definition;
using Sys.Rules.DmnEngine.Engine.Execution.Context;
using Sys.Rules.DmnEngine.Parser;
using Xunit;

namespace Sys.Rules.DmnEngine.Tests
{
    /// <summary>
    /// 定义首批上游源码纳管后的 DMN 解析烟囱测试。
    /// </summary>
    public class DmnParserSmokeTests
    {
        /// <summary>
        /// 验证上游样例 DMN 在纳管后仍可完成解析、定义构建与执行上下文创建。
        /// </summary>
        [Fact]
        public void ParseAutoDetect_Should_CreateDefinitionAndExecutionContext_ForImportedSample()
        {
            var samplePath = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn", "test.dmn");

            Assert.True(File.Exists(samplePath), $"样例文件不存在: {samplePath}");

            var model = DmnParser.ParseAutoDetect(samplePath);
            var definition = DmnDefinitionFactory.CreateDmnDefinition(model);
            var context = DmnExecutionContextFactory.CreateExecutionContext(definition);

            Assert.NotNull(model);
            Assert.NotNull(definition);
            Assert.NotNull(context);
            Assert.NotEmpty(definition.Decisions);
            Assert.NotEmpty(definition.Variables);
        }
    }
}
