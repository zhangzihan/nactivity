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
namespace org.activiti.engine.impl.asyncexecutor
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;

    /// 
    public class ResetExpiredJobsCmd : ICommand<object>
	{

	  protected internal ICollection<string> jobIds;

	  public ResetExpiredJobsCmd(ICollection<string> jobsIds)
	  {
		this.jobIds = jobsIds;
	  }

	  public virtual object execute(ICommandContext commandContext)
	  {
		bool messageQueueMode = commandContext.ProcessEngineConfiguration.AsyncExecutorIsMessageQueueMode;
		foreach (string jobId in jobIds)
		{
		  if (!messageQueueMode)
		  {
			IJob job = commandContext.JobEntityManager.findById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));
			commandContext.JobManager.unacquire(job);
		  }
		  else
		  {
			commandContext.JobEntityManager.resetExpiredJob(jobId);
		  }
		}
		return null;
	  }

	}

}