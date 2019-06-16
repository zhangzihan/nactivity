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
namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Asyncexecutor;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class AcquireTimerJobsCmd : ICommand<AcquiredTimerJobEntities>
    {

        private readonly IAsyncExecutor asyncExecutor;

        public AcquireTimerJobsCmd(IAsyncExecutor asyncExecutor)
        {
            this.asyncExecutor = asyncExecutor;
        }

        public virtual AcquiredTimerJobEntities Execute(ICommandContext commandContext)
        {
            AcquiredTimerJobEntities acquiredJobs = new AcquiredTimerJobEntities();

            IList<ITimerJobEntity> timerJobs = commandContext.TimerJobEntityManager.FindTimerJobsToExecute(new Page(0, asyncExecutor.MaxAsyncJobsDuePerAcquisition));

            foreach (ITimerJobEntity job in timerJobs)
            {
                LockJob(commandContext, job, asyncExecutor.AsyncJobLockTimeInMillis);
                acquiredJobs.AddJob(job);
            }

            return acquiredJobs;
        }

        protected internal virtual void LockJob(ICommandContext commandContext, ITimerJobEntity job, int lockTimeInMillis)
        {
            // This will trigger an optimistic locking exception when two concurrent executors 
            // try to lock, as the revision will not match.

            var cl = new DateTime(commandContext.ProcessEngineConfiguration.Clock.CurrentTime.Ticks);

            cl.AddMilliseconds(lockTimeInMillis);

            job.LockOwner = asyncExecutor.LockOwner;
            job.LockExpirationTime = cl;
        }
    }
}