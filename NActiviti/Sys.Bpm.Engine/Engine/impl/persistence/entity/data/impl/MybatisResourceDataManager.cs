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

    /// 
    public class MybatisResourceDataManager : AbstractDataManager<IResourceEntity>, IResourceDataManager
    {

        public MybatisResourceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ResourceEntityImpl);
            }
        }

        public override IResourceEntity create()
        {
            return new ResourceEntityImpl();
        }

        public virtual void deleteResourcesByDeploymentId(string deploymentId)
        {
            DbSqlSession.delete("deleteResourcesByDeploymentId", new { deploymentId }, typeof(ResourceEntityImpl));
        }

        public virtual IResourceEntity findResourceByDeploymentIdAndResourceName(string deploymentId, string resourceName)
        {
            return (IResourceEntity)DbSqlSession.selectOne<ResourceEntityImpl, IResourceEntity>("selectResourceByDeploymentIdAndResourceName", new
            {
                deploymentId,
                resourceName
            });
        }

        public virtual IList<IResourceEntity> findResourcesByDeploymentId(string deploymentId)
        {
            return DbSqlSession.selectList<ResourceEntityImpl, IResourceEntity>("selectResourcesByDeploymentId", new { deploymentId });
        }

    }

}