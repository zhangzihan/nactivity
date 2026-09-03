using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Rules;

namespace Sys.Workflow.Validation.Validators.Impl
{
    /// <summary>
    /// 对 BusinessRuleTask 的宿主接入与决策绑定执行部署期校验。
    /// </summary>
    public class BusinessRuleTaskDeploymentValidator : ProcessLevelValidator
    {
        private const string MissingDecisionExecutionServiceProblem = "businessRuleTaskDecisionExecutionServiceMissing";
        private const string MissingDecisionBindingProblem = "businessRuleTaskDecisionBindingMissing";

        /// <summary>
        /// 校验流程中的 BusinessRuleTask 是否具备可执行的决策绑定，并确认宿主已接入决策执行服务。
        /// </summary>
        /// <param name="bpmnModel">待校验的 BPMN 模型。</param>
        /// <param name="process">当前流程定义。</param>
        /// <param name="errors">校验错误集合。</param>
        protected override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<BusinessRuleTask> businessRuleTasks = process.FindFlowElementsOfType<BusinessRuleTask>(true);
            if (businessRuleTasks == null || businessRuleTasks.Count == 0)
            {
                return;
            }

            bool hasDecisionExecutionService = HasDecisionExecutionService();
            foreach (BusinessRuleTask businessRuleTask in businessRuleTasks)
            {
                if (string.IsNullOrWhiteSpace(ResolveDecisionKey(businessRuleTask)))
                {
                    AddError(
                        errors,
                        MissingDecisionBindingProblem,
                        process,
                        businessRuleTask,
                        "BusinessRuleTask 未配置可执行的决策标识，请通过决策绑定扩展属性或 RuleNames 指定 decisionKey。");
                }

                if (!hasDecisionExecutionService)
                {
                    AddError(
                        errors,
                        MissingDecisionExecutionServiceProblem,
                        process,
                        businessRuleTask,
                        "当前宿主未接入规则宿主装配，无法部署包含 BusinessRuleTask 的流程。请改用 AddProcessEngineWithRules(...) 或显式调用 AddDecisionRulesHosting(...)。");
                }
            }
        }

        /// <summary>
        /// 判断当前宿主是否已注册决策执行服务。
        /// </summary>
        /// <returns>已注册返回 true，否则返回 false。</returns>
        private static bool HasDecisionExecutionService()
        {
            try
            {
                return ProcessEngineServiceProvider.Resolve<IDecisionExecutionService>() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 解析 BusinessRuleTask 的有效决策标识，兼容扩展绑定与旧版 RuleNames 写法。
        /// </summary>
        /// <param name="businessRuleTask">业务规则任务。</param>
        /// <returns>决策标识；若未配置则返回 null。</returns>
        private static string? ResolveDecisionKey(BusinessRuleTask businessRuleTask)
        {
            BusinessRuleTaskDecisionBinding? decisionBinding = businessRuleTask.GetDecisionBinding();
            if (!string.IsNullOrWhiteSpace(decisionBinding?.DecisionKey))
            {
                return decisionBinding.DecisionKey.Trim();
            }

            string? configuredRuleId = businessRuleTask.RuleNames?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(configuredRuleId))
            {
                return null;
            }

            string[] parts = configuredRuleId.Split(new[] { '#' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string decisionKey = parts.Length > 0 ? parts[0].Trim() : configuredRuleId.Trim();
            return string.IsNullOrWhiteSpace(decisionKey) ? null : decisionKey;
        }
    }
}
