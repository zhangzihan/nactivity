using Newtonsoft.Json.Linq;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Bpmn.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Client.Tests.rest.client
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

                ProcessInstance[] inst = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[0], new Dictionary<string, object>
                {
                    { "guidValue",  guidValue},
                    { "longValue", longValue},
                    { "intValue", intValue},
                    { "decValue", decValue},
                    { "dateValue", dateValue},
                    { "textValue", textValue}
                }));

                Assert.NotNull(inst);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Resources<ProcessInstanceVariable> variables = AsyncHelper.RunSync(() => varClient.GetVariables(inst[0].Id));

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
                ProcessInstance[] inst = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();
                long longValue = new Random().Next(100, 100000);
                int intValue = new Random().Next(1, 10000);
                decimal decValue = 10000M * (decimal)new Random().NextDouble();
                DateTime dateValue = DateTime.Now;
                string textValue = "this is a string.";

                AsyncHelper.RunSync(() => varClient.SetVariables(inst[0].Id, new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                 {
                    { "guidValue",  guidValue},
                    { "longValue", longValue},
                    { "intValue", intValue},
                    { "decValue", decValue},
                    { "dateValue", dateValue},
                    { "textValue", textValue}
                 })));

                Resources<ProcessInstanceVariable> variables = AsyncHelper.RunSync(() => varClient.GetVariablesLocal(inst[0].Id));

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
                ProcessInstance[] inst = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();

                Resources<ProcessInstanceVariable> variables = null;

                AsyncHelper.RunSync(() => varClient.SetVariables(inst[0].Id, new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                    {
                        { "guidValue",  guidValue}
                    })));

                variables = AsyncHelper.RunSync<Resources<ProcessInstanceVariable>>(() => varClient.GetVariables(inst[0].Id));

                ProcessInstanceVariable varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.True(guidValue == new Guid(varInst.Value.ToString()));

                guidValue = Guid.NewGuid();

                AsyncHelper.RunSync(() => varClient.SetVariables(inst[0].Id, new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                    {
                        { "guidValue", guidValue }
                    })));

                variables = AsyncHelper.RunSync<Resources<ProcessInstanceVariable>>(() => varClient.GetVariables(inst[0].Id));

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
                ProcessInstance[] inst = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                Assert.True(inst.Length > 0);

                IProcessInstanceVariableController varClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceVariableClient();

                Guid guidValue = Guid.NewGuid();

                AsyncHelper.RunSync(() => varClient.SetVariables(inst[0].Id, new SetProcessVariablesCmd(inst[0].Id, new Dictionary<string, object>
                {
                    { "guidValue",  guidValue}
                })));

                Resources<ProcessInstanceVariable> variables = AsyncHelper.RunSync<Resources<ProcessInstanceVariable>>(() => varClient.GetVariables(inst[0].Id));

                ProcessInstanceVariable varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.True(guidValue == new Guid(varInst.Value.ToString()));

                AsyncHelper.RunSync(() => varClient.RemoveVariables(inst[0].Id, new RemoveProcessVariablesCmd(inst[0].Id, new string[] { "guidValue" })));

                variables = AsyncHelper.RunSync<Resources<ProcessInstanceVariable>>(() => varClient.GetVariables(inst[0].Id));

                varInst = variables.List.FirstOrDefault(x => x.Name == "guidValue");

                Assert.Null(varInst);
            });

            Assert.Null(ex);
        }
    }
}
