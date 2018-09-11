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

    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// <summary>
    /// An <seealso cref="org.activiti.engine.delegate.event.ActivitiCancelledEvent"/> implementation.
    /// 
    /// 
    /// </summary>
    public class ActivitiProcessStartedEventImpl : ActivitiEntityWithVariablesEventImpl, IActivitiProcessStartedEvent
    {

        protected internal readonly string nestedProcessInstanceId;

        protected internal readonly string nestedProcessDefinitionId;

        public ActivitiProcessStartedEventImpl(object entity, IDictionary<string, object> variables, bool localScope) : base(entity, variables, localScope, ActivitiEventType.PROCESS_STARTED)
        {
            if (entity is IExecutionEntity)
            {
                IExecutionEntity executionEntity = (IExecutionEntity)entity;
                if (!executionEntity.ProcessInstanceType)
                {
                    executionEntity = executionEntity.Parent;
                }

                IExecutionEntity superExecution = executionEntity.SuperExecution;
                if (superExecution != null)
                {
                    this.nestedProcessDefinitionId = superExecution.ProcessDefinitionId;
                    this.nestedProcessInstanceId = superExecution.ProcessInstanceId;
                }
                else
                {
                    this.nestedProcessDefinitionId = null;
                    this.nestedProcessInstanceId = null;
                }

            }
            else
            {
                this.nestedProcessDefinitionId = null;
                this.nestedProcessInstanceId = null;
            }
        }

        public virtual string NestedProcessInstanceId
        {
            get
            {
                return this.nestedProcessInstanceId;
            }
        }

        public virtual string NestedProcessDefinitionId
        {
            get
            {
                return this.nestedProcessDefinitionId;
            }
        }

    }

}