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
namespace org.activiti.engine.impl.persistence.entity
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;

    /// <summary>
    /// Contains a predefined set of states for process definitions and process instances
    /// 
    /// 
    /// </summary>
    public interface ISuspensionState
    {

        int StateCode { get; }

        // default implementation /////////////////////////////////////////////////// 

        // helper class ///////////////////////////////////////// 

    }

    public static class SuspensionState_Fields
    {
        public static readonly ISuspensionState ACTIVE = new SuspensionState_SuspensionStateImpl(1, "active");
        public static readonly ISuspensionState SUSPENDED = new SuspensionState_SuspensionStateImpl(2, "suspended");
    }

    public class SuspensionState_SuspensionStateImpl : ISuspensionState
    {

        public readonly int stateCode;
        protected internal readonly string name;

        public SuspensionState_SuspensionStateImpl(int suspensionCode, string @string)
        {
            this.stateCode = suspensionCode;
            this.name = @string;
        }

        public virtual int StateCode
        {
            get
            {
                return stateCode;
            }
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + stateCode;
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            SuspensionState_SuspensionStateImpl other = (SuspensionState_SuspensionStateImpl)obj;
            if (stateCode != other.stateCode)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class SuspensionState_SuspensionStateUtil
    {

        public static void setSuspensionState(IProcessDefinitionEntity processDefinitionEntity, ISuspensionState state)
        {
            if (processDefinitionEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + processDefinitionEntity + "': already in state '" + state + "'.");
            }
            processDefinitionEntity.SuspensionState = state.StateCode;
            dispatchStateChangeEvent(processDefinitionEntity, state);
        }

        public static void setSuspensionState(IExecutionEntity executionEntity, ISuspensionState state)
        {
            if (executionEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + executionEntity + "': already in state '" + state + "'.");
            }
            executionEntity.SuspensionState = state.StateCode;
            dispatchStateChangeEvent(executionEntity, state);
        }

        public static void setSuspensionState(ITaskEntity taskEntity, ISuspensionState state)
        {
            if (taskEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + taskEntity + "': already in state '" + state + "'.");
            }
            taskEntity.SuspensionState = state.StateCode;
            dispatchStateChangeEvent(taskEntity, state);
        }

        protected internal static void dispatchStateChangeEvent(object entity, ISuspensionState state)
        {
            if (Context.CommandContext != null && Context.CommandContext.EventDispatcher.Enabled)
            {
                ActivitiEventType eventType = null;
                if (state == SuspensionState_Fields.ACTIVE)
                {
                    eventType = ActivitiEventType.ENTITY_ACTIVATED;
                }
                else
                {
                    eventType = ActivitiEventType.ENTITY_SUSPENDED;
                }
                Context.CommandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(eventType, entity));
            }
        }
    }

}