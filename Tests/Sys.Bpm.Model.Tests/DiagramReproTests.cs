using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using System.IO;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Model.Tests
{
    /// <summary>
    /// 回归测试：元素内包含空扩展元素（如 &lt;camunda:properties /&gt;）时，
    /// ParseExtensionElement 不得吞掉后续兄弟元素（曾导致 sequenceFlow 未解析、StartEvent.OutgoingFlows 为空）。
    /// </summary>
    public class DiagramReproTests
    {
        [Fact]
        public void ConvertToBpmnModel_EmptyExtensionElement_StartEventOutgoingFlowsLinked()
        {
            var converter = new BpmnXMLConverter();
            string path = Path.Combine("resources", "samples", "空扩展元素顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);

            Process process = model.GetProcessById("Process_t151iu7u0");
            Assert.NotNull(process);

            // 全部流程元素都应被解析（含网关后的 sequenceFlow）
            Assert.Equal(2, process.FindFlowElementsOfType<SequenceFlow>().Count);
            Assert.Single(process.FindFlowElementsOfType<StartEvent>());
            Assert.Single(process.FindFlowElementsOfType<EndEvent>());
            Assert.Single(process.FindFlowElementsOfType<ExclusiveGateway>());

            var startEvent = process.FindFlowElementsOfType<StartEvent>().First();
            Assert.Single(startEvent.OutgoingFlows);
            Assert.Equal("Flow_06qlov6", startEvent.OutgoingFlows[0].Id);
        }
    }
}
