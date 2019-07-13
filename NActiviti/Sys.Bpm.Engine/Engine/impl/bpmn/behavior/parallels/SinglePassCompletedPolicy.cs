using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Services.Api.Commands;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// 多任务场景下如果有任意一人提交了该任务，则当前多任务就算完成，并删除所有其它未完成的子任务
    /// </summary>
    class SinglePassCompletedPolicy : DefaultMultiInstanceCompletedPolicy
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<SinglePassCompletedPolicy>();

        private string completeConditionVarName = null;
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
        /// 只要有人提交了,该任务就完成.
        /// </summary>
        /// <param name="parent">当前任务节点的运行实例</param>
        /// <param name="multiInstanceActivity">多任务活动行为</param>
        /// <param name="signalData">提交的变量数据</param>
        /// <returns></returns>
        public override bool CompletionConditionSatisfied(IExecutionEntity parent, MultiInstanceActivityBehavior multiInstanceActivity, object signalData)
        {
            return true;
        }
    }
}
