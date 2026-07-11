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
    /// 定义规则运行时适配器的首批集成边界测试。
    /// </summary>
    public class DmnDecisionRuntimeAdapterTests
    {
        /// <summary>
        /// 创建带样例目录的规则运行时适配器。
        /// </summary>
        /// <returns>规则运行时适配器。</returns>
        private static DmnDecisionRuntimeAdapter CreateAdapter()
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            return new DmnDecisionRuntimeAdapter(new DmnEngineFacade(new[] { sampleDirectory }));
        }

        /// <summary>
        /// 验证适配器可以透传定义元数据查询。
        /// </summary>
        [Fact]
        public async Task GetDefinitionAsync_Should_ReturnDescriptor_WhenDecisionKeyIsValid()
        {
            var adapter = CreateAdapter();

            var descriptor = await adapter.GetDefinitionAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);

            Assert.Equal("single-root-expression", descriptor.DecisionKey);
            Assert.Equal("Age +10", descriptor.DecisionName);
            Assert.Equal("v1.0.0", descriptor.VersionTag);
        }

        /// <summary>
        /// 验证适配器可以返回真实执行结果。
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_ReturnExecutionResult_WhenKernelIsIntegrated()
        {
            var adapter = CreateAdapter();

            var result = await adapter.ExecuteAsync(new DecisionExecutionRequest
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
        /// 验证适配器构造函数会拒绝空内核引用。
        /// </summary>
        [Fact]
        public void Constructor_Should_Throw_WhenFacadeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DmnDecisionRuntimeAdapter(null!));
        }

        /// <summary>
        /// 验证适配器可以代理定义目录查询。
        /// </summary>
        [Fact]
        public async Task QueryDefinitionsAsync_Should_ReturnRepositoryResults_WhenRepositoryIsConfigured()
        {
            var adapter = new DmnDecisionRuntimeAdapter(
                new DmnEngineFacade(new[] { Path.Combine(AppContext.BaseDirectory, "Samples", "dmn") }),
                new StubDecisionDefinitionRepository(),
                new StubDecisionVersionRepository(),
                new StubDecisionExecutionLogRepository());

            var result = await adapter.QueryDefinitionsAsync(new DecisionDefinitionRepositoryQuery
            {
                Keyword = "expense"
            }).ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("expense.approval", result[0].DecisionKey);
        }

        /// <summary>
        /// 验证适配器可以代理执行日志查询。
        /// </summary>
        [Fact]
        public async Task QueryExecutionLogsAsync_Should_ReturnRepositoryResults_WhenRepositoryIsConfigured()
        {
            var adapter = new DmnDecisionRuntimeAdapter(
                new DmnEngineFacade(new[] { Path.Combine(AppContext.BaseDirectory, "Samples", "dmn") }),
                new StubDecisionDefinitionRepository(),
                new StubDecisionVersionRepository(),
                new StubDecisionExecutionLogRepository());

            var result = await adapter.QueryExecutionLogsAsync(new DecisionExecutionLogQuery
            {
                DecisionKey = "expense.approval"
            }).ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal("expense.approval", result[0].DecisionKey);
        }

        /// <summary>
        /// 验证适配器可以基于执行结果返回解释信息。
        /// </summary>
        [Fact]
        public async Task ExplainAsync_Should_ReturnExplanation_WhenExecutionIsTriggered()
        {
            var adapter = CreateAdapter();

            var result = await adapter.ExplainAsync(new DecisionExplanationRequest
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
        /// 验证适配器可以批量执行决策测试并返回通过结果。
        /// </summary>
        [Fact]
        public async Task TestAsync_Should_ReturnPassedResult_WhenExpectedOutputsMatch()
        {
            var adapter = CreateAdapter();

            var result = await adapter.TestAsync(new DecisionTestRequest
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
        /// 验证适配器可以导入决策定义。
        /// </summary>
        [Fact]
        public async Task ImportAsync_Should_SaveDraftThroughGovernanceService_WhenOverwriteIsAllowed()
        {
            var adapter = new DmnDecisionRuntimeAdapter(
                new DmnEngineFacade(new[] { Path.Combine(AppContext.BaseDirectory, "Samples", "dmn") }),
                new StubDecisionDefinitionRepository(),
                new StubDecisionVersionRepository(),
                new StubDecisionExecutionLogRepository(),
                new StubDecisionGovernanceService());

            var result = await adapter.ImportAsync(new DecisionImportRequest
            {
                FileName = "expense.approval.dmn",
                Content = "<definitions />",
                OverwriteExisting = true
            }).ConfigureAwait(false);

            Assert.True(result.Succeeded);
            Assert.Contains("expense.approval", result.ImportedDecisionKeys);
        }

        /// <summary>
        /// 验证适配器可以导出已发布决策定义。
        /// </summary>
        [Fact]
        public async Task ExportAsync_Should_ReturnDefinitionContent_WhenVersionExists()
        {
            var adapter = new DmnDecisionRuntimeAdapter(
                new DmnEngineFacade(new[] { Path.Combine(AppContext.BaseDirectory, "Samples", "dmn") }),
                new StubDecisionDefinitionRepository(),
                new StubDecisionVersionRepository(),
                new StubDecisionExecutionLogRepository(),
                new StubDecisionGovernanceService());

            var result = await adapter.ExportAsync(new DecisionExportRequest
            {
                DecisionKey = "expense.approval",
                VersionTag = "v1.0.0"
            }).ConfigureAwait(false);

            Assert.Equal("expense.approval.v1.0.0.dmn", result.FileName);
            Assert.False(string.IsNullOrWhiteSpace(result.Content));
            Assert.False(string.IsNullOrWhiteSpace(result.Checksum));
        }

        /// <summary>
        /// 验证适配器可以返回执行快照。
        /// </summary>
        [Fact]
        public async Task GetExecutionSnapshotAsync_Should_ReturnSnapshots_WhenExecutionLogExists()
        {
            var adapter = new DmnDecisionRuntimeAdapter(
                new DmnEngineFacade(new[] { Path.Combine(AppContext.BaseDirectory, "Samples", "dmn") }),
                new StubDecisionDefinitionRepository(),
                new StubDecisionVersionRepository(),
                new StubDecisionExecutionLogRepository(),
                new StubDecisionGovernanceService());

            var result = await adapter.GetExecutionSnapshotAsync("exec-1").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("expense.approval", result!.DecisionKey);
            Assert.NotNull(result.InputSnapshot);
        }

        /// <summary>
        /// 验证适配器可以返回缓存状态并执行缓存失效。
        /// </summary>
        [Fact]
        public async Task CacheApis_Should_ReportAndInvalidateCache_WhenDefinitionWasLoaded()
        {
            var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "Samples", "dmn");
            var adapter = new DmnDecisionRuntimeAdapter(new DmnEngineFacade(new[] { sampleDirectory }));

            await adapter.GetDefinitionAsync("single-root-expression", "v1.0.0").ConfigureAwait(false);
            var statusBefore = await adapter.GetCacheStatusAsync("single-root-expression").ConfigureAwait(false);
            Assert.NotEmpty(statusBefore.CachedVersions);

            await adapter.InvalidateCacheAsync(new DecisionCacheInvalidationRequest
            {
                DecisionKey = "single-root-expression"
            }).ConfigureAwait(false);
            var statusAfter = await adapter.GetCacheStatusAsync("single-root-expression").ConfigureAwait(false);

            Assert.Empty(statusAfter.CachedVersions);
        }

        /// <summary>
        /// 用于测试的定义仓储桩实现。
        /// </summary>
        private sealed class StubDecisionDefinitionRepository : IDecisionDefinitionRepository
        {
            public Task<DecisionDefinitionRecord?> GetByDecisionIdAsync(string decisionId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionDefinitionRecord?>(null);
            }

            public Task<DecisionDefinitionRecord?> GetByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionDefinitionRecord?>(new DecisionDefinitionRecord
                {
                    DecisionKey = decisionKey,
                    Name = "费用审批"
                });
            }

            public Task<IReadOnlyList<DecisionDefinitionRecord>> QueryAsync(DecisionDefinitionRepositoryQuery query, CancellationToken cancellationToken = default)
            {
                IReadOnlyList<DecisionDefinitionRecord> result = new List<DecisionDefinitionRecord>
                {
                    new DecisionDefinitionRecord
                    {
                        DecisionKey = "expense.approval",
                        Name = "费用审批",
                        Status = "Published"
                    }
                };

                return Task.FromResult(result);
            }

            public Task<DecisionDefinitionRecord> SaveAsync(DecisionDefinitionRecord record, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(record);
            }
        }

        /// <summary>
        /// 用于测试的版本仓储桩实现。
        /// </summary>
        private sealed class StubDecisionVersionRepository : IDecisionVersionRepository
        {
            public Task<DecisionVersionRecord?> GetByVersionTagAsync(string decisionKey, string versionTag, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionVersionRecord?>(new DecisionVersionRecord
                {
                    DecisionKey = decisionKey,
                    VersionTag = versionTag,
                    DefinitionContent = "<definitions id=\"expense.approval\" />",
                    Checksum = "checksum"
                });
            }

            public Task<DecisionVersionRecord?> GetLatestPublishedAsync(string decisionKey, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionVersionRecord?>(new DecisionVersionRecord
                {
                    DecisionKey = decisionKey,
                    VersionTag = "v1.0.0",
                    IsPublished = true
                });
            }

            public Task<IReadOnlyList<DecisionVersionRecord>> QueryByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default)
            {
                IReadOnlyList<DecisionVersionRecord> result = new List<DecisionVersionRecord>
                {
                    new DecisionVersionRecord
                    {
                        DecisionKey = decisionKey,
                        VersionTag = "v1.0.0",
                        IsPublished = true
                    }
                };

                return Task.FromResult(result);
            }

            public Task<DecisionVersionRecord> SaveAsync(DecisionVersionRecord record, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(record);
            }

            public Task<DecisionVersionResolutionResult> ResolveAsync(DecisionVersionResolutionRequest request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new DecisionVersionResolutionResult
                {
                    DecisionKey = request.DecisionKey,
                    VersionTag = request.VersionTag,
                    DefinitionSource = string.Empty
                });
            }
        }

        /// <summary>
        /// 用于测试的执行日志仓储桩实现。
        /// </summary>
        private sealed class StubDecisionExecutionLogRepository : IDecisionExecutionLogRepository
        {
            public Task<DecisionExecutionLogRecord?> GetByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<DecisionExecutionLogRecord?>(new DecisionExecutionLogRecord
                {
                    ExecutionId = executionId,
                    DecisionKey = "expense.approval",
                    Succeeded = true,
                    InputSnapshot = new Dictionary<string, object?>
                    {
                        ["Age"] = 20
                    },
                    OutputSnapshot = new Dictionary<string, object?>
                    {
                        ["Age10"] = 30
                    }
                });
            }

            public Task<IReadOnlyList<DecisionExecutionLogRecord>> QueryAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
            {
                IReadOnlyList<DecisionExecutionLogRecord> result = new List<DecisionExecutionLogRecord>
                {
                    new DecisionExecutionLogRecord
                    {
                        ExecutionId = Guid.NewGuid().ToString("D"),
                        DecisionKey = query.DecisionKey ?? "expense.approval",
                        Succeeded = true
                    }
                };

                return Task.FromResult(result);
            }

            public Task<long> CountAsync(DecisionExecutionLogQuery query, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(1L);
            }

            public Task<DecisionExecutionLogRecord> SaveAsync(DecisionExecutionLogRecord record, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(record);
            }
        }

        /// <summary>
        /// 用于测试的治理服务桩实现。
        /// </summary>
        private sealed class StubDecisionGovernanceService : IDecisionGovernanceService
        {
            public Task<DecisionDraftSaveResult> SaveDraftAsync(DecisionDraftSaveRequest request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new DecisionDraftSaveResult
                {
                    Definition = new DecisionDefinitionRecord
                    {
                        DecisionKey = request.DecisionKey,
                        Name = request.Name
                    },
                    DraftVersion = new DecisionVersionRecord
                    {
                        DecisionKey = request.DecisionKey,
                        VersionTag = request.VersionTag ?? "draft",
                        DefinitionContent = request.DefinitionContent
                    }
                });
            }

            public Task<DecisionVersionPublishResult> PublishVersionAsync(PublishDecisionVersionRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public Task<DecisionVersionPublishResult> RollbackVersionAsync(RollbackDecisionVersionRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }
        }
    }
}
