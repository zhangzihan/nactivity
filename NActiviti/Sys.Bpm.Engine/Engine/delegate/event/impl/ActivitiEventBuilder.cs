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
namespace org.activiti.engine.@delegate.@event.impl
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.variable;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
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
        public static IActivitiEvent createGlobalEvent(ActivitiEventType type)
        {
            ActivitiEventImpl newEvent = new ActivitiEventImpl(type);
            return newEvent;
        }

        public static IActivitiEvent createEvent(ActivitiEventType type, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEventImpl newEvent = new ActivitiEventImpl(type);
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related event fields will be populated. If not, execution details will be retrieved from the
        ///         <seealso cref="Object"/> if possible. </returns>
        public static IActivitiEntityEvent createEntityEvent(ActivitiEventType type, object entity)
        {
            ActivitiEntityEventImpl newEvent = new ActivitiEntityEventImpl(entity, type);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            populateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="entity">
        ///            the entity this event targets </param>
        /// <param name="variables">
        ///            the variables associated with this entity </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/>. In case an <seealso cref="ExecutionContext"/> is active, the execution related
        ///         event fields will be populated. If not, execution details will be reteived from the <seealso cref="Object"/> if
        ///         possible. </returns>
        public static IActivitiProcessStartedEvent createProcessStartedEvent(object entity, IDictionary<string, object> variables, bool localScope)
        {
            ActivitiProcessStartedEventImpl newEvent = new ActivitiProcessStartedEventImpl(entity, variables, localScope);

            // In case an execution-context is active, populate the event fields related to the execution
            populateEventWithCurrentContext(newEvent);
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
        public static IActivitiEntityWithVariablesEvent createEntityWithVariablesEvent(ActivitiEventType type, object entity, IDictionary<string, object> variables, bool localScope)
        {
            ActivitiEntityWithVariablesEventImpl newEvent = new ActivitiEntityWithVariablesEventImpl(entity, variables, localScope, type);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            populateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> </returns>
        public static IActivitiEntityEvent createEntityEvent(ActivitiEventType type, object entity, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEntityEventImpl newEvent = new ActivitiEntityEventImpl(entity, type);

            newEvent.ExecutionId = executionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            return newEvent;
        }

        ///
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> </returns>
        public static IActivitiEntityEvent createCustomTaskCompletedEvent(ITaskEntity entity, ActivitiEventType type)
        {
            CustomTaskCompletedEntityEventImpl newEvent = new CustomTaskCompletedEntityEventImpl(entity, type);

            return newEvent;
        }

        public static IActivitiSequenceFlowTakenEvent createSequenceFlowTakenEvent(IExecutionEntity executionEntity, ActivitiEventType type, string sequenceFlowId, string sourceActivityId, string sourceActivityName, string sourceActivityType, object sourceActivityBehavior, string targetActivityId, string targetActivityName, string targetActivityType, object targetActivityBehavior)
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
            newEvent.SourceActivityBehaviorClass = sourceActivityBehavior != null ? sourceActivityBehavior.GetType().FullName : null;
            newEvent.TargetActivityId = targetActivityId;
            newEvent.TargetActivityName = targetActivityName;
            newEvent.TargetActivityType = targetActivityType;
            newEvent.TargetActivityBehaviorClass = targetActivityBehavior != null ? targetActivityBehavior.GetType().FullName : null;

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
        public static IActivitiEntityEvent createEntityExceptionEvent(ActivitiEventType type, object entity, Exception cause)
        {
            ActivitiEntityExceptionEventImpl newEvent = new ActivitiEntityExceptionEventImpl(entity, type, cause);

            // In case an execution-context is active, populate the event fields
            // related to the execution
            populateEventWithCurrentContext(newEvent);
            return newEvent;
        }

        /// <param name="type">
        ///          type of event </param>
        /// <param name="entity">
        ///          the entity this event targets </param>
        /// <param name="cause">
        ///          the cause of the event </param>
        /// <returns> an <seealso cref="IActivitiEntityEvent"/> that is also instance of <seealso cref="IActivitiExceptionEvent"/>. </returns>
        public static IActivitiEntityEvent createEntityExceptionEvent(ActivitiEventType type, object entity, Exception cause, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiEntityExceptionEventImpl newEvent = new ActivitiEntityExceptionEventImpl(entity, type, cause);

            newEvent.ExecutionId = executionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            return newEvent;
        }

        public static IActivitiActivityEvent createActivityEvent(ActivitiEventType type, string activityId, string activityName, string executionId, string processInstanceId, string processDefinitionId, FlowElement flowElement)
        {

            ActivitiActivityEventImpl newEvent = new ActivitiActivityEventImpl(type);
            newEvent.ActivityId = activityId;
            newEvent.ActivityName = activityName;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;

            if (flowElement is FlowNode)
            {
                FlowNode flowNode = (FlowNode)flowElement;
                newEvent.ActivityType = parseActivityType(flowNode);
                object behaviour = flowNode.Behavior;
                if (behaviour != null)
                {
                    newEvent.BehaviorClass = behaviour.GetType().FullName;
                }
            }

            return newEvent;
        }

        protected internal static string parseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

        public static IActivitiActivityCancelledEvent createActivityCancelledEvent(string activityId, string activityName, string executionId, string processInstanceId, string processDefinitionId, string activityType, object cause)
        {

            ActivitiActivityCancelledEventImpl newEvent = new ActivitiActivityCancelledEventImpl();
            newEvent.ActivityId = activityId;
            newEvent.ActivityName = activityName;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.ActivityType = activityType;
            newEvent.Cause = cause;
            return newEvent;
        }

        public static IActivitiCancelledEvent createCancelledEvent(string executionId, string processInstanceId, string processDefinitionId, object cause)
        {
            ActivitiProcessCancelledEventImpl newEvent = new ActivitiProcessCancelledEventImpl();
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.Cause = cause;
            return newEvent;
        }

        public static IActivitiSignalEvent createSignalEvent(ActivitiEventType type, string activityId, string signalName, object signalData, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiSignalEventImpl newEvent = new ActivitiSignalEventImpl(type);
            newEvent.ActivityId = activityId;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.SignalName = signalName;
            newEvent.SignalData = signalData;
            return newEvent;
        }

        public static IActivitiMessageEvent createMessageEvent(ActivitiEventType type, string activityId, string messageName, object payload, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiMessageEventImpl newEvent = new ActivitiMessageEventImpl(type);
            newEvent.ActivityId = activityId;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.MessageName = messageName;
            newEvent.MessageData = payload;
            return newEvent;
        }

        public static IActivitiErrorEvent createErrorEvent(ActivitiEventType type, string activityId, string errorId, string errorCode, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiErrorEventImpl newEvent = new ActivitiErrorEventImpl(type);
            newEvent.ActivityId = activityId;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            newEvent.ErrorId = errorId;
            newEvent.ErrorCode = errorCode;
            return newEvent;
        }

        public static IActivitiVariableEvent createVariableEvent(ActivitiEventType type, string variableName, object variableValue, IVariableType variableType, string taskId, string executionId, string processInstanceId, string processDefinitionId)
        {
            ActivitiVariableEventImpl newEvent = new ActivitiVariableEventImpl(type);
            newEvent.VariableName = variableName;
            newEvent.VariableValue = variableValue;
            newEvent.VariableType = variableType;
            newEvent.TaskId = taskId;
            newEvent.ExecutionId = executionId;
            newEvent.ProcessDefinitionId = processDefinitionId;
            newEvent.ProcessInstanceId = processInstanceId;
            return newEvent;
        }

        public static IActivitiMembershipEvent createMembershipEvent(ActivitiEventType type, string groupId, string userId)
        {
            ActivitiMembershipEventImpl newEvent = new ActivitiMembershipEventImpl(type);
            newEvent.UserId = userId;
            newEvent.GroupId = groupId;
            return newEvent;
        }

        protected internal static void populateEventWithCurrentContext(ActivitiEventImpl @event)
        {
            if (@event is IActivitiEntityEvent)
            {
                object persistedObject = ((IActivitiEntityEvent)@event).Entity;
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
                else if (persistedObject is IIdentityLinkEntity il)
                {
                    IIdentityLinkEntity idLink = il;
                    if (!ReferenceEquals(idLink.ProcessDefinitionId, null))
                    {
                        @event.ProcessDefinitionId = idLink.ProcessDefId;
                    }
                    else if (idLink.ProcessInstance != null)
                    {
                        @event.ProcessDefinitionId = idLink.ProcessInstance.ProcessDefinitionId;
                        @event.ProcessInstanceId = idLink.ProcessInstanceId;
                        @event.ExecutionId = idLink.ProcessInstanceId;
                    }
                    else if (idLink.Task != null)
                    {
                        @event.ProcessDefinitionId = idLink.Task.ProcessDefinitionId;
                        @event.ProcessInstanceId = idLink.Task.ProcessInstanceId;
                        @event.ExecutionId = idLink.Task.ExecutionId;
                    }
                }
                else if (persistedObject is task.ITask)
                {
                    @event.ProcessInstanceId = ((task.ITask)persistedObject).ProcessInstanceId;
                    @event.ExecutionId = ((task.ITask)persistedObject).ExecutionId;
                    @event.ProcessDefinitionId = ((task.ITask)persistedObject).ProcessDefinitionId;
                }
                else if (persistedObject is IProcessDefinition)
                {
                    @event.ProcessDefinitionId = ((IProcessDefinition)persistedObject).Id;
                }
            }
        }
    }

}