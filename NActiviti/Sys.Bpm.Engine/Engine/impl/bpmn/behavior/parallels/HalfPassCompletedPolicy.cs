using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// 半数通过
    /// </summary>
    class HalfPassCompletedPolicy : DefaultMultiInstanceCompletedPolicy
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<OnePassCompletedPolicy>();


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
        /// 完成条件判断，根据signalData传递的数据加上历史数据判断当半数人员同意就流程就结束，同时记录该流程的终值，半数通过记true否则false.
        /// </summary>
        /// <param name="parent">当前任务节点的运行实例</param>
        /// <param name="multiInstanceActivity">多任务活动行为</param>
        /// <param name="signalData">提交的变量数据</param>
        /// <returns></returns>
        public override bool CompletionConditionSatisfied(IExecutionEntity parent, MultiInstanceActivityBehavior multiInstanceActivity, object signalData)
        {
            var config = Context.CommandContext.ProcessEngineConfiguration;

            object passed = parent.GetVariable(CompleteConditionVarName, true);
            if (passed is null || (bool.TryParse(passed.ToString(), out var p) && p))
            {
                return true;
            }

            var tf = (signalData as Dictionary<string, object>)[CompleteConditionVarName];
            bool.TryParse(tf?.ToString(), out bool approvaled);

            int nrOfInstances = multiInstanceActivity.GetLoopVariable(parent, MultiInstanceActivityBehavior.NUMBER_OF_INSTANCES).GetValueOrDefault(0);

            int nrOfActiveInstances = multiInstanceActivity.GetLoopVariable(parent, MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES).GetValueOrDefault(0);

            int nrOfCompletedInstances = multiInstanceActivity.GetLoopVariable(parent, MultiInstanceActivityBehavior.NUMBER_OF_COMPLETED_INSTANCES).GetValueOrDefault(0);

            var assigneeType = (LeaveExection.CurrentFlowElement as UserTask).AssigneeType;

            bool halfPassed = AssigneeType.HALF_PASSED.Equals(assigneeType?.Value, StringComparison.OrdinalIgnoreCase);

            int votes = config.HistoryService.CreateHistoricVariableInstanceQuery()
                .SetProcessInstanceId(LeaveExection.ProcessInstanceId)
                .VariableValueEquals(CompleteConditionVarName, halfPassed)
                .List()
                .Where(x => string.IsNullOrWhiteSpace(x.TaskId) == false)
                .Count();

            if ((votes + (approvaled & halfPassed ? 1 : !approvaled ? 1 : 0)) >= Math.Ceiling(nrOfInstances / 2M))
            {
                parent.SetVariable(CompleteConditionVarName, true);
                return true;
            }
            else if (votes + nrOfActiveInstances < Math.Ceiling(nrOfInstances / 2M))
            {
                parent.SetVariable(CompleteConditionVarName, false);
                return true;
            }
            else if (nrOfActiveInstances == 0)
            {
                parent.SetVariable(CompleteConditionVarName, false);
                return true;
            }

            return false;
        }
    }
}
