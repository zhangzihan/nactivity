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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.repository;

    /// 
    public class MybatisProcessDefinitionDataManager : AbstractDataManager<IProcessDefinitionEntity>, IProcessDefinitionDataManager
    {

        public MybatisProcessDefinitionDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ProcessDefinitionEntityImpl);
            }
        }

        public override IProcessDefinitionEntity create()
        {
            return new ProcessDefinitionEntityImpl();
        }

        public virtual IProcessDefinitionEntity findLatestProcessDefinitionByKey(string processDefinitionKey)
        {
            return DbSqlSession.selectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectLatestProcessDefinitionByKey", new KeyValuePair<string, object>("processDefinitionKey", processDefinitionKey));
        }

        public virtual IProcessDefinitionEntity findLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2);
            @params["processDefinitionKey"] = processDefinitionKey;
            @params["tenantId"] = tenantId;
            return DbSqlSession.selectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectLatestProcessDefinitionByKeyAndTenantId", @params);
        }

        public virtual void deleteProcessDefinitionsByDeploymentId(string deploymentId)
        {
            DbSqlSession.delete("deleteProcessDefinitionsByDeploymentId", new KeyValuePair<string, object>("deploymentId", deploymentId), typeof(ProcessDefinitionEntityImpl));
        }

        public virtual IList<IProcessDefinition> findProcessDefinitionsByQueryCriteria(ProcessDefinitionQueryImpl processDefinitionQuery, Page page)
        {
            // List<ProcessDefinition> processDefinitions =
            return DbSqlSession.selectList<ProcessDefinitionEntityImpl, IProcessDefinition>("selectProcessDefinitionsByQueryCriteria", processDefinitionQuery, page);

            // skipped this after discussion within the team
            // // retrieve process definitions from cache
            // (https://activiti.atlassian.net/browse/ACT-1020) to have all available
            // information
            // ArrayList<ProcessDefinition> result = new
            // ArrayList<ProcessDefinition>();
            // for (ProcessDefinition processDefinitionEntity : processDefinitions)
            // {
            // ProcessDefinitionEntity fullProcessDefinition = Context
            // .getProcessEngineConfiguration()
            // .getDeploymentCache().resolveProcessDefinition((ProcessDefinitionEntity)processDefinitionEntity);
            // result.add(fullProcessDefinition);
            // }
            // return result;
        }

        public virtual long findProcessDefinitionCountByQueryCriteria(ProcessDefinitionQueryImpl processDefinitionQuery)
        {
            return ((long?)DbSqlSession.selectOne<ProcessDefinitionEntityImpl, long?>("selectProcessDefinitionCountByQueryCriteria", processDefinitionQuery)).GetValueOrDefault();
        }

        public virtual IProcessDefinitionEntity findProcessDefinitionByDeploymentAndKey(string deploymentId, string processDefinitionKey)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["deploymentId"] = deploymentId;
            parameters["processDefinitionKey"] = processDefinitionKey;
            return DbSqlSession.selectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionByDeploymentAndKey", parameters);
        }

        public virtual IProcessDefinitionEntity findProcessDefinitionByDeploymentAndKeyAndTenantId(string deploymentId, string processDefinitionKey, string tenantId)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["deploymentId"] = deploymentId;
            parameters["processDefinitionKey"] = processDefinitionKey;
            parameters["tenantId"] = tenantId;
            return DbSqlSession.selectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionByDeploymentAndKeyAndTenantId", parameters);
        }

        public virtual IProcessDefinitionEntity findProcessDefinitionByKeyAndVersion(string processDefinitionKey, int? processDefinitionVersion)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["processDefinitionKey"] = processDefinitionKey;
            @params["processDefinitionVersion"] = processDefinitionVersion;
            IList<IProcessDefinitionEntity> results = DbSqlSession.selectList<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionsByKeyAndVersion", @params);
            if (results.Count == 1)
            {
                return results[0];
            }
            else if (results.Count > 1)
            {
                throw new ActivitiException("There are " + results.Count + " process definitions with key = '" + processDefinitionKey + "' and version = '" + processDefinitionVersion + "'.");
            }
            return null;
        }

        public virtual IProcessDefinitionEntity findProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["processDefinitionKey"] = processDefinitionKey;
            @params["processDefinitionVersion"] = processDefinitionVersion;
            @params["tenantId"] = tenantId;
            IList<IProcessDefinitionEntity> results = DbSqlSession.selectList<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionsByKeyAndVersionAndTenantId", @params);
            if (results.Count == 1)
            {
                return results[0];
            }
            else if (results.Count > 1)
            {
                throw new ActivitiException("There are " + results.Count + " process definitions with key = '" + processDefinitionKey + "' and version = '" + processDefinitionVersion + "'.");
            }
            return null;
        }

        public virtual IList<IProcessDefinition> findProcessDefinitionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<ProcessDefinitionEntityImpl, IProcessDefinition>("selectProcessDefinitionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findProcessDefinitionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return ((long?)DbSqlSession.selectOne<ProcessDefinitionEntityImpl, long?>("selectProcessDefinitionCountByNativeQuery", parameterMap)).GetValueOrDefault();
        }

        public virtual void updateProcessDefinitionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<ProcessDefinitionEntityImpl>("updateProcessDefinitionTenantIdForDeploymentId", @params);
        }

    }

}