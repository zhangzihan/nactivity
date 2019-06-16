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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;

    public class SuspensionStateUtil
    {

        public static void SetSuspensionState(IProcessDefinitionEntity processDefinitionEntity, ISuspensionState state)
        {
            if (processDefinitionEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + processDefinitionEntity + "': already in state '" + state + "'.");
            }
            processDefinitionEntity.SuspensionState = state.StateCode;
            DispatchStateChangeEvent(processDefinitionEntity, state);
        }

        public static void SetSuspensionState(IExecutionEntity executionEntity, ISuspensionState state)
        {
            if (executionEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + executionEntity + "': already in state '" + state + "'.");
            }
            executionEntity.SuspensionState = state.StateCode;
            DispatchStateChangeEvent(executionEntity, state);
        }

        public static void SetSuspensionState(ITaskEntity taskEntity, ISuspensionState state)
        {
            if (taskEntity.SuspensionState == state.StateCode)
            {
                throw new ActivitiException("Cannot set suspension state '" + state + "' for " + taskEntity + "': already in state '" + state + "'.");
            }
            taskEntity.SuspensionState = state.StateCode;
            DispatchStateChangeEvent(taskEntity, state);
        }

        protected internal static void DispatchStateChangeEvent(object entity, ISuspensionState state)
        {
            if (Context.CommandContext != null && Context.CommandContext.EventDispatcher.Enabled)
            {
                ActivitiEventType eventType;
                if (state == SuspensionStateProvider.ACTIVE)
                {
                    eventType = ActivitiEventType.ENTITY_ACTIVATED;
                }
                else
                {
                    eventType = ActivitiEventType.ENTITY_SUSPENDED;
                }
                Context.CommandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(eventType, entity));
            }
        }
    }

}