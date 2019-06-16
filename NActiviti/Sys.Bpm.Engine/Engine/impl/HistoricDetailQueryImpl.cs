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
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;

    /// 
    /// 
    [Serializable]
    public class HistoricDetailQueryImpl : AbstractQuery<IHistoricDetailQuery, IHistoricDetail>, IHistoricDetailQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string _id;
        protected internal string _taskId;
        protected internal string _processInstanceId;
        protected internal string _executionId;
        protected internal string _activityId;
        protected internal string _activityInstanceId;
        protected internal string type;
        protected internal bool excludeTaskRelated;

        public HistoricDetailQueryImpl()
        {
        }

        public HistoricDetailQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricDetailQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IHistoricDetailQuery SetId(string id)
        {
            this._id = id;
            return this;
        }

        public virtual IHistoricDetailQuery SetProcessInstanceId(string processInstanceId)
        {
            this._processInstanceId = processInstanceId;
            return this;
        }

        public virtual IHistoricDetailQuery SetExecutionId(string executionId)
        {
            this._executionId = executionId;
            return this;
        }

        public virtual IHistoricDetailQuery SetActivityId(string activityId)
        {
            this._activityId = activityId;
            return this;
        }

        public virtual IHistoricDetailQuery FormProperties()
        {
            this.type = "FormProperty";
            return this;
        }

        public virtual IHistoricDetailQuery SetActivityInstanceId(string activityInstanceId)
        {
            this._activityInstanceId = activityInstanceId;
            return this;
        }

        public virtual IHistoricDetailQuery SetTaskId(string taskId)
        {
            this._taskId = taskId;
            return this;
        }

        public virtual IHistoricDetailQuery SetVariableUpdates()
        {
            this.type = "VariableUpdate";
            return this;
        }

        public virtual IHistoricDetailQuery SetExcludeTaskDetails()
        {
            this.excludeTaskRelated = true;
            return this;
        }

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.HistoricDetailEntityManager.FindHistoricDetailCountByQueryCriteria(this);
        }

        public override IList<IHistoricDetail> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            IList<IHistoricDetail> historicDetails = commandContext.HistoricDetailEntityManager.FindHistoricDetailsByQueryCriteria(this, page) ?? new List<IHistoricDetail>();
            if (historicDetails != null)
            {
                foreach (IHistoricDetail historicDetail in historicDetails)
                {
                    if (historicDetail is IHistoricDetailVariableInstanceUpdateEntity varUpdate)
                    {
                        // Touch byte-array to ensure initialized inside context
                        // TODO there should be a generic way to initialize variable
                        // values
                        //varUpdate.Bytes;

                        // ACT-863: EntityManagerFactorySession instance needed for
                        // fetching value, touch while inside context to store
                        // cached value
                        if (varUpdate.VariableType is JPAEntityVariableType)
                        {
                            // Use HistoricJPAEntityVariableType to force caching of
                            // value to return from query
                            varUpdate.VariableType = HistoricJPAEntityVariableType.SharedInstance;
                            //varUpdate.Value;
                        }
                        else if (varUpdate.VariableType is JPAEntityListVariableType)
                        {
                            // Use HistoricJPAEntityListVariableType to force
                            // caching of list to return from query
                            varUpdate.VariableType = HistoricJPAEntityListVariableType.SharedInstance;
                            //varUpdate.Value;
                        }
                    }
                }
            }
            return historicDetails;
        }

        // order by
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricDetailQuery OrderByProcessInstanceId()
        {
            SetOrderBy(HistoricDetailQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricDetailQuery OrderByTime()
        {
            SetOrderBy(HistoricDetailQueryProperty.TIME);
            return this;
        }

        public virtual IHistoricDetailQuery OrderByVariableName()
        {
            SetOrderBy(HistoricDetailQueryProperty.VARIABLE_NAME);
            return this;
        }

        public virtual IHistoricDetailQuery OrderByFormPropertyId()
        {
            SetOrderBy(HistoricDetailQueryProperty.VARIABLE_NAME);
            return this;
        }

        public virtual IHistoricDetailQuery OrderByVariableRevision()
        {
            SetOrderBy(HistoricDetailQueryProperty.VARIABLE_REVISION);
            return this;
        }

        public virtual IHistoricDetailQuery OrderByVariableType()
        {
            SetOrderBy(HistoricDetailQueryProperty.VARIABLE_TYPE);
            return this;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string Id
        {
            get
            {
                return _id;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return _processInstanceId;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return _taskId;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return _activityId;
            }
        }

        public virtual string Type
        {
            get
            {
                return type;
            }
        }

        public virtual bool ExcludeTaskRelated
        {
            get
            {
                return excludeTaskRelated;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return _executionId;
            }
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return _activityInstanceId;
            }
        }

    }

}