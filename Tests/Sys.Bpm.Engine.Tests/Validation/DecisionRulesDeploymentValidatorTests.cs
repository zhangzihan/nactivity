using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Sys.Workflow.Validation;
using Sys.Workflow.Validation.Validators.Impl;
using Xunit;

namespace Sys.Workflow.Validation.Tests
{
    /// <summary>
    /// 覆盖规则宿主相关的部署期校验行为。
    /// </summary>
    public class DecisionRulesDeploymentValidatorTests
    {
        /// <summary>
        /// 验证 pure engine 宿主在未接入决策执行服务时会阻止 BusinessRuleTask 部署。
        /// </summary>
        [Fact]
        public void BusinessRuleTaskDeploymentValidator_Should_ReportMissingDecisionExecutionService()
        {
            using var scope = new ProcessEngineServiceProviderScope(new ServiceCollection().BuildServiceProvider());
            var validator = new BusinessRuleTaskDeploymentValidator();
            var errors = new List<ValidationError>();
            var model = CreateBusinessRuleTaskModel("expense.approval");

            validator.Validate(model, errors);

            Assert.Contains(errors, error => error.Problem == "businessRuleTaskDecisionExecutionServiceMissing");
        }

        /// <summary>
        /// 验证规则宿主目录校验器会把目录服务异常转成部署期校验错误。
        /// </summary>
        [Fact]
        public void DecisionBindingCatalogValidator_Should_ReportCatalogFailure()
        {
            var decisionCatalogServiceMock = new Mock<IDecisionCatalogService>(MockBehavior.Strict);
            decisionCatalogServiceMock
                .Setup(service => service.ValidateDecisionBindingsAsync(
                    It.Is<DecisionBindingValidationRequest>(request =>
                        request.Bindings.Count == 1 &&
                        request.Bindings[0].DecisionKey == "expense.approval"),
                    default))
                .ThrowsAsync(new InvalidOperationException("决策 expense.approval 尚未发布。"));

            var validator = new DecisionBindingCatalogValidator(decisionCatalogServiceMock.Object);
            var errors = new List<ValidationError>();
            var model = CreateBusinessRuleTaskModel("expense.approval");

            validator.Validate(model, errors);

            Assert.Contains(errors, error => error.DefaultDescription.Contains("尚未发布", StringComparison.Ordinal));
            decisionCatalogServiceMock.VerifyAll();
        }

        /// <summary>
        /// 创建包含单个 BusinessRuleTask 的 BPMN 模型。
        /// </summary>
        /// <param name="decisionKey">决策标识。</param>
        /// <returns>BPMN 模型。</returns>
        private static BpmnModel CreateBusinessRuleTaskModel(string decisionKey)
        {
            var process = new Process
            {
                Id = "process_1",
                Name = "测试流程"
            };
            process.AddFlowElement(new BusinessRuleTask
            {
                Id = "task_rule",
                Name = "规则任务",
                RuleNames = new List<string> { decisionKey }
            });

            var model = new BpmnModel();
            model.Processes.Add(process);
            return model;
        }

        /// <summary>
        /// 在测试期间替换静态流程引擎服务提供器，并在结束后自动恢复。
        /// </summary>
        private sealed class ProcessEngineServiceProviderScope : IDisposable
        {
            private readonly PropertyInfo serviceProviderProperty;
            private readonly IServiceProvider? originalServiceProvider;

            /// <summary>
            /// 初始化静态服务提供器作用域。
            /// </summary>
            /// <param name="serviceProvider">测试期使用的服务提供器。</param>
            public ProcessEngineServiceProviderScope(IServiceProvider serviceProvider)
            {
                serviceProviderProperty = typeof(ProcessEngineServiceProvider).GetProperty(
                    "ServiceProvider",
                    BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("未找到 ProcessEngineServiceProvider.ServiceProvider 属性。");
                originalServiceProvider = (IServiceProvider?)serviceProviderProperty.GetValue(null);
                serviceProviderProperty.SetValue(null, serviceProvider);
            }

            /// <summary>
            /// 恢复测试前的静态服务提供器。
            /// </summary>
            public void Dispose()
            {
                serviceProviderProperty.SetValue(null, originalServiceProvider);
            }
        }
    }
}
