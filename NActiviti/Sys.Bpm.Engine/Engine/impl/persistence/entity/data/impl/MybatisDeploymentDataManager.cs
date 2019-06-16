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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl
{

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Repository;

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

        public override IDeploymentEntity Create()
        {
            return new DeploymentEntityImpl();
        }

        public virtual IDeploymentEntity FindLatestDeploymentByName(string deploymentName)
        {
            IList<IDeploymentEntity> list = DbSqlSession.SelectList<DeploymentEntityImpl, IDeploymentEntity>("selectDeploymentsByName", new { deploymentName }, 0, 1);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        public virtual long FindDeploymentCountByQueryCriteria(IDeploymentQuery deploymentQuery)
        {
            return DbSqlSession.SelectOne<DeploymentEntityImpl, long?>("selectDeploymentCountByQueryCriteria", deploymentQuery).GetValueOrDefault();
        }

        public virtual IList<IDeployment> FindDeploymentsByQueryCriteria(IDeploymentQuery deploymentQuery, Page page)
        {
            const string query = "selectDeploymentsByQueryCriteria";
            return DbSqlSession.SelectList<DeploymentEntityImpl, IDeployment>(query, deploymentQuery, page);
        }

        public virtual IList<string> GetDeploymentResourceNames(string deploymentId)
        {
            return DbSqlSession.SelectList<ResourceEntityImpl, string>("selectResourceNamesByDeploymentId", new { deploymentId });
        }

        public virtual IList<IDeployment> FindDeploymentsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<DeploymentEntityImpl, IDeployment>("selectDeploymentByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindDeploymentCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<DeploymentEntityImpl, long?>("selectDeploymentCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

        public IList<IDeployment> FindDeploymentDrafts(IDeploymentQuery query)
        {
            return DbSqlSession.SelectList<DeploymentEntityImpl, IDeployment>("selectDeploymentDrafts", query);
        }
    }

}