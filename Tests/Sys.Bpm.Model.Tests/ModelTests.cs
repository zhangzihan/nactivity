using Sys.Workflow.Bpmn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Model.Tests
{
    public class ModelTests
    {
        #region BaseElement Tests

        [Fact]
        public void BaseElement_Id_SetAndGet()
        {
            var task = new UserTask { Id = "task_1" };
            Assert.Equal("task_1", task.Id);
        }

        [Fact]
        public void BaseElement_AddExtensionElement_AddsToCollection()
        {
            var task = new UserTask { Id = "task_1" };
            var ext = new ExtensionElement { Name = "customProp", ElementText = "test" };

            task.AddExtensionElement(ext);

            Assert.True(task.ExtensionElements.ContainsKey("customProp"));
            Assert.Single(task.ExtensionElements["customProp"]);
            Assert.Equal("customProp", task.ExtensionElements["customProp"][0].Name);
        }

        [Fact]
        public void BaseElement_AddExtensionElement_NullOrEmptyName_NotAdded()
        {
            var task = new UserTask { Id = "task_1" };

            task.AddExtensionElement(null);
            task.AddExtensionElement(new ExtensionElement { Name = "" });
            task.AddExtensionElement(new ExtensionElement { Name = null });

            Assert.Empty(task.ExtensionElements);
        }

        [Fact]
        public void BaseElement_AddAttribute_AddsToCollection()
        {
            var task = new UserTask { Id = "task_1" };
            var attr = new ExtensionAttribute { Name = "formKey", Value = "3" };

            task.AddAttribute(attr);

            Assert.True(task.Attributes.ContainsKey("formKey"));
            Assert.Equal("3", task.GetAttributeValue(null, "formKey"));
        }

        [Fact]
        public void BaseElement_Values_DeepCopiesExtensionElements()
        {
            var task1 = new UserTask { Id = "task_1" };
            var ext = new ExtensionElement { Name = "prop" };
            task1.AddExtensionElement(ext);

            var task2 = new UserTask();
            task2.Values = task1;

            // 修改原始对象不应影响拷贝
            task1.ExtensionElements["prop"][0].ElementText = "modified";
            Assert.NotEqual("modified", task2.ExtensionElements["prop"][0].ElementText);
        }

        [Fact]
        public void BaseElement_Values_NullArgument_Throws()
        {
            var task = new UserTask();
            Assert.Throws<ArgumentNullException>(() => task.Values = null);
        }

        [Fact]
        public void BaseElement_Equals_SameId_ReturnsTrue()
        {
            var task1 = new UserTask { Id = "task_1" };
            var task2 = new UserTask { Id = "task_1" };
            Assert.True(task1 == task2);
            Assert.True(task1.Equals(task2));
        }

        [Fact]
        public void BaseElement_Equals_DifferentId_ReturnsFalse()
        {
            var task1 = new UserTask { Id = "task_1" };
            var task2 = new UserTask { Id = "task_2" };
            Assert.False(task1 == task2);
        }

        [Fact]
        public void BaseElement_Equals_Null_ReturnsFalse()
        {
            var task = new UserTask { Id = "task_1" };
            Assert.False(task.Equals(null));
        }

        [Fact]
        public void BaseElement_GetHashCode_ConsistentWithEquals()
        {
            var task1 = new UserTask { Id = "task_1" };
            var task2 = new UserTask { Id = "task_1" };
            Assert.Equal(task1.GetHashCode(), task2.GetHashCode());
        }

        #endregion

        #region BpmnModel Tests

        [Fact]
        public void BpmnModel_AddProcess_IncreasesCount()
        {
            var model = new BpmnModel();
            var process = new Process { Id = "proc_1" };

            model.AddProcess(process);

            Assert.Single(model.Processes);
            Assert.Equal("proc_1", model.Processes[0].Id);
        }

        [Fact]
        public void BpmnModel_GetProcessById_ReturnsCorrectProcess()
        {
            var model = new BpmnModel();
            model.AddProcess(new Process { Id = "proc_1" });
            model.AddProcess(new Process { Id = "proc_2" });

            Process result = model.GetProcessById("proc_2");
            Assert.NotNull(result);
            Assert.Equal("proc_2", result.Id);
        }

        [Fact]
        public void BpmnModel_AddGraphicInfo_StoresLocation()
        {
            var model = new BpmnModel();
            var graphicInfo = new GraphicInfo { X = 100, Y = 200, Width = 80, Height = 60 };

            model.AddGraphicInfo("task_1", graphicInfo);

            GraphicInfo retrieved = model.GetGraphicInfo("task_1");
            Assert.NotNull(retrieved);
            Assert.Equal(100, retrieved.X);
            Assert.Equal(200, retrieved.Y);
        }

        [Fact]
        public void BpmnModel_AddSignal_IncreasesCount()
        {
            var model = new BpmnModel();
            var signal = new Signal { Id = "signal_1", Name = "Alert" };

            model.AddSignal(signal);

            Assert.Single(model.Signals);
            Assert.Equal("signal_1", model.Signals.First().Id);
        }

        [Fact]
        public void BpmnModel_AddMessage_StoresInMap()
        {
            var model = new BpmnModel();
            var message = new Message { Id = "msg_1", Name = "Notify" };

            model.AddMessage(message);

            Message retrieved = model.GetMessage("msg_1");
            Assert.NotNull(retrieved);
            Assert.Equal("Notify", retrieved.Name);
        }

        [Fact]
        public void BpmnModel_AddNamespace_StoresInMap()
        {
            var model = new BpmnModel();
            model.AddNamespace("bpmn2", "http://www.omg.org/spec/BPMN/20100524/MODEL");

            Assert.True(model.ContainsNamespacePrefix("bpmn2"));
        }

        #endregion

        #region UserTask Tests

        [Fact]
        public void UserTask_Properties_SetAndGet()
        {
            var task = new UserTask
            {
                Id = "task_1",
                Name = "审批",
                Assignee = "${user_}",
                FormKey = "3",
                Owner = "admin",
                Priority = "high",
                DueDate = "2026-01-01",
                Category = "approval"
            };

            Assert.Equal("task_1", task.Id);
            Assert.Equal("审批", task.Name);
            Assert.Equal("${user_}", task.Assignee);
            Assert.Equal("3", task.FormKey);
            Assert.Equal("admin", task.Owner);
            Assert.Equal("high", task.Priority);
        }

        [Fact]
        public void UserTask_CandidateUsers_AddAndCount()
        {
            var task = new UserTask { Id = "task_1" };
            task.CandidateUsers.Add("user1");
            task.CandidateUsers.Add("user2");

            Assert.Equal(2, task.CandidateUsers.Count);
            Assert.Contains("user1", task.CandidateUsers);
        }

        [Fact]
        public void UserTask_CandidateGroups_AddAndCount()
        {
            var task = new UserTask { Id = "task_1" };
            task.CandidateGroups.Add("managers");
            task.CandidateGroups.Add("admins");

            Assert.Equal(2, task.CandidateGroups.Count);
            Assert.Contains("managers", task.CandidateGroups);
        }

        [Fact]
        public void UserTask_Clone_PreservesProperties()
        {
            var task = new UserTask
            {
                Id = "task_1",
                Name = "审批",
                Assignee = "admin",
                FormKey = "3"
            };

            var clone = task.Clone() as UserTask;

            Assert.NotNull(clone);
            Assert.Equal("task_1", clone.Id);
            Assert.Equal("审批", clone.Name);
            Assert.Equal("admin", clone.Assignee);
            Assert.Equal("3", clone.FormKey);
        }

        [Fact]
        public void UserTask_Clone_IsIndependentCopy()
        {
            var task = new UserTask { Id = "task_1", Name = "审批" };
            var clone = task.Clone() as UserTask;

            clone.Name = "修改后";

            Assert.Equal("审批", task.Name);
        }

        #endregion

        #region Process Tests

        [Fact]
        public void Process_AddFlowElement_IncreasesCount()
        {
            var process = new Process { Id = "proc_1" };
            var task = new UserTask { Id = "task_1", Name = "任务" };

            process.AddFlowElement(task);

            Assert.Single(process.FlowElements);
            Assert.Equal("task_1", process.FindFlowElement("task_1").Id);
        }

        [Fact]
        public void Process_FindFlowElement_ReturnsCorrectElement()
        {
            var process = new Process { Id = "proc_1" };
            process.AddFlowElement(new StartEvent { Id = "start_1" });
            process.AddFlowElement(new UserTask { Id = "task_1", Name = "任务" });
            process.AddFlowElement(new EndEvent { Id = "end_1" });

            FlowElement found = process.FindFlowElement("task_1");
            Assert.NotNull(found);
            Assert.IsType<UserTask>(found);
        }

        [Fact]
        public void Process_FindFlowElementsOfType_ReturnsCorrectType()
        {
            var process = new Process { Id = "proc_1" };
            process.AddFlowElement(new StartEvent { Id = "start_1" });
            process.AddFlowElement(new UserTask { Id = "task_1" });
            process.AddFlowElement(new UserTask { Id = "task_2" });
            process.AddFlowElement(new EndEvent { Id = "end_1" });

            var userTasks = process.FindFlowElementsOfType<UserTask>();
            Assert.Equal(2, userTasks.Count);
        }

        [Fact]
        public void Process_AddFlowElement_SetsParentContainer()
        {
            var process = new Process { Id = "proc_1" };
            var task = new UserTask { Id = "task_1" };

            process.AddFlowElement(task);

            Assert.Equal(process, task.ParentContainer);
        }

        [Fact]
        public void Process_Name_SetAndGet()
        {
            var process = new Process { Id = "proc_1", Name = "测试流程" };
            Assert.Equal("测试流程", process.Name);
        }

        [Fact]
        public void Process_Executable_DefaultTrue()
        {
            var process = new Process { Id = "proc_1" };
            Assert.True(process.Executable);
        }

        #endregion

        #region ExtensionElement Tests

        [Fact]
        public void ExtensionElement_Clone_DeepCopiesChildElements()
        {
            var parent = new ExtensionElement { Name = "parent" };
            var child = new ExtensionElement { Name = "child", ElementText = "hello" };
            parent.AddChildElement(child);

            var clone = parent.Clone() as ExtensionElement;

            Assert.NotNull(clone);
            Assert.True(clone.ChildElements.ContainsKey("child"));
            Assert.Equal("hello", clone.ChildElements["child"][0].ElementText);

            // 修改原始不影响克隆
            child.ElementText = "modified";
            Assert.Equal("hello", clone.ChildElements["child"][0].ElementText);
        }

        [Fact]
        public void ExtensionElement_AddChildElement_NullOrEmptyName_NotAdded()
        {
            var parent = new ExtensionElement { Name = "parent" };

            parent.AddChildElement(null);
            parent.AddChildElement(new ExtensionElement { Name = "" });

            Assert.Empty(parent.ChildElements);
        }

        [Fact]
        public void ExtensionElement_Properties_SetAndGet()
        {
            var ext = new ExtensionElement
            {
                Name = "taskListener",
                NamespacePrefix = "camunda",
                Namespace = "http://camunda.org/schema/1.0/bpmn",
                ElementText = "class name"
            };

            Assert.Equal("taskListener", ext.Name);
            Assert.Equal("camunda", ext.NamespacePrefix);
            Assert.Equal("http://camunda.org/schema/1.0/bpmn", ext.Namespace);
            Assert.Equal("class name", ext.ElementText);
        }

        #endregion

        #region SequenceFlow Tests

        [Fact]
        public void SequenceFlow_Properties_SetAndGet()
        {
            var flow = new SequenceFlow
            {
                Id = "flow_1",
                Name = "to task",
                SourceRef = "start_1",
                TargetRef = "task_1"
            };

            Assert.Equal("flow_1", flow.Id);
            Assert.Equal("to task", flow.Name);
            Assert.Equal("start_1", flow.SourceRef);
            Assert.Equal("task_1", flow.TargetRef);
        }

        #endregion
    }
}
