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

namespace Sys.Workflow.engine.impl
{

    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.runtime;

    /// 
    /// 
    [Serializable]
    public class TimerJobQueryImpl : AbstractQuery<ITimerJobQuery, IJob>, ITimerJobQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string id;

        protected internal string _processInstanceId;
        protected internal string _executionId;
        protected internal string _processDefinitionId;
        protected internal bool retriesLeft;
        protected internal bool _executable;
        protected internal bool onlyTimers;
        protected internal bool onlyMessages;

        protected internal DateTime? _duedateHigherThan;
        protected internal DateTime? _duedateLowerThan;
        protected internal DateTime? duedateHigherThanOrEqual;
        protected internal DateTime? duedateLowerThanOrEqual;

        protected internal bool _withException;
        protected internal string _exceptionMessage;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal bool _noRetriesLeft;

        public TimerJobQueryImpl()
        {
        }

        public TimerJobQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public TimerJobQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual ITimerJobQuery SetJobId(string jobId)
        {
            this.id = jobId;
            return this;
        }

        public virtual ITimerJobQuery SetProcessInstanceId(string processInstanceId)
        {
            this._processInstanceId = processInstanceId;
            return this;
        }

        public virtual ITimerJobQuery SetProcessDefinitionId(string processDefinitionId)
        {
            this._processDefinitionId = processDefinitionId;
            return this;
        }

        public virtual ITimerJobQuery SetExecutionId(string executionId)
        {
            this._executionId = executionId;
            return this;
        }

        public virtual ITimerJobQuery SetWithRetriesLeft()
        {
            retriesLeft = true;
            return this;
        }

        public virtual ITimerJobQuery SetExecutable()
        {
            _executable = true;
            return this;
        }

        public virtual ITimerJobQuery SetTimers()
        {
            if (onlyMessages)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyTimers = true;
            return this;
        }

        public virtual ITimerJobQuery SetMessages()
        {
            if (onlyTimers)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyMessages = true;
            return this;
        }

        public virtual ITimerJobQuery SetDuedateHigherThan(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this._duedateHigherThan = date;
            return this;
        }

        public virtual ITimerJobQuery SetDuedateLowerThan(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this._duedateLowerThan = date;
            return this;
        }

        public virtual ITimerJobQuery SetDuedateHigherThen(DateTime? date)
        {
            return SetDuedateHigherThan(date);
        }

        public virtual ITimerJobQuery SetDuedateHigherThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateHigherThanOrEqual = date;
            return this;
        }

        public virtual ITimerJobQuery SetDuedateLowerThen(DateTime? date)
        {
            return SetDuedateLowerThan(date);
        }

        public virtual ITimerJobQuery SetDuedateLowerThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateLowerThanOrEqual = date;
            return this;
        }

        public virtual ITimerJobQuery SetNoRetriesLeft()
        {
            _noRetriesLeft = true;
            return this;
        }

        public virtual ITimerJobQuery SetWithException()
        {
            this._withException = true;
            return this;
        }

        public virtual ITimerJobQuery SetExceptionMessage(string exceptionMessage)
        {
            this._exceptionMessage = exceptionMessage;
            return this;
        }

        public virtual ITimerJobQuery SetJobTenantId(string tenantId)
        {
            this.tenantId = tenantId;
            return this;
        }

        public virtual ITimerJobQuery SetJobTenantIdLike(string tenantIdLike)
        {
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual ITimerJobQuery SetJobWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        // sorting //////////////////////////////////////////

        public virtual ITimerJobQuery OrderByJobDuedate()
        {
            return SetOrderBy(JobQueryProperty.DUEDATE);
        }

        public virtual ITimerJobQuery OrderByExecutionId()
        {
            return SetOrderBy(JobQueryProperty.EXECUTION_ID);
        }

        public virtual ITimerJobQuery OrderByJobId()
        {
            return SetOrderBy(JobQueryProperty.JOB_ID);
        }

        public virtual ITimerJobQuery OrderByProcessInstanceId()
        {
            return SetOrderBy(JobQueryProperty.PROCESS_INSTANCE_ID);
        }

        public virtual ITimerJobQuery OrderByJobRetries()
        {
            return SetOrderBy(JobQueryProperty.RETRIES);
        }

        public virtual ITimerJobQuery OrderByTenantId()
        {
            return SetOrderBy(JobQueryProperty.TENANT_ID);
        }

        // results //////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.TimerJobEntityManager.FindJobCountByQueryCriteria(this);
        }

        public override IList<IJob> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.TimerJobEntityManager.FindJobsByQueryCriteria(this, page);
        }

        // getters //////////////////////////////////////////

        public virtual string ProcessInstanceId
        {
            get
            {
                return _processInstanceId;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return _executionId;
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
                return _executable;
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
                return _withException;
            }
        }

        public virtual string ExceptionMessage
        {
            get
            {
                return _exceptionMessage;
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
                return _processDefinitionId;
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
                return _duedateHigherThan;
            }
        }

        public virtual DateTime? DuedateLowerThan
        {
            get
            {
                return _duedateLowerThan;
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
                return _noRetriesLeft;
            }
        }

    }

}