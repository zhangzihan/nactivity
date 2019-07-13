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
namespace Sys.Workflow.Engine.Delegate.Events.Impl
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;
    using System.Collections.Generic;

    /// <summary>
    /// Builder class used to create <seealso cref="IActivitiEvent"/> implementations.
    /// 
    /// 
    /// </summary>
    public class ActivitiEventBuilder
    {

        /// <param name="type">
        ///          type of event </param>
        /// <returns> an <seealso cref="IActivitiEvent"/> that doesn't have it's execution context-fields filled, as the event is a global event, independent of any running execution. </returns>
        public static IActivitiEvent CreateGlobalEvent(ActivitiEventType type)
        {
            ActivitiEventImpl newEvent = new ActivitiEventImpl(type);
            return newEvent;
        }

        public static IActivitiEvent CreateEvent(ActivitiEventType type, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEventImpl newEvent = new ActivitiEventImpl(type)
            {
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId
            };
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related event fields will be populated. If not, execution details will be retrieved from the
        ///         <seealso cref="Object"/> if possible. </returns>
        public static IActivitiEntityEvent CreateTaskReturnEntityEvent(object entity, string activityId)
        {
            ActivitiTaskReturnToEventImpl newEvent = new ActivitiTaskReturnToEventImpl(entity, ActivitiEventType.TASK_RETURN_TO, activityId);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            PopulateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related event fields will be populated. If not, execution details will be retrieved from the
        ///         <seealso cref="Object"/> if possible. </returns>
        public static IActivitiEntityEvent CreateEntityEvent(ActivitiEventType type, object entity)
        {
            ActivitiEntityEventImpl newEvent = new ActivitiEntityEventImpl(entity, type);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            PopulateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="entity">
        ///            the entity this event targets </param>
        /// <param name="variables">
        ///            the variables associated with this entity </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related
        ///         event fields will be populated. If not, execution details will be reteived from the <seealso cref="Object"/> if
        ///         possible. </returns>
        public static IActivitiProcessStartedEvent CreateProcessStartedEvent(object entity, IDictionary<string, object> variables, bool localScope)
        {
            ActivitiProcessStartedEventImpl newEvent = new ActivitiProcessStartedEventImpl(entity, variables, localScope);

            // In case an execution-context is active, populate the event fields related to the execution
            PopulateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <param name="variables">
        ///          the variables associated with this entity </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related event fields will be populated. If not, execution details will be retrieved from the
        ///         <seealso cref="Object"/> if possible. </returns>
        public static IActivitiEntityWithVariablesEvent CreateEntityWithVariablesEvent(ActivitiEventType type, object entity, IDictionary<string, object> variables, bool localScope)
        {
            ActivitiEntityWithVariablesEventImpl newEvent = new ActivitiEntityWithVariablesEventImpl(entity, variables, localScope, type);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            PopulateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> </returns>
        public static IActivitiEntityEvent CreateEntityEvent(ActivitiEventType type, object entity, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEntityEventImpl newEvent = new ActivitiEntityEventImpl(entity, type)
            {
                ExecutionId = executionId,
                ProcessInstanceId = processInstanceId,
                ProcessDefinitionId = processDefinitionId
            };
            return newEvent;
        }

        ///
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> </returns>
        public static IActivitiEntityEvent CreateCustomTaskCompletedEvent(ITaskEntity entity, ActivitiEventType type)
        {
            CustomTaskCompletedEntityEventImpl newEvent = new CustomTaskCompletedEntityEventImpl(entity, type);

            return newEvent;
        }

        public static IActivitiSequenceFlowTakenEvent CreateSequenceFlowTakenEvent(IExecutionEntity executionEntity, ActivitiEventType type, string sequenceFlowId, string sourceActivityId, string sourceActivityName, string sourceActivityType, object sourceActivityBehavior, string targetActivityId, string targetActivityName, string targetActivityType, object targetActivityBehavior)
        {

            ActivitiSequenceFlowTakenEventImpl newEvent = new ActivitiSequenceFlowTakenEventImpl(type);

            if (executionEntity != null)
            {
                newEvent.ExecutionId = executionEntity.Id;
                newEvent.ProcessInstanceId = executionEntity.ProcessInstanceId;
                newEvent.ProcessDefinitionId = executionEntity.ProcessDefinitionId;
            }

            newEvent.Id = sequenceFlowId;
            newEvent.SourceActivityId = sourceActivityId;
            newEvent.SourceActivityName = sourceActivityName;
            newEvent.SourceActivityType = sourceActivityType;
            newEvent.SourceActivityBehaviorClass = sourceActivityBehavior?.GetType().FullName;
            newEvent.TargetActivityId = targetActivityId;
            newEvent.TargetActivityName = targetActivityName;
            newEvent.TargetActivityType = targetActivityType;
            newEvent.TargetActivityBehaviorClass = targetActivityBehavior?.GetType().FullName;

            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <param name="cause">
        ///          the cause of the event </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> that is also instance of <seealso cref="IActivitiExceptionEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related event fields will be
        ///         populated. </returns>
        public static IActivitiEntityEvent CreateEntityExceptionEvent(ActivitiEventType type, object entity, Exception cause)
        {
            ActivitiEntityExceptionEventImpl newEvent = new ActivitiEntityExceptionEventImpl(entity, type, cause);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            PopulateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <param name="cause">
        ///          the cause of the event </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> that is also instance of <seealso cref="IActivitiExceptionEvent"/>. </returns>
        public static IActivitiEntityEvent CreateEntityExceptionEvent(ActivitiEventType type, object entity, Exception cause, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEntityExceptionEventImpl newEvent = new ActivitiEntityExceptionEventImpl(entity, type, cause)
            {
                ExecutionId = executionId,
                ProcessInstanceId = processInstanceId,
                ProcessDefinitionId = processDefinitionId
            };
            return newEvent;
        }

        public static IActivitiActivityEvent CreateActivityEvent(ActivitiEventType type, string activityId, string activityName, string executionId, string processInstanceId, string processDefinitionId, FlowElement flowElement)
        {
            ActivitiActivityEventImpl newEvent = new ActivitiActivityEventImpl(type)
            {
                ActivityId = activityId,
                ActivityName = activityName,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId
            };

            if (flowElement is FlowNode flowNode)
            {
                newEvent.ActivityType = ParseActivityType(flowNode);
                object behaviour = flowNode.Behavior;
                if (behaviour != null)
                {
                    newEvent.BehaviorClass = behaviour.GetType().FullName;
                }
            }

            return newEvent;
        }

        protected internal static string ParseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

        public static IActivitiActivityCancelledEvent CreateActivityCancelledEvent(string activityId, string activityName, string executionId, string processInstanceId, string processDefinitionId, string activityType, object cause)
        {

            ActivitiActivityCancelledEventImpl newEvent = new ActivitiActivityCancelledEventImpl
            {
                ActivityId = activityId,
                ActivityName = activityName,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId,
                ActivityType = activityType,
                Cause = cause
            };
            return newEvent;
        }

        public static IActivitiCancelledEvent CreateCancelledEvent(string executionId, string processInstanceId, string processDefinitionId, object cause)
        {
            ActivitiProcessCancelledEventImpl newEvent = new ActivitiProcessCancelledEventImpl
            {
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId,
                Cause = cause
            };
            return newEvent;
        }

        public static IActivitiSignalEvent CreateSignalEvent(ActivitiEventType type, string activityId, string signalName, object signalData, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiSignalEventImpl newEvent = new ActivitiSignalEventImpl(type)
            {
                ActivityId = activityId,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId,
                SignalName = signalName,
                SignalData = signalData
            };
            return newEvent;
        }

        public static IActivitiMessageEvent CreateMessageEvent(ActivitiEventType type, string activityId, string messageName, object payload, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiMessageEventImpl newEvent = new ActivitiMessageEventImpl(type)
            {
                ActivityId = activityId,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId,
                MessageName = messageName,
                MessageData = payload
            };
            return newEvent;
        }

        public static IActivitiErrorEvent CreateErrorEvent(ActivitiEventType type, string activityId, string errorId, string errorCode, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiErrorEventImpl newEvent = new ActivitiErrorEventImpl(type)
            {
                ActivityId = activityId,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId,
                ErrorId = errorId,
                ErrorCode = errorCode
            };
            return newEvent;
        }

        public static IActivitiVariableEvent CreateVariableEvent(ActivitiEventType type, string variableName, object variableValue, IVariableType variableType, string taskId, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiVariableEventImpl newEvent = new ActivitiVariableEventImpl(type)
            {
                VariableName = variableName,
                VariableValue = variableValue,
                VariableType = variableType,
                TaskId = taskId,
                ExecutionId = executionId,
                ProcessDefinitionId = processDefinitionId,
                ProcessInstanceId = processInstanceId
            };
            return newEvent;
        }

        public static IActivitiMembershipEvent CreateMembershipEvent(ActivitiEventType type, string groupId, string userId)
        {
            ActivitiMembershipEventImpl newEvent = new ActivitiMembershipEventImpl(type)
            {
                UserId = userId,
                GroupId = groupId
            };
            return newEvent;
        }

        protected internal static void PopulateEventWithCurrentContext(ActivitiEventImpl @event)
        {
            if (@event is IActivitiEntityEvent actEvent)
            {
                object persistedObject = actEvent.Entity;
                if (persistedObject is IJob jo)
                {
                    @event.ExecutionId = jo.ExecutionId;
                    @event.ProcessInstanceId = jo.ProcessInstanceId;
                    @event.ProcessDefinitionId = jo.ProcessDefinitionId;
                }
                else if (persistedObject is IExecutionEntity po)
                {
                    @event.ExecutionId = po.Id;
                    @event.ProcessInstanceId = po.ProcessInstanceId;
                    @event.ProcessDefinitionId = po.ProcessDefinitionId;
                }
                else if (persistedObject is IIdentityLinkEntity idLink)
                {
                    if (idLink.ProcessDefinitionId is object)
                    {
                        @event.ProcessDefinitionId = idLink.ProcessDefId;
                    }
                    else if (idLink.ProcessInstance is object)
                    {
                        @event.ProcessDefinitionId = idLink.ProcessInstance.ProcessDefinitionId;
                        @event.ProcessInstanceId = idLink.ProcessInstanceId;
                        @event.ExecutionId = idLink.ProcessInstanceId;
                    }
                    else if (idLink.Task is object)
                    {
                        @event.ProcessDefinitionId = idLink.Task.ProcessDefinitionId;
                        @event.ProcessInstanceId = idLink.Task.ProcessInstanceId;
                        @event.ExecutionId = idLink.Task.ExecutionId;
                    }
                }
                else if (persistedObject is ITask task)
                {
                    @event.ProcessInstanceId = task.ProcessInstanceId;
                    @event.ExecutionId = task.ExecutionId;
                    @event.ProcessDefinitionId = task.ProcessDefinitionId;
                }
                else if (persistedObject is IProcessDefinition procDef)
                {
                    @event.ProcessDefinitionId = procDef.Id;
                }
            }
        }
    }
}