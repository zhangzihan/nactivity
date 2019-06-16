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
namespace Sys.Workflow.Engine.Impl.JobExecutors
{

    /// 
    /// 
    public class AcquiredJobs
    {

        protected internal IList<IList<string>> acquiredJobBatches = new List<IList<string>>();
        protected internal ISet<string> acquiredJobs = new HashSet<string>();

        public virtual IList<IList<string>> JobIdBatches
        {
            get
            {
                return acquiredJobBatches;
            }
        }

        public virtual void AddJobIdBatch(IList<string> jobIds)
        {
            acquiredJobBatches.Add(jobIds);
            jobIds.ToList().ForEach(x => acquiredJobs.Add(x));
        }

        public virtual bool Contains(string jobId)
        {
            return acquiredJobs.Contains(jobId);
        }

        public virtual int Size()
        {
            return acquiredJobs.Count;
        }
    }
}