using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sys.Workflow.Client.Tests.Benchmark
{
    /// <summary>
    /// 单个审批流基准场景的定义。运行器只依赖委托，与具体引擎类型解耦，
    /// 由测试类负责在委托内部使用 IntegrationTestContext / 客户端 API。
    /// </summary>
    public sealed class BenchmarkScenario
    {
        /// <summary>场景名称（同时作为基线对比的键）。</summary>
        public string Name { get; set; }

        /// <summary>使用的 BPMN 文件。</summary>
        public string BpmnFile { get; set; }

        /// <summary>正式测量迭代次数。</summary>
        public int Iterations { get; set; } = 20;

        /// <summary>预热迭代次数（不计入统计）。</summary>
        public int Warmup { get; set; } = 3;

        /// <summary>参与用户规模（会签/大规模场景）。</summary>
        public int UserScale { get; set; } = 1;

        /// <summary>并发处理并发度。</summary>
        public int ConcurrencyDegree { get; set; } = 8;

        /// <summary>
        /// 生成一次迭代的隔离状态（如唯一 businessKey、随机用户集），
        /// 返回的对象会依次传递给 Start/Assign/Complete，保证每次迭代数据隔离。
        /// </summary>
        public Func<object> Prepare { get; set; }

        /// <summary>测量阶段一：流程启动（创建流程实例）。</summary>
        public Action<object> Start { get; set; }

        /// <summary>测量阶段二：任务分配（检索被分配的任务）。</summary>
        public Action<object> Assign { get; set; }

        /// <summary>测量阶段三：任务完成（驱动流程直至结束）。</summary>
        public Action<object> Complete { get; set; }
    }

    /// <summary>
    /// 轻量级审批流基准执行器：分阶段计时、内存分配 / GC 统计、串行与并发吞吐量测量。
    /// 不引入第三方基准框架，直接基于 <see cref="Stopwatch"/> 与 GC API，
    /// 复用现有集成测试宿主（TestServer + SQL Server）。
    /// </summary>
    public static class WorkflowBenchmarkRunner
    {
        /// <summary>
        /// 执行单个场景的完整基准测量并产出 <see cref="BenchmarkResult"/>。
        /// </summary>
        public static BenchmarkResult Run(BenchmarkScenario scenario)
        {
            if (scenario is null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }
            if (scenario.Prepare is null || scenario.Start is null || scenario.Assign is null || scenario.Complete is null)
            {
                throw new ArgumentException($"场景 {scenario.Name} 的 Prepare/Start/Assign/Complete 委托不能为空。");
            }

            // 预热：触发 JIT、连接池、EF/SmartSql 缓存等，避免首次开销污染测量。
            for (int i = 0; i < scenario.Warmup; i++)
            {
                RunOnce(scenario);
            }

            var startTimes = new List<double>(scenario.Iterations);
            var assignTimes = new List<double>(scenario.Iterations);
            var completeTimes = new List<double>(scenario.Iterations);

            // 内存/GC 基线：强制回收后取分配字节数与各代回收次数。
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            int gen0Before = GC.CollectionCount(0);
            int gen1Before = GC.CollectionCount(1);
            int gen2Before = GC.CollectionCount(2);
            long allocBefore = GC.GetTotalAllocatedBytes(true);

            var totalSw = Stopwatch.StartNew();
            var phaseSw = new Stopwatch();

            for (int i = 0; i < scenario.Iterations; i++)
            {
                object state = scenario.Prepare();

                phaseSw.Restart();
                scenario.Start(state);
                phaseSw.Stop();
                startTimes.Add(phaseSw.Elapsed.TotalMilliseconds);

                phaseSw.Restart();
                scenario.Assign(state);
                phaseSw.Stop();
                assignTimes.Add(phaseSw.Elapsed.TotalMilliseconds);

                phaseSw.Restart();
                scenario.Complete(state);
                phaseSw.Stop();
                completeTimes.Add(phaseSw.Elapsed.TotalMilliseconds);
            }

            totalSw.Stop();
            long allocAfter = GC.GetTotalAllocatedBytes(true);

            double totalStartSeconds = startTimes.Sum() / 1000.0;
            double throughputPerSec = totalStartSeconds > 0 ? scenario.Iterations / totalStartSeconds : 0d;

            var result = new BenchmarkResult
            {
                Scenario = scenario.Name,
                BpmnFile = scenario.BpmnFile,
                Iterations = scenario.Iterations,
                Warmup = scenario.Warmup,
                UserScale = scenario.UserScale,
                ConcurrencyDegree = scenario.ConcurrencyDegree,
                Phases = new List<PhaseStat>
                {
                    BuildStat("Start", startTimes),
                    BuildStat("Assignment", assignTimes),
                    BuildStat("Completion", completeTimes),
                },
                ThroughputPerSec = Math.Round(throughputPerSec, 3),
                AllocatedBytes = Math.Max(0, allocAfter - allocBefore),
                AllocatedBytesPerIteration = scenario.Iterations > 0
                    ? Math.Max(0, allocAfter - allocBefore) / scenario.Iterations
                    : 0,
                Gen0Collections = GC.CollectionCount(0) - gen0Before,
                Gen1Collections = GC.CollectionCount(1) - gen1Before,
                Gen2Collections = GC.CollectionCount(2) - gen2Before,
                TotalElapsedMs = Math.Round(totalSw.Elapsed.TotalMilliseconds, 3),
                TimestampUtc = DateTime.UtcNow,
            };

            result.ConcurrentThroughputPerSec = Math.Round(MeasureConcurrentThroughput(scenario), 3);

            return result;
        }

        /// <summary>
        /// 并发处理能力测量：以 <see cref="BenchmarkScenario.ConcurrencyDegree"/> 个并行完整生命周期
        /// （启动+分配+完成）测量墙钟耗时，得到并发吞吐量（实例/秒）。
        /// </summary>
        private static double MeasureConcurrentThroughput(BenchmarkScenario scenario)
        {
            int degree = Math.Max(1, scenario.ConcurrencyDegree);

            // 预先生成隔离状态，避免把状态构造成本计入并发墙钟。
            var states = new object[degree];
            for (int i = 0; i < degree; i++)
            {
                states[i] = scenario.Prepare();
            }

            var sw = Stopwatch.StartNew();
            Task[] tasks = new Task[degree];
            for (int i = 0; i < degree; i++)
            {
                object state = states[i];
                tasks[i] = Task.Run(() =>
                {
                    scenario.Start(state);
                    scenario.Assign(state);
                    scenario.Complete(state);
                });
            }

            Task.WaitAll(tasks);
            sw.Stop();

            double seconds = sw.Elapsed.TotalSeconds;
            return seconds > 0 ? degree / seconds : 0d;
        }

        private static void RunOnce(BenchmarkScenario scenario)
        {
            object state = scenario.Prepare();
            scenario.Start(state);
            scenario.Assign(state);
            scenario.Complete(state);
        }

        private static PhaseStat BuildStat(string phase, List<double> samples)
        {
            var stat = new PhaseStat
            {
                Phase = phase,
                Samples = samples.Count,
            };

            if (samples.Count == 0)
            {
                return stat;
            }

            var sorted = samples.OrderBy(x => x).ToList();
            stat.AvgMs = Math.Round(samples.Average(), 3);
            stat.MinMs = Math.Round(sorted[0], 3);
            stat.MaxMs = Math.Round(sorted[sorted.Count - 1], 3);
            stat.P50Ms = Math.Round(Percentile(sorted, 0.50), 3);
            stat.P95Ms = Math.Round(Percentile(sorted, 0.95), 3);
            return stat;
        }

        /// <summary>
        /// 计算已升序排序样本的分位数（最近秩方法）。
        /// </summary>
        private static double Percentile(IReadOnlyList<double> sorted, double percentile)
        {
            if (sorted.Count == 0)
            {
                return 0d;
            }
            if (sorted.Count == 1)
            {
                return sorted[0];
            }

            int rank = (int)Math.Ceiling(percentile * sorted.Count);
            int index = Math.Min(sorted.Count - 1, Math.Max(0, rank - 1));
            return sorted[index];
        }
    }
}
