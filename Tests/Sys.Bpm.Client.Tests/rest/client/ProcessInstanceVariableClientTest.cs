using Newtonsoft.Json.Linq;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sys.Workflow.Client.Tests.Rest.Client
{
    //[Order(6)]
    public class ProcessInstanceVariableClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();
        private readonly IProcessInstanceController client = null;

        public ProcessInstanceVariableClientTest()
        {
            client = ctx.CreateWorkflowHttpProxy().GetProcessInstanceClient();
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetVariables_启动流程时设置流程实例变量_读取流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                Guid guidValue = Guid.NewGuid();
                long longValue = new Random().Next(100, 100000);
                int intValue = new Random().Next(1, 10000);
                decimal decValue = 10000M * (decimal)new Random().NextDouble();
                DateTime dateValue = DateTime.Now;
                string textValue = "this is a string.";
                string uid = Guid.NewGuid().ToString();

                ProcessInstance[] inst = ctx.StartUseFile(bpmnFile, new string[] { uid }, new Dictionary<string, object>
                {
                    { "guidValue",  guidValue},
                    { "longValue", longValue},
                    { "intValue", intValue},
                    { "decValue", decValue},
                    { "dateValue", dateValue},
                    { "textValue", textValue}
                }).GetAwaiter().GetResult();

                Assert.NotNull(inst);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Resources<ProcessInstanceVariable> variables = varClient.GetVariables(inst[0].Id).GetAwaiter().GetResult();

                foreach (var var in variables.List)
                {
                    switch (var.Name)
                    {
                        case "guidValue":
                            Assert.True(guidValue == new Guid(var.Value.ToString()));
                            break;
                        case "longValue":
                            Assert.True(longValue == long.Parse(var.Value.ToString()));
                            break;
                        case "intValue":
                            Assert.True(intValue == int.Parse(var.Value.ToString()));
                            break;
                        case "decValue":
                            Assert.True(decValue == decimal.Parse(var.Value.ToString()));
                            break;
                        case "dateValue":
                            Assert.True(dateValue == (DateTime)var.Value);
                            break;
                        case "textValue":
                            Assert.True(textValue == var.Value.ToString());
                            break;
                        case "name":
                            Assert.Equal(uid, JToken.FromObject(var.Value).ToObject<string[]>()[0], ignoreCase: true);
                            break;
                    }
                }
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetVariablesLocal_启动流程后设置实例变量_读取流程实例(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] inst = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();
                long longValue = new Random().Next(100, 100000);
                int intValue = new Random().Next(1, 10000);
                decimal decValue = 10000M * (decimal)new Random().NextDouble();
                DateTime dateValue = DateTime.Now;
                string textValue = "this is a string.";

                varClient.SetVariables(new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                 {
                    { "guidValue",  guidValue},
                    { "longValue", longValue},
                    { "intValue", intValue},
                    { "decValue", decValue},
                    { "dateValue", dateValue},
                    { "textValue", textValue}
                 })).GetAwaiter().GetResult();

                Resources<ProcessInstanceVariable> variables = varClient.GetVariablesLocal(inst[0].Id).GetAwaiter().GetResult();

                foreach (var var in variables.List)
                {
                    switch (var.Name)
                    {
                        case "guidValue":
                            Assert.True(guidValue == new Guid(var.Value.ToString()));
                            break;
                        case "longValue":
                            Assert.True(longValue == long.Parse(var.Value.ToString()));
                            break;
                        case "intValue":
                            Assert.True(intValue == int.Parse(var.Value.ToString()));
                            break;
                        case "decValue":
                            Assert.True(decValue == decimal.Parse(var.Value.ToString()));
                            break;
                        case "dateValue":
                            Assert.True(dateValue == (DateTime)var.Value);
                            break;
                        case "textValue":
                            Assert.True(textValue == var.Value.ToString());
                            break;
                        case "name":
                            Assert.Equal(uid, JToken.FromObject(var.Value).ToObject<string[]>()[0], ignoreCase: true);
                            break;
                    }
                }
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void SetVariables_启动流程实例设置实例变量_改变流程变量(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] inst = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();

                Resources<ProcessInstanceVariable> variables = null;

                varClient.SetVariables(new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                    {
                        { "guidValue",  guidValue}
                    })).GetAwaiter().GetResult();

                variables = varClient.GetVariables(inst[0].Id).GetAwaiter().GetResult();

                ProcessInstanceVariable varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.True(guidValue == new Guid(varInst.Value.ToString()));

                guidValue = Guid.NewGuid();
            
                varClient.SetVariables(new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                    {
                        { "guidValue", guidValue }
                    })).GetAwaiter().GetResult();

                variables = varClient.GetVariables(inst[0].Id).GetAwaiter().GetResult();

                varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.True(guidValue == new Guid(varInst.Value.ToString()));
            });

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void RemoveVariables_启动流程实例_设置变量_移除变量(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] inst = ctx.StartUseFile(bpmnFile, new string[] { uid }).GetAwaiter().GetResult();

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();

                varClient.SetVariables(new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                {
                    { "guidValue",  guidValue}
                })).GetAwaiter().GetResult();

                Resources<ProcessInstanceVariable> variables = varClient.GetVariables(inst[0].Id).GetAwaiter().GetResult();

                ProcessInstanceVariable varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.True(guidValue == new Guid(varInst.Value.ToString()));

                varClient.RemoveVariables(new RemoveProcessVariablesCmd(inst[0].Id, new string[] { "guidValue" })).GetAwaiter().GetResult();

                variables = varClient.GetVariables(inst[0].Id).GetAwaiter().GetResult();

                varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.Null(varInst);
            });

            Assert.Null(ex);
        }
    }
}
