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
        private string eventType;
        private string eventName;
        private string executionId;
        private string processInstanceId;
        private string activityId;
        private string configuration;
        private DateTime created;
        private string processDefinitionId;
        private string tenantId;

        // runtime state /////////////////////////////
        private IExecutionEntity execution;

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
                if (execution is null && executionId is object && ctx is object)
                {
                    execution = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(executionId);
                }
                return execution;
            }
            set
            {
                this.execution = value;
                if (value is object)
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


        public static bool operator ==(EventSubscriptionEntityImpl a, EventSubscriptionEntityImpl b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null && b is object)
            {
                return false;
            }

            if (a is object && b is null)
            {
                return false;
            }

            return a.Id == b.Id;
        }

        public static bool operator !=(EventSubscriptionEntityImpl a, EventSubscriptionEntityImpl b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = (prime * result) + ((Id is null) ? 0 : Id.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is EventSubscriptionEntityImpl ese)
            {
                return this == ese;
            }

            return false;
        }
    }
}