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
namespace Sys.Workflow.engine.impl.asyncexecutor
{
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;

    /// 
    public class ResetExpiredJobsCmd : ICommand<object>
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ICollection<string> jobIds;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobsIds"></param>
        public ResetExpiredJobsCmd(ICollection<string> jobsIds)
        {
            this.jobIds = jobsIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public virtual object Execute(ICommandContext commandContext)
        {
            bool messageQueueMode = commandContext.ProcessEngineConfiguration.AsyncExecutorIsMessageQueueMode;
            foreach (string jobId in jobIds)
            {
                if (!messageQueueMode)
                {
                    IJob job = commandContext.JobEntityManager.FindById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));
                    commandContext.JobManager.Unacquire(job);
                }
                else
                {
                    commandContext.JobEntityManager.ResetExpiredJob(jobId);
                }
            }
            return null;
        }
    }
}