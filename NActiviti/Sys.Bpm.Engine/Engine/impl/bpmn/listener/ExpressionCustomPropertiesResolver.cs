using System.Collections.Generic;

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

namespace Sys.Workflow.engine.impl.bpmn.listener
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// An <seealso cref="ICustomPropertiesResolver"/> that evaluates a <seealso cref="IExpression"/> when notified.
    /// 
    /// 
    /// </summary>
    public class ExpressionCustomPropertiesResolver : ICustomPropertiesResolver
    {

        protected internal IExpression expression;

        public ExpressionCustomPropertiesResolver(IExpression expression)
        {
            this.expression = expression;
        }

        public virtual IDictionary<string, object> GetCustomPropertiesMap(IExecutionEntity execution)
        {
            object expressionValue = expression.GetValue(execution);
            if (expressionValue is System.Collections.IDictionary)
            {
                return (IDictionary<string, object>)expressionValue;
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Custom properties resolver expression " + expression + " did not return a Map<String, Object>");
            }
        }

        /// <summary>
        /// returns the expression text for this execution listener. Comes in handy if you want to check which listeners you already have.
        /// </summary>
        public virtual string ExpressionText
        {
            get
            {
                return expression.ExpressionText;
            }
        }
    }

}