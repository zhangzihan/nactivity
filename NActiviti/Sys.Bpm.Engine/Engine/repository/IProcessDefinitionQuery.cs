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

namespace org.activiti.engine.repository
{

    using org.activiti.engine.query;

    /// <summary>
    /// Allows programmatic querying of <seealso cref="IProcessDefinition"/>s.
    /// </summary>
    public interface IProcessDefinitionQuery : IQuery<IProcessDefinitionQuery, IProcessDefinition>
    {

        /// <summary>
        /// Only select process definition with the given id.
        /// </summary>
        IProcessDefinitionQuery processDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select process definitions with the given ids.
        /// </summary>
        IProcessDefinitionQuery processDefinitionIds(ISet<string> processDefinitionIds);

        /// <summary>
        /// Only select process definitions with the given category.
        /// </summary>
        IProcessDefinitionQuery processDefinitionCategory(string processDefinitionCategory);

        /// <summary>
        /// Only select process definitions where the category matches the given parameter. The syntax that should be used is the same as in SQL, eg. %activiti%
        /// </summary>
        IProcessDefinitionQuery processDefinitionCategoryLike(string processDefinitionCategoryLike);

        /// <summary>
        /// Only select deployments that have a different category then the given one.
        /// </summary>
        /// <seealso cref= IDeploymentBuilder#category(String) </seealso>
        IProcessDefinitionQuery processDefinitionCategoryNotEquals(string categoryNotEquals);

        /// <summary>
        /// Only select process definitions with the given name.
        /// </summary>
        IProcessDefinitionQuery processDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select process definitions where the name matches the given parameter. The syntax that should be used is the same as in SQL, eg. %activiti%
        /// </summary>
        IProcessDefinitionQuery processDefinitionNameLike(string processDefinitionNameLike);

        /// <summary>
        /// Only select process definitions that are deployed in a deployment with the given deployment id
        /// </summary>
        IProcessDefinitionQuery deploymentId(string deploymentId);

        /// <summary>
        /// Select process definitions that are deployed in deployments with the given set of ids
        /// </summary>
        IProcessDefinitionQuery deploymentIds(ISet<string> deploymentIds);

        /// <summary>
        /// Only select process definition with the given key.
        /// </summary>
        IProcessDefinitionQuery processDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// 业务键值
        /// </summary>
        /// <param name="businessKey"></param>
        /// <returns></returns>
        IProcessDefinitionQuery processDefinitionBusinessKey(string businessKey);

        /// <summary>
        /// 业务路径
        /// </summary>
        /// <param name="businessPath"></param>
        /// <returns></returns>
        IProcessDefinitionQuery processDefinitionBusinessPath(string businessPath);

        /// <summary>
        /// 开始表单
        /// </summary>
        /// <param name="startForm"></param>
        /// <returns></returns>
        IProcessDefinitionQuery processDefinitionStartForm(string startForm);

        /// <summary>
        /// Only select process definition with the given keys.
        /// </summary>
        IProcessDefinitionQuery processDefinitionKeys(ISet<string> processDefinitionKeys);

        /// <summary>
        /// Only select process definitions where the key matches the given parameter. The syntax that should be used is the same as in SQL, eg. %activiti%
        /// </summary>
        IProcessDefinitionQuery processDefinitionKeyLike(string processDefinitionKeyLike);

        /// <summary>
        /// Only select process definition with a certain version. Particulary useful when used in combination with <seealso cref="#processDefinitionKey(String)"/>
        /// </summary>
        IProcessDefinitionQuery processDefinitionVersion(int? processDefinitionVersion);

        /// <summary>
        /// Only select process definitions which version are greater than a certain version.
        /// </summary>
        IProcessDefinitionQuery processDefinitionVersionGreaterThan(int? processDefinitionVersion);

        /// <summary>
        /// Only select process definitions which version are greater than or equals a certain version.
        /// </summary>
        IProcessDefinitionQuery processDefinitionVersionGreaterThanOrEquals(int? processDefinitionVersion);

        /// <summary>
        /// Only select process definitions which version are lower than a certain version.
        /// </summary>
        IProcessDefinitionQuery processDefinitionVersionLowerThan(int? processDefinitionVersion);

        /// <summary>
        /// Only select process definitions which version are lower than or equals a certain version.
        /// </summary>
        IProcessDefinitionQuery processDefinitionVersionLowerThanOrEquals(int? processDefinitionVersion);

        /// <summary>
        /// Only select the process definitions which are the latest deployed (ie. which have the highest version number for the given key).
        /// <para>
        /// Can also be used without any other criteria (ie. query.latest().list()),
        /// which will then give all the latest versions of all the deployed process definitions.
        /// 
        /// </para>
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException"> if used in combination with <seealso cref="#groupId(string)"/>, <seealso cref="#processDefinitionVersion(int)"/> or <seealso cref="#deploymentId(String)"/> </exception>
        IProcessDefinitionQuery latestVersion();

        /// <summary>
        /// Only select process definition with the given resource name.
        /// </summary>
        IProcessDefinitionQuery processDefinitionResourceName(string resourceName);

        /// <summary>
        /// Only select process definition with a resource name like the given .
        /// </summary>
        IProcessDefinitionQuery processDefinitionResourceNameLike(string resourceNameLike);

        /// <summary>
        /// select process definition with procDefId
        /// </summary>
        /// <param name="procDefId"></param>
        /// <returns></returns>
        IProcessDefinitionQuery processDefinitionStarter(string procDefId);

        /// <summary>
        /// Only selects process definitions which given userId is authoriezed to start
        /// </summary>
        IProcessDefinitionQuery startableByUser(string userId);

        /// <summary>
        /// Only selects process definitions which are suspended
        /// </summary>
        IProcessDefinitionQuery suspended();

        /// <summary>
        /// Only selects process definitions which are active
        /// </summary>
        IProcessDefinitionQuery active();

        /// <summary>
        /// Only select process definitions that have the given tenant id.
        /// </summary>
        IProcessDefinitionQuery processDefinitionTenantId(string tenantId);

        /// <summary>
        /// Only select process definitions with a tenant id like the given one.
        /// </summary>
        IProcessDefinitionQuery processDefinitionTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select process definitions that do not have a tenant id.
        /// </summary>
        IProcessDefinitionQuery processDefinitionWithoutTenantId();

        // Support for event subscriptions /////////////////////////////////////

        /// <summary>
        /// Selects the single process definition which has a start message event with the messageName.
        /// </summary>
        IProcessDefinitionQuery messageEventSubscriptionName(string messageName);

        // ordering ////////////////////////////////////////////////////////////

        /// <summary>
        /// Order by the category of the process definitions (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByProcessDefinitionCategory();

        /// <summary>
        /// Order by process definition key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByProcessDefinitionKey();

        /// <summary>
        /// Order by the id of the process definitions (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByProcessDefinitionId();

        /// <summary>
        /// Order by the version of the process definitions (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByProcessDefinitionVersion();

        /// <summary>
        /// Order by the name of the process definitions (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByProcessDefinitionName();

        /// <summary>
        /// Order by deployment id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByDeploymentId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessDefinitionQuery orderByTenantId();

    }

}