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

    using org.activiti.engine.repository;

    /// 
    public interface IDeploymentDataManager : IDataManager<IDeploymentEntity>
    {
        IDeploymentEntity FindLatestDeploymentByName(string deploymentName);

        long FindDeploymentCountByQueryCriteria(IDeploymentQuery deploymentQuery);

        IList<IDeployment> FindDeploymentsByQueryCriteria(IDeploymentQuery deploymentQuery, Page page);

        IList<IDeployment> FindDeploymentDrafts(IDeploymentQuery deploymentQuery);

        IList<string> GetDeploymentResourceNames(string deploymentId);

        IList<IDeployment> FindDeploymentsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long FindDeploymentCountByNativeQuery(IDictionary<string, object> parameterMap);
    }
}