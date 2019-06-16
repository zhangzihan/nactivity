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

namespace Sys.Workflow.engine.impl.el
{
    using Sys.Workflow.engine.@delegate;

    /// <summary>
    /// Expression that always returns the same value when <code>getValue</code> is called. Setting of the value is not supported.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class FixedValue : IExpression
    {

        private const long serialVersionUID = 1L;
        private readonly object value;

        public FixedValue(object value)
        {
            this.value = value;
        }

        public virtual object GetValue(IVariableScope variableScope)
        {
            return value;
        }

        public virtual void SetValue(object value, IVariableScope variableScope)
        {
            throw new ActivitiException("Cannot change fixed value");
        }

        public virtual string ExpressionText
        {
            get
            {
                return value.ToString();
            }
        }

    }

}