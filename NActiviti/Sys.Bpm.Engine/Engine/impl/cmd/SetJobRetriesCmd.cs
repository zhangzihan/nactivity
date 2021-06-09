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

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class SetJobRetriesCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        private readonly string jobId;
        private readonly int retries;

        public SetJobRetriesCmd(string jobId, int retries)
        {
            if (jobId is null || jobId.Length < 1)
            {
                throw new ActivitiIllegalArgumentException("The job id is mandatory, but '" + jobId + "' has been provided.");
            }
            if (retries < 0)
            {
                throw new ActivitiIllegalArgumentException("The number of job retries must be a non-negative Integer, but '" + retries + "' has been provided.");
            }
            this.jobId = jobId;
            this.retries = retries;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IJobEntity job = commandContext.JobEntityManager.FindById<IJobEntity>(jobId);
            if (job is object)
            {
                job.Retries = retries;

                if (commandContext.EventDispatcher.Enabled)
                {
                    commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, job));
                }
            }
            else
            {
                throw new ActivitiObjectNotFoundException("No job found with id '" + jobId + "'.", typeof(IJob));
            }
            return null;
        }
    }

}