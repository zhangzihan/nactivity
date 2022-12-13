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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

	using Sys.Workflow.Engine.Impl.Cfg;
	using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
	using Sys.Workflow.Engine.Repository;

	/// 
	/// 
	/// 
	/// 
	public class ProcessDefinitionEntityManagerImpl : AbstractEntityManager<IProcessDefinitionEntity>, IProcessDefinitionEntityManager
	{

		protected internal IProcessDefinitionDataManager processDefinitionDataManager;

		public ProcessDefinitionEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IProcessDefinitionDataManager processDefinitionDataManager) : base(processEngineConfiguration)
		{
			this.processDefinitionDataManager = processDefinitionDataManager;
		}

		protected internal override IDataManager<IProcessDefinitionEntity> DataManager
		{
			get
			{
				return processDefinitionDataManager;
			}
		}

		public virtual IProcessDefinitionEntity FindLatestProcessDefinitionByKey(string processDefinitionKey)
		{
			return processDefinitionDataManager.FindLatestProcessDefinitionByKey(processDefinitionKey);
		}

		public virtual IProcessDefinitionEntity FindLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
		{
			return processDefinitionDataManager.FindLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
		}

		public virtual void DeleteProcessDefinitionsByDeploymentId(string deploymentId)
		{
			processDefinitionDataManager.DeleteProcessDefinitionsByDeploymentId(deploymentId);
		}

		public virtual IList<IProcessDefinition> FindProcessDefinitionsByQueryCriteria(IProcessDefinitionQuery processDefinitionQuery, Page page)
		{
			return processDefinitionDataManager.FindProcessDefinitionsByQueryCriteria(processDefinitionQuery, page);
		}

		public virtual long FindProcessDefinitionCountByQueryCriteria(IProcessDefinitionQuery processDefinitionQuery)
		{
			return processDefinitionDataManager.FindProcessDefinitionCountByQueryCriteria(processDefinitionQuery);
		}

		public virtual IProcessDefinitionEntity FindProcessDefinitionByDeploymentAndKey(string deploymentId, string processDefinitionKey)
		{
			return processDefinitionDataManager.FindProcessDefinitionByDeploymentAndKey(deploymentId, processDefinitionKey);
		}

		public virtual IProcessDefinitionEntity FindProcessDefinitionByDeploymentAndKeyAndTenantId(string deploymentId, string processDefinitionKey, string tenantId)
		{
			return processDefinitionDataManager.FindProcessDefinitionByDeploymentAndKeyAndTenantId(deploymentId, processDefinitionKey, tenantId);
		}

		public virtual IProcessDefinition FindProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
		{
			if (string.IsNullOrWhiteSpace(tenantId))
			{
				return processDefinitionDataManager.FindProcessDefinitionByKeyAndVersion(processDefinitionKey, processDefinitionVersion);
			}
			else
			{
				return processDefinitionDataManager.FindProcessDefinitionByKeyAndVersionAndTenantId(processDefinitionKey, processDefinitionVersion, tenantId);
			}
		}

		public virtual IList<IProcessDefinition> FindProcessDefinitionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
		{
			return processDefinitionDataManager.FindProcessDefinitionsByNativeQuery(parameterMap, firstResult, maxResults);
		}

		public virtual long FindProcessDefinitionCountByNativeQuery(IDictionary<string, object> parameterMap)
		{
			return processDefinitionDataManager.FindProcessDefinitionCountByNativeQuery(parameterMap);
		}

		public virtual void UpdateProcessDefinitionTenantIdForDeployment(string deploymentId, string newTenantId)
		{
			processDefinitionDataManager.UpdateProcessDefinitionTenantIdForDeployment(deploymentId, newTenantId);
		}

		public override TOut FindById<TOut>(object entityId)
		{
			return base.FindById<TOut>(new KeyValuePair<string, object>("processDefinitionId", entityId));
		}

		public virtual IProcessDefinitionDataManager ProcessDefinitionDataManager
		{
			get
			{
				return processDefinitionDataManager;
			}
			set
			{
				this.processDefinitionDataManager = value;
			}
		}


	}

}