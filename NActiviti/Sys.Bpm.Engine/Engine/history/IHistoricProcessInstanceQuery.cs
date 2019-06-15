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

namespace org.activiti.engine.history
{

    using org.activiti.engine.query;
    using org.activiti.engine.runtime;

    /// <summary>
    /// Allows programmatic querying of <seealso cref="IHistoricProcessInstance"/>s.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricProcessInstanceQuery : IQuery<IHistoricProcessInstanceQuery, IHistoricProcessInstance>
    {

        /// <summary>
        /// Only select historic process instances with the given process instance. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic process instances whose id is in the given set of ids. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceIds(string[] processInstanceIds);

        /// <summary>
        /// Only select historic process instances for the given process definition </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select historic process instances that are defined by a process definition with the given key.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Only select historic process instances that are defined by a process definition with one of the given process definition keys.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionKeyIn(string[] processDefinitionKeys);

        /// <summary>
        /// Only select historic process instances that don't have a process-definition of which the key is present in the given list
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionKeyNotIn(string[] processDefinitionKeys);

        /// <summary>
        /// Only select historic process instances whose process definition category is processDefinitionCategory. </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionCategory(string processDefinitionCategory);

        /// <summary>
        /// Select process historic instances whose process definition name is processDefinitionName </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select historic process instances with a certain process definition version.
        /// Particulary useful when used in combination with <seealso cref="#processDefinitionKey(String)"/>
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessDefinitionVersion(int? processDefinitionVersion);

        /// <summary>
        /// Only select historic process instances with the given business key </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey);

        /// <summary>
        /// Only select historic process instances that are defined by a process definition with the given deployment identifier.
        /// </summary>
        IHistoricProcessInstanceQuery SetDeploymentId(string deploymentId);

        /// <summary>
        /// Only select historic process instances that are defined by a process definition with one of the given deployment identifiers.
        /// </summary>
        IHistoricProcessInstanceQuery SetDeploymentIdIn(string[] deploymentIds);

        /// <summary>
        /// Only select historic process instances that are completely finished. </summary>
        IHistoricProcessInstanceQuery SetFinished();

        /// <summary>
        /// Only select historic process instance that are not yet finished. </summary>
        IHistoricProcessInstanceQuery SetUnfinished();

        /// <summary>
        /// Only select historic process instances that are deleted. </summary>
        IHistoricProcessInstanceQuery SetDeleted();

        /// <summary>
        /// Only select historic process instance that are not deleted. </summary>
        IHistoricProcessInstanceQuery SetNotDeleted();

        /// <summary>
        /// Only select the historic process instances with which the user with the given id is involved.
        /// </summary>
        IHistoricProcessInstanceQuery SetInvolvedUser(string userId);

        IHistoricProcessInstanceQuery SetInvolvedGroups(string[] involvedGroups);

        /// <summary>
        /// Only select process instances which had a global variable with the given value when they ended. The type only applies to already ended process instances, otherwise use a
        /// <seealso cref="IProcessInstanceQuery"/> instead! of variable is determined based on the value, using types configured in <seealso cref="ProcessEngineConfiguration#GetVariableTypes()"/>. Byte-arrays and
        /// <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          of the variable, cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueEquals(string name, object value);

        /// <summary>
        /// Only select process instances which had at least one global variable with the given value when they ended. The type only applies to already ended process instances, otherwise use a
        /// <seealso cref="IProcessInstanceQuery"/> instead! of variable is determined based on the value, using types configured in <seealso cref="ProcessEngineConfiguration#GetVariableTypes()"/>. Byte-arrays and
        /// <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        IHistoricProcessInstanceQuery VariableValueEquals(object value);

        /// <summary>
        /// Only select historic process instances which have a local string variable with the given value, case insensitive.
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select process instances which had a global variable with the given name, but with a different value than the passed value when they ended. Only select process instances which have a
        /// variable value greater than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          of the variable, cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueNotEquals(string name, object value);

        /// <summary>
        /// Only select process instances which had a global variable value greater than the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported. Only select process instances which have a variable value greater than the passed value.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select process instances which had a global variable value greater than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not
        /// primitive type wrappers) are not supported. Only applies to already ended process instances, otherwise use a <seealso cref="IHistoricProcessInstanceQuery"/> instead!
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select process instances which had a global variable value less than the passed value when the ended. Only applies to already ended process instances, otherwise use a
        /// <seealso cref="IHistoricProcessInstanceQuery"/> instead! Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select process instances which has a global variable value less than or equal to the passed value when they ended. Only applies to already ended process instances, otherwise use a
        /// <seealso cref="IProcessInstanceQuery"/> instead! Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        IHistoricProcessInstanceQuery VariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select process instances which had global variable value like the given value when they ended. Only applies to already ended process instances, otherwise use a <seealso cref="IProcessInstanceQuery"/>
        /// instead! This can be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        IHistoricProcessInstanceQuery VariableValueLike(string name, string value);

        /// <summary>
        /// Only select process instances which had global variable value like (case insensitive)
        /// the given value when they ended. Only applies to already ended process instances,
        /// otherwise use a <seealso cref="IHistoricProcessInstanceQuery"/> instead! This can be used on string
        /// variables only. </summary>
        /// <param name="name"> cannot be null. </param>
        /// <param name="value"> cannot be null. The string can include the
        ///          wildcard character '%' to express like-strategy: starts with
        ///          (string%), ends with (%string) or contains (%string%).  </param>
        IHistoricProcessInstanceQuery VariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Only select historic process instances that were started before the given date.
        /// </summary>
        IHistoricProcessInstanceQuery SetStartedBefore(DateTime? date);

        /// <summary>
        /// Only select historic process instances that were started after the given date.
        /// </summary>
        IHistoricProcessInstanceQuery SetStartedAfter(DateTime? date);

        /// <summary>
        /// Only select historic process instances that were started before the given date.
        /// </summary>
        IHistoricProcessInstanceQuery SetFinishedBefore(DateTime? date);

        /// <summary>
        /// Only select historic process instances that were started after the given date.
        /// </summary>
        IHistoricProcessInstanceQuery SetFinishedAfter(DateTime? date);

        /// <summary>
        /// Only select historic process instance that are started by the given user.
        /// </summary>
        IHistoricProcessInstanceQuery SetStartedBy(string userId);

        /// <summary>
        /// Only select process instances that have the given tenant id. </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceTenantId(string tenantId);

        /// <summary>
        /// Only select process instances with a tenant id like the given one. </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select process instances that do not have a tenant id. </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceWithoutTenantId();

        /// <summary>
        /// Begin an OR statement. Make sure you invoke the endOr method at the end of your OR statement. Only one OR statement is allowed, for the second call to this method an exception will be thrown.
        /// </summary>
        IHistoricProcessInstanceQuery Or();

        /// <summary>
        /// End an OR statement. Only one OR statement is allowed, for the second call to this method an exception will be thrown.
        /// </summary>
        IHistoricProcessInstanceQuery EndOr();

        /// <summary>
        /// Order by the process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by the process definition id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessDefinitionId();

        /// <summary>
        /// Order by the business key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessInstanceBusinessKey();

        /// <summary>
        /// Order by the start time (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessInstanceStartTime();

        /// <summary>
        /// Order by the end time (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessInstanceEndTime();

        /// <summary>
        /// Order by the duration of the process instance (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByProcessInstanceDuration();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricProcessInstanceQuery OrderByTenantId();

        /// <summary>
        /// Only select historic process instances started by the given process instance. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricProcessInstanceQuery SetSuperProcessInstanceId(string superProcessInstanceId);

        /// <summary>
        /// Exclude sub processes from the query result;
        /// </summary>
        IHistoricProcessInstanceQuery SetExcludeSubprocesses(bool excludeSubprocesses);

        /// <summary>
        /// Include process variables in the process query result
        /// </summary>
        IHistoricProcessInstanceQuery SeIncludeProcessVariables();

        /// <summary>
        /// Limit process instance variables
        /// </summary>
        IHistoricProcessInstanceQuery SetLimitProcessInstanceVariables(int? processInstanceVariablesLimit);

        /// <summary>
        /// Only select process instances that failed due to an exception happening during a job execution.
        /// </summary>
        IHistoricProcessInstanceQuery SetWithJobException();

        /// <summary>
        /// Only select process instances with the given name.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceName(string name);

        /// <summary>
        /// Only select process instances with a name like the given value.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceNameLike(string nameLike);

        /// <summary>
        /// Only select process instances with a name like the given value, ignoring upper/lower case.
        /// </summary>
        IHistoricProcessInstanceQuery SetProcessInstanceNameLikeIgnoreCase(string nameLikeIgnoreCase);

        /// <summary>
        /// Localize historic process name and description to specified locale.
        /// </summary>
        IHistoricProcessInstanceQuery Locale(string locale);

        /// <summary>
        /// Instruct localization to fallback to more general locales including the default locale of the JVM if the specified locale is not found. 
        /// </summary>
        IHistoricProcessInstanceQuery WithLocalizationFallback();
    }

}