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
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher
{

    /// 
    public class TimerJobsByExecutionIdMatcher : CachedEntityMatcherAdapter<ITimerJobEntity>
    {

        public override bool IsRetained(ITimerJobEntity jobEntity, object parameter)
        {
            if (jobEntity == null || jobEntity.ExecutionId == null || parameter == null)
            {
                return false;
            }

            if (parameter is string)
            {
                return jobEntity.ExecutionId == parameter.ToString();
            }

            if (parameter is KeyValuePair<string, object> p)
            {
                return jobEntity.ExecutionId == p.Value?.ToString();
            }

            return jobEntity.ExecutionId == parameter.ToString();
            //return !string.ReferenceEquals(jobEntity.ExecutionId, null) && jobEntity.ExecutionId.Equals(param); // param = executionId
        }


    }
}