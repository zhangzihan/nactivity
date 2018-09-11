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

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    /// 
    [Serializable]
	public class SetTimerJobRetriesCmd : ICommand<object>
	{

	  private const long serialVersionUID = 1L;

	  private readonly string jobId;
	  private readonly int retries;

	  public SetTimerJobRetriesCmd(string jobId, int retries)
	  {
		if (string.ReferenceEquals(jobId, null) || jobId.Length < 1)
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

	  public  virtual object  execute(ICommandContext commandContext)
	  {
		ITimerJobEntity job = commandContext.TimerJobEntityManager.findById<ITimerJobEntity>(new KeyValuePair<string, object>("id", jobId));
		if (job != null)
		{

		  job.Retries = retries;

		  if (commandContext.EventDispatcher.Enabled)
		  {
			commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_UPDATED, job));
		  }
		}
		else
		{
		  throw new ActivitiObjectNotFoundException("No timer job found with id '" + jobId + "'.", typeof(IJob));
		}
		return null;
	  }
	}

}