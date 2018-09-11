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
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class AcquireJobsCmd : ICommand<AcquiredJobEntities>
    {

        private readonly IAsyncExecutor asyncExecutor;

        public AcquireJobsCmd(IAsyncExecutor asyncExecutor)
        {
            this.asyncExecutor = asyncExecutor;
        }

        public virtual AcquiredJobEntities execute(ICommandContext commandContext)
        {
            AcquiredJobEntities acquiredJobs = new AcquiredJobEntities();
            IList<IJobEntity> jobs = commandContext.JobEntityManager.findJobsToExecute(new Page(0, asyncExecutor.MaxAsyncJobsDuePerAcquisition));

            foreach (IJobEntity job in jobs)
            {
                lockJob(commandContext, job, asyncExecutor.AsyncJobLockTimeInMillis);
                acquiredJobs.addJob(job);
            }

            return acquiredJobs;
        }

        protected internal virtual void lockJob(ICommandContext commandContext, IJobEntity job, int lockTimeInMillis)
        {
            var cl = new DateTime(commandContext.ProcessEngineConfiguration.Clock.CurrentTime.Ticks);

            cl.AddMilliseconds(lockTimeInMillis);

            job.LockOwner = asyncExecutor.LockOwner;
            job.LockExpirationTime = cl;
        }
    }
}