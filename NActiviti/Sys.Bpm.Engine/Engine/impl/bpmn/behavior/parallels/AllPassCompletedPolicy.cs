using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Services.Api.Commands;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// 一票否决全部通过，多任务场景下如果有人提交了该任务，并且提交条件为false则当前多任务就算完成，并删除所有其它未完成的子任务，否则需要等待其他人得投票.
    /// </summary>
    class AllPassCompletedPolicy : DefaultMultiInstanceCompletedPolicy
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<AllPassCompletedPolicy>();

        private string completeConditionVarName = null;
        /// <summary>
        /// 条件表达式变量名
        /// </summary>
        public override string CompleteConditionVarName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(completeConditionVarName))
                {
                    completeConditionVarName = WorkflowVariable.GLOBAL_APPROVALED_VARIABLE;
                }

                return completeConditionVarName;
            }
            set { completeConditionVarName = value; }
        }

        /// <summary>
        /// 完成条件判断，根据signalData传递的数据判断当前提交人是否同意，如果有一个提交人不同意，该任务就结束.
        /// </summary>
        /// <param name="parent">当前任务节点的运行实例</param>
        /// <param name="multiInstanceActivity">多任务活动行为</param>
        /// <param name="signalData">提交的变量数据</param>
        /// <returns></returns>
        public override bool CompletionConditionSatisfied(IExecutionEntity parent, MultiInstanceActivityBehavior multiInstanceActivity, object signalData)
        {
            int nrOfActiveInstances = multiInstanceActivity.GetLoopVariable(parent, MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES).GetValueOrDefault(0);
            bool pass = base.CompletionConditionSatisfied(parent, multiInstanceActivity, signalData);
            if (pass)
            {
                parent.SetVariable(CompleteConditionVarName, true);
                return true;
            }

            if (signalData is IDictionary<string, object> dict)
            {
                _ = dict.TryGetValue(CompleteConditionVarName, out var tf);
                _ = bool.TryParse(tf?.ToString(), out bool approvaled);
                parent.SetVariable(CompleteConditionVarName, approvaled);
                return nrOfActiveInstances == 0 && approvaled;
            }

            return nrOfActiveInstances == 0;
        }
    }
}
