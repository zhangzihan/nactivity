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

namespace org.activiti.engine.repository
{
    using org.activiti.engine.query;
    using System.Collections.Generic;

    /// <summary>
    /// Allows programmatic querying of <seealso cref="IDeployment"/>s.
    /// 
    /// Note that it is impossible to retrieve the deployment resources through the results of this operation, since that would cause a huge transfer of (possibly) unneeded bytes over the wire.
    /// 
    /// To retrieve the actual bytes of a deployment resource use the operations on the <seealso cref="IRepositoryService#getDeploymentResourceNames(String)"/> and
    /// <seealso cref="IRepositoryService#getResourceAsStream(String, String)"/>
    /// 
    /// 
    /// 
    /// </summary>
    public interface IDeploymentQuery : IQuery<IDeploymentQuery, IDeployment>
    {

        /// <summary>
        /// Only select deployments with the given deployment id.
        /// </summary>
        IDeploymentQuery SetDeploymentId(string deploymentId);

        /// <summary>
        /// Only select deployments with the given deployment ids.
        /// </summary>
        IDeploymentQuery SetDeploymentIds(string[] ids);

        /// <summary>
        /// Only select deployments with the given name.
        /// </summary>
        IDeploymentQuery SetDeploymentName(string name);

        /// <summary>
        /// Only select deployments with a name like the given string.
        /// </summary>
        IDeploymentQuery SetDeploymentNameLike(string nameLike);

        /// <summary>
        /// Only select deployments with the given category.
        /// </summary>
        /// <seealso cref= IDeploymentBuilder#category(String) </seealso>
        IDeploymentQuery SetDeploymentCategory(string category);

        /// <summary>
        /// Only select deployments with a category like the given string.
        /// </summary>
        IDeploymentQuery SetDeploymentCategoryLike(string categoryLike);

        /// <summary>
        /// Only select deployments that have a different category then the given one.
        /// </summary>
        /// <seealso cref= IDeploymentBuilder#category(String) </seealso>
        IDeploymentQuery SetDeploymentCategoryNotEquals(string categoryNotEquals);

        /// <summary>
        /// Only select deployments with the given key.
        /// </summary>
        IDeploymentQuery SetDeploymentKey(string key);

        /// <summary>
        /// Only select deployments with a key like the given string.
        /// </summary>
        IDeploymentQuery SetDeploymentKeyLike(string keyLike);

        /// <summary>
        /// Only select deployments with the given business key.
        /// </summary>
        /// <param name="businessKey"></param>
        /// <returns></returns>
        IDeploymentQuery SetDeploymentBusinessKey(string businessKey);

        /// <summary>
        /// Only select deployment that have the given tenant id.
        /// </summary>
        IDeploymentQuery SetDeploymentTenantId(string tenantId);

        /// <summary>
        /// Only select deployments with a tenant id like the given one.
        /// </summary>
        IDeploymentQuery SetDeploymentTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select deployments that do not have a tenant id.
        /// </summary>
        IDeploymentQuery SetDeploymentWithoutTenantId();

        /// <summary>
        /// Only select deployments with the given process definition key. </summary>
        IDeploymentQuery SetProcessDefinitionKey(string key);

        /// <summary>
        /// Only select deployments with a process definition key like the given string.
        /// </summary>
        IDeploymentQuery SetProcessDefinitionKeyLike(string keyLike);

        /// <summary>
        /// Only select deployments where the deployment time is the latest value.
        /// Can only be used together with the deployment key.
        /// </summary>
        IDeploymentQuery SetLatest();

        IDeploymentQuery SetLatestDeployment();

        // sorting ////////////////////////////////////////////////////////

        /// <summary>
        /// Order by deployment id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IDeploymentQuery SetOrderByDeploymentId();

        /// <summary>
        /// Order by deployment name (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IDeploymentQuery SetOrderByDeploymentName();

        /// <summary>
        /// Order by deployment time (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IDeploymentQuery SetOrderByDeploymenTime();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IDeploymentQuery SetOrderByTenantId();

        IList<IDeployment> FindDrafts();

        IDeploymentQuery SetOnlyDrafts();
    }

}