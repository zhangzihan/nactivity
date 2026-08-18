using System;
using System.Collections.Generic;

namespace Sys.Workflow.Client.Tests.Benchmark
{
    /// <summary>
    /// 单个阶段（流程启动 / 任务分配 / 任务完成）的耗时统计（毫秒）。
    /// </summary>
    public sealed class PhaseStat
    {
        /// <summary>阶段名称，如 Start / Assignment / Completion。</summary>
        public string Phase { get; set; }

        /// <summary>样本数量。</summary>
        public int Samples { get; set; }

        /// <summary>平均耗时（毫秒）。</summary>
        public double AvgMs { get; set; }

        /// <summary>最小耗时（毫秒）。</summary>
        public double MinMs { get; set; }

        /// <summary>最大耗时（毫秒）。</summary>
        public double MaxMs { get; set; }

        /// <summary>中位数（P50）耗时（毫秒）。</summary>
        public double P50Ms { get; set; }

        /// <summary>P95 耗时（毫秒）。</summary>
        public double P95Ms { get; set; }
    }

    /// <summary>
    /// 单个审批流场景的基准测试结果。
    /// </summary>
    public sealed class BenchmarkResult
    {
        /// <summary>场景名称（同时作为基线对比的键）。</summary>
        public string Scenario { get; set; }

        /// <summary>使用的 BPMN 文件。</summary>
        public string BpmnFile { get; set; }

        /// <summary>正式测量的迭代次数。</summary>
        public int Iterations { get; set; }

        /// <summary>预热迭代次数（不计入统计）。</summary>
        public int Warmup { get; set; }

        /// <summary>参与用户规模（会签/大规模场景）。</summary>
        public int UserScale { get; set; }

        /// <summary>各阶段耗时统计。</summary>
        public List<PhaseStat> Phases { get; set; } = new List<PhaseStat>();

        /// <summary>串行流程实例创建吞吐量（实例/秒）。</summary>
        public double ThroughputPerSec { get; set; }

        /// <summary>并发流程实例创建吞吐量（实例/秒）。</summary>
        public double ConcurrentThroughputPerSec { get; set; }

        /// <summary>并发处理并发度。</summary>
        public int ConcurrencyDegree { get; set; }

        /// <summary>测量期间总分配字节数。</summary>
        public long AllocatedBytes { get; set; }

        /// <summary>单次迭代平均分配字节数。</summary>
        public long AllocatedBytesPerIteration { get; set; }

        /// <summary>Gen0 回收次数。</summary>
        public int Gen0Collections { get; set; }

        /// <summary>Gen1 回收次数。</summary>
        public int Gen1Collections { get; set; }

        /// <summary>Gen2 回收次数。</summary>
        public int Gen2Collections { get; set; }

        /// <summary>测量总墙钟耗时（毫秒）。</summary>
        public double TotalElapsedMs { get; set; }

        /// <summary>生成时间（UTC）。</summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>按阶段名取统计（大小写不敏感），不存在返回 null。</summary>
        public PhaseStat GetPhase(string phase)
        {
            return Phases?.Find(p => string.Equals(p.Phase, phase, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// 一次基准运行的完整报告（多个场景）。
    /// </summary>
    public sealed class BenchmarkReport
    {
        /// <summary>运行时间（UTC）。</summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>运行机器名。</summary>
        public string MachineName { get; set; }

        /// <summary>运行时框架描述。</summary>
        public string Runtime { get; set; }

        /// <summary>各场景结果。</summary>
        public List<BenchmarkResult> Results { get; set; } = new List<BenchmarkResult>();
    }

    /// <summary>
    /// 单场景基线阈值条目。数值为“可接受上限/下限”的参考基准值。
    /// </summary>
    public sealed class BaselineEntry
    {
        /// <summary>基线：流程启动平均耗时（毫秒）。</summary>
        public double StartAvgMs { get; set; }

        /// <summary>基线：任务完成平均耗时（毫秒）。</summary>
        public double CompleteAvgMs { get; set; }

        /// <summary>基线：串行吞吐量（实例/秒），实测低于该值一定比例视为回归。</summary>
        public double ThroughputPerSec { get; set; }

        /// <summary>基线：单次迭代分配字节数。</summary>
        public long AllocatedBytesPerIteration { get; set; }
    }

    /// <summary>
    /// 基准回归基线：包含全局阈值与各场景基线值。
    /// </summary>
    public sealed class BenchmarkBaseline
    {
        /// <summary>
        /// 回归阈值比率（默认 1.20，即耗时/内存超过基线 20% 或吞吐低于基线 1/1.20 即判定回归）。
        /// </summary>
        public double ThresholdRatio { get; set; } = 1.20;

        /// <summary>各场景基线值，键为场景名称。</summary>
        public Dictionary<string, BaselineEntry> Scenarios { get; set; } =
            new Dictionary<string, BaselineEntry>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 单场景回归检测结论。
    /// </summary>
    public sealed class RegressionFinding
    {
        /// <summary>场景名称。</summary>
        public string Scenario { get; set; }

        /// <summary>被检测指标名称。</summary>
        public string Metric { get; set; }

        /// <summary>基线值。</summary>
        public double Baseline { get; set; }

        /// <summary>实测值。</summary>
        public double Actual { get; set; }

        /// <summary>允许阈值。</summary>
        public double AllowedThreshold { get; set; }

        /// <summary>是否回归。</summary>
        public bool IsRegression { get; set; }

        /// <summary>可读描述。</summary>
        public string Message { get; set; }
    }
}
