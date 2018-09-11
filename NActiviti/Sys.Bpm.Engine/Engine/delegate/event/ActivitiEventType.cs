using System.Collections.Generic;
using System.Linq;

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
namespace org.activiti.engine.@delegate.@event
{
    /// <summary>
    /// Enumeration containing all possible types of <seealso cref="IActivitiEvent"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public sealed class ActivitiEventType
    {

        /// <summary>
        /// New entity is created.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_CREATED = new ActivitiEventType("ENTITY_CREATED", InnerEnum.ENTITY_CREATED);

        /// <summary>
        /// New entity has been created and all child-entities that are created as a result of the creation of this particular entity are also created and initialized.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_INITIALIZED = new ActivitiEventType("ENTITY_INITIALIZED", InnerEnum.ENTITY_INITIALIZED);

        /// <summary>
        /// Existing entity us updated.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_UPDATED = new ActivitiEventType("ENTITY_UPDATED", InnerEnum.ENTITY_UPDATED);

        /// <summary>
        /// Existing entity is deleted.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_DELETED = new ActivitiEventType("ENTITY_DELETED", InnerEnum.ENTITY_DELETED);

        /// <summary>
        /// Existing entity has been suspended.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_SUSPENDED = new ActivitiEventType("ENTITY_SUSPENDED", InnerEnum.ENTITY_SUSPENDED);

        /// <summary>
        /// Existing entity has been activated.
        /// </summary>
        public static readonly ActivitiEventType ENTITY_ACTIVATED = new ActivitiEventType("ENTITY_ACTIVATED", InnerEnum.ENTITY_ACTIVATED);

        /// <summary>
        /// A Timer has been scheduled.
        /// </summary>
        public static readonly ActivitiEventType TIMER_SCHEDULED = new ActivitiEventType("TIMER_SCHEDULED", InnerEnum.TIMER_SCHEDULED);

        /// <summary>
        /// Timer has been fired successfully.
        /// </summary>
        public static readonly ActivitiEventType TIMER_FIRED = new ActivitiEventType("TIMER_FIRED", InnerEnum.TIMER_FIRED);

        /// <summary>
        /// Timer has been cancelled (e.g. user task on which it was bounded has been completed earlier than expected)
        /// </summary>
        public static readonly ActivitiEventType JOB_CANCELED = new ActivitiEventType("JOB_CANCELED", InnerEnum.JOB_CANCELED);

        /// <summary>
        /// A job has been successfully executed.
        /// </summary>
        public static readonly ActivitiEventType JOB_EXECUTION_SUCCESS = new ActivitiEventType("JOB_EXECUTION_SUCCESS", InnerEnum.JOB_EXECUTION_SUCCESS);

        /// <summary>
        /// A job has been executed, but failed. Event should be an instance of a <seealso cref="IActivitiExceptionEvent"/>.
        /// </summary>
        public static readonly ActivitiEventType JOB_EXECUTION_FAILURE = new ActivitiEventType("JOB_EXECUTION_FAILURE", InnerEnum.JOB_EXECUTION_FAILURE);

        /// <summary>
        /// The retry-count on a job has been decremented.
        /// </summary>
        public static readonly ActivitiEventType JOB_RETRIES_DECREMENTED = new ActivitiEventType("JOB_RETRIES_DECREMENTED", InnerEnum.JOB_RETRIES_DECREMENTED);

        /// <summary>
        /// An event type to be used by custom events. These types of events are never thrown by the engine itself, only be an external API call to dispatch an event.
        /// </summary>
        public static readonly ActivitiEventType CUSTOM = new ActivitiEventType("CUSTOM", InnerEnum.CUSTOM);

        /// <summary>
        /// The process-engine that dispatched this event has been created and is ready for use.
        /// </summary>
        public static readonly ActivitiEventType ENGINE_CREATED = new ActivitiEventType("ENGINE_CREATED", InnerEnum.ENGINE_CREATED);

        /// <summary>
        /// The process-engine that dispatched this event has been closed and cannot be used anymore.
        /// </summary>
        public static readonly ActivitiEventType ENGINE_CLOSED = new ActivitiEventType("ENGINE_CLOSED", InnerEnum.ENGINE_CLOSED);

        /// <summary>
        /// An activity is starting to execute. This event is dispatch right before an activity is executed.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_STARTED = new ActivitiEventType("ACTIVITY_STARTED", InnerEnum.ACTIVITY_STARTED);

        /// <summary>
        /// An activity has been completed successfully.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_COMPLETED = new ActivitiEventType("ACTIVITY_COMPLETED", InnerEnum.ACTIVITY_COMPLETED);

        /// <summary>
        /// An activity has been cancelled because of boundary event.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_CANCELLED = new ActivitiEventType("ACTIVITY_CANCELLED", InnerEnum.ACTIVITY_CANCELLED);

        /// <summary>
        /// An activity has received a signal. Dispatched after the activity has responded to the signal.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_SIGNALED = new ActivitiEventType("ACTIVITY_SIGNALED", InnerEnum.ACTIVITY_SIGNALED);

        /// <summary>
        /// An activity is about to be executed as a compensation for another activity. The event targets the activity that is about to be executed for compensation.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_COMPENSATE = new ActivitiEventType("ACTIVITY_COMPENSATE", InnerEnum.ACTIVITY_COMPENSATE);

        /// <summary>
        /// A boundary, intermediate, or subprocess start message catching event has started and it is waiting for message.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_MESSAGE_WAITING = new ActivitiEventType("ACTIVITY_MESSAGE_WAITING", InnerEnum.ACTIVITY_MESSAGE_WAITING);

        /// <summary>
        /// An activity has received a message event. Dispatched before the actual message has been received by the activity. This event will be either followed by a <seealso cref="#ACTIVITY_SIGNALLED"/> event or
        /// <seealso cref="#ACTIVITY_COMPLETE"/> for the involved activity, if the message was delivered successfully.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_MESSAGE_RECEIVED = new ActivitiEventType("ACTIVITY_MESSAGE_RECEIVED", InnerEnum.ACTIVITY_MESSAGE_RECEIVED);

        /// <summary>
        /// An activity has received an error event. Dispatched before the actual error has been received by the activity. This event will be either followed by a <seealso cref="#ACTIVITY_SIGNALLED"/> event or
        /// <seealso cref="#ACTIVITY_COMPLETE"/> for the involved activity, if the error was delivered successfully.
        /// </summary>
        public static readonly ActivitiEventType ACTIVITY_ERROR_RECEIVED = new ActivitiEventType("ACTIVITY_ERROR_RECEIVED", InnerEnum.ACTIVITY_ERROR_RECEIVED);

        /// <summary>
        /// A event dispatched when a <seealso cref="IHistoricActivityInstance"/> is created. 
        /// This is a specialized version of the <seealso cref="ActivitiEventType#ENTITY_CREATED"/> and <seealso cref="ActivitiEventType#ENTITY_INITIALIZED"/> event,
        /// with the same use case as the <seealso cref="ActivitiEventType#ACTIVITY_STARTED"/>, but containing
        /// slightly different data.
        /// 
        /// Note this will be an <seealso cref="IActivitiEntityEvent"/>, where the entity is the <seealso cref="IHistoricActivityInstance"/>.
        /// 
        /// Note that history (minimum level ACTIVITY) must be enabled to receive this event.  
        /// </summary>
        public static readonly ActivitiEventType HISTORIC_ACTIVITY_INSTANCE_CREATED = new ActivitiEventType("HISTORIC_ACTIVITY_INSTANCE_CREATED", InnerEnum.HISTORIC_ACTIVITY_INSTANCE_CREATED);

        /// <summary>
        /// A event dispatched when a <seealso cref="IHistoricActivityInstance"/> is marked as ended. 
        /// his is a specialized version of the <seealso cref="ActivitiEventType#ENTITY_UPDATED"/> event,
        /// with the same use case as the <seealso cref="ActivitiEventType#ACTIVITY_COMPLETED"/>, but containing
        /// slightly different data (e.g. the end time, the duration, etc.). 
        /// 
        /// Note that history (minimum level ACTIVITY) must be enabled to receive this event.  
        /// </summary>
        public static readonly ActivitiEventType HISTORIC_ACTIVITY_INSTANCE_ENDED = new ActivitiEventType("HISTORIC_ACTIVITY_INSTANCE_ENDED", InnerEnum.HISTORIC_ACTIVITY_INSTANCE_ENDED);

        /// <summary>
        /// Indicates the engine has taken (ie. followed) a sequenceflow from a source activity to a target activity.
        /// </summary>
        public static readonly ActivitiEventType SEQUENCEFLOW_TAKEN = new ActivitiEventType("SEQUENCEFLOW_TAKEN", InnerEnum.SEQUENCEFLOW_TAKEN);

        /// <summary>
        /// When a BPMN Error was thrown, but was not caught within in the process.
        /// </summary>
        public static readonly ActivitiEventType UNCAUGHT_BPMN_ERROR = new ActivitiEventType("UNCAUGHT_BPMN_ERROR", InnerEnum.UNCAUGHT_BPMN_ERROR);

        /// <summary>
        /// A new variable has been created.
        /// </summary>
        public static readonly ActivitiEventType VARIABLE_CREATED = new ActivitiEventType("VARIABLE_CREATED", InnerEnum.VARIABLE_CREATED);

        /// <summary>
        /// An existing variable has been updated.
        /// </summary>
        public static readonly ActivitiEventType VARIABLE_UPDATED = new ActivitiEventType("VARIABLE_UPDATED", InnerEnum.VARIABLE_UPDATED);

        /// <summary>
        /// An existing variable has been deleted.
        /// </summary>
        public static readonly ActivitiEventType VARIABLE_DELETED = new ActivitiEventType("VARIABLE_DELETED", InnerEnum.VARIABLE_DELETED);

        /// <summary>
        /// A task has been created. This is thrown when task is fully initialized (before TaskListener.EVENTNAME_CREATE).
        /// </summary>
        public static readonly ActivitiEventType TASK_CREATED = new ActivitiEventType("TASK_CREATED", InnerEnum.TASK_CREATED);

        /// <summary>
        /// A task as been assigned. This is thrown alongside with an <seealso cref="#ENTITY_UPDATED"/> event.
        /// </summary>
        public static readonly ActivitiEventType TASK_ASSIGNED = new ActivitiEventType("TASK_ASSIGNED", InnerEnum.TASK_ASSIGNED);

        /// <summary>
        /// A task has been completed. Dispatched before the task entity is deleted ( <seealso cref="#ENTITY_DELETED"/>). If the task is part of a process, this event is dispatched before the process moves on, as a
        /// result of the task completion. In that case, a <seealso cref="#ACTIVITY_COMPLETED"/> will be dispatched after an event of this type for the activity corresponding to the task.
        /// </summary>
        public static readonly ActivitiEventType TASK_COMPLETED = new ActivitiEventType("TASK_COMPLETED", InnerEnum.TASK_COMPLETED);

        /// <summary>
        /// A process instance has been started. Dispatched when starting a process instance previously created. The event
        /// PROCESS_STARTED is dispatched after the associated event ENTITY_INITIALIZED.
        /// </summary>
        public static readonly ActivitiEventType PROCESS_STARTED = new ActivitiEventType("PROCESS_STARTED", InnerEnum.PROCESS_STARTED);

        /// <summary>
        /// A process has been completed. Dispatched after the last activity is ACTIVITY_COMPLETED. Process is completed when it reaches state in which process instance does not have any transition to take.
        /// </summary>
        public static readonly ActivitiEventType PROCESS_COMPLETED = new ActivitiEventType("PROCESS_COMPLETED", InnerEnum.PROCESS_COMPLETED);

        /// <summary>
        /// A process has been completed with an error end event.
        /// </summary>
        public static readonly ActivitiEventType PROCESS_COMPLETED_WITH_ERROR_END_EVENT = new ActivitiEventType("PROCESS_COMPLETED_WITH_ERROR_END_EVENT", InnerEnum.PROCESS_COMPLETED_WITH_ERROR_END_EVENT);

        /// <summary>
        /// A process has been cancelled. Dispatched when process instance is deleted by
        /// </summary>
        /// <seealso cref= org.activiti.engine.impl.RuntimeServiceImpl#deleteProcessInstance(java.lang.String, java.lang.String), before DB delete. </seealso>
        public static readonly ActivitiEventType PROCESS_CANCELLED = new ActivitiEventType("PROCESS_CANCELLED", InnerEnum.PROCESS_CANCELLED);

        /// <summary>
        /// A event dispatched when a <seealso cref="IHistoricProcessInstance"/> is created. 
        /// This is a specialized version of the <seealso cref="ActivitiEventType#ENTITY_CREATED"/> and <seealso cref="ActivitiEventType#ENTITY_INITIALIZED"/> event,
        /// with the same use case as the <seealso cref="ActivitiEventType#PROCESS_STARTED"/>, but containing
        /// slightly different data (e.g. the start time, the start user id, etc.). 
        /// 
        /// Note this will be an <seealso cref="IActivitiEntityEvent"/>, where the entity is the <seealso cref="IHistoricProcessInstance"/>.
        /// 
        /// Note that history (minimum level ACTIVITY) must be enabled to receive this event.  
        /// </summary>
        public static readonly ActivitiEventType HISTORIC_PROCESS_INSTANCE_CREATED = new ActivitiEventType("HISTORIC_PROCESS_INSTANCE_CREATED", InnerEnum.HISTORIC_PROCESS_INSTANCE_CREATED);

        /// <summary>
        /// A event dispatched when a <seealso cref="IHistoricProcessInstance"/> is marked as ended. 
        /// his is a specialized version of the <seealso cref="ActivitiEventType#ENTITY_UPDATED"/> event,
        /// with the same use case as the <seealso cref="ActivitiEventType#PROCESS_COMPLETED"/>, but containing
        /// slightly different data (e.g. the end time, the duration, etc.). 
        /// 
        /// Note that history (minimum level ACTIVITY) must be enabled to receive this event.  
        /// </summary>
        public static readonly ActivitiEventType HISTORIC_PROCESS_INSTANCE_ENDED = new ActivitiEventType("HISTORIC_PROCESS_INSTANCE_ENDED", InnerEnum.HISTORIC_PROCESS_INSTANCE_ENDED);

        /// <summary>
        /// A new membership has been created.
        /// </summary>
        public static readonly ActivitiEventType MEMBERSHIP_CREATED = new ActivitiEventType("MEMBERSHIP_CREATED", InnerEnum.MEMBERSHIP_CREATED);

        /// <summary>
        /// A single membership has been deleted.
        /// </summary>
        public static readonly ActivitiEventType MEMBERSHIP_DELETED = new ActivitiEventType("MEMBERSHIP_DELETED", InnerEnum.MEMBERSHIP_DELETED);

        /// <summary>
        /// All memberships in the related group have been deleted. No individual <seealso cref="#MEMBERSHIP_DELETED"/> events will be dispatched due to possible performance reasons. The event is dispatched before the
        /// memberships are deleted, so they can still be accessed in the dispatch method of the listener.
        /// </summary>
        public static readonly ActivitiEventType MEMBERSHIPS_DELETED = new ActivitiEventType("MEMBERSHIPS_DELETED", InnerEnum.MEMBERSHIPS_DELETED);

        private static readonly IList<ActivitiEventType> valueList = new List<ActivitiEventType>();

        static ActivitiEventType()
        {
            valueList.Add(ENTITY_CREATED);
            valueList.Add(ENTITY_INITIALIZED);
            valueList.Add(ENTITY_UPDATED);
            valueList.Add(ENTITY_DELETED);
            valueList.Add(ENTITY_SUSPENDED);
            valueList.Add(ENTITY_ACTIVATED);
            valueList.Add(TIMER_SCHEDULED);
            valueList.Add(TIMER_FIRED);
            valueList.Add(JOB_CANCELED);
            valueList.Add(JOB_EXECUTION_SUCCESS);
            valueList.Add(JOB_EXECUTION_FAILURE);
            valueList.Add(JOB_RETRIES_DECREMENTED);
            valueList.Add(CUSTOM);
            valueList.Add(ENGINE_CREATED);
            valueList.Add(ENGINE_CLOSED);
            valueList.Add(ACTIVITY_STARTED);
            valueList.Add(ACTIVITY_COMPLETED);
            valueList.Add(ACTIVITY_CANCELLED);
            valueList.Add(ACTIVITY_SIGNALED);
            valueList.Add(ACTIVITY_COMPENSATE);
            valueList.Add(ACTIVITY_MESSAGE_WAITING);
            valueList.Add(ACTIVITY_MESSAGE_RECEIVED);
            valueList.Add(ACTIVITY_ERROR_RECEIVED);
            valueList.Add(HISTORIC_ACTIVITY_INSTANCE_CREATED);
            valueList.Add(HISTORIC_ACTIVITY_INSTANCE_ENDED);
            valueList.Add(SEQUENCEFLOW_TAKEN);
            valueList.Add(UNCAUGHT_BPMN_ERROR);
            valueList.Add(VARIABLE_CREATED);
            valueList.Add(VARIABLE_UPDATED);
            valueList.Add(VARIABLE_DELETED);
            valueList.Add(TASK_CREATED);
            valueList.Add(TASK_ASSIGNED);
            valueList.Add(TASK_COMPLETED);
            valueList.Add(PROCESS_STARTED);
            valueList.Add(PROCESS_COMPLETED);
            valueList.Add(PROCESS_COMPLETED_WITH_ERROR_END_EVENT);
            valueList.Add(PROCESS_CANCELLED);
            valueList.Add(HISTORIC_PROCESS_INSTANCE_CREATED);
            valueList.Add(HISTORIC_PROCESS_INSTANCE_ENDED);
            valueList.Add(MEMBERSHIP_CREATED);
            valueList.Add(MEMBERSHIP_DELETED);
            valueList.Add(MEMBERSHIPS_DELETED);
        }

        public enum InnerEnum
        {
            ENTITY_CREATED,
            ENTITY_INITIALIZED,
            ENTITY_UPDATED,
            ENTITY_DELETED,
            ENTITY_SUSPENDED,
            ENTITY_ACTIVATED,
            TIMER_SCHEDULED,
            TIMER_FIRED,
            JOB_CANCELED,
            JOB_EXECUTION_SUCCESS,
            JOB_EXECUTION_FAILURE,
            JOB_RETRIES_DECREMENTED,
            CUSTOM,
            ENGINE_CREATED,
            ENGINE_CLOSED,
            ACTIVITY_STARTED,
            ACTIVITY_COMPLETED,
            ACTIVITY_CANCELLED,
            ACTIVITY_SIGNALED,
            ACTIVITY_COMPENSATE,
            ACTIVITY_MESSAGE_WAITING,
            ACTIVITY_MESSAGE_RECEIVED,
            ACTIVITY_ERROR_RECEIVED,
            HISTORIC_ACTIVITY_INSTANCE_CREATED,
            HISTORIC_ACTIVITY_INSTANCE_ENDED,
            SEQUENCEFLOW_TAKEN,
            UNCAUGHT_BPMN_ERROR,
            VARIABLE_CREATED,
            VARIABLE_UPDATED,
            VARIABLE_DELETED,
            TASK_CREATED,
            TASK_ASSIGNED,
            TASK_COMPLETED,
            PROCESS_STARTED,
            PROCESS_COMPLETED,
            PROCESS_COMPLETED_WITH_ERROR_END_EVENT,
            PROCESS_CANCELLED,
            HISTORIC_PROCESS_INSTANCE_CREATED,
            HISTORIC_PROCESS_INSTANCE_ENDED,
            MEMBERSHIP_CREATED,
            MEMBERSHIP_DELETED,
            MEMBERSHIPS_DELETED
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private ActivitiEventType(string name, InnerEnum innerEnum)
        {
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        public static readonly ActivitiEventType[] EMPTY_ARRAY = new ActivitiEventType[] { };

        /// <param name="string">
        ///          the string containing a comma-separated list of event-type names </param>
        /// <returns> a list of <seealso cref="ActivitiEventType"/> based on the given list. </returns>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           when one of the given string is not a valid type name </exception>
        public static ActivitiEventType[] getTypesFromString(string @string)
        {
            IList<ActivitiEventType> result = new List<ActivitiEventType>();
            if (!string.ReferenceEquals(@string, null) && @string.Length > 0)
            {
                string[] split = @string.Split(',');
                foreach (string typeName in split)
                {
                    bool found = false;
                    foreach (ActivitiEventType type in values())
                    {
                        if (typeName.Equals(type.nameValue))
                        {
                            result.Add(type);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        throw new ActivitiIllegalArgumentException("Invalid event-type: " + typeName);
                    }
                }
            }

            return result.ToArray();
        }

        public static IList<ActivitiEventType> values()
        {
            return valueList;
        }

        public int ordinal()
        {
            return ordinalValue;
        }

        public override string ToString()
        {
            return nameValue;
        }

        public static ActivitiEventType valueOf(string name)
        {
            foreach (ActivitiEventType enumInstance in ActivitiEventType.valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }

}