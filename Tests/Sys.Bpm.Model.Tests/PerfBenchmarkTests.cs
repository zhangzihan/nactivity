using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Sys.Bpm.Model.Tests
{
    /// <summary>
    /// 大文件解析性能冒烟测试：生成约 2000 个流程元素的 BPMN，
    /// 验证解析与连线的正确性并输出耗时（用于性能优化前后对比）。
    /// </summary>
    public class PerfBenchmarkTests
    {
        private readonly ITestOutputHelper output;

        public PerfBenchmarkTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private static string GenerateLargeBpmn(int taskCount)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<bpmn2:definitions xmlns:bpmn2=\"http://www.omg.org/spec/BPMN/20100524/MODEL\" targetNamespace=\"http://bpmn.io/schema/bpmn\">");
            sb.Append("<bpmn2:process id=\"Process_Perf\" isExecutable=\"true\">");
            sb.Append("<bpmn2:startEvent id=\"StartEvent_1\"><bpmn2:outgoing>Flow_0</bpmn2:outgoing></bpmn2:startEvent>");
            for (int i = 1; i <= taskCount; i++)
            {
                sb.Append($"<bpmn2:serviceTask id=\"Task_{i}\" name=\"任务{i}\"><bpmn2:incoming>Flow_{i - 1}</bpmn2:incoming><bpmn2:outgoing>Flow_{i}</bpmn2:outgoing></bpmn2:serviceTask>");
            }
            sb.Append($"<bpmn2:endEvent id=\"EndEvent_1\"><bpmn2:incoming>Flow_{taskCount}</bpmn2:incoming></bpmn2:endEvent>");
            for (int i = 0; i <= taskCount; i++)
            {
                string source = i == 0 ? "StartEvent_1" : $"Task_{i}";
                string target = i == taskCount ? "EndEvent_1" : $"Task_{i + 1}";
                sb.Append($"<bpmn2:sequenceFlow id=\"Flow_{i}\" sourceRef=\"{source}\" targetRef=\"{target}\" />");
            }
            sb.Append("</bpmn2:process></bpmn2:definitions>");
            return sb.ToString();
        }

        [Fact]
        public void Parse_LargeModel_CompletesAndLinksFlows()
        {
            const int taskCount = 1000;
            string bpmn = GenerateLargeBpmn(taskCount);
            var converter = new BpmnXMLConverter();

            var sw = Stopwatch.StartNew();
            BpmnModel model;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(bpmn)))
            {
                model = converter.ConvertToBpmnModel(ms);
            }
            sw.Stop();
            output.WriteLine($"解析 {taskCount} 个服务任务（约 {2 * taskCount + 3} 个元素）耗时: {sw.ElapsedMilliseconds} ms");

            Sys.Workflow.Bpmn.Models.Process process = model.GetProcessById("Process_Perf");
            Assert.NotNull(process);
            Assert.Equal(taskCount, process.FindFlowElementsOfType<ServiceTask>().Count);

            var startEvent = process.FindFlowElementsOfType<StartEvent>().Single();
            Assert.Single(startEvent.OutgoingFlows);
            Assert.Equal("Flow_0", startEvent.OutgoingFlows[0].Id);

            // 抽查中间节点连线
            var task500 = (FlowNode)process.GetFlowElement("Task_500", true);
            Assert.NotNull(task500);
            Assert.Single(task500.IncomingFlows);
            Assert.Single(task500.OutgoingFlows);
        }
    }
}
