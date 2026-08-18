using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sys.Rules.DmnEngine;
using Sys.Rules.DmnEngine.Adapter;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Xunit;

namespace Sys.Rules.DmnEngine.BusinessRuleTaskTests
{
    /// <summary>
    /// 定义决策执行服务的集成边界测试。
    /// </summary>
    public class DmnDecisionRuntimeAdapterTests
    {
        /// <summary>
        /// 创建带样例目录的决策执行服务。
        /// </summary>
        /// <returns>决策执行服务。</returns>
        private static DmnDecisionExecutionService CreateService()
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            return new DmnDecisionExecutionService(new DmnEngineFacade(new[] { sampleDirectory }));
        }

        /// <summary>
        /// 验证执行服务可以透传定义元数据查询。
        /// </summary>
        [Fact]
        public async Task GetDefinitionAsync_Should_ReturnDescriptor_WhenDecisionKeyIsValid()
        {
            var service = CreateService();

            var descriptor = await service.GetDefinitionAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);

            Assert.Equal("single-root-expression", descriptor.DecisionKey);
            Assert.Equal("Age +10", descriptor.DecisionName);
            Assert.Equal("v1.0.0", descriptor.VersionTag);
        }

        /// <summary>
        /// 验证执行服务可以返回真实执行结果。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_ReturnExecutionResult_WhenKernelIsIntegrated()
        {
            var service = CreateService();

            var result = await service.ExecuteAsync(new DecisionExecutionRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished",
                DecisionType = "LiteralExpression",
                ExecutionMode = "Explain",
                TraceEnabled = true,
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 20
                }
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.False(string.IsNullOrWhiteSpace(result.ExecutionId));
            Assert.Equal("LiteralExpression", result.DecisionType);
            Assert.True(result.TraceEnabled);
            Assert.NotEmpty(result.TraceEntries);
            Assert.Equal(40, result.Outputs["Age10"]);
            Assert.Equal("v2.0.0", result.ResolvedVersionTag);
            Assert.Null(result.ErrorMessage);
        }

        /// <summary>
        /// 验证执行服务构造函数会拒绝空内核引用。
        /// </summary>
        [Fact]
        public void Constructor_Should_Throw_WhenFacadeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DmnDecisionExecutionService(null!));
        }

        /// <summary>
        /// 验证执行服务可以基于执行结果返回解释信息。
        /// </summary>
        [Fact]
        public async Task ExplainAsync_Should_ReturnExplanation_WhenExecutionIsTriggered()
        {
            var service = CreateService();

            var result = await service.ExplainAsync(new DecisionExplanationRequest
            {
                DecisionKey = "single-root-expression",
                VersionTag = "v1.0.0",
                Inputs = new Dictionary<string, object?>
                {
                    ["Age"] = 20
                }
            }).ConfigureAwait(false);

            Assert.False(string.IsNullOrWhiteSpace(result.ExecutionId));
            Assert.Equal("single-root-expression", result.DecisionKey);
            Assert.NotEmpty(result.EvaluationSteps);
        }

        /// <summary>
        /// 验证执行服务可以优先复用执行日志构造解释结果。
        /// </summary>
        [Fact]
        public async Task ExplainAsync_Should_ReadExecutionLog_WhenExecutionIdIsProvided()
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            var service = new DmnDecisionExecutionService(
                new DmnEngineFacade(new[] { sampleDirectory }),
                new StubDecisionExecutionLogRepository());

            var result = await service.ExplainAsync(new DecisionExplanationRequest
            {
                ExecutionId = "exec-1"
            }).ConfigureAwait(false);

            Assert.Equal("exec-1", result.ExecutionId);
            Assert.Equal("expense.approval", result.DecisionKey);
            Assert.NotEmpty(result.EvaluationSteps);
        }

        /// <summary>
        /// 验证执行服务可以批量执行决策测试并返回通过结果。
        /// </summary>
        [Fact]
        public async Task TestAsync_Should_ReturnPassedResult_WhenExpectedOutputsMatch()
        {
            var service = CreateService();

            var result = await service.TestAsync(new DecisionTestRequest
            {
                DecisionKey = "single-root-expression",
                BindingMode = "LatestPublished",
                TestCases = new List<DecisionTestCase>
                {
                    new DecisionTestCase
                    {
                        CaseName = "age20",
                        Inputs = new Dictionary<string, object?>
                        {
                            ["Age"] = 20
                        },
                        ExpectedOutputs = new Dictionary<string, object?>
                        {
                            ["Age10"] = 40
                        }
                    }
                }
            }).ConfigureAwait(false);

            Assert.True(result.Passed);
            Assert.Equal(1, result.TotalCases);
            Assert.Single(result.CaseResults);
            Assert.True(result.CaseResults[0].Passed);
        }

        /// <summary>
        /// 验证执行服务可以返回缓存状态并执行缓存失效。
        /// </summary>
        [Fact]
        public async Task CacheApis_Should_ReportAndInvalidateCache_WhenDefinitionWasLoaded()
        {
            var service = CreateService();

            await service.GetDefinitionAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);
            var statusBefore = await service.GetCacheStatusAsync("single-root-expression").ConfigureAwait(false);
            Assert.NotEmpty(statusBefore.CachedVersions);

            await service.InvalidateCacheAsync(new DecisionCacheInvalidationRequest
            {
                DecisionKey = "single-root-expression"
            }).ConfigureAwait(false);
            var statusAfter = await service.GetCacheStatusAsync("single-root-expression").ConfigureAwait(false);

            Assert.Empty(statusAfter.CachedVersions);
        }

        /// <summary>
        /// 用于测试的执行日志仓储桩实现。
        /// </summary>
        private sealed class StubDecisionExecutionLogRepository : IDecisionExecutionLogRepository
        {
            /// <summary>
            /// 按执行记录标识加载日志明细。
            /// </summary>
            /// <param name="executionId">执行记录标识。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>执行日志记录。</returns>
            public Task<DecisionExecutionLogRecord?> GetByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionExecutionLogRecord?>(new DecisionExecutionLogRecord
                {
                    ExecutionId = executionId,
                    DecisionKey = "expense.approval",
                    Succeeded = true,
                    ResolvedVersionTag = "v1.0.0",
                    MatchedRules = new List<string> { "rule-1" },
                    TraceEntries = new List<string> { "step-1" }
                });
            }

            /// <summary>
            /// 测试桩不提供日志列表查询。
            /// </summary>
            /// <param name="query">查询条件。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 测试桩不提供日志计数。
            /// </summary>
            /// <param name="query">查询条件。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<long> CountAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 测试桩不提供日志保存。
            /// </summary>
            /// <param name="record">待保存日志。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<DecisionExecutionLogRecord> SaveAsync(DecisionExecutionLogRecord record, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }
        }
    }
}
