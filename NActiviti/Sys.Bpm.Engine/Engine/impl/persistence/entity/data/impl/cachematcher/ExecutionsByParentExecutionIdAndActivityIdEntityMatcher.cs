using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    public class ExecutionsByParentExecutionIdAndActivityIdEntityMatcher : CachedEntityMatcherAdapter<IExecutionEntity>
    {

        public override bool IsRetained(IExecutionEntity executionEntity, object parameter)
        {
            IDictionary<string, object> paramMap = (IDictionary<string, object>)parameter ?? new Dictionary<string, object>();
            paramMap.TryGetValue("parentExecutionId", out object parentExecutionId);
            ICollection<string> activityIds = null;

            if (paramMap.TryGetValue("activityIds", out var list))
            {
                activityIds = ((IEnumerable)list).Cast<string>().ToList();
            }
            else
            {
                activityIds = new List<string>();
            }

            return executionEntity.ParentId != null &&
                string.Compare(executionEntity.ParentId, parentExecutionId?.ToString(), true) == 0 &&
                executionEntity.ActivityId != null &&
                activityIds.Any(x => string.Compare(x, executionEntity.ActivityId, true) == 0);
        }

    }
}