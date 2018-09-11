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
namespace org.activiti.engine.impl.bpmn.data
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Implementation of the BPMN 2.0 'assignment'
    /// 
    /// 
    /// </summary>
    public class Assignment
    {

        protected internal IExpression fromExpression;

        protected internal IExpression toExpression;

        public Assignment(IExpression fromExpression, IExpression toExpression)
        {
            this.fromExpression = fromExpression;
            this.toExpression = toExpression;
        }

        public virtual void evaluate(IExecutionEntity execution)
        {
            IVariableScope variableScope = (IVariableScope)execution;
            object value = this.fromExpression.getValue(variableScope);
            this.toExpression.setValue(value, variableScope);
        }
    }

}