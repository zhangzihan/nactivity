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
    public class MybatisDeploymentDataManager : AbstractDataManager<IDeploymentEntity>, IDeploymentDataManager
    {

        public MybatisDeploymentDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(DeploymentEntityImpl);
            }
        }

        public override IDeploymentEntity create()
        {
            return new DeploymentEntityImpl();
        }

        public virtual IDeploymentEntity findLatestDeploymentByName(string deploymentName)
        {
            IList<IDeploymentEntity> list = DbSqlSession.selectList<DeploymentEntityImpl, IDeploymentEntity>("selectDeploymentsByName", new { deploymentName }, 0, 1);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        public virtual long findDeploymentCountByQueryCriteria(DeploymentQueryImpl deploymentQuery)
        {
            return DbSqlSession.selectOne<DeploymentEntityImpl, long?>("selectDeploymentCountByQueryCriteria", deploymentQuery).GetValueOrDefault();
        }

        public virtual IList<IDeployment> findDeploymentsByQueryCriteria(DeploymentQueryImpl deploymentQuery, Page page)
        {
            const string query = "selectDeploymentsByQueryCriteria";
            return DbSqlSession.selectList<DeploymentEntityImpl, IDeployment>(query, deploymentQuery, page);
        }

        public virtual IList<string> getDeploymentResourceNames(string deploymentId)
        {
            return DbSqlSession.selectList<ResourceEntityImpl, string>("selectResourceNamesByDeploymentId", new { deploymentId });
        }

        public virtual IList<IDeployment> findDeploymentsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<DeploymentEntityImpl, IDeployment>("selectDeploymentByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findDeploymentCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.selectOne<DeploymentEntityImpl, long?>("selectDeploymentCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

    }

}