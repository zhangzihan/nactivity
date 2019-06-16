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
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class MoveJobToDeadLetterJobCmd : ICommand<IDeadLetterJobEntity>
    {
        private static readonly ILogger<MoveJobToDeadLetterJobCmd> log = ProcessEngineServiceProvider.LoggerService<MoveJobToDeadLetterJobCmd>();

        private const long serialVersionUID = 1L;

        protected internal string jobId;

        public MoveJobToDeadLetterJobCmd(string jobId)
        {
            this.jobId = jobId;
        }

        public virtual IDeadLetterJobEntity Execute(ICommandContext commandContext)
        {

            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("jobId and job is null");
            }

            IAbstractJobEntity job = commandContext.TimerJobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
            if (job == null)
            {
                job = commandContext.JobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
            }

            if (job == null)
            {
                throw new JobNotFoundException(jobId);
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Moving job to deadletter job table {job.Id}");
            }

            IDeadLetterJobEntity deadLetterJob = commandContext.JobManager.MoveJobToDeadLetterJob(job);

            return deadLetterJob;
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