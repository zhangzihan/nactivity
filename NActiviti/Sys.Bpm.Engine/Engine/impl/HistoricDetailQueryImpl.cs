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
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.variable;

    /// 
    /// 
    [Serializable]
    public class HistoricDetailQueryImpl : AbstractQuery<IHistoricDetailQuery, IHistoricDetail>, IHistoricDetailQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string id_Renamed;
        protected internal string taskId_Renamed;
        protected internal string processInstanceId_Renamed;
        protected internal string executionId_Renamed;
        protected internal string activityId_Renamed;
        protected internal string activityInstanceId_Renamed;
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

        public virtual IHistoricDetailQuery id(string id)
        {
            this.id_Renamed = id;
            return this;
        }

        public virtual IHistoricDetailQuery processInstanceId(string processInstanceId)
        {
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IHistoricDetailQuery executionId(string executionId)
        {
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IHistoricDetailQuery activityId(string activityId)
        {
            this.activityId_Renamed = activityId;
            return this;
        }

        public virtual IHistoricDetailQuery activityInstanceId(string activityInstanceId)
        {
            this.activityInstanceId_Renamed = activityInstanceId;
            return this;
        }

        public virtual IHistoricDetailQuery taskId(string taskId)
        {
            this.taskId_Renamed = taskId;
            return this;
        }

        public virtual IHistoricDetailQuery formProperties()
        {
            this.type = "FormProperty";
            return this;
        }

        public virtual IHistoricDetailQuery variableUpdates()
        {
            this.type = "VariableUpdate";
            return this;
        }

        public virtual IHistoricDetailQuery excludeTaskDetails()
        {
            this.excludeTaskRelated = true;
            return this;
        }

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.HistoricDetailEntityManager.findHistoricDetailCountByQueryCriteria(this);
        }

        public override IList<IHistoricDetail> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            IList<IHistoricDetail> historicDetails = commandContext.HistoricDetailEntityManager.findHistoricDetailsByQueryCriteria(this, page) ?? new List<IHistoricDetail>();

            IHistoricDetailVariableInstanceUpdateEntity varUpdate = null;
            if (historicDetails != null)
            {
                foreach (IHistoricDetail historicDetail in historicDetails)
                {
                    if (historicDetail is IHistoricDetailVariableInstanceUpdateEntity)
                    {
                        varUpdate = (IHistoricDetailVariableInstanceUpdateEntity)historicDetail;

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

        public virtual IHistoricDetailQuery orderByProcessInstanceId()
        {
            orderBy(HistoricDetailQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricDetailQuery orderByTime()
        {
            orderBy(HistoricDetailQueryProperty.TIME);
            return this;
        }

        public virtual IHistoricDetailQuery orderByVariableName()
        {
            orderBy(HistoricDetailQueryProperty.VARIABLE_NAME);
            return this;
        }

        public virtual IHistoricDetailQuery orderByFormPropertyId()
        {
            orderBy(HistoricDetailQueryProperty.VARIABLE_NAME);
            return this;
        }

        public virtual IHistoricDetailQuery orderByVariableRevision()
        {
            orderBy(HistoricDetailQueryProperty.VARIABLE_REVISION);
            return this;
        }

        public virtual IHistoricDetailQuery orderByVariableType()
        {
            orderBy(HistoricDetailQueryProperty.VARIABLE_TYPE);
            return this;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string Id
        {
            get
            {
                return id_Renamed;
            }
        }

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

        public virtual string ActivityId
        {
            get
            {
                return activityId_Renamed;
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
                return executionId_Renamed;
            }
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return activityInstanceId_Renamed;
            }
        }

    }

}