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
namespace org.activiti.engine.impl.persistence.entity.data
{

	using org.activiti.engine.runtime;

	/// 
	/// 
	public interface ITimerJobDataManager : IDataManager<ITimerJobEntity>
	{

	  IList<ITimerJobEntity> findTimerJobsToExecute(Page page);

	  IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId);

	  IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyNoTenantId(string jobHandlerType, string processDefinitionKey);

	  IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyAndTenantId(string jobHandlerType, string processDefinitionKey, string tenantId);

	  IList<ITimerJobEntity> findJobsByExecutionId(string executionId);

	  IList<ITimerJobEntity> findJobsByProcessInstanceId(string processInstanceId);

	  IList<IJob> findJobsByQueryCriteria(TimerJobQueryImpl jobQuery, Page page);

	  long findJobCountByQueryCriteria(TimerJobQueryImpl jobQuery);

	  void updateJobTenantIdForDeployment(string deploymentId, string newTenantId);
	}

}