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
    public class UnfinishedHistoricActivityInstanceMatcher : CachedEntityMatcherAdapter<IHistoricActivityInstanceEntity>
    {

        public override bool IsRetained(IHistoricActivityInstanceEntity entity, object parameter)
        {
            IDictionary<string, object> paramMap = (IDictionary<string, object>)parameter ?? new Dictionary<string, object>();
            paramMap.TryGetValue("executionId", out object executionId);
            paramMap.TryGetValue("activityId", out object activityId);

            return entity.ExecutionId != null &&
                string.Compare(entity.ExecutionId, executionId?.ToString(), true) == 0 &&
                entity.ActivityId != null &&
                string.Compare(entity.ActivityId, activityId?.ToString(), true) == 0 && entity.EndTime == null;
        }

    }
}