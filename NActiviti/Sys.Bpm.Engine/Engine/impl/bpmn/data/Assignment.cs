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
namespace Sys.Workflow.engine.impl.bpmn.data
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// Implementation of the BPMN 2.0 'assignment'
    /// 
    /// 
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal IExpression fromExpression;

        /// <summary>
        /// 
        /// </summary>
        protected internal IExpression toExpression;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromExpression"></param>
        /// <param name="toExpression"></param>
        public Assignment(IExpression fromExpression, IExpression toExpression)
        {
            this.fromExpression = fromExpression;
            this.toExpression = toExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void Evaluate(IExecutionEntity execution)
        {
            object value = this.fromExpression.GetValue(execution);
            this.toExpression.SetValue(value, execution);
        }
    }
}