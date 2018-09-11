namespace org.activiti.engine.impl.util.condition
{
    using Newtonsoft.Json.Linq;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    /// 
    public class ConditionUtil
    {

        public static bool hasTrueCondition(SequenceFlow sequenceFlow, IExecutionEntity execution)
        {
            string conditionExpression = null;
            if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                JToken elementProperties = Context.getBpmnOverrideElementProperties(sequenceFlow.Id, execution.ProcessDefinitionId);
                conditionExpression = getActiveValue(sequenceFlow.ConditionExpression, DynamicBpmnConstants_Fields.SEQUENCE_FLOW_CONDITION, elementProperties);
            }
            else
            {
                conditionExpression = sequenceFlow.ConditionExpression;
            }

            if (!string.IsNullOrWhiteSpace(conditionExpression))
            {

                IExpression expression = Context.ProcessEngineConfiguration.ExpressionManager.createExpression(conditionExpression);
                ICondition condition = new UelExpressionCondition(expression);
                if (condition.evaluate(sequenceFlow.Id, execution))
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

        protected internal static string getActiveValue(string originalValue, string propertyName, JToken elementProperties)
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