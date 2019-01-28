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

    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.runtime;

    /// 
    /// 
    /// 
    [Serializable]
    public class JobQueryImpl : AbstractQuery<IJobQuery, IJob>, IJobQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string id;

        protected internal string processInstanceId_Renamed;

        protected internal string executionId_Renamed;

        protected internal string processDefinitionId_Renamed;
        protected internal bool retriesLeft;

        protected internal bool executable_Renamed;
        protected internal bool onlyTimers;
        protected internal bool onlyMessages;

        protected internal DateTime? duedateHigherThan_Renamed;
        protected internal DateTime? duedateLowerThan_Renamed;
        protected internal DateTime? duedateHigherThanOrEqual;
        protected internal DateTime? duedateLowerThanOrEqual;

        protected internal bool withException_Renamed;

        protected internal string exceptionMessage_Renamed;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;

        protected internal bool noRetriesLeft_Renamed;
        protected internal bool onlyLocked;
        protected internal bool onlyUnlocked;

        public JobQueryImpl()
        {
        }

        public JobQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public JobQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IJobQuery jobId(string jobId)
        {
            if (ReferenceEquals(jobId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided job id is null");
            }
            this.id = jobId;
            return this;
        }

        public virtual IJobQuery processInstanceId(string processInstanceId)
        {
            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided process instance id is null");
            }
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IJobQuery processDefinitionId(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Provided process definition id is null");
            }
            this.processDefinitionId_Renamed = processDefinitionId;
            return this;
        }

        public virtual IJobQuery executionId(string executionId)
        {
            if (ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided execution id is null");
            }
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IJobQuery withRetriesLeft()
        {
            retriesLeft = true;
            return this;
        }

        public virtual IJobQuery executable()
        {
            executable_Renamed = true;
            return this;
        }

        public virtual IJobQuery timers()
        {
            if (onlyMessages)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyTimers = true;
            return this;
        }

        public virtual IJobQuery messages()
        {
            if (onlyTimers)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyMessages = true;
            return this;
        }

        public virtual IJobQuery duedateHigherThan(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateHigherThan_Renamed = date;
            return this;
        }

        public virtual IJobQuery duedateLowerThan(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateLowerThan_Renamed = date;
            return this;
        }

        public virtual IJobQuery duedateHigherThen(DateTime? date)
        {
            return duedateHigherThan(date);
        }

        public virtual IJobQuery duedateHigherThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateHigherThanOrEqual = date;
            return this;
        }

        public virtual IJobQuery duedateLowerThen(DateTime? date)
        {
            return duedateLowerThan(date);
        }

        public virtual IJobQuery duedateLowerThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateLowerThanOrEqual = date;
            return this;
        }

        public virtual IJobQuery noRetriesLeft()
        {
            noRetriesLeft_Renamed = true;
            return this;
        }

        public virtual IJobQuery withException()
        {
            this.withException_Renamed = true;
            return this;
        }

        public virtual IJobQuery exceptionMessage(string exceptionMessage)
        {
            if (ReferenceEquals(exceptionMessage, null))
            {
                throw new ActivitiIllegalArgumentException("Provided exception message is null");
            }
            this.exceptionMessage_Renamed = exceptionMessage;
            return this;
        }

        public virtual IJobQuery jobTenantId(string tenantId)
        {
            if (ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("job is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IJobQuery jobTenantIdLike(string tenantIdLike)
        {
            if (ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("job is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IJobQuery jobWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IJobQuery locked()
        {
            this.onlyLocked = true;
            return this;
        }

        public virtual IJobQuery unlocked()
        {
            this.onlyUnlocked = true;
            return this;
        }

        // sorting //////////////////////////////////////////

        public virtual IJobQuery orderByJobDuedate()
        {
            return orderBy(JobQueryProperty.DUEDATE);
        }

        public virtual IJobQuery orderByExecutionId()
        {
            return orderBy(JobQueryProperty.EXECUTION_ID);
        }

        public virtual IJobQuery orderByJobId()
        {
            return orderBy(JobQueryProperty.JOB_ID);
        }

        public virtual IJobQuery orderByProcessInstanceId()
        {
            return orderBy(JobQueryProperty.PROCESS_INSTANCE_ID);
        }

        public virtual IJobQuery orderByJobRetries()
        {
            return orderBy(JobQueryProperty.RETRIES);
        }

        public virtual IJobQuery orderByTenantId()
        {
            return orderBy(JobQueryProperty.TENANT_ID);
        }

        // results //////////////////////////////////////////

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.JobEntityManager.findJobCountByQueryCriteria(this);
        }

        public override IList<IJob> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.JobEntityManager.findJobsByQueryCriteria(this, page);
        }

        // getters //////////////////////////////////////////

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

        public virtual bool RetriesLeft
        {
            get
            {
                return retriesLeft;
            }
        }

        public virtual bool Executable
        {
            get
            {
                return executable_Renamed;
            }
        }

        public virtual DateTime Now
        {
            get
            {
                return Context.ProcessEngineConfiguration.Clock.CurrentTime;
            }
        }

        public virtual bool WithException
        {
            get
            {
                return withException_Renamed;
            }
        }

        public virtual string ExceptionMessage
        {
            get
            {
                return exceptionMessage_Renamed;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
        }

        public static long Serialversionuid
        {
            get
            {
                return serialVersionUID;
            }
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_Renamed;
            }
        }

        public virtual bool OnlyTimers
        {
            get
            {
                return onlyTimers;
            }
        }

        public virtual bool OnlyMessages
        {
            get
            {
                return onlyMessages;
            }
        }

        public virtual DateTime? DuedateHigherThan
        {
            get
            {
                return duedateHigherThan_Renamed;
            }
        }

        public virtual DateTime? DuedateLowerThan
        {
            get
            {
                return duedateLowerThan_Renamed;
            }
        }

        public virtual DateTime? DuedateHigherThanOrEqual
        {
            get
            {
                return duedateHigherThanOrEqual;
            }
        }

        public virtual DateTime? DuedateLowerThanOrEqual
        {
            get
            {
                return duedateLowerThanOrEqual;
            }
        }

        public virtual bool NoRetriesLeft
        {
            get
            {
                return noRetriesLeft_Renamed;
            }
        }

        public virtual bool OnlyLocked
        {
            get
            {
                return onlyLocked;
            }
        }

        public virtual bool OnlyUnlocked
        {
            get
            {
                return onlyUnlocked;
            }
        }

    }

}