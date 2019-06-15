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

        public override IProcessDefinitionEntity Create()
        {
            return new ProcessDefinitionEntityImpl();
        }

        public virtual IProcessDefinitionEntity FindLatestProcessDefinitionByKey(string processDefinitionKey)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectLatestProcessDefinitionByKey", new { processDefinitionKey });
        }

        public virtual IProcessDefinitionEntity FindLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["processDefinitionKey"] = processDefinitionKey,
                ["tenantId"] = tenantId
            };
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectLatestProcessDefinitionByKeyAndTenantId", @params);
        }

        public virtual void DeleteProcessDefinitionsByDeploymentId(string deploymentId)
        {
            DbSqlSession.Delete("deleteProcessDefinitionsByDeploymentId", new { deploymentId }, typeof(ProcessDefinitionEntityImpl));
        }

        public virtual IList<IProcessDefinition> FindProcessDefinitionsByQueryCriteria(IProcessDefinitionQuery processDefinitionQuery, Page page)
        {
            // List<ProcessDefinition> processDefinitions =
            return DbSqlSession.SelectList<ProcessDefinitionEntityImpl, IProcessDefinition>("selectProcessDefinitionsByQueryCriteria", processDefinitionQuery, page);

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

        public virtual long FindProcessDefinitionCountByQueryCriteria(IProcessDefinitionQuery processDefinitionQuery)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, long?>("selectProcessDefinitionCountByQueryCriteria", processDefinitionQuery).GetValueOrDefault();
        }

        public virtual IProcessDefinitionEntity FindProcessDefinitionByDeploymentAndKey(string deploymentId, string processDefinitionKey)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionByDeploymentAndKey", new { deploymentId, processDefinitionKey });
        }

        public virtual IProcessDefinitionEntity FindProcessDefinitionByDeploymentAndKeyAndTenantId(string deploymentId, string processDefinitionKey, string tenantId)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionByDeploymentAndKeyAndTenantId", new
            {
                deploymentId,
                processDefinitionKey,
                tenantId
            });
        }

        public virtual IProcessDefinitionEntity FindProcessDefinitionByKeyAndVersion(string processDefinitionKey, int? processDefinitionVersion)
        {
            IList<IProcessDefinitionEntity> results = DbSqlSession.SelectList<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionsByKeyAndVersion", new { processDefinitionKey, processDefinitionVersion });
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

        public virtual IProcessDefinitionEntity FindProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
        {
            IList<IProcessDefinitionEntity> results = DbSqlSession.SelectList<ProcessDefinitionEntityImpl, IProcessDefinitionEntity>("selectProcessDefinitionsByKeyAndVersionAndTenantId", new
            { processDefinitionKey, processDefinitionVersion, tenantId });
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

        public virtual IList<IProcessDefinition> FindProcessDefinitionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<ProcessDefinitionEntityImpl, IProcessDefinition>("selectProcessDefinitionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindProcessDefinitionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<ProcessDefinitionEntityImpl, long?>("selectProcessDefinitionCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

        public virtual void UpdateProcessDefinitionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<ProcessDefinitionEntityImpl>("updateProcessDefinitionTenantIdForDeploymentId", @params);
        }

    }

}