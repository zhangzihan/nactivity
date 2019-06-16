﻿namespace Sys.Workflow.engine.impl.util.condition
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.el;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    /// 
    public class ConditionUtil
    {

        public static bool HasTrueCondition(SequenceFlow sequenceFlow, IExecutionEntity execution)
        {
            string conditionExpression;
            if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                JToken elementProperties = Context.GetBpmnOverrideElementProperties(sequenceFlow.Id, execution.ProcessDefinitionId);
                conditionExpression = GetActiveValue(sequenceFlow.ConditionExpression, DynamicBpmnConstants.SEQUENCE_FLOW_CONDITION, elementProperties);
            }
            else
            {
                conditionExpression = sequenceFlow.ConditionExpression;
            }

            if (!string.IsNullOrWhiteSpace(conditionExpression))
            {
                IExpression expression = Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(conditionExpression);
                ICondition condition = new UelExpressionCondition(expression);
                if (condition.Evaluate(sequenceFlow.Id, execution))
                {
                    return true;
                }

                return false;
            }
            else
            {
                return true;
            }

        }

        protected internal static string GetActiveValue(string originalValue, string propertyName, JToken elementProperties)
        {
            string activeValue = originalValue;
            if (elementProperties != null)
            {
                JToken overrideValueNode = elementProperties[propertyName];
                if (overrideValueNode == null)
                {
                    activeValue = null;
                }
                else
                {
                    activeValue = overrideValueNode.ToString();
                }
            }
            return activeValue;
        }

    }

}