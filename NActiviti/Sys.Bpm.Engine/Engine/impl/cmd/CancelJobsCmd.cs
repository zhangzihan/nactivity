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

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// Send job cancelled event and delete job
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class CancelJobsCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        internal IList<string> jobIds;

        public CancelJobsCmd(IList<string> jobIds)
        {
            this.jobIds = jobIds;
        }

        public CancelJobsCmd(string jobId)
        {
            this.jobIds = new List<string>();
            jobIds.Add(jobId);
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            foreach (string jobId in jobIds)
            {
                IJobEntity jobToDelete = commandContext.JobEntityManager.FindById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));

                if (jobToDelete != null)
                {
                    // When given job doesn't exist, ignore
                    if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                    {
                        commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, jobToDelete));
                    }

                    commandContext.JobEntityManager.Delete(jobToDelete);

                }
                else
                {
                    ITimerJobEntity timerJobToDelete = commandContext.TimerJobEntityManager.FindById<ITimerJobEntity>(new KeyValuePair<string, object>("id", jobId));

                    if (timerJobToDelete != null)
                    {
                        // When given job doesn't exist, ignore
                        if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                        {
                            commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, timerJobToDelete));
                        }

                        commandContext.TimerJobEntityManager.Delete(timerJobToDelete);
                    }
                }
            }
            return null;
        }
    }
}