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
namespace org.activiti.engine.impl.cmd
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.runtime;
    using Sys;
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

        public virtual object execute(ICommandContext commandContext)
        {

            if (string.ReferenceEquals(jobId, null))
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }

            // We need to refetch the job, as it could have been deleted by another concurrent job
            // For exampel: an embedded subprocess with a couple of async tasks and a timer on the boundary of the subprocess
            // when the timer fires, all executions and thus also the jobs inside of the embedded subprocess are destroyed.
            // However, the async task jobs could already have been fetched and put in the queue.... while in reality they have been deleted. 
            // A refetch is thus needed here to be sure that it exists for this transaction.

            IJob job = commandContext.JobEntityManager.findById<IJob>(new KeyValuePair<string, object>("id", jobId));
            if (job == null)
            {
                log.LogDebug("Job does not exist anymore and will not be executed. It has most likely been deleted as part of another concurrent part of the process instance.");
                return null;
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Executing async job {job.Id}");
            }

            commandContext.JobManager.execute(job);

            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_EXECUTION_SUCCESS, job));
            }

            return null;
        }
    }
}