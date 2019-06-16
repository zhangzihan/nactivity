using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spring.Expressions;
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
namespace Sys.Workflow.engine.impl.persistence.entity.data.impl.cachematcher
{


    /// 
    public class SignalEventSubscriptionByNameAndExecutionMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {

        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            JToken @params = JToken.FromObject(parameter);
            string executionId = @params["executionId"]?.ToString();
            string name = @params["eventName"]?.ToString();

            return eventSubscriptionEntity.EventType != null && 
                eventSubscriptionEntity.EventType.Equals(SignalEventSubscriptionEntityFields.EVENT_TYPE) && 
                eventSubscriptionEntity.ExecutionId != null && 
                string.Compare(eventSubscriptionEntity.ExecutionId, executionId, true) == 0 && 
                eventSubscriptionEntity.EventName != null && 
                string.Compare(eventSubscriptionEntity.EventName, name, true) == 0;
        }

    }
}