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
using org.activiti.engine.impl.el;
using Sys;
using System;

namespace org.activiti.engine.impl.@delegate.invocation
{

    /// <summary>
    /// Class responsible for handling Expression.getValue invocations
    /// 
    /// 
    /// </summary>
    public class ExpressionGetInvocation : ExpressionInvocation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal readonly ELContext elContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueExpression"></param>
        /// <param name="elContext"></param>
        public ExpressionGetInvocation(ValueExpression valueExpression, ELContext elContext) : base(valueExpression)
        {
            this.elContext = elContext;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void Invoke()
        {
            invocationResult = valueExpression.GetValue(elContext);
        }
    }
}