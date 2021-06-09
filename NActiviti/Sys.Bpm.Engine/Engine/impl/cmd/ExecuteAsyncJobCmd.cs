using System;

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
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class ExecuteAsyncJobCmd : ICommand<object>
    {
        private static readonly ILogger<ExecuteAsyncJobCmd> log = ProcessEngineServiceProvider.LoggerService<ExecuteAsyncJobCmd>();

        private const long serialVersionUID = 1L;

        protected internal string jobId;

        public ExecuteAsyncJobCmd(string jobId)
        {
            this.jobId = jobId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {

            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }

            // We need to refetch the job, as it could have been deleted by another concurrent job
            // For exampel: an embedded subprocess with a couple of async tasks and a timer on the boundary of the subprocess
            // when the timer fires, all executions and thus also the jobs inside of the embedded subprocess are destroyed.
            // However, the async task jobs could already have been fetched and put in the queue.... while in reality they have been deleted. 
            // A refetch is thus needed here to be sure that it exists for this transaction.

            IJob job = commandContext.JobEntityManager.FindById<IJobEntity>( jobId);
            if (job is null)
            {
                log.LogDebug("Job does not exist anymore and will not be executed. It has most likely been deleted as part of another concurrent part of the process instance.");
                return null;
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Executing async job {job.Id}");
            }

            commandContext.JobManager.Execute(job);

            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_EXECUTION_SUCCESS, job));
            }

            return null;
        }
    }
}