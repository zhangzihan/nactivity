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

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;

    /// 
    /// 
    [Serializable]
    public class HistoricVariableInstanceQueryImpl : AbstractQuery<IHistoricVariableInstanceQuery, IHistoricVariableInstance>, IHistoricVariableInstanceQuery
    {
        private const long serialVersionUID = 1L;
        protected internal string id_;
        protected internal string taskId_;
        protected internal string[] taskIds_;
        protected internal string executionId_;
        protected internal string[] executionIds_;
        protected internal string processInstanceId_;
        protected internal string activityInstanceId_;
        protected internal string variableName_;
        protected internal string variableNameLike_;
        protected internal bool excludeTaskRelated;
        protected internal bool excludeVariableInitialization_;
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

        public virtual IHistoricVariableInstanceQuery SetId(string id)
        {
            this.id_ = id;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetProcessInstanceId(string processInstanceId)
        {
            this.processInstanceId_ = processInstanceId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetExecutionId(string executionId)
        {
            this.executionId_ = executionId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetExecutionIds(string[] executionIds)
        {
            this.executionIds_ = executionIds;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetActivityInstanceId(string activityInstanceId)
        {
            this.activityInstanceId_ = activityInstanceId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetTaskId(string taskId)
        {
            if (excludeTaskRelated)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskId together with excludeTaskVariables");
            }
            this.taskId_ = taskId;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetTaskIds(string[] taskIds)
        {
            if (excludeTaskRelated)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskIds together with excludeTaskVariables");
            }
            this.taskIds_ = taskIds;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetExcludeTaskVariables()
        {
            if (taskId_ is object)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskId together with excludeTaskVariables");
            }
            if (taskIds_ is object)
            {
                throw new ActivitiIllegalArgumentException("Cannot use taskIds together with excludeTaskVariables");
            }
            excludeTaskRelated = true;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetExcludeVariableInitialization()
        {
            excludeVariableInitialization_ = true;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetVariableName(string variableName)
        {
            //if (ReferenceEquals(variableName, null))
            //{
            //    throw new ActivitiIllegalArgumentException("variableName is null");
            //}
            this.variableName_ = variableName;
            return this;
        }

        public virtual IHistoricVariableInstanceQuery VariableValueEquals(string variableName, object variableValue)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue is null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_ = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.EQUALS, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery VariableValueNotEquals(string variableName, object variableValue)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue is null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_ = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.NOT_EQUALS, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery VariableValueLike(string variableName, string variableValue)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue is null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_ = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue, QueryOperator.LIKE, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery VariableValueLikeIgnoreCase(string variableName, string variableValue)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            if (variableValue is null)
            {
                throw new ActivitiIllegalArgumentException("variableValue is null");
            }
            this.variableName_ = variableName;
            queryVariableValue = new QueryVariableValue(variableName, variableValue.ToLower(), QueryOperator.LIKE_IGNORE_CASE, true);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery SetVariableNameLike(string variableNameLike)
        {
            if (variableNameLike is null)
            {
                throw new ActivitiIllegalArgumentException("variableNameLike is null");
            }
            this.variableNameLike_ = variableNameLike;
            return this;
        }

        protected internal virtual void EnsureVariablesInitialized()
        {
            if (this.queryVariableValue is object)
            {
                IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;
                queryVariableValue.Initialize(variableTypes);
            }
        }

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            return commandContext.HistoricVariableInstanceEntityManager.FindHistoricVariableInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricVariableInstance> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();

            IList<IHistoricVariableInstance> historicVariableInstances = commandContext.HistoricVariableInstanceEntityManager.FindHistoricVariableInstancesByQueryCriteria(this, page);

            return historicVariableInstances;
        }

        // order by
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricVariableInstanceQuery OrderByProcessInstanceId()
        {
            SetOrderBy(HistoricVariableInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricVariableInstanceQuery OrderByVariableName()
        {
            SetOrderBy(HistoricVariableInstanceQueryProperty.VARIABLE_NAME);
            return this;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public string Id
        {
            get => id_;
            set => id_ = value;
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_;
            }
            set => processInstanceId_ = value;
        }

        public virtual string ExecutionId
        {
            get => executionId_;
            set => SetExecutionId(value);
        }

        public virtual string[] ExecutionIds
        {
            get => executionIds_;
            set => SetExecutionIds(value);
        }

        public virtual string TaskId
        {
            get => taskId_;
            set => SetTaskId(value);
        }

        public virtual string[] TaskIds
        {
            get => taskIds_;
            set => SetTaskIds(value);
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return activityInstanceId_;
            }
            set => activityInstanceId_ = value;
        }

        public virtual bool ExcludeTaskRelated
        {
            get
            {
                return excludeTaskRelated;
            }
            set => excludeTaskRelated = value;
        }

        public virtual string VariableName
        {
            get
            {
                return variableName_;
            }
            set => variableName_ = value;
        }

        public virtual string VariableNameLike
        {
            get
            {
                return variableNameLike_;
            }
            set => variableNameLike_ = value;
        }

        public virtual QueryVariableValue QueryVariableValue
        {
            get
            {
                return queryVariableValue;
            }
            set => queryVariableValue = value;
        }
    }

}