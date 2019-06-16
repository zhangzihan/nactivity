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

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class EventSubscriptionQueryImpl : AbstractQuery<IEventSubscriptionQuery, IEventSubscriptionEntity>, IEventSubscriptionQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string _eventSubscriptionId;
        protected internal string _eventName;
        protected internal string _eventType;
        protected internal string _executionId;
        protected internal string _processInstanceId;
        protected internal string _activityId;
        protected internal string _tenantId;

        public EventSubscriptionQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public EventSubscriptionQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IEventSubscriptionQuery SetEventSubscriptionId(string eventSubscriptionId)
        {
            this._eventSubscriptionId = eventSubscriptionId;
            return this;
        }

        public virtual IEventSubscriptionQuery SetEventName(string eventName)
        {
            this._eventName = eventName;
            return this;
        }

        public virtual IEventSubscriptionQuery SetExecutionId(string executionId)
        {
            this._executionId = executionId;
            return this;
        }

        public virtual IEventSubscriptionQuery SetProcessInstanceId(string processInstanceId)
        {
            this._processInstanceId = processInstanceId;
            return this;
        }

        public virtual IEventSubscriptionQuery SetActivityId(string activityId)
        {
            this._activityId = activityId;
            return this;
        }

        public virtual IEventSubscriptionQuery SetEventType(string eventType)
        {
            this._eventType = eventType;
            return this;
        }

        public virtual string TenantId
        {
            get
            {
                return _tenantId;
            }
        }

        public virtual IEventSubscriptionQuery SetTenantId(string tenantId)
        {
            this._tenantId = tenantId;
            return this;
        }

        public virtual IEventSubscriptionQuery SetOrderByCreated()
        {
            return SetOrderBy(EventSubscriptionQueryProperty.CREATED);
        }

        // results //////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.EventSubscriptionEntityManager.FindEventSubscriptionCountByQueryCriteria(this);
        }

        public override IList<IEventSubscriptionEntity> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.EventSubscriptionEntityManager.FindEventSubscriptionsByQueryCriteria(this, page);
        }

        // getters //////////////////////////////////////////

        public virtual string EventSubscriptionId
        {
            get
            {
                return _eventSubscriptionId;
            }
        }

        public virtual string EventName
        {
            get
            {
                return _eventName;
            }
        }

        public virtual string EventType
        {
            get
            {
                return _eventType;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return _executionId;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return _processInstanceId;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return _activityId;
            }
        }

    }

}