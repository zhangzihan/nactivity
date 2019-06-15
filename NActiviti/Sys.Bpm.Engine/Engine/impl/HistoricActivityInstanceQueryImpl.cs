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
        protected internal string activityInstanceId_;
        protected internal string processInstanceId_;
        protected internal string executionId_;
        protected internal string processDefinitionId_;
        protected internal string activityId_;
        protected internal string activityName_;
        protected internal string activityType_;
        protected internal string assignee;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal bool finished_;
        protected internal bool unfinished_;
        protected internal string deleteReason_;
        protected internal string deleteReasonLike_;

        public HistoricActivityInstanceQueryImpl()
        {
        }

        public HistoricActivityInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricActivityInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.HistoricActivityInstanceEntityManager.FindHistoricActivityInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricActivityInstance> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.HistoricActivityInstanceEntityManager.FindHistoricActivityInstancesByQueryCriteria(this, page);
        }

        public virtual IHistoricActivityInstanceQuery SetProcessInstanceId(string processInstanceId)
        {
            this.processInstanceId_ = processInstanceId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetExecutionId(string executionId)
        {
            this.executionId_ = executionId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetProcessDefinitionId(string processDefinitionId)
        {
            this.processDefinitionId_ = processDefinitionId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetActivityId(string activityId)
        {
            this.activityId_ = activityId;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetActivityName(string activityName)
        {
            this.activityName_ = activityName;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetActivityType(string activityType)
        {
            this.activityType_ = activityType;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetTaskAssignee(string assignee)
        {
            this.assignee = assignee;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetFinished()
        {
            this.finished_ = true;
            this.unfinished_ = false;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetUnfinished()
        {
            this.unfinished_ = true;
            this.finished_ = false;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetDeleteReason(string deleteReason)
        {
            this.deleteReason_ = deleteReason;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetDeleteReasonLike(string deleteReasonLike)
        {
            this.deleteReasonLike_ = deleteReasonLike;
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetActivityTenantId(string tenantId)
        {
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

        public virtual IHistoricActivityInstanceQuery SetActivityTenantIdLike(string tenantIdLike)
        {
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

        public virtual IHistoricActivityInstanceQuery SetActivityWithoutTenantId()
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

        public virtual IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceDuration()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.DURATION);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceEndTime()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.END);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByExecutionId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.EXECUTION_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.HISTORIC_ACTIVITY_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByProcessDefinitionId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByProcessInstanceId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceStartTime()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByActivityId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByActivityName()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_NAME);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByActivityType()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.ACTIVITY_TYPE);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery OrderByTenantId()
        {
            SetOrderBy(HistoricActivityInstanceQueryProperty.TENANT_ID);
            return this;
        }

        public virtual IHistoricActivityInstanceQuery SetActivityInstanceId(string activityInstanceId)
        {
            this.activityInstanceId_ = activityInstanceId;
            return this;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId_;
            }
        }

        public virtual string ActivityName
        {
            get
            {
                return activityName_;
            }
        }

        public virtual string ActivityType
        {
            get
            {
                return activityType_;
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
                return finished_;
            }
        }

        public virtual bool Unfinished
        {
            get
            {
                return unfinished_;
            }
        }

        public virtual string ActivityInstanceId
        {
            get
            {
                return activityInstanceId_;
            }
        }

        public virtual string DeleteReason
        {
            get
            {
                return deleteReason_;
            }
        }

        public virtual string DeleteReasonLike
        {
            get
            {
                return deleteReasonLike_;
            }
        }

    }

}