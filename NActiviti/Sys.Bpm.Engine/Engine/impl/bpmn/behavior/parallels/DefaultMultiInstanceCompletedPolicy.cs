using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    class DefaultMultiInstanceCompletedPolicy : IMultiinstanceCompletedPolicy
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<DefaultMultiInstanceCompletedPolicy>();

        public virtual string CompleteConditionVarName { get; set; }

        public virtual IExecutionEntity LeaveExection { get; set; }

        public virtual ITaskEntity CompleteTask { get; set; }

        public virtual bool CompletionConditionSatisfied(IExecutionEntity execution, MultiInstanceActivityBehavior multiInstanceActivity, object signalData)
        {
            if (multiInstanceActivity.CompletionConditionExpression is object)
            {
                object value = multiInstanceActivity.CompletionConditionExpression.GetValue(execution);
                if (!(value is bool?))
                {
                    throw new ActivitiIllegalArgumentException("completionCondition '" + multiInstanceActivity.CompletionConditionExpression.ExpressionText + "' does not evaluate to a boolean value");
                }

                bool? booleanValue = (bool?)value;
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Completion condition of multi-instance satisfied: {booleanValue}");
                }
                return booleanValue.Value;
            }

            return false;
        }
    }
}
