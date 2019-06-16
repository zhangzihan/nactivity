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
namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{

    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate.Invocation;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class DelegateExpressionExecutionListener : IExecutionListener
    {

        protected internal IExpression expression;
        private readonly IList<FieldDeclaration> fieldDeclarations;

        public DelegateExpressionExecutionListener(IExpression expression, IList<FieldDeclaration> fieldDeclarations)
        {
            this.expression = expression;
            this.fieldDeclarations = fieldDeclarations;
        }

        public virtual void Notify(IExecutionEntity execution)
        {
            object @delegate = DelegateExpressionUtil.ResolveDelegateExpression(expression, execution, fieldDeclarations);
            if (@delegate is IExecutionListener)
            {
                Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new ExecutionListenerInvocation((IExecutionListener)@delegate, execution));
            }
            else if (@delegate is ICSharpDelegate)
            {
                Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new CSharpDelegateInvocation((ICSharpDelegate)@delegate, execution));
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Delegate expression " + expression + " did not resolve to an implementation of " + typeof(IExecutionListener) + " nor " + typeof(ICSharpDelegate));
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