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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.repository;

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

	  public virtual IProcessDefinitionEntity findLatestProcessDefinitionByKey(string processDefinitionKey)
	  {
		return processDefinitionDataManager.findLatestProcessDefinitionByKey(processDefinitionKey);
	  }

	  public virtual IProcessDefinitionEntity findLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
	  {
	   return processDefinitionDataManager.findLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
	  }

	  public virtual void deleteProcessDefinitionsByDeploymentId(string deploymentId)
	  {
		processDefinitionDataManager.deleteProcessDefinitionsByDeploymentId(deploymentId);
	  }

	  public virtual IList<IProcessDefinition> findProcessDefinitionsByQueryCriteria(ProcessDefinitionQueryImpl processDefinitionQuery, Page page)
	  {
	   return processDefinitionDataManager.findProcessDefinitionsByQueryCriteria(processDefinitionQuery, page);
	  }

	  public virtual long findProcessDefinitionCountByQueryCriteria(ProcessDefinitionQueryImpl processDefinitionQuery)
	  {
		return processDefinitionDataManager.findProcessDefinitionCountByQueryCriteria(processDefinitionQuery);
	  }

	  public virtual IProcessDefinitionEntity findProcessDefinitionByDeploymentAndKey(string deploymentId, string processDefinitionKey)
	  {
		return processDefinitionDataManager.findProcessDefinitionByDeploymentAndKey(deploymentId, processDefinitionKey);
	  }

	  public virtual IProcessDefinitionEntity findProcessDefinitionByDeploymentAndKeyAndTenantId(string deploymentId, string processDefinitionKey, string tenantId)
	  {
	   return processDefinitionDataManager.findProcessDefinitionByDeploymentAndKeyAndTenantId(deploymentId, processDefinitionKey, tenantId);
	  }

	  public virtual IProcessDefinition findProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
	  {
		if (ReferenceEquals(tenantId, null) || engine.ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
		{
		  return processDefinitionDataManager.findProcessDefinitionByKeyAndVersion(processDefinitionKey, processDefinitionVersion);
		}
		else
		{
		  return processDefinitionDataManager.findProcessDefinitionByKeyAndVersionAndTenantId(processDefinitionKey, processDefinitionVersion, tenantId);
		}
	  }

	  public virtual IList<IProcessDefinition> findProcessDefinitionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
	  {
		return processDefinitionDataManager.findProcessDefinitionsByNativeQuery(parameterMap, firstResult, maxResults);
	  }

	  public virtual long findProcessDefinitionCountByNativeQuery(IDictionary<string, object> parameterMap)
	  {
		return processDefinitionDataManager.findProcessDefinitionCountByNativeQuery(parameterMap);
	  }

	  public virtual void updateProcessDefinitionTenantIdForDeployment(string deploymentId, string newTenantId)
	  {
		processDefinitionDataManager.updateProcessDefinitionTenantIdForDeployment(deploymentId, newTenantId);
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