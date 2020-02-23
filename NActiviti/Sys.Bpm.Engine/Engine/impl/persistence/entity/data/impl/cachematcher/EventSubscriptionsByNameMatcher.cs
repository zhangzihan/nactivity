using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
    public class EventSubscriptionsByNameMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {
        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {
            if (parameter is null)
            {
                return false;
            }

            JToken @params = JToken.FromObject(parameter);
            string eventType = @params[nameof(eventType)]?.ToString();
            string eventName = @params[nameof(eventName)]?.ToString();
            string tenantId = @params[nameof(tenantId)]?.ToString();

            if (eventSubscriptionEntity.EventType is object &&
                eventSubscriptionEntity.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase) &&
                eventSubscriptionEntity.EventName is object &&
                eventSubscriptionEntity.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase))
            {
                if (tenantId != null && ProcessEngineConfiguration.NO_TENANT_ID != tenantId?.ToString())
                {
                    return eventSubscriptionEntity.TenantId != null &&
                        string.Compare(eventSubscriptionEntity.TenantId, tenantId?.ToString(), true) == 0;
                }
                else
                {
                    return ProcessEngineConfiguration.NO_TENANT_ID == eventSubscriptionEntity.TenantId || eventSubscriptionEntity.TenantId == null;
                }
            }
            return false;
        }

    }
}