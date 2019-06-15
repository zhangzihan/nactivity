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

    /// <summary>
    /// Allows programmatic querying of <seealso cref="IModel"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IModelQuery : IQuery<IModelQuery, IModel>
    {

        /// <summary>
        /// Only select model with the given id. </summary>
        IModelQuery SetModelId(string modelId);

        /// <summary>
        /// Only select models with the given category. </summary>
        IModelQuery SetModelCategory(string modelCategory);

        /// <summary>
        /// Only select models where the category matches the given parameter. The syntax that should be used is the same as in SQL, eg. %activiti%
        /// </summary>
        IModelQuery SetModelCategoryLike(string modelCategoryLike);

        /// <summary>
        /// Only select models that have a different category then the given one. </summary>
        IModelQuery SetModelCategoryNotEquals(string categoryNotEquals);

        /// <summary>
        /// Only select models with the given name. </summary>
        IModelQuery SetModelName(string modelName);

        /// <summary>
        /// Only select models where the name matches the given parameter. The syntax that should be used is the same as in SQL, eg. %activiti%
        /// </summary>
        IModelQuery SetModelNameLike(string modelNameLike);

        /// <summary>
        /// Only selects models with the given key. </summary>
        IModelQuery SetModelKey(string key);

        /// <summary>
        /// Only select model with a certain version. </summary>
        IModelQuery SetModelVersion(int? modelVersion);

        /// <summary>
        /// Only select models which has the highest version.
        /// 
        /// Note: if modelKey(key) is not used in this query, all the models with the highest version for each key will be returned (similar to process definitions)
        /// </summary>
        IModelQuery SetLatestVersion();

        /// <summary>
        /// Only select models that are the source for the provided deployment </summary>
        IModelQuery SetDeploymentId(string deploymentId);

        /// <summary>
        /// Only select models that are deployed (ie deploymentId != null) </summary>
        IModelQuery SetDeployed();

        /// <summary>
        /// Only select models that are not yet deployed </summary>
        IModelQuery SetNotDeployed();

        /// <summary>
        /// Only select models that have the given tenant id.
        /// </summary>
        IModelQuery SetModelTenantId(string tenantId);

        /// <summary>
        /// Only select models with a tenant id like the given one.
        /// </summary>
        IModelQuery SetModelTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select models that do not have a tenant id.
        /// </summary>
        IModelQuery SetModelWithoutTenantId();

        // ordering ////////////////////////////////////////////////////////////

        /// <summary>
        /// Order by the category of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByModelCategory();

        /// <summary>
        /// Order by the id of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByModelId();

        /// <summary>
        /// Order by the key of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByModelKey();

        /// <summary>
        /// Order by the version of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByModelVersion();

        /// <summary>
        /// Order by the name of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByModelName();

        /// <summary>
        /// Order by the creation time of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByCreateTime();

        /// <summary>
        /// Order by the last update time of the models (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByLastUpdateTime();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IModelQuery OrderByTenantId();

    }

}