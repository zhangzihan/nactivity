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
namespace org.activiti.engine.impl.bpmn.listener
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class DelegateExpressionCustomPropertiesResolver : ICustomPropertiesResolver
    {

        protected internal IExpression expression;

        public DelegateExpressionCustomPropertiesResolver(IExpression expression)
        {
            this.expression = expression;
        }

        public virtual IDictionary<string, object> getCustomPropertiesMap(IExecutionEntity execution)
        {
            // Note: we can't cache the result of the expression, because the
            // execution can change: eg.
            // delegateExpression='${mySpringBeanFactory.randomSpringBean()}'
            object @delegate = expression.getValue(execution);

            if (@delegate is ICustomPropertiesResolver)
            {
                return ((ICustomPropertiesResolver)@delegate).getCustomPropertiesMap(execution);
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Custom properties resolver delegate expression " + expression + " did not resolve to an implementation of " + typeof(ICustomPropertiesResolver));
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