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
namespace org.activiti.engine.@delegate.@event.impl
{

    /// <summary>
    /// Base class for all <seealso cref="IActivitiEvent"/> implementations.
    /// 
    /// 
    /// </summary>
    public class ActivitiEventImpl : IActivitiEvent
    {

        protected internal ActivitiEventType type;
        protected internal string executionId;
        protected internal string processInstanceId;
        protected internal string processDefinitionId;

        /// <summary>
        /// Creates a new event implementation, not part of an execution context.
        /// </summary>
        public ActivitiEventImpl(ActivitiEventType type) : this(type, null, null, null)
        {
        }

        /// <summary>
        /// Creates a new event implementation, part of an execution context.
        /// </summary>
        public ActivitiEventImpl(ActivitiEventType type, string executionId, string processInstanceId, string processDefinitionId)
        {
            if (type == null)
            {
                throw new ActivitiIllegalArgumentException("type is null");
            }
            this.type = type;
            this.executionId = executionId;
            this.processInstanceId = processInstanceId;
            this.processDefinitionId = processDefinitionId;
        }

        public virtual ActivitiEventType Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }


        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }


        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }


        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }


        public override string ToString()
        {
            return this.GetType() + " - " + type;
        }

    }

}