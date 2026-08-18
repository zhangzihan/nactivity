using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Sys.Workflow.Client.Tests.Benchmark
{
    /// <summary>
    /// 基准报告输出与基线回归对比器。
    /// 职责：
    /// 1. 将本次运行结果序列化为 JSON 报告，输出到 benchmark-reports/ 目录（可被 CI 采集）。
    /// 2. 在控制台打印可读表格。
    /// 3. 加载仓库内 JSON 基线（随输出目录复制），按阈值比率检测性能回归。
    /// 4. 支持通过环境变量 BENCHMARK_UPDATE_BASELINE=1 用本次结果更新基线文件。
    /// </summary>
    public static class BenchmarkReporter
    {
        /// <summary>基线文件（随 csproj 复制到输出目录 resources\benchmark 下）。</summary>
        public const string BaselineFileName = "approval-benchmark-baseline.json";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// 将报告写入输出目录 benchmark-reports/ 并返回完整路径。
        /// </summary>
        public static string WriteReport(BenchmarkReport report)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark-reports");
            Directory.CreateDirectory(dir);

            string file = Path.Combine(dir, $"benchmark-{report.TimestampUtc:yyyyMMdd-HHmmss}.json");
            File.WriteAllText(file, JsonConvert.SerializeObject(report, JsonSettings), Encoding.UTF8);
            return file;
        }

        /// <summary>
        /// 生成控制台可读表格（返回字符串，便于测试输出到 ITestOutputHelper）。
        /// </summary>
        public static string FormatConsole(BenchmarkResult r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"===== 基准场景: {r.Scenario} =====");
            sb.AppendLine($"BPMN文件      : {r.BpmnFile}");
            sb.AppendLine($"迭代/预热     : {r.Iterations} / {r.Warmup}    用户规模: {r.UserScale}");
            sb.AppendLine("阶段耗时(ms)  : 阶段        平均      P50       P95       最小      最大");
            foreach (var p in r.Phases)
            {
                sb.AppendLine(
                    $"                {p.Phase,-11} {p.AvgMs,-9:F2} {p.P50Ms,-9:F2} {p.P95Ms,-9:F2} {p.MinMs,-9:F2} {p.MaxMs:F2}");
            }
            sb.AppendLine($"串行吞吐量    : {r.ThroughputPerSec:F2} 实例/秒");
            sb.AppendLine($"并发吞吐量    : {r.ConcurrentThroughputPerSec:F2} 实例/秒 (并发度 {r.ConcurrencyDegree})");
            sb.AppendLine($"内存分配      : 总 {FormatBytes(r.AllocatedBytes)}，单次 {FormatBytes(r.AllocatedBytesPerIteration)}");
            sb.AppendLine($"GC 回收       : Gen0={r.Gen0Collections} Gen1={r.Gen1Collections} Gen2={r.Gen2Collections}");
            return sb.ToString();
        }

        /// <summary>
        /// 加载输出目录中的基线文件；不存在时返回 null。
        /// </summary>
        public static BenchmarkBaseline LoadBaseline()
        {
            string file = ResolveBaselinePath();
            if (!File.Exists(file))
            {
                return null;
            }

            try
            {
                var baseline = JsonConvert.DeserializeObject<BenchmarkBaseline>(File.ReadAllText(file));
                return baseline;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// 依据基线检测单场景回归。基线缺失或该场景无基线条目时返回空列表（视为首次基准，不告警）。
        /// 检测规则：
        /// - 启动/完成耗时、单次内存分配：实测 &gt; 基线 * ThresholdRatio 判定回归；
        /// - 串行吞吐量：实测 &lt; 基线 / ThresholdRatio 判定回归。
        /// </summary>
        public static IList<RegressionFinding> DetectRegressions(BenchmarkBaseline baseline, BenchmarkResult result)
        {
            var findings = new List<RegressionFinding>();
            if (baseline?.Scenarios is null ||
                !baseline.Scenarios.TryGetValue(result.Scenario, out BaselineEntry entry) ||
                entry is null)
            {
                return findings;
            }

            double ratio = baseline.ThresholdRatio <= 0 ? 1.20 : baseline.ThresholdRatio;

            PhaseStat start = result.GetPhase("Start");
            if (start is not null && entry.StartAvgMs > 0)
            {
                findings.Add(BuildUpperBoundFinding(result.Scenario, "启动平均耗时(ms)", entry.StartAvgMs, start.AvgMs, ratio));
            }

            PhaseStat complete = result.GetPhase("Completion");
            if (complete is not null && entry.CompleteAvgMs > 0)
            {
                findings.Add(BuildUpperBoundFinding(result.Scenario, "完成平均耗时(ms)", entry.CompleteAvgMs, complete.AvgMs, ratio));
            }

            if (entry.AllocatedBytesPerIteration > 0)
            {
                findings.Add(BuildUpperBoundFinding(result.Scenario, "单次内存分配(bytes)",
                    entry.AllocatedBytesPerIteration, result.AllocatedBytesPerIteration, ratio));
            }

            if (entry.ThroughputPerSec > 0)
            {
                findings.Add(BuildLowerBoundFinding(result.Scenario, "串行吞吐量(实例/秒)",
                    entry.ThroughputPerSec, result.ThroughputPerSec, ratio));
            }

            return findings;
        }

        /// <summary>
        /// 根据本次报告更新（重建）基线文件，保留原有阈值比率。仅在 BENCHMARK_UPDATE_BASELINE 开启时调用。
        /// 写入的是仓库源文件路径（若能定位），否则回退到输出目录。
        /// </summary>
        public static string UpdateBaseline(BenchmarkReport report, BenchmarkBaseline existing)
        {
            var baseline = new BenchmarkBaseline
            {
                ThresholdRatio = existing?.ThresholdRatio > 0 ? existing.ThresholdRatio : 1.20,
            };

            // 合并：先保留既有场景基线，再用本次结果覆盖对应场景，避免单场景运行清空其它场景基线。
            if (existing?.Scenarios is not null)
            {
                foreach (var kv in existing.Scenarios)
                {
                    baseline.Scenarios[kv.Key] = kv.Value;
                }
            }

            foreach (var r in report.Results)
            {
                baseline.Scenarios[r.Scenario] = new BaselineEntry
                {
                    StartAvgMs = r.GetPhase("Start")?.AvgMs ?? 0,
                    CompleteAvgMs = r.GetPhase("Completion")?.AvgMs ?? 0,
                    ThroughputPerSec = r.ThroughputPerSec,
                    AllocatedBytesPerIteration = r.AllocatedBytesPerIteration,
                };
            }

            string target = ResolveWritableBaselinePath();
            File.WriteAllText(target, JsonConvert.SerializeObject(baseline, JsonSettings), Encoding.UTF8);
            return target;
        }

        /// <summary>是否请求更新基线（环境变量 BENCHMARK_UPDATE_BASELINE == 1/true）。</summary>
        public static bool ShouldUpdateBaseline()
        {
            return IsEnvEnabled("BENCHMARK_UPDATE_BASELINE");
        }

        private static RegressionFinding BuildUpperBoundFinding(string scenario, string metric, double baseline, double actual, double ratio)
        {
            double allowed = baseline * ratio;
            bool regression = actual > allowed;
            return new RegressionFinding
            {
                Scenario = scenario,
                Metric = metric,
                Baseline = Math.Round(baseline, 3),
                Actual = Math.Round(actual, 3),
                AllowedThreshold = Math.Round(allowed, 3),
                IsRegression = regression,
                Message = regression
                    ? $"[回归] {scenario} 的 {metric} 实测 {actual:F2} 超过允许上限 {allowed:F2}（基线 {baseline:F2} × {ratio:F2}）。"
                    : $"[正常] {scenario} 的 {metric} 实测 {actual:F2} 未超过上限 {allowed:F2}。",
            };
        }

        private static RegressionFinding BuildLowerBoundFinding(string scenario, string metric, double baseline, double actual, double ratio)
        {
            double allowed = baseline / ratio;
            bool regression = actual < allowed;
            return new RegressionFinding
            {
                Scenario = scenario,
                Metric = metric,
                Baseline = Math.Round(baseline, 3),
                Actual = Math.Round(actual, 3),
                AllowedThreshold = Math.Round(allowed, 3),
                IsRegression = regression,
                Message = regression
                    ? $"[回归] {scenario} 的 {metric} 实测 {actual:F2} 低于允许下限 {allowed:F2}（基线 {baseline:F2} ÷ {ratio:F2}）。"
                    : $"[正常] {scenario} 的 {metric} 实测 {actual:F2} 未低于下限 {allowed:F2}。",
            };
        }

        private static string ResolveBaselinePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "benchmark", BaselineFileName);
        }

        /// <summary>
        /// 定位可写的基线源路径：优先写回仓库源文件（便于提交），无法定位时回退到输出目录。
        /// </summary>
        private static string ResolveWritableBaselinePath()
        {
            // 从输出目录向上回溯，尝试定位仓库内 Tests\Sys.Bpm.Client.Tests\resources\benchmark。
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir is not null)
            {
                string candidate = Path.Combine(dir.FullName, "resources", "benchmark", BaselineFileName);
                if (File.Exists(candidate) && dir.Name.Equals("Sys.Bpm.Client.Tests", StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }

            string fallbackDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "benchmark");
            Directory.CreateDirectory(fallbackDir);
            return Path.Combine(fallbackDir, BaselineFileName);
        }

        private static bool IsEnvEnabled(string name)
        {
            string v = System.Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(v))
            {
                return false;
            }
            v = v.Trim();
            return v.Equals("1", StringComparison.OrdinalIgnoreCase)
                || v.Equals("true", StringComparison.OrdinalIgnoreCase)
                || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0)
            {
                return "0 B";
            }
            string[] units = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            return size.ToString("F2", CultureInfo.InvariantCulture) + " " + units[unit];
        }
    }
}
