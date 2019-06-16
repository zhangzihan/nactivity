using System;

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

    /// <summary>
    /// An event log entry can only be inserted (and maybe deleted).
    /// 
    /// 
    /// </summary>
    public class EventLogEntryEntityImpl : AbstractEntityNoRevision, IEventLogEntryEntity
    {

        protected internal long logNumber; // cant use id here, it would clash with entity
        protected internal string type;
        protected internal string processDefinitionId;
        protected internal string processInstanceId;
        protected internal string executionId;
        protected internal string taskId;
        protected internal DateTime? timeStamp;
        protected internal string userId;
        protected internal byte[] data;
        protected internal string lockOwner;
        protected internal string lockTime;
        protected internal int isProcessed;

        public EventLogEntryEntityImpl()
        {
        }

        public override PersistentState PersistentState
        {
            get
            {
                return null; // Not updateable
            }
        }

        public virtual long LogNumber
        {
            get
            {
                return logNumber;
            }
            set
            {
                this.logNumber = value;
            }
        }


        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
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


        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set
            {
                this.taskId = value;
            }
        }


        public virtual DateTime? TimeStamp
        {
            get
            {
                return timeStamp;
            }
            set
            {
                this.timeStamp = value;
            }
        }


        public virtual string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                this.userId = value;
            }
        }


        public virtual byte[] Data
        {
            get
            {
                return data;
            }
            set
            {
                this.data = value;
            }
        }


        public virtual string LockOwner
        {
            get
            {
                return lockOwner;
            }
            set
            {
                this.lockOwner = value;
            }
        }


        public virtual string LockTime
        {
            get
            {
                return lockTime;
            }
            set
            {
                this.lockTime = value;
            }
        }


        public virtual int Processed
        {
            get
            {
                return isProcessed;
            }
            set
            {
                this.isProcessed = value;
            }
        }


        public override string ToString()
        {
            return timeStamp.ToString() + " : " + type;
        }

    }

}