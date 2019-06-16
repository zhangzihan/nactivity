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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.EL;

    /// <summary>
    /// An <seealso cref="IActivitiEventListener"/> implementation which resolves an expression to a delegate <seealso cref="IActivitiEventListener"/> instance and uses this for event notification. <br>
    /// <br>
    /// In case an entityClass was passed in the constructor, only events that are <seealso cref="IActivitiEntityEvent"/>'s that target an entity of the given type, are dispatched to the delegate.
    /// 
    /// 
    /// </summary>
    public class DelegateExpressionActivitiEventListener : BaseDelegateEventListener
    {

        protected internal IExpression expression;
        protected internal bool failOnException = false;

        public DelegateExpressionActivitiEventListener(IExpression expression, Type entityClass)
        {
            this.expression = expression;
            EntityClass = entityClass;
        }

        public override void OnEvent(IActivitiEvent @event)
        {
            throw new NotImplementedException();
            //if (isValidEvent(@event))
            //{
            //    object @delegate = DelegateExpressionUtil.resolveDelegateExpression(expression, new NoExecutionVariableScope());
            //    if (@delegate is IActivitiEventListener)
            //    {
            //        // Cache result of isFailOnException() from delegate-instance
            //        // until next event is received. This prevents us from having to resolve
            //        // the expression twice when an error occurs.
            //        failOnException = ((IActivitiEventListener)@delegate).FailOnException;

            //        // Call the delegate
            //        ((IActivitiEventListener)@delegate).onEvent(@event);
            //    }
            //    else
            //    {

            //        // Force failing, since the exception we're about to throw
            //        // cannot be ignored, because it did not originate from the listener itself
            //        failOnException = true;
            //        throw new ActivitiIllegalArgumentException("Delegate expression " + expression + " did not resolve to an implementation of " + typeof(IActivitiEventListener).FullName);
            //    }
            //}
        }

        public override bool FailOnException
        {
            get
            {
                return failOnException;
            }
        }

    }

}