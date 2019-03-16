using System.Collections.Generic;

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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public class SignalEventReceivedCmd : ICommand<object>
    {

        protected internal readonly string eventName;
        protected internal readonly string executionId;
        protected internal readonly IDictionary<string, object> payload;
        protected internal readonly bool async;
        protected internal string tenantId;

        public SignalEventReceivedCmd(string eventName, string executionId, IDictionary<string, object> processVariables, string tenantId)
        {
            this.eventName = eventName;
            this.executionId = executionId;
            if (processVariables != null)
            {
                this.payload = new Dictionary<string, object>(processVariables);

            }
            else
            {
                this.payload = null;
            }
            this.async = false;
            this.tenantId = tenantId;
        }

        public SignalEventReceivedCmd(string eventName, string executionId, bool async, string tenantId)
        {
            this.eventName = eventName;
            this.executionId = executionId;
            this.async = async;
            this.payload = null;
            this.tenantId = tenantId;
        }

        public  virtual object  execute(ICommandContext commandContext)
        {

            IList<ISignalEventSubscriptionEntity> signalEvents = null;

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
            if (ReferenceEquals(executionId, null))
            {
                signalEvents = eventSubscriptionEntityManager.findSignalEventSubscriptionsByEventName(eventName, tenantId);
            }
            else
            {

                IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(executionId);

                if (execution == null)
                {
                    throw new ActivitiObjectNotFoundException("Cannot find execution with id '" + executionId + "'", typeof(IExecution));
                }

                if (execution.Suspended)
                {
                    throw new ActivitiException("Cannot throw signal event '" + eventName + "' because execution '" + executionId + "' is suspended");
                }
                signalEvents = eventSubscriptionEntityManager.findSignalEventSubscriptionsByNameAndExecution(eventName, executionId);

                if (signalEvents.Count == 0)
                {
                    throw new ActivitiException("Execution '" + executionId + "' has not subscribed to a signal event with name '" + eventName + "'.");
                }
            }

            foreach (ISignalEventSubscriptionEntity signalEventSubscriptionEntity in signalEvents)
            {
                // We only throw the event to globally scoped signals.
                // Process instance scoped signals must be thrown within the process itself
                if (signalEventSubscriptionEntity.GlobalScoped)
                {
                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createSignalEvent(ActivitiEventType.ACTIVITY_SIGNALED, signalEventSubscriptionEntity.ActivityId, eventName, payload, signalEventSubscriptionEntity.ExecutionId, signalEventSubscriptionEntity.ProcessInstanceId, signalEventSubscriptionEntity.ProcessDefinitionId));

                    eventSubscriptionEntityManager.eventReceived(signalEventSubscriptionEntity, payload, async);
                }
            }

            return null;
        }

    }

}