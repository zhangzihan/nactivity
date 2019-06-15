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
    using System.Linq;

    /// 
    public class MybatisEventSubscriptionDataManager : AbstractDataManager<IEventSubscriptionEntity>, IEventSubscriptionDataManager
    {

        private static readonly IList<Type> ENTITY_SUBCLASSES = new List<Type>();

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

        public override IEventSubscriptionEntity Create()
        {
            // only allowed to create subclasses
            throw new System.NotSupportedException();
        }

        public virtual ICompensateEventSubscriptionEntity CreateCompensateEventSubscription()
        {
            return new CompensateEventSubscriptionEntityImpl();
        }

        public virtual IMessageEventSubscriptionEntity CreateMessageEventSubscription()
        {
            return new MessageEventSubscriptionEntityImpl();
        }

        public virtual ISignalEventSubscriptionEntity CreateSignalEventSubscription()
        {
            return new SignalEventSubscriptionEntityImpl();
        }

        public virtual long FindEventSubscriptionCountByQueryCriteria(IEventSubscriptionQuery eventSubscriptionQueryImpl)
        {
            const string query = "selectEventSubscriptionCountByQueryCriteria";
            return DbSqlSession.SelectOne<EventSubscriptionEntityImpl, long?>(query, eventSubscriptionQueryImpl).GetValueOrDefault();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByQueryCriteria(IEventSubscriptionQuery eventSubscriptionQueryImpl, Page page)
        {
            const string query = "selectEventSubscriptionByQueryCriteria";
            return DbSqlSession.SelectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, eventSubscriptionQueryImpl, page);
        }

        public virtual IList<IMessageEventSubscriptionEntity> FindMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            var list = GetList("selectMessageEventSubscriptionsByProcessInstanceAndEventName", new { processInstanceId, eventName }, messageEventSubscriptionsByProcInstAndEventNameMatcher, true, typeof(MessageEventSubscriptionEntityImpl), typeof(MessageEventSubscriptionEntityImpl));

            return list.Cast<IMessageEventSubscriptionEntity>().ToList();
        }

        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByEventName(string eventName, string tenantId)
        {
            const string query = "selectSignalEventSubscriptionsByEventName";

            IDictionary<string, string> @params = new Dictionary<string, string>
            {
                ["eventName"] = eventName,
                ["tenantId"] = tenantId
            };

            var list = GetList(query, @params, signalEventSubscriptionByEventNameMatcher, true, typeof(SignalEventSubscriptionEntityImpl), typeof(SignalEventSubscriptionEntityImpl));

            return list.Cast<ISignalEventSubscriptionEntity>().ToList();
        }

        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName)
        {
            const string query = "selectSignalEventSubscriptionsByProcessInstanceAndEventName";

            var list = GetList(query, new { processInstanceId, eventName }, signalEventSubscriptionByProcInstAndEventNameMatcher, true, typeof(SignalEventSubscriptionEntityImpl), typeof(SignalEventSubscriptionEntityImpl));

            return list.Cast<ISignalEventSubscriptionEntity>().ToList();
        }

        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByNameAndExecution(string eventName, string executionId)
        {
            var list = GetList("selectSignalEventSubscriptionsByNameAndExecution", new { executionId, eventName }, signalEventSubscriptionByNameAndExecutionMatcher, true, typeof(SignalEventSubscriptionEntityImpl), typeof(SignalEventSubscriptionEntityImpl));

            return list.Cast<ISignalEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecutionAndType(string executionId, string eventType)
        {
            var list = GetList("selectEventSubscriptionsByExecutionAndType", new { executionId, eventType }, eventSubscriptionsByExecutionAndTypeMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string eventType)
        {
            var list = GetList("selectEventSubscriptionsByProcessInstanceTypeAndActivity", new { processInstanceId, eventType, activityId }, eventSubscriptionsByProcInstTypeAndActivityMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecution(string executionId)
        {
            var list = GetList("selectEventSubscriptionsByExecution", new { executionId }, eventSubscritionsByExecutionIdMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId)
        {
            const string query = "selectEventSubscriptionsByTypeAndProcessDefinitionId";
            IDictionary<string, string> @params = new Dictionary<string, string>
            {
                ["eventType"] = type,
                ["processDefinitionId"] = processDefinitionId,
                ["tenantId"] = tenantId
            };

            return DbSqlSession.SelectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, @params);
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByName(string type, string eventName, string tenantId)
        {

            IDictionary<string, string> @params = new Dictionary<string, string>
            {
                ["eventType"] = type,
                ["eventName"] = eventName,
                ["tenantId"] = tenantId
            };

            var list = GetList("selectEventSubscriptionsByName", @params, eventSubscriptionsByNameMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByNameAndExecution(string eventType, string eventName, string executionId)
        {
            const string query = "selectEventSubscriptionsByNameAndExecution";
            return DbSqlSession.SelectList<EventSubscriptionEntityImpl, IEventSubscriptionEntity>(query, new { eventType, eventName, executionId });
        }

        public virtual IMessageEventSubscriptionEntity FindMessageStartEventSubscriptionByName(string messageName, string tenantId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>
            {
                ["eventName"] = messageName,
                ["tenantId"] = tenantId
            };

            IMessageEventSubscriptionEntity entity = DbSqlSession.SelectOne<MessageEventSubscriptionEntityImpl, IMessageEventSubscriptionEntity>("selectMessageStartEventSubscriptionByName", @params);

            return entity;
        }

        public virtual void UpdateEventSubscriptionTenantId(string oldTenantId, string newTenantId)
        {
            DbSqlSession.Update<EventSubscriptionEntityImpl>("updateTenantIdOfEventSubscriptions", new { oldTenantId, newTenantId });
        }

        public override void Delete(IEventSubscriptionEntity entity)
        {
            base.Delete(entity);
        }

        public virtual void DeleteEventSubscriptionsForProcessDefinition(string processDefinitionId)
        {
            DbSqlSession.Delete("deleteEventSubscriptionsForProcessDefinition", new { processDefinitionId }, typeof(EventSubscriptionEntityImpl));
        }
    }
}