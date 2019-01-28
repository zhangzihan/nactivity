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
namespace org.activiti.engine.impl.bpmn.helper
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    public class SkipExpressionUtil
	{

	  public static bool isSkipExpressionEnabled(IExecutionEntity execution, string skipExpression)
	  {
		if (ReferenceEquals(skipExpression, null))
		{
		  return false;
		}
		return checkSkipExpressionVariable(execution);
	  }

	  public static bool isSkipExpressionEnabled(IExecutionEntity execution, IExpression skipExpression)
	  {
		if (skipExpression == null)
		{
		  return false;
		}
		return checkSkipExpressionVariable(execution);
	  }

	  private static bool checkSkipExpressionVariable(IExecutionEntity execution)
	  {
		const string skipExpressionEnabledVariable = "_ACTIVITI_SKIP_EXPRESSION_ENABLED";
		object isSkipExpressionEnabled = execution.getVariable(skipExpressionEnabledVariable);

		if (isSkipExpressionEnabled == null)
		{
		  return false;

		}
		else if (isSkipExpressionEnabled is bool?)
		{
		  return ((bool?) isSkipExpressionEnabled).Value;

		}
		else
		{
		  throw new ActivitiIllegalArgumentException(skipExpressionEnabledVariable + " variable does not resolve to a boolean. " + isSkipExpressionEnabled);
		}
	  }

	  public static bool shouldSkipFlowElement(ICommandContext commandContext, IExecutionEntity execution, string skipExpressionString)
	  {
		IExpression skipExpression = commandContext.ProcessEngineConfiguration.ExpressionManager.createExpression(skipExpressionString);
		object value = skipExpression.getValue(execution);

		if (value is bool?)
		{
		  return ((bool?) value).Value;

		}
		else
		{
		  throw new ActivitiIllegalArgumentException("Skip expression does not resolve to a boolean: " + skipExpression.ExpressionText);
		}
	  }

	  public static bool shouldSkipFlowElement(IExecutionEntity execution, IExpression skipExpression)
	  {
		object value = skipExpression.getValue(execution);

		if (value is bool?)
		{
		  return ((bool?) value).Value;

		}
		else
		{
		  throw new ActivitiIllegalArgumentException("Skip expression does not resolve to a boolean: " + skipExpression.ExpressionText);
		}
	  }
	}

}