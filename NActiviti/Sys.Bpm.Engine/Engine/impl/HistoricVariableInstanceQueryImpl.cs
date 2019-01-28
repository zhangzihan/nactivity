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

namespace org.activiti.engine.impl
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.variable;

    /// 
    /// 
    [Serializable]
    public class HistoricVariableInstanceQueryImpl : AbstractQuery<IHistoricVariableInstanceQuery, IHistoricVariableInstance>, IHistoricVariableInstanceQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string id_Renamed;
        protected internal string taskId_Renamed;
        protected internal ISet<string> taskIds_Renamed;
        protected internal string executionId_Renamed;
        protected internal ISet<string> executionIds_Renamed;
        protected internal string processInstanceId_Renamed;
        protected internal string activityInstanceId_Renamed;
        protected internal string variableName_Renamed;
        protected internal string variableNameLike_Renamed;
        protected internal bool excludeTaskRelated;
        protected internal bool excludeVariableInitialization_Renamed;
        protected internal QueryVariableValue queryVariableValue;

        public HistoricVariableInstanceQueryImpl()
        {
        }

        public HistoricVariableInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricVariableInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IHistoricVariableInstanceQuery id(string id)
        {
            this.id_Renamed = id;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery processInstanceId(string processInstanceId)
        {
            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery executionId(string executionId)
        {
            if (ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("Execution id is null");
            }
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery executionIds(ISet<string> executionIds)
        {
            if (executionIds == null)
            {
                throw new ActivitiIllegalArgumentException("executionIds is null");
            }
            if (executionIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Set of executionIds is empty");
            }
            this.executionIds_Renamed = executionIds;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery activityInstanceId(string activityInstanceId)
        {
            this.activityInstanceId_Renamed = activityInstanceId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery taskId(string taskId)
        {
            if (ReferenceEquals(taskId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }
            if (excludeTaskRelated)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskId together with excludeTaskVariables");
            }
            this.taskId_Renamed = taskId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery taskIds(ISet<string> taskIds)
        {
            if (taskIds == null)
            {
                throw new ActivitiIllegalArgumentException("taskIds is null");
            }
            if (taskIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Set of taskIds is empty");
            }
            if (excludeTaskRelated)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskIds together with excludeTaskVariables");
            }
            this.taskIds_Renamed = taskIds;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery excludeTaskVariables()
        {
            if (!ReferenceEquals(taskId_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskId together with excludeTaskVariables");
            }
            if (taskIds_Renamed != null)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskIds together with excludeTaskVariables");
            }
            excludeTaskRelated = true;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery excludeVariableInitialization()
        {
            excludeVariableInitialization_Renamed = true;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableName(string variableName)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            this.variableName_Renamed = variableName;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableValueEquals(string variableName, object variableValue)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue == null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_Renamed = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.EQUALS, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableValueNotEquals(string variableName, object variableValue)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue == null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_Renamed = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.NOT_EQUALS, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableValueLike(string variableName, string variableValue)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (ReferenceEquals(variableValue, null))
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_Renamed = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.LIKE, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableValueLikeIgnoreCase(string variableName, string variableValue)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (ReferenceEquals(variableValue, null))
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_Renamed = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue.ToLower(), QueryOperator.LIKE_IGNORE_CASE, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery variableNameLike(string variableNameLike)
        {
            if (ReferenceEquals(variableNameLike, null))
            {
                throw new ActivitiIllegalArgumentException("variableNameLike is null");
            }
            this.variableNameLike_Renamed = variableNameLike;
            return this;
        }

        protected internal virtual void ensureVariablesInitialized()
        {
            if (this.queryVariableValue != null)
            {
                IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;
                queryVariableValue.initialize(variableTypes);
            }
        }

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            ensureVariablesInitialized();
            return commandContext.HistoricVariableInstanceEntityManager.findHistoricVariableInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricVariableInstance> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            ensureVariablesInitialized();

            IList<IHistoricVariableInstance> historicVariableInstances = commandContext.HistoricVariableInstanceEntityManager.findHistoricVariableInstancesByQueryCriteria(this, page);

            if (!excludeVariableInitialization_Renamed)
            {
                foreach (IHistoricVariableInstance historicVariableInstance in historicVariableInstances)
                {
                    if (historicVariableInstance is IHistoricVariableInstanceEntity)
                    {
                        IHistoricVariableInstanceEntity variableEntity = (IHistoricVariableInstanceEntity)historicVariableInstance;
                        if (variableEntity != null && variableEntity.VariableType != null)
                        {
                            //variableEntity.Value;

                            // make sure JPA entities are cached for later retrieval
                            if (JPAEntityVariableType.TYPE_NAME.Equals(variableEntity.VariableType.TypeName) || JPAEntityListVariableType.TYPE_NAME.Equals(variableEntity.VariableType.TypeName))
                            {
                                ((ICacheableVariable)variableEntity.VariableType).ForceCacheable = true;
                            }
                        }
                    }
                }
            }
            return historicVariableInstances;
        }

        // order by
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricVariableInstanceQuery orderByProcessInstanceId()
        {
            orderBy(HistoricVariableInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery orderByVariableName()
        {
            orderBy(HistoricVariableInstanceQueryProperty.VARIABLE_NAME);
            return this;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_Renamed;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return taskId_Renamed;
            }
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return activityInstanceId_Renamed;
            }
        }

        public virtual bool ExcludeTaskRelated
        {
            get
            {
                return excludeTaskRelated;
            }
        }

        public virtual string VariableName
        {
            get
            {
                return variableName_Renamed;
            }
        }

        public virtual string VariableNameLike
        {
            get
            {
                return variableNameLike_Renamed;
            }
        }

        public virtual QueryVariableValue QueryVariableValue
        {
            get
            {
                return queryVariableValue;
            }
        }

    }

}