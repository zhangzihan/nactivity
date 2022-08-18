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

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Impl.Variable;

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

        public virtual IVariableInstanceEntity Create(string name, IVariableType type, object value)
        {
            IVariableInstanceEntity variableInstance = Create();
            variableInstance.Name = name;
            variableInstance.Type = type;
            variableInstance.TypeName = type.TypeName;
            variableInstance.Value = value;
            return variableInstance;
        }

        public override void Insert(IVariableInstanceEntity entity, bool fireCreateEvent)
        {
            base.Insert(entity, fireCreateEvent);

            if (entity.ExecutionId is not null && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(entity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.VariableCount += 1;
                }
            }
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskId(string taskId)
        {
            return variableInstanceDataManager.FindVariableInstancesByTaskId(taskId);
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskIds(string[] taskIds)
        {
            return variableInstanceDataManager.FindVariableInstancesByTaskIds(taskIds);
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionId(string executionId)
        {
            return variableInstanceDataManager.FindVariableInstancesByExecutionId(executionId);
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionIds(string[] executionIds)
        {
            return variableInstanceDataManager.FindVariableInstancesByExecutionIds(executionIds);
        }

        public virtual IVariableInstanceEntity FindVariableInstanceByExecutionAndName(string executionId, string variableName)
        {
            return variableInstanceDataManager.FindVariableInstanceByExecutionAndName(executionId, variableName);
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionAndNames(string executionId, IEnumerable<string> names)
        {
            return variableInstanceDataManager.FindVariableInstancesByExecutionAndNames(executionId, names);
        }

        public virtual IVariableInstanceEntity FindVariableInstanceByTaskAndName(string taskId, string variableName)
        {
            return variableInstanceDataManager.FindVariableInstanceByTaskAndName(taskId, variableName);
        }

        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskAndNames(string taskId, IEnumerable<string> names)
        {
            return variableInstanceDataManager.FindVariableInstancesByTaskAndNames(taskId, names);
        }

        public override void Delete(IVariableInstanceEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, false);
            IByteArrayRef byteArrayRef = entity.ByteArrayRef;
            if (byteArrayRef is object)
            {
                byteArrayRef.Delete();
            }
            entity.Deleted = true;

            if (entity.ExecutionId is not null && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(entity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.VariableCount -= 1;
                }
            }

            IActivitiEventDispatcher eventDispatcher = EventDispatcher;
            if (fireDeleteEvent && eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, entity));

                eventDispatcher.DispatchEvent(CreateVariableDeleteEvent(entity));
            }

        }

        protected internal virtual IActivitiVariableEvent CreateVariableDeleteEvent(IVariableInstanceEntity variableInstance)
        {
            string processDefinitionId = null;
            if (variableInstance.ProcessInstanceId is not null)
            {
                IExecutionEntity executionEntity = ExecutionEntityManager.FindById<IExecutionEntity>(variableInstance.ProcessInstanceId);
                if (executionEntity is object)
                {
                    processDefinitionId = executionEntity.ProcessDefinitionId;
                }
            }

            return ActivitiEventBuilder.CreateVariableEvent(ActivitiEventType.VARIABLE_DELETED, variableInstance.Name, null, variableInstance.Type, variableInstance.TaskId, variableInstance.ExecutionId, variableInstance.ProcessInstanceId, processDefinitionId);
        }

        public virtual void DeleteVariableInstanceByTask(ITaskEntity task)
        {
            IDictionary<string, IVariableInstanceEntity> variableInstances = task.VariableInstanceEntities;
            if (variableInstances is object)
            {
                foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
                {
                    Delete(variableInstance);
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