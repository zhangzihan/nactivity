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
    public class DeadLetterJobQueryImpl : AbstractQuery<IDeadLetterJobQuery, IJob>, IDeadLetterJobQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string id;

        protected internal string processInstanceId_Renamed;

        protected internal string executionId_Renamed;

        protected internal string processDefinitionId_Renamed;

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

        public DeadLetterJobQueryImpl()
        {
        }

        public  DeadLetterJobQueryImpl(ICommandContext  commandContext) : base(commandContext)
        {
        }

        public DeadLetterJobQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IDeadLetterJobQuery SetJobId(string jobId)
        {
            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("Provided job id is null");
            }
            this.id = jobId;
            return this;
        }

        public virtual IDeadLetterJobQuery SetProcessInstanceId(string processInstanceId)
        {
            if (processInstanceId is null)
            {
                throw new ActivitiIllegalArgumentException("Provided process instance id is null");
            }
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IDeadLetterJobQuery SetProcessDefinitionId(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Provided process definition id is null");
            }
            this.processDefinitionId_Renamed = processDefinitionId;
            return this;
        }

        public virtual IDeadLetterJobQuery SetExecutionId(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ActivitiIllegalArgumentException("Provided execution id is null");
            }
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IDeadLetterJobQuery SetExecutable()
        {
            executable_Renamed = true;
            return this;
        }

        public virtual IDeadLetterJobQuery SetTimers()
        {
            if (onlyMessages)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyTimers = true;
            return this;
        }

        public virtual IDeadLetterJobQuery SetMessages()
        {
            if (onlyTimers)
            {
                throw new ActivitiIllegalArgumentException("Cannot combine onlyTimers() with onlyMessages() in the same query");
            }
            this.onlyMessages = true;
            return this;
        }

        public virtual IDeadLetterJobQuery SetDuedateHigherThen(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateHigherThan_Renamed = date;
            return this;
        }

        public virtual IDeadLetterJobQuery SetDuedateLowerThan(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateLowerThan_Renamed = date;
            return this;
        }

        public virtual IDeadLetterJobQuery SetDuedateHigherThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateHigherThanOrEqual = date;
            return this;
        }

        public virtual IDeadLetterJobQuery SetDuedateLowerThenOrEquals(DateTime? date)
        {
            if (!date.HasValue)
            {
                throw new ActivitiIllegalArgumentException("Provided date is null");
            }
            this.duedateLowerThanOrEqual = date;
            return this;
        }

        public virtual IDeadLetterJobQuery SetWithException()
        {
            this.withException_Renamed = true;
            return this;
        }

        public virtual IDeadLetterJobQuery SetExceptionMessage(string exceptionMessage)
        {
            if (exceptionMessage is null)
            {
                throw new ActivitiIllegalArgumentException("Provided exception message is null");
            }
            this.exceptionMessage_Renamed = exceptionMessage;
            return this;
        }

        public virtual IDeadLetterJobQuery SetJobTenantId(string tenantId)
        {
            if (tenantId is null)
            {
                throw new ActivitiIllegalArgumentException("job is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IDeadLetterJobQuery SetJobTenantIdLike(string tenantIdLike)
        {
            if (tenantIdLike is null)
            {
                throw new ActivitiIllegalArgumentException("job is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IDeadLetterJobQuery SetJobWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        // sorting //////////////////////////////////////////

        public virtual IDeadLetterJobQuery SetOrderByJobDuedate()
        {
            return SetOrderBy(JobQueryProperty.DUEDATE);
        }

        public virtual IDeadLetterJobQuery SetOrderByExecutionId()
        {
            return SetOrderBy(JobQueryProperty.EXECUTION_ID);
        }

        public virtual IDeadLetterJobQuery SetOrderByJobId()
        {
            return SetOrderBy(JobQueryProperty.JOB_ID);
        }

        public virtual IDeadLetterJobQuery SetOrderByProcessInstanceId()
        {
            return SetOrderBy(JobQueryProperty.PROCESS_INSTANCE_ID);
        }

        public virtual IDeadLetterJobQuery SetOrderByJobRetries()
        {
            return SetOrderBy(JobQueryProperty.RETRIES);
        }

        public virtual IDeadLetterJobQuery SetOrderByTenantId()
        {
            return SetOrderBy(JobQueryProperty.TENANT_ID);
        }

        // results //////////////////////////////////////////

        public  override long ExecuteCount(ICommandContext  commandContext)
        {
            CheckQueryOk();
            return commandContext.DeadLetterJobEntityManager.FindJobCountByQueryCriteria(this);
        }

        public  override IList<IJob> ExecuteList(ICommandContext  commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.DeadLetterJobEntityManager.FindJobsByQueryCriteria(this, page);
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

    }

}