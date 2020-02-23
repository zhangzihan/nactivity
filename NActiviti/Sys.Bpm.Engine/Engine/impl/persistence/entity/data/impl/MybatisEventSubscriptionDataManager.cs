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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl
{

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Caches;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher;
    using System.Linq;

    /// <inheritdoc />
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

        protected internal ICachedEntityMatcher<IEventSubscriptionEntity> eventSubscriptionsByProcInstTypeMatcher = new EventSubscriptionsByProcInstTypeMatcher();

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
            var list = GetList("selectMessageEventSubscriptionsByProcessInstanceAndEventName", new Dictionary<string, object>
            {
                [nameof(processInstanceId)] = processInstanceId,
                [nameof(eventName)] = eventName,
            }, messageEventSubscriptionsByProcInstAndEventNameMatcher, true, typeof(MessageEventSubscriptionEntityImpl), typeof(MessageEventSubscriptionEntityImpl));

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

            var list = GetList(query, new Dictionary<string, object>
            {
                [nameof(processInstanceId)] = processInstanceId,
                [nameof(eventName)] = eventName
            }, signalEventSubscriptionByProcInstAndEventNameMatcher, true, typeof(SignalEventSubscriptionEntityImpl), typeof(SignalEventSubscriptionEntityImpl));

            return list.Cast<ISignalEventSubscriptionEntity>().ToList();
        }

        public virtual IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByNameAndExecution(string eventName, string executionId)
        {
            var list = GetList("selectSignalEventSubscriptionsByNameAndExecution", new Dictionary<string, object>
            {
                [nameof(executionId)] = executionId,
                [nameof(eventName)] = eventName
            }, signalEventSubscriptionByNameAndExecutionMatcher, true, typeof(SignalEventSubscriptionEntityImpl), typeof(SignalEventSubscriptionEntityImpl));

            return list.Cast<ISignalEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecutionAndType(string executionId, string eventType)
        {
            var list = GetList("selectEventSubscriptionsByExecutionAndType", new
            Dictionary<string, object>
            {
                [nameof(executionId)] = executionId,
                [nameof(eventType)] = eventType
            }, eventSubscriptionsByExecutionAndTypeMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string eventType)
        {
            var list = GetList("selectEventSubscriptionsByProcessInstanceTypeAndActivity", new Dictionary<string, object>
            {
                [nameof(processInstanceId)] = processInstanceId,
                [nameof(eventType)] = eventType,
                [nameof(activityId)] = activityId
            }, eventSubscriptionsByProcInstTypeAndActivityMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

            return list.Cast<IEventSubscriptionEntity>().ToList();
        }

        public IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionId(string executionId)
        {
            var list = GetList("selectEventSubscriptionsByExecutionAndType", new
            Dictionary<string, object>
            {
                [nameof(executionId)] = executionId,
                ["eventType"] = CompensateEventSubscriptionEntityFields.EVENT_TYPE,
            }, eventSubscriptionsByExecutionAndTypeMatcher, true, typeof(CompensateEventSubscriptionEntityImpl), typeof(ICompensateEventSubscriptionEntity));

            return list.Cast<ICompensateEventSubscriptionEntity>().ToList();
        }

        public IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceId(string processInstanceId)
        {
            var list = GetList("selectEventSubscriptionsByProcessInstanceType", new
            Dictionary<string, object>
            {
                [nameof(processInstanceId)] = processInstanceId,
                ["eventType"] = CompensateEventSubscriptionEntityFields.EVENT_TYPE,
            }, eventSubscriptionsByProcInstTypeMatcher, true, typeof(CompensateEventSubscriptionEntityImpl), typeof(ICompensateEventSubscriptionEntity));

            return list.Cast<ICompensateEventSubscriptionEntity>().ToList();
        }

        public IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
            {
                return FindCompensateEventSubscriptionsByProcessInstanceId(processInstanceId);
            }

            var list = GetList("selectEventSubscriptionsByProcessInstanceTypeAndActivity", new Dictionary<string, object>
            {
                [nameof(processInstanceId)] = processInstanceId,
                ["eventType"] = CompensateEventSubscriptionEntityFields.EVENT_TYPE,
                [nameof(activityId)] = activityId
            }, eventSubscriptionsByProcInstTypeAndActivityMatcher, true, typeof(CompensateEventSubscriptionEntityImpl), typeof(ICompensateEventSubscriptionEntity));

            return list.Cast<ICompensateEventSubscriptionEntity>().ToList();
        }

        protected internal override ICollection<IEventSubscriptionEntity> GetList(string dbQueryName, object parameter, ICachedEntityMatcher<IEventSubscriptionEntity> cachedEntityMatcher, bool checkCache, Type managedEntityClass, Type outType)
        {
            ICollection<IEventSubscriptionEntity> result = selectList(DbSqlSession, managedEntityClass ?? typeof(EventSubscriptionEntityImpl), outType, dbQueryName, parameter);

            if (checkCache)
            {
                ICollection<CachedEntity> cachedObjects = EntityCache.FindInCacheAsCachedObjects(managedEntityClass);

                if ((cachedObjects != null && cachedObjects.Count > 0) || ManagedEntitySubClasses != null)
                {
                    Dictionary<string, IEventSubscriptionEntity> entityMap = new Dictionary<string, IEventSubscriptionEntity>(result.Count);

                    // Database entities
                    foreach (IEventSubscriptionEntity entity in result)
                    {
                        entityMap[entity.Id] = entity;
                    }

                    // Cache entities
                    if (cachedObjects != null && cachedEntityMatcher != null)
                    {
                        foreach (CachedEntity cachedObject in cachedObjects)
                        {
                            IEventSubscriptionEntity cachedEntity = (IEventSubscriptionEntity)cachedObject.Entity;
                            if (cachedEntityMatcher.IsRetained(result, cachedObjects, cachedEntity, parameter))
                            {
                                entityMap[cachedEntity.Id] = cachedEntity; // will overwite db version with newer version
                            }
                        }
                    }

                    if (ManagedEntitySubClasses != null && cachedEntityMatcher != null)
                    {
                        foreach (Type entitySubClass in ManagedEntitySubClasses)
                        {
                            ICollection<CachedEntity> subclassCachedObjects = EntityCache.FindInCacheAsCachedObjects(entitySubClass);
                            if (subclassCachedObjects != null)
                            {
                                foreach (CachedEntity subclassCachedObject in subclassCachedObjects)
                                {
                                    IEventSubscriptionEntity cachedSubclassEntity = (IEventSubscriptionEntity)subclassCachedObject.Entity;
                                    if (cachedEntityMatcher.IsRetained(result, cachedObjects, cachedSubclassEntity, parameter))
                                    {
                                        entityMap[cachedSubclassEntity.Id] = cachedSubclassEntity; // will overwite db version with newer version
                                    }
                                }
                            }
                        }
                    }

                    result = entityMap.Values;

                }

            }

            // Remove entries which are already deleted
            if (result.Count > 0)
            {
                var list = new List<IEventSubscriptionEntity>(result);
                for (var idx = list.Count - 1; idx >= 0; idx--)
                {
                    var item = list[idx];
                    if (DbSqlSession.IsEntityToBeDeleted(item))
                    {
                        list.RemoveAt(idx);
                    }
                }

                return list;
            }

            return new List<IEventSubscriptionEntity>();
        }

        public virtual IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecution(string executionId)
        {
            var list = GetList("selectEventSubscriptionsByExecution", new Dictionary<string, object> { [nameof(executionId)] = executionId }, eventSubscritionsByExecutionIdMatcher, true, typeof(EventSubscriptionEntityImpl), typeof(EventSubscriptionEntityImpl));

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

        /// <inheritdoc />
        public virtual IMessageEventSubscriptionEntity FindMessageStartEventSubscriptionByName(string messageName, string tenantId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>
            {
                ["eventName"] = messageName,
                ["tenantId"] = tenantId
            };

            IMessageEventSubscriptionEntity entity = DbSqlSession.SelectOne<MessageEventSubscriptionEntityImpl, MessageEventSubscriptionEntityImpl>("selectMessageStartEventSubscriptionByName", @params);

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