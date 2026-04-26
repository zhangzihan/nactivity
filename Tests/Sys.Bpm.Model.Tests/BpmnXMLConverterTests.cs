using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Sys.Bpm.Model.Tests
{
    public class BpmnXMLConverterTests
    {
        private static string GetSamplePath(string filename)
            => Path.Combine("resources", "samples", filename);

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        [InlineData("分支合并.bpmn")]
        [InlineData("会签流程.bpmn")]
        [InlineData("条件分支流程.bpmn")]
        [InlineData("指定节点启动.bpmn")]
        [InlineData("角色组一人通过.bpmn")]
        public void ConvertToBpmnModel_ValidBpmnFile_ParsesSuccessfully(string bpmnFile)
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath(bpmnFile);

            BpmnModel model = converter.ConvertToBpmnModel(path);

            Assert.NotNull(model);
            Assert.NotEmpty(model.Processes);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn", "Process_SimpleFlow")]
        [InlineData("分支合并.bpmn", "Process_ForkJoin")]
        [InlineData("条件分支流程.bpmn", "Process_19876")]
        public void ConvertToBpmnModel_ValidBpmnFile_ContainsExpectedProcess(string bpmnFile, string expectedProcessId)
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath(bpmnFile);

            BpmnModel model = converter.ConvertToBpmnModel(path);

            Process process = model.GetProcessById(expectedProcessId);
            Assert.NotNull(process);
        }

        [Fact]
        public void ConvertToBpmnModel_SimpleFlow_ContainsUserTasks()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);
            Process process = model.GetProcessById("Process_SimpleFlow");

            var userTasks = process.FindFlowElementsOfType<UserTask>();
            Assert.True(userTasks.Count >= 2, "简单顺序流应包含至少2个用户任务");
        }

        [Fact]
        public void ConvertToBpmnModel_SimpleFlow_ContainsSequenceFlows()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);
            Process process = model.GetProcessById("Process_SimpleFlow");

            var flows = process.FindFlowElementsOfType<SequenceFlow>();
            Assert.True(flows.Count >= 2, "简单顺序流应包含至少2条顺序流");
        }

        [Fact]
        public void ConvertToBpmnModel_SimpleFlow_StartEventExists()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);
            Process process = model.GetProcessById("Process_SimpleFlow");

            var startEvents = process.FindFlowElementsOfType<StartEvent>();
            Assert.Single(startEvents);
        }

        [Fact]
        public void ConvertToXML_ConvertsBackToValidModel()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            // 第一次解析
            BpmnModel originalModel = converter.ConvertToBpmnModel(path);
            Assert.NotNull(originalModel);

            // 序列化为 XML
            byte[] xmlBytes = converter.ConvertToXML(originalModel);
            Assert.NotNull(xmlBytes);
            Assert.True(xmlBytes.Length > 0);

            // 再从 XML 字节解析回来
            using var stream = new MemoryStream(xmlBytes);
            BpmnModel roundTripModel = converter.ConvertToBpmnModel(stream);

            Assert.NotNull(roundTripModel);
            Assert.Equal(originalModel.Processes.Count, roundTripModel.Processes.Count);
        }

        [Fact]
        public void RoundTrip_SimpleFlow_PreservesProcessStructure()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel originalModel = converter.ConvertToBpmnModel(path);
            Process originalProcess = originalModel.GetProcessById("Process_SimpleFlow");
            int originalUserTaskCount = originalProcess.FindFlowElementsOfType<UserTask>().Count;
            int originalSequenceFlowCount = originalProcess.FindFlowElementsOfType<SequenceFlow>().Count;

            byte[] xmlBytes = converter.ConvertToXML(originalModel);
            using var stream = new MemoryStream(xmlBytes);
            BpmnModel roundTripModel = converter.ConvertToBpmnModel(stream);

            Process roundTripProcess = roundTripModel.GetProcessById("Process_SimpleFlow");
            Assert.Equal(originalUserTaskCount, roundTripProcess.FindFlowElementsOfType<UserTask>().Count);
            Assert.Equal(originalSequenceFlowCount, roundTripProcess.FindFlowElementsOfType<SequenceFlow>().Count);
        }

        [Fact]
        public void RoundTrip_SimpleFlow_PreservesFlowElementNames()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel originalModel = converter.ConvertToBpmnModel(path);
            Process originalProcess = originalModel.GetProcessById("Process_SimpleFlow");
            var originalTaskNames = originalProcess.FindFlowElementsOfType<UserTask>()
                .Select(t => t.Name).ToList();

            byte[] xmlBytes = converter.ConvertToXML(originalModel);
            using var stream = new MemoryStream(xmlBytes);
            BpmnModel roundTripModel = converter.ConvertToBpmnModel(stream);

            Process roundTripProcess = roundTripModel.GetProcessById("Process_SimpleFlow");
            var roundTripTaskNames = roundTripProcess.FindFlowElementsOfType<UserTask>()
                .Select(t => t.Name).ToList();

            Assert.Equal(originalTaskNames, roundTripTaskNames);
        }

        [Fact]
        public void ConvertToBpmnModel_SimpleFlow_GraphicInfoExists()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);

            // 验证图形信息已解析
            Assert.True(model.LocationMap.Count > 0 || model.GetFlowLocationGraphicInfo("SequenceFlow_0ihgz1d") != null,
                "BPMN 模型应包含图形定位信息");
        }

        [Fact]
        public void ConvertToXML_ProducesNonEmptyXml()
        {
            var converter = new BpmnXMLConverter();
            string path = GetSamplePath("简单顺序流.bpmn");

            BpmnModel model = converter.ConvertToBpmnModel(path);
            byte[] xmlBytes = converter.ConvertToXML(model);
            string xmlContent = Encoding.UTF8.GetString(xmlBytes);

            Assert.Contains("definitions", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("process", xmlContent, StringComparison.OrdinalIgnoreCase);
        }
    }
}
