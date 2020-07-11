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
using Sys.Workflow.Engine.Impl.EL;
using Sys;

namespace Sys.Workflow.Engine.Impl.Delegate.Invocation
{

    /// <summary>
    /// Class responsible for handling Expression.setValue() invocations.
    /// 
    /// 
    /// </summary>
    public class ExpressionSetInvocation : ExpressionInvocation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal readonly object value;

        /// <summary>
        /// 
        /// </summary>
        protected internal ELContext elContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueExpression"></param>
        /// <param name="elContext"></param>
        /// <param name="value"></param>
        public ExpressionSetInvocation(IValueExpression valueExpression, ELContext elContext, object value) : base(valueExpression)
        {
            this.value = value;
            this.elContext = elContext;
            this.invocationParameters = new object[] { value };
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void Invoke()
        {
            valueExpression.SetValue(elContext, value);
        }

    }

}