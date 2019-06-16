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
namespace Sys.Workflow.engine.impl.persistence.entity.data
{

	using Sys.Workflow.engine.runtime;

	/// 
	/// 
	public interface ITimerJobDataManager : IDataManager<ITimerJobEntity>
	{

	  IList<ITimerJobEntity> FindTimerJobsToExecute(Page page);

	  IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId);

	  IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyNoTenantId(string jobHandlerType, string processDefinitionKey);

	  IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyAndTenantId(string jobHandlerType, string processDefinitionKey, string tenantId);

	  IList<ITimerJobEntity> FindJobsByExecutionId(string executionId);

	  IList<ITimerJobEntity> FindJobsByProcessInstanceId(string processInstanceId);

	  IList<IJob> FindJobsByQueryCriteria(ITimerJobQuery jobQuery, Page page);

	  long FindJobCountByQueryCriteria(ITimerJobQuery jobQuery);

	  void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId);
	}

}