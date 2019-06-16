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
    /// Allows programmatic querying of <seealso cref="IExecution"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IExecutionQuery : IQuery<IExecutionQuery, IExecution>
    {

        /// <summary>
        /// Only select executions which have the given process definition key. * </summary>
        IExecutionQuery SetProcessDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Only select executions which have process definitions with the given keys. * </summary>
        IExecutionQuery SetProcessDefinitionKeys(string[] processDefinitionKeys);

        /// <summary>
        /// Only select executions which have the given process definition id. * </summary>
        IExecutionQuery SetProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select executions which have the given process deployment id. *
        /// </summary>
        /// <param name="deploymentId"></param>
        /// <returns></returns>
        IExecutionQuery SetProcessDeploymentId(string deploymentId);

        /// <summary>
        /// Only select executions which have the given process definition category. </summary>
        IExecutionQuery SetProcessDefinitionCategory(string processDefinitionCategory);

        /// <summary>
        /// Only select executions which have the given process definition name. </summary>
        IExecutionQuery SetProcessDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select executions which have the given process definition version.
        /// Particulary useful when used in combination with <seealso cref="#processDefinitionKey(String)"/>
        /// </summary>
        IExecutionQuery SetProcessDefinitionVersion(int? processDefinitionVersion);

        /// <summary>
        /// Only select executions which have the given process instance id. * </summary>
        IExecutionQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select executions which have the given root process instance id. * </summary>
        IExecutionQuery SetRootProcessInstanceId(string rootProcessInstanceId);

        /// <summary>
        /// Only executions with the given business key.
        /// 
        /// Note that only process instances have a business key and as such, child executions will NOT be returned. If you want to return child executions of the process instance with the given business key
        /// too, use the <seealso cref="#processInstanceBusinessKey(String, boolean)"/> method with a boolean value of <i>true</i> instead.
        /// </summary>
        IExecutionQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey);

        /// <summary>
        /// Only executions with the given business key. Similar to <seealso cref="#processInstanceBusinessKey(String)"/>, but allows to choose whether child executions are returned or not.
        /// </summary>
        IExecutionQuery ProcessInstanceBusinessKey(string processInstanceBusinessKey, bool includeChildExecutions);

        /// <summary>
        /// Only select executions with the given id. * </summary>
        IExecutionQuery SetExecutionId(string executionId);

        /// <summary>
        /// Only select executions which contain an activity with the given id. * </summary>
        IExecutionQuery SetActivityId(string activityId);

        /// <summary>
        /// Only select executions which are a direct child-execution of the execution with the given id.
        /// 
        /// </summary>
        IExecutionQuery SetParentId(string parentId);

        /// <summary>
        /// Only selects executions that have a parent id set, ie non-processinstance executions.
        /// </summary>
        IExecutionQuery SetOnlyChildExecutions();

        /// <summary>
        /// Only selects executions that are a subprocess.
        /// </summary>
        IExecutionQuery SetOnlySubProcessExecutions();

        /// <summary>
        /// Only selects executions that have no parent id set, ie process instance executions
        /// </summary>
        IExecutionQuery SetOnlyProcessInstanceExecutions();

        /// <summary>
        /// Only select process instances that have the given tenant id.
        /// </summary>
        IExecutionQuery SetExecutionTenantId(string tenantId);

        /// <summary>
        /// Only select process instances with a tenant id like the given one.
        /// </summary>
        IExecutionQuery SetExecutionTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select process instances that do not have a tenant id.
        /// </summary>
        IExecutionQuery SetExecutionWithoutTenantId();

        /// <summary>
        /// Only select executions which have a local variable with the given value. The type of variable is determined based on the value, using types configured in
        /// <seealso cref="ProcessEngineConfiguration#getVariableTypes()"/>. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        IExecutionQuery VariableValueEquals(string name, object value);

        /// <summary>
        /// Only select executions which have a local string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IExecutionQuery VariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select executions which have at least one local variable with the given value. The type of variable is determined based on the value, using types configured in
        /// <seealso cref="ProcessEngineConfiguration#getVariableTypes()"/> . Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        IExecutionQuery VariableValueEquals(object value);

        /// <summary>
        /// Only select executions which have a local variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive
        /// type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        IExecutionQuery VariableValueNotEquals(string name, object value);

        /// <summary>
        /// Only select executions which have a local string variable which is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IExecutionQuery VariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select executions which have a local variable value greater than the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not
        /// supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IExecutionQuery VariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select executions which have a local variable value greater than or equal to the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IExecutionQuery VariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select executions which have a local variable value less than the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not
        /// supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IExecutionQuery VariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select executions which have a local variable value less than or equal to the passed value. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. </param>
        IExecutionQuery VariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select executions which have a local variable value like the given value. This be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          variable name, cannot be null. </param>
        /// <param name="value">
        ///          variable value, cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        IExecutionQuery VariableValueLike(string name, string value);

        /// <summary>
        /// Only select executions which have a local variable value like the given value (case insensitive).
        /// This be used on string variables only. </summary>
        /// <param name="name"> variable name, cannot be null. </param>
        /// <param name="value"> variable value, cannot be null. The string can include the
        /// wildcard character '%' to express like-strategy: 
        /// starts with (string%), ends with (%string) or contains (%string%). </param>
        IExecutionQuery VariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Only select executions which are part of a process that have a variable with the given name set to the given value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        IExecutionQuery ProcessVariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select executions which are part of a process that have at least one variable with the given value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        IExecutionQuery ProcessVariableValueEquals(object variableValue);

        /// <summary>
        /// Only select executions which are part of a process that have a variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which
        /// are not primitive type wrappers) are not supported.
        /// </summary>
        IExecutionQuery ProcessVariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select executions which are part of a process that have a local string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IExecutionQuery ProcessVariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select executions which are part of a process that have a local string variable which is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        /// <param name="name">
        ///          name of the variable, cannot be null. </param>
        /// <param name="value">
        ///          value of the variable, cannot be null. </param>
        IExecutionQuery ProcessVariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select executions which are part of a process that have at least one variable like the given value.
        /// Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        IExecutionQuery ProcessVariableValueLike(string name, string value);

        /// <summary>
        /// Only select executions which are part of a process that have at least one variable like the given value (case insensitive).
        /// Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are not supported.
        /// </summary>
        IExecutionQuery ProcessVariableValueLikeIgnoreCase(string name, string value);

        // event subscriptions //////////////////////////////////////////////////

        /// <summary>
        /// Only select executions which have a signal event subscription for the given signal name.
        /// 
        /// (The signalName is specified using the 'name' attribute of the signal element in the BPMN 2.0 XML.)
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal the execution has subscribed to </param>
        IExecutionQuery SignalEventSubscriptionName(string signalName);

        /// <summary>
        /// Only select executions which have a message event subscription for the given messageName.
        /// 
        /// (The messageName is specified using the 'name' attribute of the message element in the BPMN 2.0 XML.)
        /// </summary>
        /// <param name="messageName">
        ///          the name of the message the execution has subscribed to </param>
        IExecutionQuery MessageEventSubscriptionName(string messageName);

        /// <summary>
        /// Localize execution name and description to specified locale.
        /// </summary>
        IExecutionQuery Locale(string locale);

        /// <summary>
        /// Instruct localization to fallback to more general locales including the default locale of the JVM if the specified locale is not found. 
        /// </summary>
        IExecutionQuery WithLocalizationFallback();


        /// <summary>
        /// Only select executions that were started before the given start time.
        /// </summary>
        /// <param name="beforeTime">
        ///          executions started before this time will be returned (cannot be null) </param>
        IExecutionQuery SetStartedBefore(DateTime beforeTime);

        /// <summary>
        /// Only select executions that were started after the given start time.
        /// </summary>
        /// <param name="afterTime">
        ///          executions started after this time will be returned (cannot be null) </param>
        IExecutionQuery SetStartedAfter(DateTime afterTime);

        /// <summary>
        /// Only select executions that were started after by the given user id.
        /// </summary>
        /// <param name="userId">
        ///          the user id of the authenticated user that started the execution (cannot be null) </param>
        IExecutionQuery SetStartedBy(string userId);

        // ordering //////////////////////////////////////////////////////////////

        /// <summary>
        /// Order by id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). </summary>
        IExecutionQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by process definition key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IExecutionQuery OrderByProcessDefinitionKey();

        /// <summary>
        /// Order by process definition id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IExecutionQuery OrderByProcessDefinitionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IExecutionQuery OrderByTenantId();
    }

}