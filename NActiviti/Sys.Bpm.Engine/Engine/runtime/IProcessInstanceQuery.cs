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
namespace Sys.Workflow.Engine.Runtime
{

    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows programmatic querying of <seealso cref="IProcessInstance"/>s.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IProcessInstanceQuery : IQuery<IProcessInstanceQuery, IProcessInstance>
    {

        /// <summary>
        /// Select the process instance with the given id </summary>
        IProcessInstanceQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Select process instances whose id is in the given set of ids </summary>
        IProcessInstanceQuery SetProcessInstanceIds(string[] processInstanceIds);

        /// <summary>
        /// Select process instances with the given business key </summary>
        IProcessInstanceQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey);

        /// <summary>
        /// Select process instance with the given business key, unique for the given process definition
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey, string processDefinitionKey);

        /// <summary>
        /// Only select process instances that have the given tenant id.
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceTenantId(string tenantId);

        /// <summary>
        /// Only select process instances with a tenant id like the given one.
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select process instances that do not have a tenant id.
        /// </summary>
        IProcessInstanceQuery ProcessInstanceWithoutTenantId();

        /// <summary>
        /// Only select process instances whose process definition category is processDefinitionCategory. </summary>
        IProcessInstanceQuery SetProcessDefinitionCategory(string processDefinitionCategory);


        /// <summary>
        /// Select process instances whose process definition name is processDefinitionName
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select process instances with a certain process definition version.
        /// Particulary useful when used in combination with <seealso cref="#processDefinitionKey(String)"/>
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionVersion(int? processDefinitionVersion);

        /// <summary>
        /// Select the process instances which are defined by a process definition with the given key.
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Select the process instances which are defined by process definitions with the given keys.
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionKeys(string[] processDefinitionKeys);

        /// <summary>
        /// Select the process instances which are defined by a process definition with the given id.
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Select the process instances which are defined by process definitions with the given ids.
        /// </summary>
        IProcessInstanceQuery SetProcessDefinitionIds(string[] processDefinitionIds);

        /// <summary>
        /// Select the process instances which are defined by a deployment with the given id.
        /// </summary>
        IProcessInstanceQuery SetDeploymentId(string deploymentId);

        /// <summary>
        /// Select the process instances which are defined by one of the given deployment ids
        /// </summary>
        IProcessInstanceQuery SetDeploymentIdIn(IList<string> deploymentIds);

        /// <summary>
        /// Select the process instances which are a sub process instance of the given super process instance.
        /// </summary>
        IProcessInstanceQuery SetSuperProcessInstanceId(string superProcessInstanceId);

        /// <summary>
        /// Select the process instance that have as sub process instance the given process instance. Note that there will always be maximum only <b>one</b> such process instance that can be the result of
        /// this query.
        /// </summary>
        IProcessInstanceQuery SetSubProcessInstanceId(string subProcessInstanceId);

        /// <summary>
        /// Exclude sub processes from the query result;
        /// </summary>
        IProcessInstanceQuery SetExcludeSubprocesses(bool excludeSubprocesses);

        /// <summary>
        /// Select the process instances with which the user with the given id is involved.
        /// </summary>
        IProcessInstanceQuery SetInvolvedUser(string userId);

        /// <summary>
        /// Only select process instances which have a global variable with the given value. The type of variable is determined based on the value, using types configured in
        /// <seealso cref="ProcessEngineConfiguration#getVariableTypes()"/>. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        IProcessInstanceQuery VariableValueEquals(string name, object value);

        /// <summary>
        /// Only select process instances which have at least one global variable with the given value. The type of variable is determined based on the value, using types configured in
        /// <seealso cref="ProcessEngineConfiguration#getVariableTypes()"/>. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        IProcessInstanceQuery VariableValueEquals(object value);

        /// <summary>
        /// Only select process instances which have a local string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IProcessInstanceQuery VariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select process instances which have a global variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not
        /// primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        IProcessInstanceQuery VariableValueNotEquals(string name, object value);

        /// <summary>
        /// Only select process instances which have a local string variable which is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IProcessInstanceQuery VariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select process instances which have a variable value greater than the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not
        /// supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IProcessInstanceQuery VariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select process instances which have a global variable value greater than or equal to the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IProcessInstanceQuery VariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select process instances which have a global variable value less than the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not
        /// supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IProcessInstanceQuery VariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select process instances which have a global variable value less than or equal to the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IProcessInstanceQuery VariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select process instances which have a global variable value like the given value. This be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        IProcessInstanceQuery VariableValueLike(string name, string value);

        /// <summary>
        /// Only select process instances which have a global variable value like the given value (case insensitive).
        /// This be used on string variables only. </summary>
        /// <param name="name"> variable name, cannot be null. </param>
        /// <param name="value"> variable value, cannot be null. The string can include the
        /// wildcard character '%' to express like-strategy: 
        /// starts with (string%), ends with (%string) or contains (%string%). </param>
        IProcessInstanceQuery VariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Only select process instances which are suspended, either because the process instance itself is suspended or because the corresponding process definition is suspended
        /// </summary>
        IProcessInstanceQuery Suspended();

        /// <summary>
        /// Only select process instances which are active, which means that neither the process instance nor the corresponding process definition are suspended.
        /// </summary>
        IProcessInstanceQuery Active();

        /// <summary>
        /// Only select process instances with the given name.
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceName(string name);

        /// <summary>
        /// Only select process instances with a name like the given value.
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceNameLike(string nameLike);

        /// <summary>
        /// Only select process instances with a name like the given value, ignoring upper/lower case.
        /// </summary>
        IProcessInstanceQuery SetProcessInstanceNameLikeIgnoreCase(string nameLikeIgnoreCase);

        /// <summary>
        /// Localize process name and description to specified locale.
        /// </summary>
        IProcessInstanceQuery SetLocale(string locale);

        /// <summary>
        /// Instruct localization to fallback to more general locales including the default locale of the JVM if the specified locale is not found. 
        /// </summary>
        IProcessInstanceQuery WithLocalizationFallback();

        /// <summary>
        /// Include process variables in the process query result
        /// </summary>
        IProcessInstanceQuery SetIncludeProcessVariables();

        /// <summary>
        /// Limit process instance variables
        /// </summary>
        IProcessInstanceQuery SetLimitProcessInstanceVariables(int? processInstanceVariablesLimit);

        /// <summary>
        /// Only select process instances that failed due to an exception happening during a job execution.
        /// </summary>
        IProcessInstanceQuery WithJobException();

        /// <summary>
        /// Begin an OR statement. Make sure you invoke the endOr method at the end of your OR statement.
        /// Only one OR statement is allowed, for the second call to this method an exception will be thrown.
        /// </summary>
        IProcessInstanceQuery Or();

        /// <summary>
        /// End an OR statement. Only one OR statement is allowed, for the second call to this method an exception will be thrown.
        /// </summary>
        IProcessInstanceQuery EndOr();

        /// <summary>
        /// Only select process instances started before the given time
        /// </summary>
        IProcessInstanceQuery SetStartedBefore(DateTime beforeTime);

        /// <summary>
        /// Only select process instances started after the given time
        /// </summary>
        IProcessInstanceQuery SetStartedAfter(DateTime afterTime);

        /// <summary>
        /// Only select process instances started by the given user id
        /// </summary>
        IProcessInstanceQuery SetStartedBy(string userId);

        // ordering
        // /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Order by id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). </summary>
        IProcessInstanceQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by process definition key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessInstanceQuery OrderByProcessDefinitionKey();

        /// <summary>
        /// Order by process definition id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessInstanceQuery OrderByProcessDefinitionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IProcessInstanceQuery OrderByTenantId();
    }

}