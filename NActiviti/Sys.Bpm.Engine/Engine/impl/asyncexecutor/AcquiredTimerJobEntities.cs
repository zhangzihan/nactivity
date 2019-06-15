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
namespace org.activiti.engine.impl.asyncexecutor
{

    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Concurrent;

    /// 
    public class AcquiredTimerJobEntities
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<string, ITimerJobEntity> acquiredJobs = new ConcurrentDictionary<string, ITimerJobEntity>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        public virtual void AddJob(ITimerJobEntity job)
        {
            acquiredJobs.AddOrUpdate(job.Id, job, (key, old) => job);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<ITimerJobEntity> Jobs
        {
            get
            {
                return acquiredJobs.Values;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public virtual bool Contains(string jobId)
        {
            return acquiredJobs.ContainsKey(jobId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual int Size()
        {
            return acquiredJobs.Count;
        }
    }
}