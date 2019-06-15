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
    public class EventSubscriptionsByProcInstTypeAndActivityMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {

        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {

            IDictionary<string, object> @params = (IDictionary<string, object>)parameter ?? new Dictionary<string, object>();
            @params.TryGetValue("eventType", out object type);
            @params.TryGetValue("processInstanceId", out object processInstanceId);
            @params.TryGetValue("activityId", out object activityId);

            return eventSubscriptionEntity.EventType != null && 
                string.Compare(eventSubscriptionEntity.EventType, type?.ToString(), true) == 0 && 
                eventSubscriptionEntity.ProcessInstanceId != null && 
                string.Compare(eventSubscriptionEntity.ProcessInstanceId, processInstanceId?.ToString(), true) == 0 && 
                eventSubscriptionEntity.ActivityId != null && 
                string.Compare(eventSubscriptionEntity.ActivityId, activityId?.ToString(), true) == 0;
        }

    }
}