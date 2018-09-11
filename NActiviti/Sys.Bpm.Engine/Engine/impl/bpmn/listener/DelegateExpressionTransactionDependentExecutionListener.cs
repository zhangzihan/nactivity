using System;
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

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.el;

    /// 
    [Serializable]
    public class DelegateExpressionTransactionDependentExecutionListener : ITransactionDependentExecutionListener
    {

        protected internal IExpression expression;

        public DelegateExpressionTransactionDependentExecutionListener(IExpression expression)
        {
            this.expression = expression;
        }

        public virtual void notify(string processInstanceId, string executionId, FlowElement flowElement, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap)
        {
            throw new System.NotImplementedException();
            //NoExecutionVariableScope scope = new NoExecutionVariableScope();

            //object @delegate = expression.getValue(scope);

            //if (@delegate is ITransactionDependentExecutionListener)
            //{
            //  ((ITransactionDependentExecutionListener) @delegate).notify(processInstanceId, executionId, flowElement, executionVariables, customPropertiesMap);
            //}
            //else
            //{
            //  throw new ActivitiIllegalArgumentException("Delegate expression " + expression + " did not resolve to an implementation of " + typeof(ITransactionDependentExecutionListener));
            //}

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