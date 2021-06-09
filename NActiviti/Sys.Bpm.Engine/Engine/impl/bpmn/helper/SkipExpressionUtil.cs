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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    public class SkipExpressionUtil
    {

        public static bool IsSkipExpressionEnabled(IExecutionEntity execution, string skipExpression)
        {
            if (skipExpression is null)
            {
                return false;
            }
            return CheckSkipExpressionVariable(execution);
        }

        public static bool IsSkipExpressionEnabled(IExecutionEntity execution, IExpression skipExpression)
        {
            if (skipExpression is null)
            {
                return false;
            }
            return CheckSkipExpressionVariable(execution);
        }

        private static bool CheckSkipExpressionVariable(IExecutionEntity execution)
        {
            const string skipExpressionEnabledVariable = "_ACTIVITI_SKIP_EXPRESSION_ENABLED";
            object isSkipExpressionEnabled = execution.GetVariable(skipExpressionEnabledVariable);

            if (isSkipExpressionEnabled is null)
            {
                return false;

            }
            else if (isSkipExpressionEnabled is bool?)
            {
                return ((bool?)isSkipExpressionEnabled).Value;

            }
            else
            {
                throw new ActivitiIllegalArgumentException(skipExpressionEnabledVariable + " variable does not resolve to a boolean. " + isSkipExpressionEnabled);
            }
        }

        public static bool ShouldSkipFlowElement(ICommandContext commandContext, IExecutionEntity execution, string skipExpressionString)
        {
            IExpression skipExpression = commandContext.ProcessEngineConfiguration.ExpressionManager.CreateExpression(skipExpressionString);
            object value = skipExpression.GetValue(execution);

            if (value is bool?)
            {
                return ((bool?)value).Value;

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Skip expression does not resolve to a boolean: " + skipExpression.ExpressionText);
            }
        }

        public static bool ShouldSkipFlowElement(IExecutionEntity execution, IExpression skipExpression)
        {
            object value = skipExpression.GetValue(execution);

            if (value is bool?)
            {
                return ((bool?)value).Value;

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Skip expression does not resolve to a boolean: " + skipExpression.ExpressionText);
            }
        }
    }

}