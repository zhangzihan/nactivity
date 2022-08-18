using System;
using System.Collections.Generic;
using System.Text;

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

    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Workflow.Engine.Runtime;


    /// <summary>
    /// Abstract job entity class.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class AbstractJobEntityImpl : AbstractEntity, IAbstractJobEntity, IBulkDeleteable
    {
        private const long serialVersionUID = 1L;

        protected internal DateTime? duedate;
        protected internal string executionId;
        protected internal string processInstanceId;
        protected internal string processDefinitionId;
        protected internal bool isExclusive = JobFields.DEFAULT_EXCLUSIVE;
        protected internal int retries;
        protected internal int maxIterations;
        protected internal string repeat;
        protected internal DateTime? endDate;
        protected internal string jobHandlerType;
        protected internal string jobHandlerConfiguration;
        protected internal ByteArrayRef exceptionByteArrayRef;
        protected internal string exceptionMessage;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal string jobType;

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState ps = base.PersistentState;
                ps[nameof(Duedate)] = duedate;
                ps[nameof(ExecutionId)] = executionId;
                ps[nameof(ProcessDefinitionId)] = processDefinitionId;
                ps[nameof(ProcessInstanceId)] = processDefinitionId;
                ps[nameof(Exclusive)] = isExclusive;
                ps[nameof(processDefinitionId)] = processDefinitionId;
                ps[nameof(processDefinitionId)] = processDefinitionId;
                ps[nameof(Retries)] = retries;
                ps[nameof(Repeat)] = repeat;
                ps[nameof(EndDate)] = endDate;
                ps[nameof(MaxIterations)] = maxIterations;
                ps[nameof(JobHandlerType)] = jobHandlerType;
                ps[nameof(JobHandlerConfiguration)] = jobHandlerConfiguration;
                ps[nameof(JobType)] = jobType;
                ps[nameof(TenantId)] = tenantId; ;
                ps[nameof(ExceptionStacktrace)] = ExceptionStacktrace;
                ps[nameof(ExceptionMessage)] = exceptionMessage;

                if (exceptionByteArrayRef is not null)
                {
                    ps["exceptionByteArrayId"] = exceptionByteArrayRef.Id;
                }

                return ps;
            }
        }

        // getters and setters ////////////////////////////////////////////////////////

        public virtual IExecutionEntity Execution
        {
            set
            {
                executionId = value.Id;
                processInstanceId = value.ProcessInstanceId;
                processDefinitionId = value.ProcessDefinitionId;
            }
        }

        public virtual DateTime? Duedate
        {
            get
            {
                return duedate;
            }
            set
            {
                this.duedate = value;
            }
        }


        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }


        public virtual int Retries
        {
            get
            {
                return retries;
            }
            set
            {
                this.retries = value;
            }
        }


        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }


        public virtual bool Exclusive
        {
            get
            {
                return isExclusive;
            }
            set
            {
                this.isExclusive = value;
            }
        }


        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }


        public virtual string Repeat
        {
            get
            {
                return repeat;
            }
            set
            {
                this.repeat = value;
            }
        }


        public virtual DateTime? EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                this.endDate = value;
            }
        }


        public virtual int MaxIterations
        {
            get
            {
                return maxIterations;
            }
            set
            {
                this.maxIterations = value;
            }
        }


        public virtual string JobHandlerType
        {
            get
            {
                return jobHandlerType;
            }
            set
            {
                this.jobHandlerType = value;
            }
        }


        public virtual string JobHandlerConfiguration
        {
            get
            {
                return jobHandlerConfiguration;
            }
            set
            {
                this.jobHandlerConfiguration = value;
            }
        }


        public virtual string JobType
        {
            get
            {
                return jobType;
            }
            set
            {
                this.jobType = value;
            }
        }


        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                this.tenantId = value;
            }
        }


        public virtual string ExceptionStacktrace
        {
            get
            {
                if (exceptionByteArrayRef is null)
                {
                    return null;
                }

                byte[] bytes = exceptionByteArrayRef.Bytes;
                if (bytes is null)
                {
                    return null;
                }

                try
                {
                    return StringHelper.NewString(bytes, "UTF-8");
                }
                catch (Exception)
                {
                    throw new ActivitiException("UTF-8 is not a supported encoding");
                }
            }
            set
            {
                if (exceptionByteArrayRef is null)
                {
                    exceptionByteArrayRef = new ByteArrayRef();
                }
                exceptionByteArrayRef.SetValue("stacktrace", GetUtf8Bytes(value));
            }
        }


        public virtual string ExceptionMessage
        {
            get
            {
                return exceptionMessage;
            }
            set
            {
                this.exceptionMessage = value;
            }
        }


        public virtual IByteArrayRef ExceptionByteArrayRef
        {
            get
            {
                return exceptionByteArrayRef;
            }
        }

        protected internal virtual byte[] GetUtf8Bytes(string str)
        {
            if (str is null)
            {
                return null;
            }
            try
            {
                return str.GetBytes(new UTF8Encoding(false));
            }
            catch (Exception)
            {
                throw new ActivitiException("UTF-8 is not a supported encoding");
            }
        }

        public override string ToString()
        {
            return this.GetType().FullName + " [id=" + Id + "]";
        }

    }

}