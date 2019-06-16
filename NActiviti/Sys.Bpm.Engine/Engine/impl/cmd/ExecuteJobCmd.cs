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
namespace Sys.Workflow.engine.impl.cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class ExecuteJobCmd : ICommand<object>
    {
        private static readonly ILogger<ExecuteJobCmd> log = ProcessEngineServiceProvider.LoggerService<ExecuteJobCmd>();

        private const long serialVersionUID = 1L;

        protected internal string jobId;

        public ExecuteJobCmd(string jobId)
        {
            this.jobId = jobId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("jobId and job is null");
            }

            IJob job = commandContext.JobEntityManager.FindById<IJob>(new KeyValuePair<string, object>("id", jobId));

            if (job == null)
            {
                throw new JobNotFoundException(jobId);
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Executing job {job.Id}");
            }

            commandContext.AddCloseListener(new FailedJobListener(commandContext.ProcessEngineConfiguration.CommandExecutor, job));

            try
            {
                commandContext.JobManager.Execute(job);
            }
            catch (Exception exception)
            {
                // Finally, Throw the exception to indicate the ExecuteJobCmd failed
                throw new ActivitiException("Job " + jobId + " failed", exception);
            }

            return null;
        }

        public virtual string JobId
        {
            get
            {
                return jobId;
            }
        }

    }

}