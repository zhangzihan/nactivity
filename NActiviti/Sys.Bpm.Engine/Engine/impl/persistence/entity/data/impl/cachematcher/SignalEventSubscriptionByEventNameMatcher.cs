using Newtonsoft.Json.Linq;
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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher
{
    /// 
    public class SignalEventSubscriptionByEventNameMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {

        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            JObject token = JObject.FromObject(parameter);

            token.TryGetValue("eventName", StringComparison.OrdinalIgnoreCase, out var eventName);
            token.TryGetValue("tenantId", StringComparison.OrdinalIgnoreCase, out var tenantId);

            return eventSubscriptionEntity.EventType != null && 
                string.Compare(eventSubscriptionEntity.EventType, SignalEventSubscriptionEntityFields.EVENT_TYPE, true) == 0 && 
                eventSubscriptionEntity.EventName != null && 
                string.Compare(eventSubscriptionEntity.EventName, eventName?.ToString(), true) == 0 &&
                (eventSubscriptionEntity.ExecutionId == null || 
                    (eventSubscriptionEntity.ExecutionId != null && 
                        eventSubscriptionEntity.Execution != null && 
                            eventSubscriptionEntity.Execution.SuspensionState == SuspensionStateProvider.ACTIVE.StateCode)) && 
                ((tenantId != null && string.Compare(tenantId?.ToString(), eventSubscriptionEntity.TenantId, true) == 0) || 
                    (tenantId != null && string.IsNullOrWhiteSpace(eventSubscriptionEntity.TenantId)));
        }

    }
}