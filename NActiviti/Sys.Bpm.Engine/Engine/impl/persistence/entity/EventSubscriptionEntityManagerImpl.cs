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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Events;
    using Sys.Workflow.Engine.Impl.JobExecutors;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Runtime;

    /// 
    /// 
    public class EventSubscriptionEntityManagerImpl : AbstractEntityManager<IEventSubscriptionEntity>, IEventSubscriptionEntityManager
    {

        /// <inheritdoc />
        protected internal IEventSubscriptionDataManager eventSubscriptionDataManager;

        /// <inheritdoc />
        public EventSubscriptionEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IEventSubscriptionDataManager eventSubscriptionDataManager) : base(processEngineConfiguration)
        {
            this.eventSubscriptionDataManager = eventSubscriptionDataManager;
        }

        /// <inheritdoc />
        protected internal override IDataManager<IEventSubscriptionEntity> DataManager
        {
            get
            {
                return eventSubscriptionDataManager;
            }
        }

        /// <inheritdoc />
        public virtual ICompensateEventSubscriptionEntity CreateCompensateEventSubscription()
        {
            return eventSubscriptionDataManager.CreateCompensateEventSubscription();
        }

        /// <inheritdoc />
        public virtual IMessageEventSubscriptionEntity CreateMessageEventSubscription()
        {
            return eventSubscriptionDataManager.CreateMessageEventSubscription();
        }

        /// <inheritdoc />
        public virtual ISignalEventSubscriptionEntity CreateSignalEventSubscription()
        {
            return eventSubscriptionDataManager.CreateSignalEventSubscription();
        }

        /// <inheritdoc />
        public virtual ISignalEventSubscriptionEntity InsertSignalEvent(string signalName, Signal signal, IExecutionEntity execution)
        {
            ISignalEventSubscriptionEntity subscriptionEntity = CreateSignalEventSubscription();
            subscriptionEntity.Execution = execution;
            if (signal != null)
            {
                subscriptionEntity.EventName = signal.Name;
                if (signal.Scope is object)
                {
                    subscriptionEntity.Configuration = signal.Scope;
                }
            }
            else
            {
                subscriptionEntity.EventName = signalName;
            }

            subscriptionEntity.ActivityId = execution.CurrentActivityId;
            subscriptionEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
            if (execution.TenantId is object)
            {
                subscriptionEntity.TenantId = execution.TenantId;
            }
            Insert(subscriptionEntity);
            execution.EventSubscriptions.Add(subscriptionEntity);
            return subscriptionEntity;
        }

        /// <inheritdoc />
        public virtual IMessageEventSubscriptionEntity InsertMessageEvent(string messageName, IExecutionEntity execution)
        {
            IMessageEventSubscriptionEntity subscriptionEntity = CreateMessageEventSubscription();
            subscriptionEntity.Execution = execution;
            subscriptionEntity.EventName = messageName;

            subscriptionEntity.ActivityId = execution.CurrentActivityId;
            subscriptionEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
            if (execution.TenantId is object)
            {
                subscriptionEntity.TenantId = execution.TenantId;
            }
            Insert(subscriptionEntity);
            execution.EventSubscriptions.Add(subscriptionEntity);
            return subscriptionEntity;
        }

        /// <inheritdoc />
        public virtual ICompensateEventSubscriptionEntity InsertCompensationEvent(IExecutionEntity execution, string activityId)
        {
            ICompensateEventSubscriptionEntity eventSubscription = CreateCompensateEventSubscription();
            eventSubscription.Execution = execution;
            eventSubscription.ActivityId = activityId;
            if (execution.TenantId is object)
            {
                eventSubscription.TenantId = execution.TenantId;
            }
            Insert(eventSubscription);
            return eventSubscription;
        }

        /// <inheritdoc />
        public override void Insert(IEventSubscriptionEntity entity, bool fireCreateEvent)
        {
            base.Insert(entity, fireCreateEvent);

            if (entity.ExecutionId is object && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)entity.Execution;
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.EventSubscriptionCount += 1;
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(IEventSubscriptionEntity entity, bool fireDeleteEvent)
        {
            if (entity.ExecutionId is object && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)entity.Execution;
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.EventSubscriptionCount -= 1;
                }
            }
            base.Delete(entity, fireDeleteEvent);
        }

        /// <inheritdoc />
        public virtual IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionId(string executionId)
        {
            IList<ICompensateEventSubscriptionEntity> eventSubscriptions = eventSubscriptionDataManager.FindCompensateEventSubscriptionsByExecutionId(executionId);

            return eventSubscriptions;
        }

        /// <inheritdoc />
        public virtual IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionIdAndActivityId(string executionId, string activityId)
        {
            IList<ICompensateEventSubscriptionEntity> eventSubscriptions = eventSubscriptionDataManager.FindCompensateEventSubscriptionsByProcessInstanceAndActivityId(executionId, activityId);
            IList<ICompensateEventSubscriptionEntity> result = new List<ICompensateEventSubscriptionEntity>();
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in eventSubscriptions)
            {
                if (eventSubscriptionEntity.EventType == CompensateEventSubscriptionEntityFields.EVENT_TYPE)
                {
                    if (activityId is null || activityId.Equals(eventSubscriptionEntity.ActivityId))
                    {
                        result.Add((ICompensateEventSubscriptionEntity)eventSubscriptionEntity);
                    }
                }
            }
            return result;
        }

        /// <inheritdoc />
        public virtual IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(string processInstanceId, string activityId)
        {
            IList<ICompensateEventSubscriptionEntity> eventSubscriptions = eventSubscriptionDataManager.FindCompensateEventSubscriptionsByProcessInstanceAndActivityId(processInstanceId, activityId);

            return eventSubscriptions;
        }

        /// <inheritdoc />
        protected internal virtual void AddToExecution(IEventSubscriptionEntity eventSubscriptionEntity)
        {
            // add reference in execution
            IExecutionEntity execution = eventSubscriptionEntity.Execution;
            if (execution != null)
            {
                execution.EventSubscriptions.Add(eventSubscriptionEntity);
            }
        }

        /// <inheritdoc />
        public virtual long FindEventSubscriptionCountByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionCountByQueryCriteria(eventSubscriptionQueryImpl);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl, Page page)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByQueryCriteria(eventSubscriptionQueryImpl, page);
        }

        /// <inheritdoc />
        public virtual IList<IMessageEventSubscriptionEntity> FindMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            return eventSubscriptionDataManager.FindMessageEventSubscriptionsByProcessInstanceAndEventName(processInstanceId, eventName);
        }

        /// <inheritdoc />
        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByEventName(string eventName, string tenantId)
        {
            return eventSubscriptionDataManager.FindSignalEventSubscriptionsByEventName(eventName, tenantId);
        }

        /// <inheritdoc />
        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            return eventSubscriptionDataManager.FindSignalEventSubscriptionsByProcessInstanceAndEventName(processInstanceId, eventName);
        }

        /// <inheritdoc />
        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByNameAndExecution(string name, string executionId)
        {
            return eventSubscriptionDataManager.FindSignalEventSubscriptionsByNameAndExecution(name, executionId);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecutionAndType(string executionId, string type)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByExecutionAndType(executionId, type);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string type)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByProcessInstanceAndActivityId(processInstanceId, activityId, type);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecution(string executionId)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByExecution(executionId);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByTypeAndProcessDefinitionId(type, processDefinitionId, tenantId);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByName(string type, string eventName, string tenantId)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByName(type, eventName, tenantId);
        }

        /// <inheritdoc />
        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByNameAndExecution(string type, string eventName, string executionId)
        {
            return eventSubscriptionDataManager.FindEventSubscriptionsByNameAndExecution(type, eventName, executionId);
        }

        /// <inheritdoc />
        public virtual IMessageEventSubscriptionEntity FindMessageStartEventSubscriptionByName(string messageName, string tenantId)
        {
            return eventSubscriptionDataManager.FindMessageStartEventSubscriptionByName(messageName, tenantId);
        }

        /// <inheritdoc />
        public virtual void UpdateEventSubscriptionTenantId(string oldTenantId, string newTenantId)
        {
            eventSubscriptionDataManager.UpdateEventSubscriptionTenantId(oldTenantId, newTenantId);
        }

        /// <inheritdoc />
        public virtual void DeleteEventSubscriptionsForProcessDefinition(string processDefinitionId)
        {
            eventSubscriptionDataManager.DeleteEventSubscriptionsForProcessDefinition(processDefinitionId);
        }

        // Processing /////////////////////////////////////////////////////////////

        /// <inheritdoc />
        public virtual void EventReceived(IEventSubscriptionEntity eventSubscriptionEntity, object payload, bool processASync)
        {
            if (processASync)
            {
                ScheduleEventAsync(eventSubscriptionEntity, payload);
            }
            else
            {
                ProcessEventSync(eventSubscriptionEntity, payload);
            }
        }

        /// <inheritdoc />
        protected internal virtual void ProcessEventSync(IEventSubscriptionEntity eventSubscriptionEntity, object payload)
        {

            // A compensate event needs to be deleted before the handlers are called
            if (eventSubscriptionEntity.EventType == CompensateEventSubscriptionEntityFields.EVENT_TYPE)
            {
                Delete(eventSubscriptionEntity);
            }

            IEventHandler eventHandler = ProcessEngineConfiguration.GetEventHandler(eventSubscriptionEntity.EventType);
            if (eventHandler == null)
            {
                throw new ActivitiException("Could not find eventhandler for event of type '" + eventSubscriptionEntity.EventType + "'.");
            }
            eventHandler.HandleEvent(eventSubscriptionEntity, payload, CommandContext);
        }

        /// <inheritdoc />
        protected internal virtual void ScheduleEventAsync(IEventSubscriptionEntity eventSubscriptionEntity, object payload)
        {
            IJobEntity message = JobEntityManager.Create();
            message.JobType = JobFields.JOB_TYPE_MESSAGE;
            message.JobHandlerType = ProcessEventJobHandler.TYPE;
            message.JobHandlerConfiguration = eventSubscriptionEntity.Id;
            message.TenantId = eventSubscriptionEntity.TenantId;

            // TODO: support payload
            // if(payload != null) {
            // message.setEventPayload(payload);
            // }

            JobManager.ScheduleAsyncJob(message);
        }

        /// <inheritdoc />
        protected internal virtual IList<ISignalEventSubscriptionEntity> ToSignalEventSubscriptionEntityList(IList<IEventSubscriptionEntity> result)
        {
            IList<ISignalEventSubscriptionEntity> signalEventSubscriptionEntities = new List<ISignalEventSubscriptionEntity>(result.Count);
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in result)
            {
                signalEventSubscriptionEntities.Add((ISignalEventSubscriptionEntity)eventSubscriptionEntity);
            }
            return signalEventSubscriptionEntities;
        }

        /// <inheritdoc />
        protected internal virtual IList<IMessageEventSubscriptionEntity> ToMessageEventSubscriptionEntityList(IList<IEventSubscriptionEntity> result)
        {
            IList<IMessageEventSubscriptionEntity> messageEventSubscriptionEntities = new List<IMessageEventSubscriptionEntity>(result.Count);
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in result)
            {
                messageEventSubscriptionEntities.Add((IMessageEventSubscriptionEntity)eventSubscriptionEntity);
            }
            return messageEventSubscriptionEntities;
        }

        /// <inheritdoc />
        public virtual IEventSubscriptionDataManager EventSubscriptionDataManager
        {
            get
            {
                return eventSubscriptionDataManager;
            }
            set
            {
                this.eventSubscriptionDataManager = value;
            }
        }
    }
}