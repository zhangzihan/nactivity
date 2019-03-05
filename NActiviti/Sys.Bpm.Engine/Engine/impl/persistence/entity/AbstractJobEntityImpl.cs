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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.db;


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

        protected internal bool isExclusive = engine.runtime.Job_Fields.DEFAULT_EXCLUSIVE;

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
                PersistentState persistentState = new PersistentState();

                persistentState["retries"] = retries;
                persistentState["duedate"] = duedate;
                persistentState["exceptionMessage"] = exceptionMessage;

                if (exceptionByteArrayRef != null)
                {
                    persistentState["exceptionByteArrayId"] = exceptionByteArrayRef.Id;
                }

                return persistentState;
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
                if (exceptionByteArrayRef == null)
                {
                    return null;
                }

                byte[] bytes = exceptionByteArrayRef.Bytes;
                if (bytes == null)
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
                if (exceptionByteArrayRef == null)
                {
                    exceptionByteArrayRef = new ByteArrayRef();
                }
                exceptionByteArrayRef.setValue("stacktrace", getUtf8Bytes(value));
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

        protected internal virtual byte[] getUtf8Bytes(string str)
        {
            if (ReferenceEquals(str, null))
            {
                return null;
            }
            try
            {
                return str.GetBytes(Encoding.UTF8);
            }
            catch (Exception)
            {
                throw new ActivitiException("UTF-8 is not a supported encoding");
            }
        }

        public override string ToString()
        {
            return this.GetType().FullName + " [id=" + id + "]";
        }

    }

}