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


        public virtual TOut FindById<TOut>(KeyValuePair<string, object> entityId)
        {
            return DataManager.FindById<TOut>(entityId);
        }

        public virtual TOut FindById<TOut>(object entityId)
        {
            return FindById<TOut>(new KeyValuePair<string, object>("id", entityId));
        }

        public virtual EntityImpl Create()
        {
            return DataManager.Create();
        }

        public virtual void Insert(EntityImpl entity)
        {
            Insert(entity, true);
        }

        public virtual void Insert(EntityImpl entity, bool fireCreateEvent)
        {
            DataManager.Insert(entity);

            IActivitiEventDispatcher eventDispatcher = EventDispatcher;
            if (fireCreateEvent && eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, entity));
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, entity));
            }
        }

        public virtual EntityImpl Update(EntityImpl entity)
        {
            return Update(entity, true);
        }

        public virtual EntityImpl Update(EntityImpl entity, bool fireUpdateEvent)
        {
            EntityImpl updatedEntity = DataManager.Update(entity);

            if (fireUpdateEvent && EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, entity));
            }

            return updatedEntity;
        }

        public virtual void Delete(KeyValuePair<string, object> id)
        {
            EntityImpl entity = FindById<EntityImpl>(id);
            Delete(entity);
        }

        public virtual void Delete(EntityImpl entity)
        {
            Delete(entity, true);
        }

        public virtual void Delete(EntityImpl entity, bool fireDeleteEvent)
        {
            DataManager.Delete(entity);

            if (fireDeleteEvent && EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, entity));
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

        protected internal virtual bool IsExecutionRelatedEntityCountEnabled(IExecutionEntity executionEntity)
        {
            if (executionEntity is ICountingExecutionEntity)
            {
                return IsExecutionRelatedEntityCountEnabled((ICountingExecutionEntity)executionEntity);
            }
            return false;
        }

        protected internal virtual bool IsExecutionRelatedEntityCountEnabled(ICountingExecutionEntity executionEntity)
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