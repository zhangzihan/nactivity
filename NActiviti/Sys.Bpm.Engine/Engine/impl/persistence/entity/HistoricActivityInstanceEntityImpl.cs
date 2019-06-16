using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    /// 
    /// 
    [Serializable]
    public class HistoricActivityInstanceEntityImpl : HistoricScopeInstanceEntityImpl, IHistoricActivityInstanceEntity
    {

        private const long serialVersionUID = 1L;

        protected internal string activityId;
        protected internal string activityName;
        protected internal string activityType;
        protected internal string executionId;
        protected internal string assignee;
        protected internal string taskId;
        protected internal string calledProcessInstanceId;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;

        public HistoricActivityInstanceEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["endTime"] = endTime,
                    ["durationInMillis"] = durationInMillis,
                    ["deleteReason"] = deleteReason,
                    ["executionId"] = executionId,
                    ["assignee"] = assignee
                };
                return persistentState;
            }
        }

        // getters and setters //////////////////////////////////////////////////////

        public virtual string ActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                this.activityId = value;
            }
        }


        public virtual string ActivityName
        {
            get
            {
                return activityName;
            }
            set
            {
                this.activityName = value;
            }
        }


        public virtual string ActivityType
        {
            get
            {
                return activityType;
            }
            set
            {
                this.activityType = value;
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


        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set
            {
                this.assignee = value;
            }
        }

        public virtual string AssigneeUser
        {
            get;
            set;
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


        public virtual string CalledProcessInstanceId
        {
            get
            {
                return calledProcessInstanceId;
            }
            set
            {
                this.calledProcessInstanceId = value;
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


        public virtual DateTime? Time
        {
            get
            {
                return StartTime;
            }
        }

        // common methods //////////////////////////////////////////////////////////

        public override string ToString()
        {
            return "HistoricActivityInstanceEntity[id=" + id + ", activityId=" + activityId + ", activityName=" + activityName + "]";
        }

    }

}