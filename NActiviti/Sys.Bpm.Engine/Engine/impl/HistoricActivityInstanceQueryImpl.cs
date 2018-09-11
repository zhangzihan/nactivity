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

    /// 
    [Serializable]
    public class HistoricActivityInstanceQueryImpl : AbstractQuery<IHistoricActivityInstanceQuery, IHistoricActivityInstance>, IHistoricActivityInstanceQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string activityInstanceId_Renamed;
        protected internal string processInstanceId_Renamed;
        protected internal string executionId_Renamed;
        protected internal string processDefinitionId_Renamed;
        protected internal string activityId_Renamed;
        protected internal string activityName_Renamed;
        protected internal string activityType_Renamed;
        protected internal string assignee;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal bool finished_Renamed;
        protected internal bool unfinished_Renamed;
        protected internal string deleteReason_Renamed;
        protected internal string deleteReasonLike_Renamed;

        public HistoricActivityInstanceQueryImpl()
        {
        }

        public HistoricActivityInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricActivityInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.HistoricActivityInstanceEntityManager.findHistoricActivityInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricActivityInstance> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.HistoricActivityInstanceEntityManager.findHistoricActivityInstancesByQueryCriteria(this, page);
        }

        public virtual IHistoricActivityInstanceQuery processInstanceId(string processInstanceId)
        {
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery executionId(string executionId)
        {
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery processDefinitionId(string processDefinitionId)
        {
            this.processDefinitionId_Renamed = processDefinitionId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery activityId(string activityId)
        {
            this.activityId_Renamed = activityId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery activityName(string activityName)
        {
            this.activityName_Renamed = activityName;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery activityType(string activityType)
        {
            this.activityType_Renamed = activityType;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery taskAssignee(string assignee)
        {
            this.assignee = assignee;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery finished()
        {
            this.finished_Renamed = true;
            this.unfinished_Renamed = false;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery unfinished()
        {
            this.unfinished_Renamed = true;
            this.finished_Renamed = false;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery deleteReason(string deleteReason)
        {
            this.deleteReason_Renamed = deleteReason;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery deleteReasonLike(string deleteReasonLike)
        {
            this.deleteReasonLike_Renamed = deleteReasonLike;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery activityTenantId(string tenantId)
        {
            if (string.ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("activity tenant id is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
        }

        public virtual IHistoricActivityInstanceQuery activityTenantIdLike(string tenantIdLike)
        {
            if (string.ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("activity tenant id is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
        }

        public virtual IHistoricActivityInstanceQuery activityWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
        }

        // ordering
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricActivityInstanceQuery orderByHistoricActivityInstanceDuration()
        {
            orderBy(HistoricActivityInstanceQueryProperty.DURATION);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByHistoricActivityInstanceEndTime()
        {
            orderBy(HistoricActivityInstanceQueryProperty.END);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByExecutionId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.EXECUTION_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByHistoricActivityInstanceId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.HISTORIC_ACTIVITY_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByProcessDefinitionId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByProcessInstanceId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByHistoricActivityInstanceStartTime()
        {
            orderBy(HistoricActivityInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByActivityId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByActivityName()
        {
            orderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_NAME);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByActivityType()
        {
            orderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_TYPE);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery orderByTenantId()
        {
            orderBy(HistoricActivityInstanceQueryProperty.TENANT_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery activityInstanceId(string activityInstanceId)
        {
            this.activityInstanceId_Renamed = activityInstanceId;
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

        public virtual string ExecutionId
        {
            get
            {
                return executionId_Renamed;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_Renamed;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId_Renamed;
            }
        }

        public virtual string ActivityName
        {
            get
            {
                return activityName_Renamed;
            }
        }

        public virtual string ActivityType
        {
            get
            {
                return activityType_Renamed;
            }
        }

        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
        }

        public virtual bool Finished
        {
            get
            {
                return finished_Renamed;
            }
        }

        public virtual bool Unfinished
        {
            get
            {
                return unfinished_Renamed;
            }
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return activityInstanceId_Renamed;
            }
        }

        public virtual string DeleteReason
        {
            get
            {
                return deleteReason_Renamed;
            }
        }

        public virtual string DeleteReasonLike
        {
            get
            {
                return deleteReasonLike_Renamed;
            }
        }

    }

}