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

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    [Serializable]
    public class EventSubscriptionQueryImpl : AbstractQuery<EventSubscriptionQueryImpl, IEventSubscriptionEntity>
    {

        private const long serialVersionUID = 1L;
        protected internal string eventSubscriptionId_Renamed;
        protected internal string eventName_Renamed;
        protected internal string eventType_Renamed;
        protected internal string executionId_Renamed;
        protected internal string processInstanceId_Renamed;
        protected internal string activityId_Renamed;
        protected internal string tenantId_Renamed;

        public EventSubscriptionQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public EventSubscriptionQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual EventSubscriptionQueryImpl eventSubscriptionId(string eventSubscriptionId)
        {
            if (ReferenceEquals(eventSubscriptionId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided event subscription id is null");
            }
            this.eventSubscriptionId_Renamed = eventSubscriptionId;
            return this;
        }

        public virtual EventSubscriptionQueryImpl eventName(string eventName)
        {
            if (ReferenceEquals(eventName, null))
            {
                throw new ActivitiIllegalArgumentException("Provided event name is null");
            }
            this.eventName_Renamed = eventName;
            return this;
        }

        public virtual EventSubscriptionQueryImpl executionId(string executionId)
        {
            if (ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided execution id is null");
            }
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual EventSubscriptionQueryImpl processInstanceId(string processInstanceId)
        {
            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided process instance id is null");
            }
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual EventSubscriptionQueryImpl activityId(string activityId)
        {
            if (ReferenceEquals(activityId, null))
            {
                throw new ActivitiIllegalArgumentException("Provided activity id is null");
            }
            this.activityId_Renamed = activityId;
            return this;
        }

        public virtual EventSubscriptionQueryImpl eventType(string eventType)
        {
            if (ReferenceEquals(eventType, null))
            {
                throw new ActivitiIllegalArgumentException("Provided event type is null");
            }
            this.eventType_Renamed = eventType;
            return this;
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId_Renamed;
            }
        }

        public virtual EventSubscriptionQueryImpl tenantId(string tenantId)
        {
            this.tenantId_Renamed = tenantId;
            return this;
        }

        public virtual EventSubscriptionQueryImpl orderByCreated()
        {
            return orderBy(EventSubscriptionQueryProperty.CREATED);
        }

        // results //////////////////////////////////////////

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.EventSubscriptionEntityManager.findEventSubscriptionCountByQueryCriteria(this);
        }

        public override IList<IEventSubscriptionEntity> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.EventSubscriptionEntityManager.findEventSubscriptionsByQueryCriteria(this, page);
        }

        // getters //////////////////////////////////////////

        public virtual string EventSubscriptionId
        {
            get
            {
                return eventSubscriptionId_Renamed;
            }
        }

        public virtual string EventName
        {
            get
            {
                return eventName_Renamed;
            }
        }

        public virtual string EventType
        {
            get
            {
                return eventType_Renamed;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_Renamed;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_Renamed;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId_Renamed;
            }
        }

    }

}