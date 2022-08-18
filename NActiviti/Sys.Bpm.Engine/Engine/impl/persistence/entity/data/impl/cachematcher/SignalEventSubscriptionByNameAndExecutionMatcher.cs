﻿using Newtonsoft.Json;
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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher
{


    /// 
    public class SignalEventSubscriptionByNameAndExecutionMatcher : CachedEntityMatcherAdapter<IEventSubscriptionEntity>
    {

        public override bool IsRetained(IEventSubscriptionEntity eventSubscriptionEntity, object parameter)
        {
            if (parameter is null)
            {
                return false;
            }

            JToken @params = JToken.FromObject(parameter);
            string eventName = @params[nameof(eventName)]?.ToString();
            string executionId = @params[nameof(executionId)]?.ToString();

            return eventSubscriptionEntity.EventType is not null &&
                eventSubscriptionEntity.EventType.Equals(SignalEventSubscriptionEntityFields.EVENT_TYPE) &&
                eventSubscriptionEntity.ExecutionId is not null &&
                string.Compare(eventSubscriptionEntity.ExecutionId, executionId, true) == 0 &&
                eventSubscriptionEntity.EventName is not null &&
                string.Compare(eventSubscriptionEntity.EventName, eventName, true) == 0;
        }
    }
}