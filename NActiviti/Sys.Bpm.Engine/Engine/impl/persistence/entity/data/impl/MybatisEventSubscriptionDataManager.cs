using System;
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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;

    /// 
    public class MybatisEventSubscriptionDataManager : AbstractDataManager<IEventSubscriptionEntity>, IEventSubscriptionDataManager
    {

        private static IList<Type> ENTITY_SUBCLASSES = new List<Type>();

        static MybatisEventSubscriptionDataManager()
        {
            ENTITY_SUBCLASSES.Add(typeof(MessageEventSubscriptionEntityImpl));
            ENTITY_SUBCLASSES.Add(typeof(SignalEventSubscriptionEntityImpl));
            ENTITY_SUBCLASSES.Add(typeof(CompensateEventSubscriptionEntityImpl));
        }

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> eventSubscriptionsByNameMatcher = new EventSubscriptionsByNameMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> eventSubscritionsByExecutionIdMatcher = new EventSubscriptionsByExecutionIdMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> eventSubscriptionsByProcInstTypeAndActivityMatcher = new EventSubscriptionsByProcInstTypeAndActivityMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> eventSubscriptionsByExecutionAndTypeMatcher = new EventSubscriptionsByExecutionAndTypeMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> signalEventSubscriptionByNameAndExecutionMatcher = new SignalEventSubscriptionByNameAndExecutionMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> signalEventSubscriptionByProcInstAndEventNameMatcher = new SignalEventSubscriptionByProcInstAndEventNameMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> signalEventSubscriptionByEventNameMatcher = new SignalEventSubscriptionByEventNameMatcher();

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> messageEventSubscriptionsByProcInstAndEventNameMatcher = new MessageEventSubscriptionsByProcInstAndEventNameMatcher();

        public MybatisEventSubscriptionDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(EventSubscriptionEntityImpl);
            }
        }

        public override IList<Type> ManagedEntitySubClasses
        {
            get
            {
                return ENTITY_SUBCLASSES;
            }
        }

        public override IEventSubscriptionEntity create()
        {
            // only allowed to create subclasses
            throw new System.NotSupportedException();
        }

        public virtual ICompensateEventSubscriptionEntity createCompensateEventSubscription()
        {
            return new CompensateEventSubscriptionEntityImpl();
        }

        public virtual IMessageEventSubscriptionEntity createMessageEventSubscription()
        {
            return new MessageEventSubscriptionEntityImpl();
        }

        public virtual ISignalEventSubscriptionEntity createSignalEventSubscription()
        {
            return new SignalEventSubscriptionEntityImpl();
        }

        public virtual long findEventSubscriptionCountByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl)
        {
            const string query = "selectEventSubscriptionCountByQueryCriteria";
            return DbSqlSession.selectOne<EventSubscriptionEntityImpl, long?>(query, eventSubscriptionQueryImpl).GetValueOrDefault();
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl, Page page)
        {
            const string query = "selectEventSubscriptionByQueryCriteria";
            return DbSqlSession.selectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, eventSubscriptionQueryImpl, page);
        }

        public virtual IList<IMessageEventSubscriptionEntity> findMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            return toMessageEventSubscriptionEntityList((IList<IEventSubscriptionEntity>)getList("selectMessageEventSubscriptionsByProcessInstanceAndEventName", new { processInstanceId, eventName }, messageEventSubscriptionsByProcInstAndEventNameMatcher, true));
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByEventName(string eventName, string tenantId)
        {
            const string query = "selectSignalEventSubscriptionsByEventName";

            IDictionary<string, string> @params = new Dictionary<string, string>();
            @params["eventName"] = eventName;
            @params["tenantId"] = tenantId;

            IList<IEventSubscriptionEntity> result = (IList<IEventSubscriptionEntity>)getList(query, @params, signalEventSubscriptionByEventNameMatcher, true);
            return toSignalEventSubscriptionEntityList(result);
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            const string query = "selectSignalEventSubscriptionsByProcessInstanceAndEventName";
            return toSignalEventSubscriptionEntityList((IList<IEventSubscriptionEntity>)getList(query, new { processInstanceId, eventName }, signalEventSubscriptionByProcInstAndEventNameMatcher, true));
        }

        public virtual IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByNameAndExecution(string eventName, string executionId)
        {
            return toSignalEventSubscriptionEntityList((IList<IEventSubscriptionEntity>)getList("selectSignalEventSubscriptionsByNameAndExecution", new { executionId, eventName }, signalEventSubscriptionByNameAndExecutionMatcher, true));
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByExecutionAndType(string executionId, string eventType)
        {
            return (IList<IEventSubscriptionEntity>)getList("selectEventSubscriptionsByExecutionAndType", new { executionId, eventType }, eventSubscriptionsByExecutionAndTypeMatcher, true);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string eventType)
        {
            return (IList<IEventSubscriptionEntity>)getList("selectEventSubscriptionsByProcessInstanceTypeAndActivity", new { processInstanceId, eventType, activityId }, eventSubscriptionsByProcInstTypeAndActivityMatcher, true);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByExecution(string executionId)
        {
            return (IList<IEventSubscriptionEntity>)getList("selectEventSubscriptionsByExecution", new { executionId }, eventSubscritionsByExecutionIdMatcher, true);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId)
        {
            const string query = "selectEventSubscriptionsByTypeAndProcessDefinitionId";
            IDictionary<string, string> @params = new Dictionary<string, string>();
            @params["eventType"] = type;
            @params["processDefinitionId"] = processDefinitionId;
            @params["tenantId"] = tenantId;

            return DbSqlSession.selectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, @params);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByName(string type, string eventName, string tenantId)
        {

            IDictionary<string, string> @params = new Dictionary<string, string>();
            @params["eventType"] = type;
            @params["eventName"] = eventName;
            @params["tenantId"] = tenantId;

            return (IList<IEventSubscriptionEntity>)getList("selectEventSubscriptionsByName", @params, eventSubscriptionsByNameMatcher, true);
        }

        public virtual IList<IEventSubscriptionEntity> findEventSubscriptionsByNameAndExecution(string eventType, string eventName, string executionId)
        {
            const string query = "selectEventSubscriptionsByNameAndExecution";
            return DbSqlSession.selectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, new { eventType, eventName, executionId });
        }

        public virtual IMessageEventSubscriptionEntity findMessageStartEventSubscriptionByName(string messageName, string tenantId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>();
            @params["eventName"] = messageName;
            @params["tenantId"] = tenantId;

            IMessageEventSubscriptionEntity entity = DbSqlSession.selectOne<MessageEventSubscriptionEntityImpl, IMessageEventSubscriptionEntity>("selectMessageStartEventSubscriptionByName", @params);
            return entity;
        }

        public virtual void updateEventSubscriptionTenantId(string oldTenantId, string newTenantId)
        {
            DbSqlSession.update<EventSubscriptionEntityImpl>("updateTenantIdOfEventSubscriptions", new { oldTenantId, newTenantId });
        }

        public virtual void deleteEventSubscriptionsForProcessDefinition(string processDefinitionId)
        {
            DbSqlSession.delete("deleteEventSubscriptionsForProcessDefinition", new { processDefinitionId }, typeof(EventSubscriptionEntityImpl));
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

    }

}