using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow;
using Sys.Workflow.Bpmn.Converters;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using Sys.Workflow.Engine.Impl.Bpmn.Parser.Factory;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Rules.DmnEngine.BusinessRuleTaskTests
{
    /// <summary>
    /// 定义 BusinessRuleTask 接入 DMN 运行时后的最小回归测试。
    /// </summary>
    public class DmnBusinessRuleTaskActivityBehaviorTests
    {
        private const string DecisionBindingSampleFileName = "business-rule-task-decision-binding.bpmn";

        /// <summary>
        /// 获取 BPMN 测试样例路径。
        /// </summary>
        /// <param name="fileName">样例文件名。</param>
        /// <returns>样例文件相对路径。</returns>
        private static string GetSamplePath(string fileName)
        {
            return Path.Combine("resources", "samples", fileName);
        }

        /// <summary>
        /// 验证工厂在未配置 ClassName 时默认返回 DMN 业务规则任务行为。
        /// </summary>
        [Fact]
        public void CreateBusinessRuleTaskActivityBehavior_Should_ReturnDmnBehavior_WhenClassNameIsEmpty()
        {
            var factory = new DefaultActivityBehaviorFactory();
            var businessRuleTask = new BusinessRuleTask
            {
                Id = "task_business_rule",
                ResultVariableName = "dmnResult"
            };

            var behavior = factory.CreateBusinessRuleTaskActivityBehavior(businessRuleTask);

            Assert.IsType<DmnBusinessRuleTaskActivityBehavior>(behavior);
        }

        /// <summary>
        /// 验证业务规则任务行为会调用 DMN 运行时并回写输出变量。
        /// </summary>
        [Fact]
        public void Execute_Should_InvokeDecisionRuntime_And_WriteOutputs()
        {
            var decisionExecutionService = new StubDecisionExecutionService();
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = "dmnResult"
            };

            behavior.AddRuleIdExpression(new LiteralExpression("single-root-expression#Age +10"));
            behavior.AddRuleVariableInputIdExpression(new LiteralExpression("Age"));

            var execution = new TestExecutionEntity
            {
                Id = "execution_1",
                CurrentFlowElement = new BusinessRuleTask
                {
                    Id = "task_business_rule"
                }
            };

            execution.SetVariable("Age", 32, false);
            execution.SetVariable("Ignored", "skip", false);

            behavior.Execute(execution);

            Assert.True(behavior.LeaveCalled);
            Assert.NotNull(decisionExecutionService.LastRequest);
            Assert.Equal("single-root-expression", decisionExecutionService.LastRequest!.DecisionKey);
            Assert.Equal("Age +10", decisionExecutionService.LastRequest.Inputs["__decisionName"]);
            Assert.Equal(32, decisionExecutionService.LastRequest.Inputs["Age"]);
            Assert.False(decisionExecutionService.LastRequest.Inputs.ContainsKey("Ignored"));
            Assert.Equal(42, execution.GetVariable("Age10", false));

            var resultPayload = Assert.IsType<Dictionary<string, object>>(execution.GetVariable("dmnResult", false));
            Assert.Equal("exec-single-root", resultPayload["executionId"]);
            Assert.Equal("single-root-expression", resultPayload["decisionKey"]);
            Assert.True((bool)resultPayload["succeeded"]);
            Assert.Equal("v2.0.0", resultPayload["resolvedVersionTag"]);
            Assert.Equal("sample explanation", resultPayload["explanation"]);

            var outputs = Assert.IsType<Dictionary<string, object?>>(resultPayload["outputs"]);
            Assert.Equal(42, outputs["Age10"]);
        }

        /// <summary>
        /// 验证结构化扩展绑定优先于旧规则字段，并可应用输入输出映射。
        /// </summary>
        [Fact]
        public void Execute_Should_UseStructuredDecisionBinding_WhenExtensionElementExists()
        {
            var decisionExecutionService = new StubDecisionExecutionService();
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = "dmnResult"
            };

            behavior.AddRuleIdExpression(new LiteralExpression("legacy-decision#legacy-name"));
            behavior.AddRuleVariableInputIdExpression(new LiteralExpression("LegacyAmount"));

            var businessRuleTask = new BusinessRuleTask
            {
                Id = "task_business_rule"
            };
            businessRuleTask.AddExtensionElement(CreateDecisionBindingExtensionElement());

            var execution = new TestExecutionEntity
            {
                Id = "execution_2",
                CurrentFlowElement = businessRuleTask
            };

            execution.SetVariable("Amount", 6800, false);
            execution.SetVariable("LegacyAmount", 1, false);

            behavior.Execute(execution);

            Assert.True(behavior.LeaveCalled);
            Assert.NotNull(decisionExecutionService.LastRequest);
            Assert.Equal("expense.approval", decisionExecutionService.LastRequest!.DecisionKey);
            Assert.Equal("v1.2.0", decisionExecutionService.LastRequest.VersionTag);
            Assert.Equal("费用审批规则", decisionExecutionService.LastRequest.Inputs["__decisionName"]);
            Assert.Equal(6800, decisionExecutionService.LastRequest.Inputs["amount"]);
            Assert.False(decisionExecutionService.LastRequest.Inputs.ContainsKey("LegacyAmount"));
            Assert.Equal(true, execution.GetVariable("approved", false));
            Assert.Equal("manager-1", execution.GetVariable("nextAssignee", false));
        }

        /// <summary>
        /// 验证可从真实 BPMN XML 解析结构化决策绑定，并驱动规则任务执行。
        /// </summary>
        [Fact]
        public void Execute_Should_ParseDecisionBindingFromBpmnXml_And_WriteOutputs()
        {
            var converter = new BpmnXMLConverter();
            var model = converter.ConvertToBpmnModel(GetSamplePath(DecisionBindingSampleFileName));
            var process = model.GetProcessById("Process_BusinessRuleTaskDecisionBinding");
            var businessRuleTask = Assert.Single(process.FindFlowElementsOfType<BusinessRuleTask>());
            var decisionBinding = businessRuleTask.GetDecisionBinding();

            Assert.NotNull(decisionBinding);
            Assert.Equal("expense.approval", decisionBinding!.DecisionKey);
            Assert.Equal("费用审批规则", decisionBinding.DecisionName);
            Assert.Equal("DecisionTable", decisionBinding.DecisionType);
            Assert.Equal("FixedVersion", decisionBinding.BindingMode);
            Assert.Equal("Sync", decisionBinding.ExecutionMode);
            Assert.True(decisionBinding.TraceEnabled);
            Assert.Equal("ManualIntervention", decisionBinding.FallbackPolicy);
            Assert.Equal(3000, decisionBinding.TimeoutMs);
            Assert.Equal("v1.2.0", decisionBinding.VersionTag);
            Assert.Equal(3, decisionBinding.InputMappings.Count);
            Assert.Equal(2, decisionBinding.OutputMappings.Count);
            Assert.Equal(3, decisionBinding.Options.Count);
            Assert.Equal("manualReviewContext", decisionBinding.Options[0].Value);

            var factory = new DefaultActivityBehaviorFactory();
            var createdBehavior = factory.CreateBusinessRuleTaskActivityBehavior(businessRuleTask);
            Assert.IsType<DmnBusinessRuleTaskActivityBehavior>(createdBehavior);

            var decisionExecutionService = new StubDecisionExecutionService();
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = businessRuleTask.ResultVariableName ?? "dmnResult"
            };

            var execution = new TestExecutionEntity
            {
                Id = "execution_xml_1",
                CurrentFlowElement = businessRuleTask
            };

            execution.SetVariable("Amount", 6800, false);
            execution.SetVariable(
                "initiator",
                new Dictionary<string, object?>
                {
                    ["deptId"] = "FIN-001"
                },
                false);

            behavior.Execute(execution);

            Assert.True(behavior.LeaveCalled);
            Assert.NotNull(decisionExecutionService.LastRequest);
            Assert.Equal("expense.approval", decisionExecutionService.LastRequest!.DecisionKey);
            Assert.Equal("DecisionTable", decisionExecutionService.LastRequest.DecisionType);
            Assert.Equal("FixedVersion", decisionExecutionService.LastRequest.BindingMode);
            Assert.Equal("Sync", decisionExecutionService.LastRequest.ExecutionMode);
            Assert.True(decisionExecutionService.LastRequest.TraceEnabled);
            Assert.Equal(3000, decisionExecutionService.LastRequest.TimeoutMs);
            Assert.Equal("v1.2.0", decisionExecutionService.LastRequest.VersionTag);
            Assert.Equal("费用审批规则", decisionExecutionService.LastRequest.Inputs["__decisionName"]);
            Assert.Equal(6800, decisionExecutionService.LastRequest.Inputs["amount"]);
            Assert.Equal("web", decisionExecutionService.LastRequest.Inputs["channel"]);
            Assert.Equal("FIN-001", decisionExecutionService.LastRequest.Inputs["deptId"]);
            Assert.Equal(true, execution.GetVariable("approved", false));
            Assert.Equal("manager-1", execution.GetVariable("nextAssignee", false));

            var resultPayload = Assert.IsType<Dictionary<string, object>>(execution.GetVariable("dmnResult", false));
            Assert.Equal("exec-expense-approval", resultPayload["executionId"]);
            Assert.Equal("expense.approval", resultPayload["decisionKey"]);
            Assert.Equal("DecisionTable", resultPayload["decisionType"]);
            Assert.Equal("v1.2.0", resultPayload["resolvedVersionTag"]);
            Assert.True((bool)resultPayload["traceEnabled"]);
            var traceEntries = Assert.IsType<List<string>>(resultPayload["traceEntries"]);
            Assert.NotEmpty(traceEntries);
        }

        /// <summary>
        /// 验证 BPMN 往返序列化后仍能保留结构化决策绑定。
        /// </summary>
        [Fact]
        public void RoundTrip_Should_PreserveDecisionBindingExtensionElements()
        {
            var converter = new BpmnXMLConverter();
            var model = converter.ConvertToBpmnModel(GetSamplePath(DecisionBindingSampleFileName));

            var xmlBytes = converter.ConvertToXML(model);
            var xmlContent = Encoding.UTF8.GetString(xmlBytes);

            Assert.Contains("qh:decisionBinding", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("decisionKey=\"expense.approval\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("decisionType=\"DecisionTable\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("bindingMode=\"FixedVersion\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("executionMode=\"Sync\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("traceEnabled=\"true\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("fallbackPolicy=\"ManualIntervention\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("timeoutMs=\"3000\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sourceType=\"constant\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sourceType=\"contextPath\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("name=\"manualIntervention.variableName\"", xmlContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("value=\"DMN_REVIEW_REQUIRED\"", xmlContent, StringComparison.OrdinalIgnoreCase);

            using var stream = new MemoryStream(xmlBytes);
            var roundTripModel = converter.ConvertToBpmnModel(stream);
            var process = roundTripModel.GetProcessById("Process_BusinessRuleTaskDecisionBinding");
            var businessRuleTask = Assert.Single(process.FindFlowElementsOfType<BusinessRuleTask>());
            var decisionBinding = businessRuleTask.GetDecisionBinding();

            Assert.NotNull(decisionBinding);
            Assert.Equal("expense.approval", decisionBinding!.DecisionKey);
            Assert.Equal("DecisionTable", decisionBinding.DecisionType);
            Assert.Equal("FixedVersion", decisionBinding.BindingMode);
            Assert.Equal("Sync", decisionBinding.ExecutionMode);
            Assert.True(decisionBinding.TraceEnabled);
            Assert.Equal("ManualIntervention", decisionBinding.FallbackPolicy);
            Assert.Equal(3000, decisionBinding.TimeoutMs);
            Assert.Equal(3, decisionBinding.InputMappings.Count);
            Assert.Equal("channel", decisionBinding.InputMappings[1].InputName);
            Assert.Equal("constant", decisionBinding.InputMappings[1].SourceType);
            Assert.Equal("web", decisionBinding.InputMappings[1].SourceValue);
            Assert.Equal("deptId", decisionBinding.InputMappings[2].InputName);
            Assert.Equal("contextPath", decisionBinding.InputMappings[2].SourceType);
            Assert.Equal("initiator.deptId", decisionBinding.InputMappings[2].SourceValue);
            Assert.Equal(2, decisionBinding.OutputMappings.Count);
            Assert.Equal(3, decisionBinding.Options.Count);
            Assert.Equal("manualIntervention.errorCode", decisionBinding.Options[1].Name);
            Assert.Equal("DMN_REVIEW_REQUIRED", decisionBinding.Options[1].Value);
        }

        /// <summary>
        /// 验证在 SkipTask 回退策略下，规则运行失败时会跳过当前任务继续离开节点。
        /// </summary>
        [Fact]
        public void Execute_Should_SkipTask_WhenFallbackPolicyIsSkipTask_AndDecisionExecutionFails()
        {
            var decisionExecutionService = new StubDecisionExecutionService
            {
                ThrowOnExpenseApproval = true
            };
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = "dmnResult"
            };

            var businessRuleTask = new BusinessRuleTask
            {
                Id = "task_business_rule"
            };
            businessRuleTask.AddExtensionElement(CreateDecisionBindingExtensionElement(fallbackPolicy: "SkipTask"));

            var execution = new TestExecutionEntity
            {
                Id = "execution_skip_1",
                CurrentFlowElement = businessRuleTask
            };

            execution.SetVariable("Amount", 6800, false);

            behavior.Execute(execution);

            Assert.True(behavior.LeaveCalled);
            Assert.Null(execution.GetVariable("dmnResult", false));
        }

        /// <summary>
        /// 验证在 UseDefault 回退策略下，规则执行失败时会写入默认输出并继续流程。
        /// </summary>
        [Fact]
        public void Execute_Should_ApplyDefaultOutputs_WhenFallbackPolicyIsUseDefault_AndDecisionExecutionFails()
        {
            var decisionExecutionService = new StubDecisionExecutionService
            {
                ThrowOnExpenseApproval = true
            };
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = "dmnResult"
            };

            var businessRuleTask = new BusinessRuleTask
            {
                Id = "task_business_rule"
            };
            businessRuleTask.AddExtensionElement(CreateDecisionBindingExtensionElement(
                fallbackPolicy: "UseDefault",
                options: new Dictionary<string, string>
                {
                    ["defaultOutput.approved"] = "true",
                    ["defaultOutput.nextAssignee"] = "backup-manager"
                }));

            var execution = new TestExecutionEntity
            {
                Id = "execution_default_1",
                CurrentFlowElement = businessRuleTask
            };

            execution.SetVariable("Amount", 6800, false);

            behavior.Execute(execution);

            Assert.True(behavior.LeaveCalled);
            Assert.True((bool)execution.GetVariable("approved", false));
            Assert.Equal("backup-manager", execution.GetVariable("nextAssignee", false));

            var resultPayload = Assert.IsType<Dictionary<string, object>>(execution.GetVariable("dmnResult", false));
            Assert.False((bool)resultPayload["succeeded"]);
            Assert.True((bool)resultPayload["fallbackApplied"]);
            Assert.Equal("UseDefault", resultPayload["fallbackPolicy"]);
            Assert.Equal("expense approval failed", resultPayload["fallbackReason"]);

            var outputs = Assert.IsAssignableFrom<IDictionary<string, object?>>(resultPayload["outputs"]);
            Assert.True((bool)outputs["approved"]!);
            Assert.Equal("backup-manager", outputs["nextAssignee"]);
        }

        /// <summary>
        /// 验证在 ManualIntervention 回退策略下，规则执行失败时会写入人工介入变量并传播 BPMN 错误。
        /// </summary>
        [Fact]
        public void Execute_Should_WriteManualInterventionContext_AndPropagateBpmnError_WhenFallbackPolicyIsManualIntervention()
        {
            var decisionExecutionService = new StubDecisionExecutionService
            {
                ThrowOnExpenseApproval = true
            };
            using var serviceProviderScope = new ProcessEngineServiceProviderScope(decisionExecutionService);

            var behavior = new TestDmnBusinessRuleTaskActivityBehavior
            {
                ResultVariable = "dmnResult"
            };

            var businessRuleTask = new BusinessRuleTask
            {
                Id = "task_business_rule"
            };
            businessRuleTask.AddExtensionElement(CreateDecisionBindingExtensionElement(
                fallbackPolicy: "ManualIntervention",
                options: new Dictionary<string, string>
                {
                    ["manualIntervention.variableName"] = "manualReviewContext",
                    ["manualIntervention.errorCode"] = "DMN_REVIEW_REQUIRED",
                    ["manualIntervention.errorMessage"] = "规则执行失败，请转人工复核。"
                }));

            var execution = new TestExecutionEntity
            {
                Id = "execution_manual_1",
                CurrentFlowElement = businessRuleTask
            };

            execution.SetVariable("Amount", 6800, false);

            behavior.Execute(execution);

            Assert.False(behavior.LeaveCalled);
            Assert.NotNull(behavior.PropagatedBpmnError);
            Assert.Equal("DMN_REVIEW_REQUIRED", GetBpmnErrorCode(behavior.PropagatedBpmnError!));
            Assert.Equal("规则执行失败，请转人工复核。", behavior.PropagatedBpmnError!.Message);

            var manualContext = Assert.IsAssignableFrom<IDictionary<string, object?>>(execution.GetVariable("manualReviewContext", false));
            Assert.Equal("expense.approval", manualContext["decisionKey"]);
            Assert.Equal("ManualIntervention", manualContext["fallbackPolicy"]);
            Assert.Equal("expense approval failed", manualContext["errorMessage"]);

            var resultPayload = Assert.IsType<Dictionary<string, object>>(execution.GetVariable("dmnResult", false));
            Assert.False((bool)resultPayload["succeeded"]);
            Assert.True((bool)resultPayload["fallbackApplied"]);
            Assert.Equal("ManualIntervention", resultPayload["fallbackPolicy"]);

            var fallbackContext = Assert.IsAssignableFrom<IDictionary<string, object?>>(resultPayload["fallbackContext"]);
            Assert.Equal("execution_manual_1", fallbackContext["executionId"]);
        }

        /// <summary>
        /// 定义用于测试的固定表达式。
        /// </summary>
        private sealed class LiteralExpression : IExpression
        {
            /// <summary>
            /// 初始化固定表达式。
            /// </summary>
            /// <param name="expressionText">表达式文本。</param>
            public LiteralExpression(string expressionText)
            {
                ExpressionText = expressionText;
            }

            /// <summary>
            /// 获取表达式文本。
            /// </summary>
            public string ExpressionText { get; }

            /// <summary>
            /// 直接返回表达式文本。
            /// </summary>
            /// <param name="variableScope">变量作用域。</param>
            /// <returns>表达式值。</returns>
            public object GetValue(IVariableScope variableScope)
            {
                return ExpressionText;
            }

            /// <summary>
            /// 测试表达式不支持反向赋值。
            /// </summary>
            /// <param name="value">待设置值。</param>
            /// <param name="variableScope">变量作用域。</param>
            public void SetValue(object value, IVariableScope variableScope)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 创建结构化决策绑定扩展元素。
        /// </summary>
        /// <returns>决策绑定扩展元素。</returns>
        private static ExtensionElement CreateDecisionBindingExtensionElement(
            string fallbackPolicy = "ManualIntervention",
            IDictionary<string, string>? options = null)
        {
            var decisionBinding = CreateExtensionElement(
                "decisionBinding",
                new Dictionary<string, string>
                {
                    ["decisionKey"] = "expense.approval",
                    ["decisionName"] = "费用审批规则",
                    ["decisionType"] = "DecisionTable",
                    ["bindingMode"] = "FixedVersion",
                    ["executionMode"] = "Sync",
                    ["traceEnabled"] = "true",
                    ["fallbackPolicy"] = fallbackPolicy,
                    ["timeoutMs"] = "3000",
                    ["versionTag"] = "v1.2.0"
                });

            var inputMappings = CreateExtensionElement("inputMappings");
            inputMappings.AddChildElement(CreateExtensionElement(
                "inputMapping",
                new Dictionary<string, string>
                {
                    ["inputName"] = "amount",
                    ["sourceType"] = "processVariable",
                    ["sourceValue"] = "Amount"
                }));

            var outputMappings = CreateExtensionElement("outputMappings");
            outputMappings.AddChildElement(CreateExtensionElement(
                "outputMapping",
                new Dictionary<string, string>
                {
                    ["outputName"] = "approved",
                    ["targetType"] = "processVariable",
                    ["targetValue"] = "approved"
                }));
            outputMappings.AddChildElement(CreateExtensionElement(
                "outputMapping",
                new Dictionary<string, string>
                {
                    ["outputName"] = "nextAssignee",
                    ["targetType"] = "processVariable",
                    ["targetValue"] = "nextAssignee"
                }));

            decisionBinding.AddChildElement(inputMappings);
            decisionBinding.AddChildElement(outputMappings);

            if (options != null)
            {
                foreach (var option in options)
                {
                    decisionBinding.AddChildElement(CreateExtensionElement(
                        "option",
                        new Dictionary<string, string>
                        {
                            ["name"] = option.Key,
                            ["value"] = option.Value
                        }));
                }
            }

            return decisionBinding;
        }

        /// <summary>
        /// 读取 BPMN 错误编码。
        /// </summary>
        /// <param name="error">BPMN 错误对象。</param>
        /// <returns>错误编码。</returns>
        private static string GetBpmnErrorCode(BpmnError error)
        {
            var propertyInfo = typeof(BpmnError).GetProperty("ErrorCode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (string?)propertyInfo?.GetValue(error) ?? string.Empty;
        }

        /// <summary>
        /// 创建测试用扩展元素。
        /// </summary>
        /// <param name="name">元素名。</param>
        /// <param name="attributes">属性集合。</param>
        /// <returns>扩展元素。</returns>
        private static ExtensionElement CreateExtensionElement(string name, IDictionary<string, string>? attributes = null)
        {
            var extensionElement = new ExtensionElement
            {
                Name = name,
                NamespacePrefix = "qh",
                Namespace = "https://sys.workflow/qhflow"
            };

            if (attributes == null)
            {
                return extensionElement;
            }

            foreach (var attribute in attributes)
            {
                extensionElement.AddAttribute(new ExtensionAttribute
                {
                    Name = attribute.Key,
                    Value = attribute.Value
                });
            }

            return extensionElement;
        }

        /// <summary>
        /// 定义用于测试的业务规则任务行为，以便屏蔽离开节点时的引擎依赖。
        /// </summary>
        private sealed class TestDmnBusinessRuleTaskActivityBehavior : DmnBusinessRuleTaskActivityBehavior
        {
            /// <summary>
            /// 获取是否已调用 Leave。
            /// </summary>
            public bool LeaveCalled { get; private set; }

            /// <summary>
            /// 获取最近一次传播的 BPMN 错误。
            /// </summary>
            public BpmnError? PropagatedBpmnError { get; private set; }

            /// <summary>
            /// 记录 Leave 调用，避免进入完整引擎链路。
            /// </summary>
            /// <param name="execution">当前执行上下文。</param>
            public override void Leave(IExecutionEntity execution)
            {
                LeaveCalled = true;
            }

            /// <summary>
            /// 记录 BPMN 错误传播，避免进入完整引擎链路。
            /// </summary>
            /// <param name="execution">当前执行上下文。</param>
            /// <param name="error">待传播的 BPMN 错误。</param>
            protected override void PropagateBpmnError(IExecutionEntity execution, BpmnError error)
            {
                PropagatedBpmnError = error;
            }
        }

        /// <summary>
        /// 定义用于测试的简化执行实体。
        /// </summary>
        private sealed class TestExecutionEntity : ExecutionEntityImpl
        {
            private readonly IDictionary<string, object> _variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// 获取或设置测试变量集合。
            /// </summary>
            public override IDictionary<string, object> Variables
            {
                get => _variables;
                set
                {
                    _variables.Clear();
                    foreach (var item in value)
                    {
                        _variables[item.Key] = item.Value;
                    }
                }
            }

            /// <summary>
            /// 获取指定变量的当前值。
            /// </summary>
            /// <param name="variableName">变量名。</param>
            /// <param name="fetchAllVariables">是否抓取全部变量。</param>
            /// <returns>变量值。</returns>
            public override object GetVariable(string variableName, bool fetchAllVariables)
            {
                _variables.TryGetValue(variableName, out var value);
                return value!;
            }

            /// <summary>
            /// 写入指定变量值。
            /// </summary>
            /// <param name="variableName">变量名。</param>
            /// <param name="value">变量值。</param>
            /// <param name="fetchAllVariables">是否抓取全部变量。</param>
            public override void SetVariable(string variableName, object value, bool fetchAllVariables)
            {
                _variables[variableName] = value;
            }
        }

        /// <summary>
        /// 定义用于测试的决策运行时桩实现。
        /// </summary>
        private sealed class StubDecisionExecutionService : IDecisionExecutionService
        {
            /// <summary>
            /// 获取或设置是否在费用审批规则执行时主动抛出异常。
            /// </summary>
            public bool ThrowOnExpenseApproval { get; set; }

            /// <summary>
            /// 获取最近一次执行请求。
            /// </summary>
            public DecisionExecutionRequest? LastRequest { get; private set; }

            /// <summary>
            /// 测试桩不提供元数据查询。
            /// </summary>
            /// <param name="decisionKey">决策键。</param>
            /// <param name="versionTag">版本标签。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<DecisionDefinitionDescriptor> GetDefinitionAsync(string decisionKey, string? versionTag = null, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 返回固定的决策执行结果。
            /// </summary>
            /// <param name="request">决策执行请求。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>固定执行结果。</returns>
            public Task<DecisionExecutionResult> ExecuteAsync(DecisionExecutionRequest request, CancellationToken cancellationToken = default)
            {
                LastRequest = request;

                if (string.Equals(request.DecisionKey, "expense.approval", StringComparison.OrdinalIgnoreCase))
                {
                    if (ThrowOnExpenseApproval)
                    {
                        throw new InvalidOperationException("expense approval failed");
                    }

                    return Task.FromResult(new DecisionExecutionResult
                    {
                        ExecutionId = "exec-expense-approval",
                        DecisionKey = request.DecisionKey,
                        DecisionType = request.DecisionType,
                        ResolvedVersionTag = request.VersionTag,
                        Succeeded = true,
                        TraceEnabled = request.TraceEnabled == true,
                        Explanation = "expense approval explanation",
                        TraceEntries = new List<string> { "step#0: 初始化执行上下文", "step#1: 决策 `费用审批规则` 执行完成" },
                        Outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["approved"] = true,
                            ["nextAssignee"] = "manager-1"
                        }
                    });
                }

                return Task.FromResult(new DecisionExecutionResult
                {
                    ExecutionId = "exec-single-root",
                    DecisionKey = request.DecisionKey,
                    DecisionType = request.DecisionType,
                    ResolvedVersionTag = "v2.0.0",
                    Succeeded = true,
                    TraceEnabled = request.TraceEnabled == true,
                    Explanation = "sample explanation",
                    TraceEntries = new List<string> { "step#0: 初始化执行上下文" },
                    Outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Age10"] = 42
                    },
                    MatchedRules = new List<string> { "rule-1" }
                });
            }

            /// <summary>
            /// 测试桩不提供批量执行。
            /// </summary>
            /// <param name="requests">决策执行请求集合。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<IReadOnlyList<DecisionExecutionResult>> ExecuteBatchAsync(IReadOnlyCollection<DecisionExecutionRequest> requests, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// <summary>
            /// 测试桩不提供解释能力。
            /// </summary>
            /// <param name="request">解释请求。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<DecisionExplanationResult> ExplainAsync(DecisionExplanationRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 测试桩不提供测试能力。
            /// </summary>
            /// <param name="request">测试请求。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<DecisionTestResult> TestAsync(DecisionTestRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// <summary>
            /// 测试桩不提供缓存失效。
            /// </summary>
            /// <param name="request">缓存失效请求。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task InvalidateCacheAsync(DecisionCacheInvalidationRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// 测试桩不提供缓存状态查询。
            /// </summary>
            /// <param name="decisionKey">决策主键。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            /// <returns>抛出未实现异常。</returns>
            public Task<DecisionCacheStatusDescriptor> GetCacheStatusAsync(string? decisionKey = null, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 管理测试期间的静态服务提供器替换与还原。
        /// </summary>
        private sealed class ProcessEngineServiceProviderScope : IDisposable
        {
            private readonly PropertyInfo _serviceProviderProperty;
            private readonly IServiceProvider? _originalServiceProvider;

            /// <summary>
            /// 初始化测试专用服务提供器作用域。
            /// </summary>
            /// <param name="decisionExecutionService">决策执行服务。</param>
            public ProcessEngineServiceProviderScope(IDecisionExecutionService decisionExecutionService)
            {
                var services = new ServiceCollection();
                services.AddSingleton(decisionExecutionService);
                services.AddLogging();

                _serviceProviderProperty = typeof(ProcessEngineServiceProvider).GetProperty("ServiceProvider", BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("未找到 ProcessEngineServiceProvider.ServiceProvider 属性。");
                _originalServiceProvider = (IServiceProvider?)_serviceProviderProperty.GetValue(null);
                _serviceProviderProperty.SetValue(null, services.BuildServiceProvider());
            }

            /// <summary>
            /// 还原原始静态服务提供器。
            /// </summary>
            public void Dispose()
            {
                _serviceProviderProperty.SetValue(null, _originalServiceProvider);
            }
        }
    }
}
