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

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.impl.variable;

    /// 
    /// 
    /// 
    public class VariableInstanceEntityManagerImpl : AbstractEntityManager<IVariableInstanceEntity>, IVariableInstanceEntityManager
    {

        protected internal IVariableInstanceDataManager variableInstanceDataManager;

        public VariableInstanceEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IVariableInstanceDataManager variableInstanceDataManager) : base(processEngineConfiguration)
        {
            this.variableInstanceDataManager = variableInstanceDataManager;
        }

        protected internal override IDataManager<IVariableInstanceEntity> DataManager
        {
            get
            {
                return variableInstanceDataManager;
            }
        }

        public virtual IVariableInstanceEntity create(string name, IVariableType type, object value)
        {
            IVariableInstanceEntity variableInstance = create();
            variableInstance.Name = name;
            variableInstance.Type = type;
            variableInstance.TypeName = type.TypeName;
            variableInstance.Value = value;
            return variableInstance;
        }

        public override void insert(IVariableInstanceEntity entity, bool fireCreateEvent)
        {
            base.insert(entity, fireCreateEvent);

            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(entity.ExecutionId);
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.VariableCount = executionEntity.VariableCount + 1;
                }
            }
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskId(string taskId)
        {
            return variableInstanceDataManager.findVariableInstancesByTaskId(taskId);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskIds(ISet<string> taskIds)
        {
            return variableInstanceDataManager.findVariableInstancesByTaskIds(taskIds);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionId(string executionId)
        {
            return variableInstanceDataManager.findVariableInstancesByExecutionId(executionId);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionIds(ISet<string> executionIds)
        {
            return variableInstanceDataManager.findVariableInstancesByExecutionIds(executionIds);
        }

        public virtual IVariableInstanceEntity findVariableInstanceByExecutionAndName(string executionId, string variableName)
        {
            return variableInstanceDataManager.findVariableInstanceByExecutionAndName(executionId, variableName);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionAndNames(string executionId, ICollection<string> names)
        {
            return variableInstanceDataManager.findVariableInstancesByExecutionAndNames(executionId, names);
        }

        public virtual IVariableInstanceEntity findVariableInstanceByTaskAndName(string taskId, string variableName)
        {
            return variableInstanceDataManager.findVariableInstanceByTaskAndName(taskId, variableName);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskAndNames(string taskId, ICollection<string> names)
        {
            return variableInstanceDataManager.findVariableInstancesByTaskAndNames(taskId, names);
        }

        public override void delete(IVariableInstanceEntity entity, bool fireDeleteEvent)
        {
            base.delete(entity, false);
            IByteArrayRef byteArrayRef = entity.ByteArrayRef;
            if (byteArrayRef != null)
            {
                byteArrayRef.delete();
            }
            entity.Deleted = true;

            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(entity.ExecutionId);
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.VariableCount = executionEntity.VariableCount - 1;
                }
            }

            IActivitiEventDispatcher eventDispatcher = EventDispatcher;
            if (fireDeleteEvent && eventDispatcher.Enabled)
            {
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, entity));

                eventDispatcher.dispatchEvent(createVariableDeleteEvent(entity));
            }

        }

        protected internal virtual IActivitiVariableEvent createVariableDeleteEvent(IVariableInstanceEntity variableInstance)
        {

            string processDefinitionId = null;
            if (!ReferenceEquals(variableInstance.ProcessInstanceId, null))
            {
                IExecutionEntity executionEntity = ExecutionEntityManager.findById<IExecutionEntity>(variableInstance.ProcessInstanceId);
                if (executionEntity != null)
                {
                    processDefinitionId = executionEntity.ProcessDefinitionId;
                }
            }

            return ActivitiEventBuilder.createVariableEvent(ActivitiEventType.VARIABLE_DELETED, variableInstance.Name, null, variableInstance.Type, variableInstance.TaskId, variableInstance.ExecutionId, variableInstance.ProcessInstanceId, processDefinitionId);
        }

        public virtual void deleteVariableInstanceByTask(ITaskEntity task)
        {
            IDictionary<string, IVariableInstanceEntity> variableInstances = task.VariableInstanceEntities;
            if (variableInstances != null)
            {
                foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
                {
                    delete(variableInstance);
                }
            }
        }

        public virtual IVariableInstanceDataManager VariableInstanceDataManager
        {
            get
            {
                return variableInstanceDataManager;
            }
            set
            {
                this.variableInstanceDataManager = value;
            }
        }
    }
}