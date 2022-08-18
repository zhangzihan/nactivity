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
    public class MessageEventSubscriptionsByProcInstAndEventNameMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {

        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {
            if (parameter is null)
            {
                return false;
            }

            JToken @params = JToken.FromObject(parameter);
            string eventName = @params[nameof(eventName)]?.ToString();
            string processInstanceId = @params[nameof(processInstanceId)]?.ToString();

            return eventSubscriptionEntity.EventType is not null &&
                string.Compare(eventSubscriptionEntity.EventType, MessageEventSubscriptionEntityFields.EVENT_TYPE, true) == 0 &&
                eventSubscriptionEntity.EventName is not null &&
                string.Compare(eventSubscriptionEntity.EventName, eventName?.ToString(), true) == 0 &&
                eventSubscriptionEntity.ProcessInstanceId is not null &&
                string.Compare(eventSubscriptionEntity.ProcessInstanceId, processInstanceId?.ToString(), true) == 0;
        }
    }
}