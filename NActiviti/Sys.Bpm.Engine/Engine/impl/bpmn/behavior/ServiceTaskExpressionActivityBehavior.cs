using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.impl.bpmn.behavior
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// ActivityBehavior that evaluates an expression when executed. Optionally, it sets the result of the expression as a variable on the execution.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ServiceTaskExpressionActivityBehavior : TaskActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal string serviceTaskId;
        protected internal IExpression expression;
        protected internal IExpression skipExpression;
        protected internal string resultVariable;

        public ServiceTaskExpressionActivityBehavior(string serviceTaskId, IExpression expression, IExpression skipExpression, string resultVariable)
        {
            this.serviceTaskId = serviceTaskId;
            this.expression = expression;
            this.skipExpression = skipExpression;
            this.resultVariable = resultVariable;
        }

        public override void execute(IExecutionEntity execution)
        {
            object value = null;
            try
            {
                bool isSkipExpressionEnabled = SkipExpressionUtil.isSkipExpressionEnabled(execution, skipExpression);
                if (!isSkipExpressionEnabled || (isSkipExpressionEnabled && !SkipExpressionUtil.shouldSkipFlowElement(execution, skipExpression)))
                {

                    if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
                    {
                        JToken taskElementProperties = Context.getBpmnOverrideElementProperties(serviceTaskId, execution.ProcessDefinitionId);
                        if (taskElementProperties != null && taskElementProperties[DynamicBpmnConstants_Fields.SERVICE_TASK_EXPRESSION] != null)
                        {
                            string overrideExpression = taskElementProperties[DynamicBpmnConstants_Fields.SERVICE_TASK_EXPRESSION].ToString();
                            if (!string.IsNullOrWhiteSpace(overrideExpression) && !overrideExpression.Equals(expression.ExpressionText))
                            {
                                expression = Context.ProcessEngineConfiguration.ExpressionManager.createExpression(overrideExpression);
                            }
                        }
                    }

                    value = expression.getValue(execution);
                    if (!string.ReferenceEquals(resultVariable, null))
                    {
                        execution.setVariable(resultVariable, value);
                    }
                }

                leave(execution);
            }
            catch (Exception exc)
            {

                Exception cause = exc;
                BpmnError error = null;
                while (cause != null)
                {
                    if (cause is BpmnError)
                    {
                        error = (BpmnError)cause;
                        break;
                    }
                    cause = cause.InnerException;
                }

                if (error != null)
                {
                    ErrorPropagation.propagateError(error, execution);
                }
                else
                {
                    throw new ActivitiException("Could not execute service task expression", exc);
                }
            }
        }
    }

}