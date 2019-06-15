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

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class GetJobExceptionStacktraceCmd : ICommand<string>
    {

        private const long serialVersionUID = 1L;
        private string jobId;
        protected internal JobType jobType;

        public GetJobExceptionStacktraceCmd(string jobId, JobType jobType)
        {
            this.jobId = jobId;
            this.jobType = jobType;
        }

        public virtual string Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(jobId, null))
            {
                throw new ActivitiIllegalArgumentException("jobId is null");
            }

            IAbstractJobEntity job = null;
            switch (jobType)
            {
                case JobType.ASYNC:
                    job = commandContext.JobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
                    break;
                case JobType.TIMER:
                    job = commandContext.TimerJobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
                    break;
                case JobType.SUSPENDED:
                    job = commandContext.SuspendedJobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
                    break;
                case JobType.DEADLETTER:
                    job = commandContext.DeadLetterJobEntityManager.FindById<IAbstractJobEntity>(new KeyValuePair<string, object>("id", jobId));
                    break;
            }

            if (job == null)
            {
                throw new ActivitiObjectNotFoundException("No job found with id " + jobId, typeof(IJob));
            }

            return job.ExceptionStacktrace;
        }

    }

}