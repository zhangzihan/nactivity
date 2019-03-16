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
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using System;
    using System.Collections.Generic;

    /// 
    public abstract class AbstractEntityManager<EntityImpl> : AbstractManager, IEntityManager<EntityImpl> where EntityImpl : IEntity
    {

        public AbstractEntityManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        /*
         * CRUD operations
         */


        public virtual TOut findById<TOut>(KeyValuePair<string, object> entityId)
        {
            Type type = typeof(TOut);
            return DataManager.findById<TOut>(entityId);
        }

        public virtual TOut findById<TOut>(object entityId)
        {
            return findById<TOut>(new KeyValuePair<string, object>("id", entityId));
        }

        public virtual EntityImpl create()
        {
            return DataManager.create();
        }

        public virtual void insert(EntityImpl entity)
        {
            insert(entity, true);
        }

        public virtual void insert(EntityImpl entity, bool fireCreateEvent)
        {
            DataManager.insert(entity);

            IActivitiEventDispatcher eventDispatcher = EventDispatcher;
            if (fireCreateEvent && eventDispatcher.Enabled)
            {
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, entity));
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, entity));
            }
        }

        public virtual EntityImpl update(EntityImpl entity)
        {
            return update(entity, true);
        }

        public virtual EntityImpl update(EntityImpl entity, bool fireUpdateEvent)
        {
            EntityImpl updatedEntity = DataManager.update(entity);

            if (fireUpdateEvent && EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_UPDATED, entity));
            }

            return updatedEntity;
        }

        public virtual void delete(KeyValuePair<string, object> id)
        {
            EntityImpl entity = findById<EntityImpl>(id);
            delete(entity);
        }

        public virtual void delete(EntityImpl entity)
        {
            delete(entity, true);
        }

        public virtual void delete(EntityImpl entity, bool fireDeleteEvent)
        {
            DataManager.delete(entity);

            if (fireDeleteEvent && EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, entity));
            }
        }

        protected internal abstract IDataManager<EntityImpl> DataManager { get; }

        /* Execution related entity count methods */

        protected internal virtual bool ExecutionRelatedEntityCountEnabledGlobally
        {
            get
            {
                return processEngineConfiguration.PerformanceSettings.EnableExecutionRelationshipCounts;
            }
        }

        protected internal virtual bool isExecutionRelatedEntityCountEnabled(IExecutionEntity executionEntity)
        {
            if (executionEntity is ICountingExecutionEntity)
            {
                return isExecutionRelatedEntityCountEnabled((ICountingExecutionEntity)executionEntity);
            }
            return false;
        }

        protected internal virtual bool isExecutionRelatedEntityCountEnabled(ICountingExecutionEntity executionEntity)
        {

            /*
             * There are two flags here: a global flag and a flag on the execution entity.
             * The global flag can be switched on and off between different reboots,
             * however the flag on the executionEntity refers to the state at that particular moment.
             * 
             * Global flag / ExecutionEntity flag : result
             * 
             * T / T : T (all true, regular mode with flags enabled)
             * T / F : F (global is true, but execution was of a time when it was disabled, thus treating it as disabled)
             * F / T : F (execution was of time when counting was done. But this is overruled by the global flag and thus the queries will be done)
             * F / F : F (all disabled)
             * 
             * From this table it is clear that only when both are true, the result should be true,
             * which is the regular AND rule for booleans.
             */

            return ExecutionRelatedEntityCountEnabledGlobally && executionEntity.IsCountEnabled;
        }


    }

}