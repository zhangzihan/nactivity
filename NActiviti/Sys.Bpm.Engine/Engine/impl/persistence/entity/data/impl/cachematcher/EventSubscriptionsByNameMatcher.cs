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
namespace org.activiti.engine.impl.persistence.entity.data.impl.cachematcher
{


    /// 
    public class EventSubscriptionsByNameMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {
        public override bool isRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {

            IDictionary<string, object> @params = (IDictionary<string, object>)parameter ?? new Dictionary<string, object>();
            @params.TryGetValue("eventType", out object type);
            @params.TryGetValue("eventName", out object eventName);
            @params.TryGetValue("tenantId", out object tenantId);

            if (eventSubscriptionEntity.EventType != null &&
                string.Compare(eventSubscriptionEntity.EventType, type?.ToString(), true) == 0 &&
                eventSubscriptionEntity.EventName != null &&
                string.Compare(eventSubscriptionEntity.EventName, eventName?.ToString(), true) == 0)
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