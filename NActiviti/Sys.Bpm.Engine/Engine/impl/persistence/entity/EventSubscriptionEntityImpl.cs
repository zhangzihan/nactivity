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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Contexts;

    /// 
    /// 
    [Serializable]
    public class EventSubscriptionEntityImpl : AbstractEntity, IEventSubscriptionEntity
    {

        private const long serialVersionUID = 1L;

        // persistent state ///////////////////////////
        protected internal string eventType;
        protected internal string eventName;
        protected internal string executionId;
        protected internal string processInstanceId;
        protected internal string activityId;
        protected internal string configuration;
        protected internal DateTime created;
        protected internal string processDefinitionId;
        protected internal string tenantId;

        // runtime state /////////////////////////////
        protected internal IExecutionEntity execution;

        public EventSubscriptionEntityImpl()
        {
            this.created = Context.ProcessEngineConfiguration.Clock.CurrentTime;
        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["executionId"] = executionId,
                    ["configuration"] = configuration
                };

                return persistentState;
            }
        }

        // getters & setters ////////////////////////////

        public virtual string EventType
        {
            get
            {
                return eventType;
            }
            set
            {
                this.eventType = value;
            }
        }


        public virtual string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                this.eventName = value;
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


        public virtual IExecutionEntity Execution
        {
            get
            {
                var ctx = Context.CommandContext;
                if (execution == null && executionId != null && ctx != null)
                {
                    execution = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(executionId);
                }
                return execution;
            }
            set
            {
                this.execution = value;
                if (value != null)
                {
                    this.executionId = value.Id;
                    this.processInstanceId = value.ProcessInstanceId;
                }
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


        public virtual string Configuration
        {
            get
            {
                return configuration;
            }
            set
            {
                this.configuration = value;
            }
        }


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


        public virtual DateTime Created
        {
            get
            {
                return created;
            }
            set
            {
                this.created = value;
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


        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = (prime * result) + ((id is null) ? 0 : id.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            EventSubscriptionEntityImpl other = (EventSubscriptionEntityImpl)obj;
            if (id is null)
            {
                if (!(other.id is null))
                {
                    return false;
                }
            }
            else if (!id.Equals(other.id))
            {
                return false;
            }
            return true;
        }

    }

}