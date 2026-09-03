using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using Sys.Workflow.Validation;
using Sys.Workflow.Validation.Validators;

namespace Sys.Workflow.Validation.Validators.Impl
{
    /// <summary>
    /// 使用决策目录服务对 BusinessRuleTask 的绑定目标执行部署期校验。
    /// </summary>
    public class DecisionBindingCatalogValidator : ProcessLevelValidator
    {
        private const string DecisionBindingCatalogProblem = "businessRuleTaskDecisionBindingCatalogInvalid";
        private readonly IDecisionCatalogService? decisionCatalogService;

        /// <summary>
        /// 初始化决策目录校验器。
        /// </summary>
        /// <param name="decisionCatalogService">决策目录服务。</param>
        public DecisionBindingCatalogValidator(IDecisionCatalogService? decisionCatalogService)
        {
            this.decisionCatalogService = decisionCatalogService;
        }

        /// <summary>
        /// 对当前流程中的 BusinessRuleTask 决策绑定执行目录校验。
        /// </summary>
        /// <param name="bpmnModel">待校验的 BPMN 模型。</param>
        /// <param name="process">当前流程定义。</param>
        /// <param name="errors">校验错误集合。</param>
        protected override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            if (decisionCatalogService == null)
            {
                return;
            }

            List<(BusinessRuleTask Task, DecisionBindingReference Binding)> bindings = process
                .FindFlowElementsOfType<BusinessRuleTask>(true)
                .Select(task => (Task: task, Binding: BuildBindingReference(task)))
                .Where(item => !string.IsNullOrWhiteSpace(item.Binding.DecisionKey))
                .ToList();

            if (bindings.Count == 0)
            {
                return;
            }

            var request = new DecisionBindingValidationRequest
            {
                Bindings = bindings.Select(item => item.Binding).ToList()
            };

            try
            {
                decisionCatalogService.ValidateDecisionBindingsAsync(request).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                foreach ((BusinessRuleTask task, _) in bindings)
                {
                    AddError(
                        errors,
                        DecisionBindingCatalogProblem,
                        process,
                        task,
                        $"BusinessRuleTask 的决策绑定校验失败：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 把 BusinessRuleTask 转换为目录服务使用的绑定引用。
        /// </summary>
        /// <param name="businessRuleTask">业务规则任务。</param>
        /// <returns>决策绑定引用。</returns>
        private static DecisionBindingReference BuildBindingReference(BusinessRuleTask businessRuleTask)
        {
            BusinessRuleTaskDecisionBinding? decisionBinding = businessRuleTask.GetDecisionBinding();
            return new DecisionBindingReference
            {
                SourceId = businessRuleTask.Id,
                DecisionKey = ResolveDecisionKey(businessRuleTask, decisionBinding),
                BindingMode = decisionBinding?.BindingMode,
                VersionTag = decisionBinding?.VersionTag
            };
        }

        /// <summary>
        /// 解析 BusinessRuleTask 的有效决策标识，兼容扩展绑定与旧版 RuleNames 写法。
        /// </summary>
        /// <param name="businessRuleTask">业务规则任务。</param>
        /// <param name="decisionBinding">扩展绑定对象。</param>
        /// <returns>决策标识；若未配置则返回空字符串。</returns>
        private static string ResolveDecisionKey(BusinessRuleTask businessRuleTask, BusinessRuleTaskDecisionBinding? decisionBinding)
        {
            if (!string.IsNullOrWhiteSpace(decisionBinding?.DecisionKey))
            {
                return decisionBinding.DecisionKey.Trim();
            }

            string? configuredRuleId = businessRuleTask.RuleNames?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(configuredRuleId))
            {
                return string.Empty;
            }

            string[] parts = configuredRuleId.Split(new[] { '#' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string decisionKey = parts.Length > 0 ? parts[0].Trim() : configuredRuleId.Trim();
            return string.IsNullOrWhiteSpace(decisionKey) ? string.Empty : decisionKey;
        }
    }
}
