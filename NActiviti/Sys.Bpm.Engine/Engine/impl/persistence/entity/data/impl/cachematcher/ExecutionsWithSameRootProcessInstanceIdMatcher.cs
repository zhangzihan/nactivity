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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher
{

	using Sys.Workflow.Engine.Impl.Persistence.Caches;

	/// 
	public class ExecutionsWithSameRootProcessInstanceIdMatcher : ICachedEntityMatcher<IExecutionEntity>
	{

	  public virtual bool IsRetained(ICollection<IExecutionEntity> databaseEntities, ICollection<CachedEntity> cachedEntities, IExecutionEntity entity, object param)
	  {
		IExecutionEntity executionEntity = getMatchingExecution(databaseEntities, cachedEntities, (string) param);
		return (!ReferenceEquals(executionEntity.RootProcessInstanceId, null) && executionEntity.RootProcessInstanceId.Equals(entity.RootProcessInstanceId));
	  }

	  public virtual IExecutionEntity getMatchingExecution(ICollection<IExecutionEntity> databaseEntities, ICollection<CachedEntity> cachedEntities, string executionId)
	  {

		// Doing some preprocessing here: we need to find the execution that matches the provided execution id,
		// as we need to match the root process instance id later on.

		if (cachedEntities is object)
		{
		  foreach (CachedEntity cachedEntity in cachedEntities)
		  {
			IExecutionEntity executionEntity = (IExecutionEntity) cachedEntity.Entity;
			if (executionId.Equals(executionEntity.Id))
			{
			  return executionEntity;
			}
		  }
		}

		if (databaseEntities is object)
		{
		  foreach (IExecutionEntity databaseExecutionEntity in databaseEntities)
		  {
			if (executionId.Equals(databaseExecutionEntity.Id))
			{
			  return databaseExecutionEntity;
			}
		  }
		}

		return null;
	  }

	}
}