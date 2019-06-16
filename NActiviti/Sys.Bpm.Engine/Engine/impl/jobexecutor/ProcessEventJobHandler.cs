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

namespace Sys.Workflow.engine.impl.jobexecutor
{
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// 
    /// 
    public class ProcessEventJobHandler : IJobHandler
    {

        public const string TYPE = "event";

        public virtual string Type
        {
            get
            {
                return TYPE;
            }
        }

        public virtual void Execute(IJobEntity job, string configuration, IExecutionEntity execution, ICommandContext commandContext)
        {

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;

            // lookup subscription:
            IEventSubscriptionEntity eventSubscriptionEntity = eventSubscriptionEntityManager.FindById<IEventSubscriptionEntity>(new KeyValuePair<string, object>("id", configuration));

            // if event subscription is null, ignore
            if (eventSubscriptionEntity != null)
            {
                eventSubscriptionEntityManager.EventReceived(eventSubscriptionEntity, null, false);
            }

        }

    }

}