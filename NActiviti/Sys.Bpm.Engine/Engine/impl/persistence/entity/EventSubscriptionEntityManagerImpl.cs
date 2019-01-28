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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.@event;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public class EventSubscriptionEntityManagerImpl : AbstractEntityManager<IEventSubscriptionEntity>, IEventSubscriptionEntityManager
    {

        protected internal IEventSubscriptionDataManager eventSubscriptionDataManager;

        public EventSubscriptionEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IEventSubscriptionDataManager eventSubscriptionDataManager) : base(processEngineConfiguration)
        {
            this.eventSubscriptionDataManager = eventSubscriptionDataManager;
        }

        protected internal override IDataManager<IEventSubscriptionEntity> DataManager
        {
            get
            {
                return eventSubscriptionDataManager;
            }
        }

        public virtual ICompensateEventSubscriptionEntity createCompensateEventSubscription()
        {
            return eventSubscriptionDataManager.createCompensateEventSubscription();
        }

        public virtual IMessageEventSubscriptionEntity createMessageEventSubscription()
        {
            return eventSubscriptionDataManager.createMessageEventSubscription();
        }

        public virtual ISignalEventSubscriptionEntity createSignalEventSubscription()
        {
            return eventSubscriptionDataManager.createSignalEventSubscription();
        }

        public virtual ISignalEventSubscriptionEntity insertSignalEvent(string signalName, Signal signal, IExecutionEntity execution)
        {
            ISignalEventSubscriptionEntity subscriptionEntity = createSignalEventSubscription();
            subscriptionEntity.Execution = execution;
            if (signal != null)
            {
                subscriptionEntity.EventName = signal.Name;
                if (!ReferenceEquals(signal.Scope, null))
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
            if (!ReferenceEquals(execution.TenantId, null))
            {
                subscriptionEntity.TenantId = execution.TenantId;
            }
            insert(subscriptionEntity);
            execution.EventSubscriptions.Add(subscriptionEntity);
            return subscriptionEntity;
        }

        public virtual IMessageEventSubscriptionEntity insertMessageEvent(string messageName, IExecutionEntity execution)
        {
            IMessageEventSubscriptionEntity subscriptionEntity = createMessageEventSubscription();
            subscriptionEntity.Execution = execution;
            subscriptionEntity.EventName = messageName;

            subscriptionEntity.ActivityId = execution.CurrentActivityId;
            subscriptionEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
            if (!ReferenceEquals(execution.TenantId, null))
            {
                subscriptionEntity.TenantId = execution.TenantId;
            }
            insert(subscriptionEntity);
            execution.EventSubscriptions.Add(subscriptionEntity);
            return subscriptionEntity;
        }

        public virtual ICompensateEventSubscriptionEntity insertCompensationEvent(IExecutionEntity execution, string activityId)
        {
            ICompensateEventSubscriptionEntity eventSubscription = createCompensateEventSubscription();
            eventSubscription.Execution = execution;
            eventSubscription.ActivityId = activityId;
            if (!ReferenceEquals(execution.TenantId, null))
            {
                eventSubscription.TenantId = execution.TenantId;
            }
            insert(eventSubscription);
            return eventSubscription;
        }

        public override void insert(IEventSubscriptionEntity entity, bool fireCreateEvent)
        {
            base.insert(entity, fireCreateEvent);

            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)entity.Execution;
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.EventSubscriptionCount = executionEntity.EventSubscriptionCount + 1;
                }
            }
        }

        public override void delete(IEventSubscriptionEntity entity, bool fireDeleteEvent)
        {
            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)entity.Execution;
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.EventSubscriptionCount = executionEntity.EventSubscriptionCount - 1;
                }
            }
            base.delete(entity, fireDeleteEvent);
        }

        public virtual IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByExecutionId(string executionId)
        {
            return findCompensateEventSubscriptionsByExecutionIdAndActivityId(executionId, null);
        }

        public virtual IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByExecutionIdAndActivityId(string executionId, string activityId)
        {
            IList<IEventSubscriptionEntity> eventSubscriptions = findEventSubscriptionsByExecutionAndType(executionId, "compensate");
            IList<ICompensateEventSubscriptionEntity> result = new List<ICompensateEventSubscriptionEntity>();
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in eventSubscriptions)
            {
                if (eventSubscriptionEntity is ICompensateEventSubscriptionEntity)
                {
                    if (ReferenceEquals(activityId, null) || activityId.Equals(eventSubscriptionEntity.ActivityId))
                    {
                        result.Add((ICompensateEventSubscriptionEntity)eventSubscriptionEntity);
                    }
                }
            }
            return result;
        }

        public virtual IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(string processInstanceId, string activityId)
        {
            IList<IEventSubscriptionEntity> eventSubscriptions = findEventSubscriptionsByProcessInstanceAndActivityId(processInstanceId, activityId, "compensate");
            IList<ICompensateEventSubscriptionEntity> result = new List<ICompensateEventSubscriptionEntity>();
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in eventSubscriptions)
            {
                result.Add((ICompensateEventSubscriptionEntity)eventSubscriptionEntity);
            }
            return result;
        }

        protected internal virtual void addToExecution(IEventSubscriptionEntity eventSubscriptionEntity)
        {
            // add reference in execution
            IExecutionEntity execution = eventSubscriptionEntity.Execution;
            if (execution != null)
            {
                execution.EventSubscriptions.Add(eventSubscriptionEntity);
            }
        }

        public virtual long findEventSubscriptionCountByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl)
        {
            return eventSubscriptionDataManager.findEventSubscriptionCountByQueryCriteria(eventSubscriptionQueryImpl);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl, Page page)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByQueryCriteria(eventSubscriptionQueryImpl, page);
        }

        public virtual IList<IMessageEventSubscriptionEntity> findMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            return eventSubscriptionDataManager.findMessageEventSubscriptionsByProcessInstanceAndEventName(processInstanceId, eventName);
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByEventName(string eventName, string tenantId)
        {
            return eventSubscriptionDataManager.findSignalEventSubscriptionsByEventName(eventName, tenantId);
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            return eventSubscriptionDataManager.findSignalEventSubscriptionsByProcessInstanceAndEventName(processInstanceId, eventName);
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByNameAndExecution(string name, string executionId)
        {
            return eventSubscriptionDataManager.findSignalEventSubscriptionsByNameAndExecution(name, executionId);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByExecutionAndType(string executionId, string type)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByExecutionAndType(executionId, type);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string type)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByProcessInstanceAndActivityId(processInstanceId, activityId, type);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByExecution(string executionId)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByExecution(executionId);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByTypeAndProcessDefinitionId(type, processDefinitionId, tenantId);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByName(string type, string eventName, string tenantId)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByName(type, eventName, tenantId);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByNameAndExecution(string type, string eventName, string executionId)
        {
            return eventSubscriptionDataManager.findEventSubscriptionsByNameAndExecution(type, eventName, executionId);
        }

        public virtual IMessageEventSubscriptionEntity findMessageStartEventSubscriptionByName(string messageName, string tenantId)
        {
            return eventSubscriptionDataManager.findMessageStartEventSubscriptionByName(messageName, tenantId);
        }

        public virtual void updateEventSubscriptionTenantId(string oldTenantId, string newTenantId)
        {
            eventSubscriptionDataManager.updateEventSubscriptionTenantId(oldTenantId, newTenantId);
        }

        public virtual void deleteEventSubscriptionsForProcessDefinition(string processDefinitionId)
        {
            eventSubscriptionDataManager.deleteEventSubscriptionsForProcessDefinition(processDefinitionId);
        }

        // Processing /////////////////////////////////////////////////////////////

        public virtual void eventReceived(IEventSubscriptionEntity eventSubscriptionEntity, object payload, bool processASync)
        {
            if (processASync)
            {
                scheduleEventAsync(eventSubscriptionEntity, payload);
            }
            else
            {
                processEventSync(eventSubscriptionEntity, payload);
            }
        }

        protected internal virtual void processEventSync(IEventSubscriptionEntity eventSubscriptionEntity, object payload)
        {

            // A compensate event needs to be deleted before the handlers are called
            if (eventSubscriptionEntity is ICompensateEventSubscriptionEntity)
            {
                delete(eventSubscriptionEntity);
            }

            IEventHandler eventHandler = ProcessEngineConfiguration.getEventHandler(eventSubscriptionEntity.EventType);
            if (eventHandler == null)
            {
                throw new ActivitiException("Could not find eventhandler for event of type '" + eventSubscriptionEntity.EventType + "'.");
            }
            eventHandler.handleEvent(eventSubscriptionEntity, payload, CommandContext);
        }

        protected internal virtual void scheduleEventAsync(IEventSubscriptionEntity eventSubscriptionEntity, object payload)
        {
            IJobEntity message = JobEntityManager.create();
            message.JobType = Job_Fields.JOB_TYPE_MESSAGE;
            message.JobHandlerType = ProcessEventJobHandler.TYPE;
            message.JobHandlerConfiguration = eventSubscriptionEntity.Id;
            message.TenantId = eventSubscriptionEntity.TenantId;

            // TODO: support payload
            // if(payload != null) {
            // message.setEventPayload(payload);
            // }

            JobManager.scheduleAsyncJob(message);
        }

        protected internal virtual IList<ISignalEventSubscriptionEntity> toSignalEventSubscriptionEntityList(IList<IEventSubscriptionEntity> result)
        {
            IList<ISignalEventSubscriptionEntity> signalEventSubscriptionEntities = new List<ISignalEventSubscriptionEntity>(result.Count);
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in result)
            {
                signalEventSubscriptionEntities.Add((ISignalEventSubscriptionEntity)eventSubscriptionEntity);
            }
            return signalEventSubscriptionEntities;
        }

        protected internal virtual IList<IMessageEventSubscriptionEntity> toMessageEventSubscriptionEntityList(IList<IEventSubscriptionEntity> result)
        {
            IList<IMessageEventSubscriptionEntity> messageEventSubscriptionEntities = new List<IMessageEventSubscriptionEntity>(result.Count);
            foreach (IEventSubscriptionEntity eventSubscriptionEntity in result)
            {
                messageEventSubscriptionEntities.Add((IMessageEventSubscriptionEntity)eventSubscriptionEntity);
            }
            return messageEventSubscriptionEntities;
        }

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