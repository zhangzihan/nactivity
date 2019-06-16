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

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;

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

        public virtual object Execute(ICommandContext commandContext)
        {
            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;

            IList<ISignalEventSubscriptionEntity> signalEvents;
            if (executionId is null)
            {
                signalEvents = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByEventName(eventName, tenantId);
            }
            else
            {

                IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);

                if (execution == null)
                {
                    throw new ActivitiObjectNotFoundException("Cannot find execution with id '" + executionId + "'", typeof(IExecution));
                }

                if (execution.Suspended)
                {
                    throw new ActivitiException("Cannot throw signal event '" + eventName + "' because execution '" + executionId + "' is suspended");
                }
                signalEvents = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByNameAndExecution(eventName, executionId);

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
                    Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateSignalEvent(ActivitiEventType.ACTIVITY_SIGNALED, signalEventSubscriptionEntity.ActivityId, eventName, payload, signalEventSubscriptionEntity.ExecutionId, signalEventSubscriptionEntity.ProcessInstanceId, signalEventSubscriptionEntity.ProcessDefinitionId));

                    eventSubscriptionEntityManager.EventReceived(signalEventSubscriptionEntity, payload, async);
                }
            }

            return null;
        }
    }
}