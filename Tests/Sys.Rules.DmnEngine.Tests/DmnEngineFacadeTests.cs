using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sys.Rules.DmnEngine;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Xunit;

namespace Sys.Rules.DmnEngine.Tests
{
    /// <summary>
    /// 定义 DMN 内核外观对象的首批单元测试。
    /// </summary>
    public class DmnEngineFacadeTests
    {
        /// <summary>
        /// 创建带样例目录的 DMN 外观对象。
        /// </summary>
        /// <returns>DMN 外观对象。</returns>
        private static DmnEngineFacade CreateFacade(IDecisionExecutionLogRepository? decisionExecutionLogRepository = null)
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            return new DmnEngineFacade(new[] { sampleDirectory }, decisionExecutionLogRepository);
        }

        /// <summary>
        /// 验证加载单根决策定义时会返回解析后的元数据。
        /// </summary>
        [Fact]
        public async Task LoadDefinitionAsync_Should_ReturnDescriptor_WhenDecisionKeyIsValid()
        {
            var facade = CreateFacade();

            var descriptor = await facade.LoadDefinitionAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);

            Assert.Equal("single-root-expression", descriptor.DecisionKey);
            Assert.Equal("Age +10", descriptor.DecisionName);
            Assert.Equal("v1.0.0", descriptor.VersionTag);
            Assert.EndsWith("single-root-expression.v1.0.0.dmn", descriptor.DefinitionSource);
        }

        /// <summary>
        /// 验证单根决策文件可以完成真实执行。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_ReturnOutputs_WhenSingleRootDecisionResolved()
        {
            var facade = CreateFacade();

            var result = await facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                DecisionType = "LiteralExpression",
                ExecutionMode = "Explain",
                TraceEnabled = true,
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 32
                }
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.False(string.IsNullOrWhiteSpace(result.ExecutionId));
            Assert.Equal("LiteralExpression", result.DecisionType);
            Assert.True(result.TraceEnabled);
            Assert.NotEmpty(result.TraceEntries);
            Assert.Equal(42, result.Outputs["Age10"]);
            Assert.Empty(result.MatchedRules);
            Assert.Contains("执行轨迹", result.Explanation);
        }

        /// <summary>
        /// 验证 LatestPublished 会选择最高版本的已发布定义。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_UseLatestPublishedVersion_WhenBindingModeIsLatestPublished()
        {
            var facade = CreateFacade();

            var result = await facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished",
                DecisionType = "LiteralExpression",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 32
                }
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.Equal(52, result.Outputs["Age10"]);
            Assert.Equal("v2.0.0", result.ResolvedVersionTag);
            Assert.Contains("v2.0.0", result.Explanation);
        }

        /// <summary>
        /// 验证 FixedVersion 会绑定到指定版本标签的定义文件。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_UseSpecifiedVersion_WhenBindingModeIsFixedVersion()
        {
            var facade = CreateFacade();

            var result = await facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "FixedVersion",
                VersionTag = "v1.0.0",
                DecisionType = "LiteralExpression",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 32
                }
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.Equal(42, result.Outputs["Age10"]);
            Assert.Equal("v1.0.0", result.ResolvedVersionTag);
            Assert.Contains("v1.0.0", result.Explanation);
        }

        /// <summary>
        /// 验证多根决策文件未显式指定决策名称时会拒绝执行。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Throw_WhenMultipleRootDecisionsWithoutExplicitDecisionName()
        {
            var facade = CreateFacade();

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "test"
            })).ConfigureAwait(false);

            Assert.Contains("__decisionName", exception.Message);
        }

        /// <summary>
        /// 验证决策类型不匹配时会拒绝执行。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Throw_WhenDecisionTypeDoesNotMatch()
        {
            var facade = CreateFacade();

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                DecisionType = "DecisionTable",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 12
                }
            })).ConfigureAwait(false);

            Assert.Contains("决策类型不匹配", exception.Message);
        }

        /// <summary>
        /// 验证固定版本绑定模式要求提供版本标签。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Throw_WhenFixedVersionWithoutVersionTag()
        {
            var facade = CreateFacade();

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "FixedVersion",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 12
                }
            })).ConfigureAwait(false);

            Assert.Contains("FixedVersion", exception.Message);
        }

        /// <summary>
        /// 验证 VersionTagPreferred 指定的版本不存在时会抛出明确异常。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Throw_WhenVersionTagPreferredTargetDoesNotExist()
        {
            var facade = CreateFacade();

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "VersionTagPreferred",
                VersionTag = "v9.9.9",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 12
                }
            })).ConfigureAwait(false);

            Assert.Contains("VersionTagPreferred", exception.Message);
            Assert.Contains("v9.9.9", exception.Message);
        }

        /// <summary>
        /// 验证超时参数非法时会拒绝执行。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Throw_WhenTimeoutIsNegative()
        {
            var facade = CreateFacade();

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                TimeoutMs = -1,
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 12
                }
            })).ConfigureAwait(false);

            Assert.Contains("timeoutMs", exception.Message);
        }

        /// <summary>
        /// 验证成功执行时会写入审计日志。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_WriteExecutionLog_WhenExecutionSucceeds()
        {
            var executionLogRepository = new TestDecisionExecutionLogRepository();
            var facade = CreateFacade(executionLogRepository);

            var result = await facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished",
                TraceId = "trace-success-001",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 18
                }
            }).ConfigureAwait(false);

            var logRecord = Assert.Single(executionLogRepository.SavedRecords);
            Assert.Equal(result.ExecutionId, logRecord.ExecutionId);
            Assert.Equal("single-root-expression", logRecord.DecisionKey);
            Assert.Equal("v2.0.0", logRecord.ResolvedVersionTag);
            Assert.True(logRecord.Succeeded);
            Assert.Equal("trace-success-001", logRecord.TraceId);
            Assert.NotNull(logRecord.InputSnapshot);
            Assert.Equal(18L, Convert.ToInt64(logRecord.InputSnapshot!["Age"]));
            Assert.NotNull(logRecord.OutputSnapshot);
            Assert.Equal(38L, Convert.ToInt64(logRecord.OutputSnapshot!["Age10"]));
        }

        /// <summary>
        /// 验证失败执行时同样会写入失败审计日志。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_WriteExecutionLog_WhenExecutionFails()
        {
            var executionLogRepository = new TestDecisionExecutionLogRepository();
            var facade = CreateFacade(executionLogRepository);

            var exception = await Assert.ThrowsAsync<DmnEngineException>(() => facade.ExecuteAsync(new DmnExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished",
                DecisionType = "DecisionTable",
                TraceId = "trace-failure-001",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 21
                }
            })).ConfigureAwait(false);

            var logRecord = Assert.Single(executionLogRepository.SavedRecords);
            Assert.False(string.IsNullOrWhiteSpace(logRecord.ExecutionId));
            Assert.Equal("single-root-expression", logRecord.DecisionKey);
            Assert.Equal("v2.0.0", logRecord.ResolvedVersionTag);
            Assert.False(logRecord.Succeeded);
            Assert.Equal("trace-failure-001", logRecord.TraceId);
            Assert.Contains("决策类型不匹配", exception.Message);
            Assert.Contains("决策类型不匹配", logRecord.ErrorMessage);
            Assert.Null(logRecord.OutputSnapshot);
        }

        /// <summary>
        /// 定义测试用执行日志仓储。
        /// </summary>
        private sealed class TestDecisionExecutionLogRepository : IDecisionExecutionLogRepository
        {
            /// <summary>
            /// 已保存的执行日志集合。
            /// </summary>
            public IList<DecisionExecutionLogRecord> SavedRecords { get; } = new List<DecisionExecutionLogRecord>();

            /// <summary>
            /// 按执行标识加载日志。
            /// </summary>
            /// <param name="executionId">执行记录标识。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>命中的日志记录。</returns>
            public Task<DecisionExecutionLogRecord?> GetByExecutionIdAsync(string executionId, System.Threading.CancellationToken cancellationToken = default)
            {
                var record = SavedRecords.FirstOrDefault(item => string.Equals(item.ExecutionId, executionId, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(record);
            }

            /// <summary>
            /// 查询执行日志集合。
            /// </summary>
            /// <param name="query">查询条件。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>日志集合。</returns>
            public Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryAsync(DecisionExecutionLogQuery query, System.Threading.CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<DecisionExecutionLogRecord>>(SavedRecords.ToList());
            }

            /// <summary>
            /// 统计执行日志条数。
            /// </summary>
            /// <param name="query">查询条件。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>日志条数。</returns>
            public Task<long> CountAsync(DecisionExecutionLogQuery query, System.Threading.CancellationToken cancellationToken = default)
            {
                return Task.FromResult((long)SavedRecords.Count);
            }

            /// <summary>
            /// 保存执行日志记录。
            /// </summary>
            /// <param name="record">待保存记录。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>保存后的记录。</returns>
            public Task<DecisionExecutionLogRecord> SaveAsync(DecisionExecutionLogRecord record, System.Threading.CancellationToken cancellationToken = default)
            {
                SavedRecords.Add(record);
                return Task.FromResult(record);
            }
        }
    }
}
