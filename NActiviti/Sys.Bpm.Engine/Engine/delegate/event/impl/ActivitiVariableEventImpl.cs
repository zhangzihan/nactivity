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
namespace Sys.Workflow.engine.@delegate.@event.impl
{
    using Sys.Workflow.engine.impl.variable;

    /// <summary>
    /// Implementation of <seealso cref="IActivitiVariableEvent"/>.
    /// 
    /// 
    /// </summary>
    public class ActivitiVariableEventImpl : ActivitiEventImpl, IActivitiVariableEvent
    {

        protected internal string variableName;
        protected internal object variableValue;
        protected internal IVariableType variableType;
        protected internal string taskId;

        public ActivitiVariableEventImpl(ActivitiEventType type) : base(type)
        {
        }

        public virtual string VariableName
        {
            get
            {
                return variableName;
            }
            set
            {
                this.variableName = value;
            }
        }


        public virtual object VariableValue
        {
            get
            {
                return variableValue;
            }
            set
            {
                this.variableValue = value;
            }
        }


        public virtual IVariableType VariableType
        {
            get
            {
                return variableType;
            }
            set
            {
                this.variableType = value;
            }
        }


        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set
            {
                this.taskId = value;
            }
        }


    }

}