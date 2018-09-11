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

	using org.activiti.engine.impl.persistence.entity;

	/// 
	public class AcquiredTimerJobEntities
	{

	  protected internal IDictionary<string, ITimerJobEntity> acquiredJobs = new Dictionary<string, ITimerJobEntity>();

	  public virtual void addJob(ITimerJobEntity job)
	  {
		acquiredJobs[job.Id] = job;
	  }

	  public virtual ICollection<ITimerJobEntity> Jobs
	  {
		  get
		  {
			return acquiredJobs.Values;
		  }
	  }

	  public virtual bool contains(string jobId)
	  {
		return acquiredJobs.ContainsKey(jobId);
	  }

	  public virtual int size()
	  {
		return acquiredJobs.Count;
	  }
	}

}