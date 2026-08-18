using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Test;
using Xunit;
using Xunit.Abstractions;

namespace Sys.Workflow.Client.Tests.Benchmark
{
    /// <summary>
    /// 审批流性能基准测试。
    ///
    /// 设计要点：
    /// 1. 复用现有集成测试宿主 <see cref="IntegrationTestContext"/>（TestServer + SQL Server DEVDB），
    ///    参考 ProcessCallActivityClientTest 的启动 / 分配 / 完成调用模式。
    /// 2. 环境变量门禁：默认不执行（return），仅当 RUN_BENCHMARKS=1/true 时运行，
    ///    避免常规测试 / 无数据库环境误触发耗时基准。
    /// 3. 数据隔离与可重复性：每次迭代使用随机用户与唯一 businessKey，互不干扰。
    /// 4. 回归检测：与仓库内 JSON 基线按阈值比率对比，超阈值场景断言失败并输出告警；
    ///    设置 BENCHMARK_UPDATE_BASELINE=1 时改为回写基线而不断言。
    ///
    /// 运行示例（PowerShell）：
    ///   $env:RUN_BENCHMARKS=1; dotnet test Tests/Sys.Bpm.Client.Tests --filter Category=Benchmark
    /// 首次建立基线：
    ///   $env:RUN_BENCHMARKS=1; $env:BENCHMARK_UPDATE_BASELINE=1; dotnet test ... --filter Category=Benchmark
    /// </summary>
    [Trait("Category", "Benchmark")]
    public class ApprovalWorkflowBenchmarkTest
    {
        private const string SingleBpmn = "基准_单级审批.bpmn";
        private const string SerialBpmn = "基准_多级串行审批.bpmn";
        private const string CountersignBpmn = "基准_并行会签审批.bpmn";
        private const string ConditionalBpmn = "基准_条件分支审批.bpmn";

        private const int MaxCompleteRounds = 12;

        private readonly ITestOutputHelper output;

        private readonly object ctxLock = new object();
        private IntegrationTestContext ctx;
        private ITaskController taskClient;

        public ApprovalWorkflowBenchmarkTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>基准门禁：仅当 RUN_BENCHMARKS 为 1/true/yes 时执行。</summary>
        private static bool BenchmarkEnabled
        {
            get
            {
                string v = System.Environment.GetEnvironmentVariable("RUN_BENCHMARKS");
                if (string.IsNullOrWhiteSpace(v))
                {
                    return false;
                }
                v = v.Trim();
                return v.Equals("1", StringComparison.OrdinalIgnoreCase)
                    || v.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact(DisplayName = "单级审批流程基准")]
        public void 单级审批流程基准()
        {
            if (!BenchmarkEnabled)
            {
                return;
            }

            var scenario = new BenchmarkScenario
            {
                Name = "单级审批",
                BpmnFile = SingleBpmn,
                Iterations = 10,
                Warmup = 2,
                UserScale = 1,
                ConcurrencyDegree = 8,
                Prepare = () => NewCollectionState(SingleBpmn, userCount: 1),
                Start = StartCollection,
                Assign = RetrieveTasks,
                Complete = CompleteToEnd,
            };

            Evaluate(WorkflowBenchmarkRunner.Run(scenario));
        }

        [Fact(DisplayName = "多级串行审批流程基准")]
        public void 多级串行审批流程基准()
        {
            if (!BenchmarkEnabled)
            {
                return;
            }

            var scenario = new BenchmarkScenario
            {
                Name = "多级串行审批",
                BpmnFile = SerialBpmn,
                Iterations = 10,
                Warmup = 2,
                UserScale = 1,
                ConcurrencyDegree = 8,
                Prepare = () => NewCollectionState(SerialBpmn, userCount: 1),
                Start = StartCollection,
                Assign = RetrieveTasks,
                Complete = CompleteToEnd,
            };

            Evaluate(WorkflowBenchmarkRunner.Run(scenario));
        }

        [Fact(DisplayName = "并行会签审批流程基准")]
        public void 并行会签审批流程基准()
        {
            if (!BenchmarkEnabled)
            {
                return;
            }

            var scenario = new BenchmarkScenario
            {
                Name = "并行会签审批",
                BpmnFile = CountersignBpmn,
                Iterations = 10,
                Warmup = 2,
                UserScale = 5,
                ConcurrencyDegree = 6,
                Prepare = () => NewCountersignState(userCount: 5),
                Start = StartCollection,
                Assign = RetrieveTasks,
                Complete = CompleteToEnd,
            };

            Evaluate(WorkflowBenchmarkRunner.Run(scenario));
        }

        [Fact(DisplayName = "条件分支复杂审批流程基准")]
        public void 条件分支复杂审批流程基准()
        {
            if (!BenchmarkEnabled)
            {
                return;
            }

            int counter = 0;
            var scenario = new BenchmarkScenario
            {
                Name = "条件分支审批",
                BpmnFile = ConditionalBpmn,
                Iterations = 10,
                Warmup = 2,
                UserScale = 1,
                ConcurrencyDegree = 8,
                // 交替走高 / 低分支，覆盖排他网关两条路径。
                Prepare = () => NewConditionalState(isHigh: System.Threading.Interlocked.Increment(ref counter) % 2 == 0),
                Start = StartCollection,
                Assign = RetrieveTasks,
                Complete = CompleteToEnd,
            };

            Evaluate(WorkflowBenchmarkRunner.Run(scenario));
        }

        [Fact(DisplayName = "大规模用户参与审批流程基准")]
        public void 大规模用户参与审批流程基准()
        {
            if (!BenchmarkEnabled)
            {
                return;
            }

            const int scale = 50;
            var scenario = new BenchmarkScenario
            {
                Name = "大规模会签审批",
                BpmnFile = CountersignBpmn,
                Iterations = 5,
                Warmup = 1,
                UserScale = scale,
                ConcurrencyDegree = 4,
                Prepare = () => NewCountersignState(userCount: scale),
                Start = StartCollection,
                Assign = RetrieveTasks,
                Complete = CompleteToEnd,
            };

            Evaluate(WorkflowBenchmarkRunner.Run(scenario));
        }

        // ---------------- 场景状态与阶段委托 ----------------

        /// <summary>单次迭代的隔离状态。</summary>
        private sealed class ScenarioState
        {
            public string BpmnFile { get; init; }

            /// <summary>用于 MyTasks 查询与完成的用户集合。</summary>
            public string[] Users { get; init; }

            /// <summary>启动流程变量。</summary>
            public WorkflowVariable Vars { get; init; }

            /// <summary>唯一业务键，保证迭代间隔离。</summary>
            public string BusinessKey { get; init; }

            /// <summary>是否将 Users 作为 "name" 集合传入（单级 / 串行 / 条件场景）。</summary>
            public bool PassUsersAsName { get; init; }
        }

        /// <summary>构造以 ${name} 集合分配的场景状态（单级 / 串行）。</summary>
        private ScenarioState NewCollectionState(string bpmnFile, int userCount)
        {
            return new ScenarioState
            {
                BpmnFile = bpmnFile,
                Users = NewUsers(userCount),
                Vars = new WorkflowVariable(),
                BusinessKey = Guid.NewGuid().ToString(),
                PassUsersAsName = true,
            };
        }

        /// <summary>构造条件分支场景状态（携带 isHigh 变量）。</summary>
        private ScenarioState NewConditionalState(bool isHigh)
        {
            var vars = new WorkflowVariable();
            vars.Add("isHigh", isHigh);
            return new ScenarioState
            {
                BpmnFile = ConditionalBpmn,
                Users = NewUsers(1),
                Vars = vars,
                BusinessKey = Guid.NewGuid().ToString(),
                PassUsersAsName = true,
            };
        }

        /// <summary>构造会签 / 大规模场景状态（以 ${countersignUsers} 集合分配）。</summary>
        private ScenarioState NewCountersignState(int userCount)
        {
            string[] users = NewUsers(userCount);
            var vars = new WorkflowVariable();
            vars.Add("countersignUsers", users);
            return new ScenarioState
            {
                BpmnFile = CountersignBpmn,
                Users = users,
                Vars = vars,
                BusinessKey = Guid.NewGuid().ToString(),
                PassUsersAsName = false,
            };
        }

        private static string[] NewUsers(int count)
        {
            var users = new string[count];
            for (int i = 0; i < count; i++)
            {
                users[i] = Guid.NewGuid().ToString();
            }
            return users;
        }

        /// <summary>阶段一：启动流程实例。</summary>
        private void StartCollection(object state)
        {
            EnsureContext();
            var s = (ScenarioState)state;
            _ = ctx.StartUseFile(s.BpmnFile, s.PassUsersAsName ? s.Users : null, s.Vars, s.BusinessKey)
                .GetAwaiter().GetResult();
        }

        /// <summary>阶段二：检索被分配的（首轮）任务。</summary>
        private void RetrieveTasks(object state)
        {
            var s = (ScenarioState)state;
            foreach (var uid in s.Users)
            {
                _ = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
            }
        }

        /// <summary>阶段三：完成全部任务，驱动流程直至结束（覆盖多级 / 分支后续节点）。</summary>
        private void CompleteToEnd(object state)
        {
            var s = (ScenarioState)state;
            for (int round = 0; round < MaxCompleteRounds; round++)
            {
                bool any = false;
                foreach (var uid in s.Users)
                {
                    var tasks = taskClient.MyTasks(uid).GetAwaiter().GetResult().List;
                    foreach (var task in tasks.ToList())
                    {
                        any = true;
                        _ = taskClient.CompleteTask(new CompleteTaskCmd
                        {
                            TaskId = task.Id,
                            NotFoundThrowError = false,
                        }).GetAwaiter().GetResult();
                    }
                }

                if (!any)
                {
                    break;
                }
            }
        }

        // ---------------- 报告与回归评估 ----------------

        private void EnsureContext()
        {
            if (ctx is not null)
            {
                return;
            }
            lock (ctxLock)
            {
                if (ctx is null)
                {
                    var created = IntegrationTestContext.CreateDefaultUnitTestContext();
                    taskClient = created.CreateWorkflowHttpProxy().GetTaskClient();
                    ctx = created;
                }
            }
        }

        /// <summary>
        /// 输出报告、执行回归检测并断言。若开启 BENCHMARK_UPDATE_BASELINE 则回写基线且不断言。
        /// </summary>
        private void Evaluate(BenchmarkResult result)
        {
            output.WriteLine(BenchmarkReporter.FormatConsole(result));

            var report = new BenchmarkReport
            {
                TimestampUtc = DateTime.UtcNow,
                MachineName = System.Environment.MachineName,
                Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                Results = new List<BenchmarkResult> { result },
            };

            string reportPath = BenchmarkReporter.WriteReport(report);
            output.WriteLine($"报告已写入: {reportPath}");

            BenchmarkBaseline baseline = BenchmarkReporter.LoadBaseline();

            if (BenchmarkReporter.ShouldUpdateBaseline())
            {
                string baselinePath = BenchmarkReporter.UpdateBaseline(report, baseline);
                output.WriteLine($"[基线更新] 已回写基线: {baselinePath}");
                return;
            }

            var findings = BenchmarkReporter.DetectRegressions(baseline, result);
            foreach (var f in findings)
            {
                output.WriteLine(f.Message);
            }

            var regressions = findings.Where(f => f.IsRegression).ToList();
            Assert.True(regressions.Count == 0,
                "检测到性能回归：" + System.Environment.NewLine +
                string.Join(System.Environment.NewLine, regressions.Select(f => f.Message)));
        }
    }
}
