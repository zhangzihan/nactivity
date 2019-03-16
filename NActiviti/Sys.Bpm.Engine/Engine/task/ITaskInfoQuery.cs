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
namespace org.activiti.engine.task
{

    using org.activiti.engine.history;
    using org.activiti.engine.query;

    /// <summary>
    /// Interface containing shared methods between the <seealso cref="ITaskQuery"/> and the <seealso cref="IHistoricTaskInstanceQuery"/>.
    /// 
    /// 
    /// </summary>
    public interface ITaskInfoQuery<T, V> : IQuery<T, V> where V : ITaskInfo
    {

        T taskIdNotIn(string[] ids);

        /// <summary>
        /// Only select tasks with the given task id (in practice, there will be maximum one of this kind)
        /// </summary>
        T taskId(string taskId);

        /// <summary>
        /// Only select tasks with the given name </summary>
        T taskName(string name);

        /// <summary>
        /// Only select tasks with a name that is in the given list
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T taskNameIn(IList<string> nameList);

        /// <summary>
        /// Only select tasks with a name that is in the given list
        /// 
        /// This method, unlike the <seealso cref="#taskNameIn(List)"/> method will not take in account the upper/lower case: both the input parameters as the column value are lowercased when the query is executed.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T taskNameInIgnoreCase(IList<string> nameList);

        /// <summary>
        /// Only select tasks with a name matching the parameter. The syntax is that of SQL: for example usage: nameLike(%activiti%)
        /// </summary>
        T taskNameLike(string nameLike);

        /// <summary>
        /// Only select tasks with a name matching the parameter. The syntax is that of SQL: for example usage: nameLike(%activiti%)
        /// 
        /// This method, unlike the <seealso cref="#taskNameLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is executed.
        /// </summary>
        T taskNameLikeIgnoreCase(string nameLike);

        /// <summary>
        /// Only select tasks with the given description. </summary>
        T taskDescription(string description);

        /// <summary>
        /// Only select tasks with a description matching the parameter . The syntax is that of SQL: for example usage: descriptionLike(%activiti%)
        /// </summary>
        T taskDescriptionLike(string descriptionLike);

        /// <summary>
        /// Only select tasks with a description matching the parameter . The syntax is that of SQL: for example usage: descriptionLike(%activiti%)
        /// 
        /// This method, unlike the <seealso cref="#taskDescriptionLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T taskDescriptionLikeIgnoreCase(string descriptionLike);

        /// <summary>
        /// Only select tasks with the given priority. </summary>
        T taskPriority(int? priority);

        /// <summary>
        /// Only select tasks with the given priority or higher. </summary>
        T taskMinPriority(int? minPriority);

        /// <summary>
        /// Only select tasks with the given priority or lower. </summary>
        T taskMaxPriority(int? maxPriority);

        /// <summary>
        /// Only select tasks which are assigned to the given user. </summary>
        T taskAssignee(string assignee);

        /// <summary>
        /// Only select tasks which were last assigned to an assignee like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T taskAssigneeLike(string assigneeLike);

        /// <summary>
        /// Only select tasks which were last assigned to an assignee like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#taskAssigneeLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T taskAssigneeLikeIgnoreCase(string assigneeLikeIgnoreCase);

        /// <summary>
        /// Only select tasks with an assignee that is in the given list
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T taskAssigneeIds(IList<string> assigneeListIds);

        /// <summary>
        /// Only select tasks for which the given user is the owner. </summary>
        T taskOwner(string owner);

        /// <summary>
        /// Only select tasks which were last assigned to an owner like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T taskOwnerLike(string ownerLike);

        /// <summary>
        /// Only select tasks which were last assigned to an owner like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#taskOwnerLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T taskOwnerLikeIgnoreCase(string ownerLikeIgnoreCase);


        /// <summary>
        /// Only select tasks for which the given user is a candidate. If identity service is available then also through user's groups. </summary>
        T taskCandidateUser(string candidateUser);

        /// <summary>
        /// Only select tasks for which the given user is a candidate. </summary>
        T taskCandidateUser(string candidateUser, IList<string> usersGroups);

        /// <summary>
        /// Only select tasks for which there exist an <seealso cref="IIdentityLink"/> with the given user, including tasks which have been assigned to the given user (assignee) or owned by the given user (owner).
        /// </summary>
        T taskInvolvedUser(string involvedUser);

        /// <summary>
        /// Only select tasks for users involved in the given groups </summary>
        T taskInvolvedGroupsIn(IList<string> involvedGroups);

        /// <summary>
        /// Only select tasks for which users in the given group are candidates. </summary>
        T taskCandidateGroup(string candidateGroup);

        /// <summary>
        /// Only select tasks for which the 'candidateGroup' is one of the given groups.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When query is executed and <seealso cref="#taskCandidateGroup(String)"/> or <seealso cref="#taskCandidateUser(String)"/> has been executed on the query instance. When passed group list is empty or
        ///           <code>null</code>. </exception>
        T taskCandidateGroupIn(IList<string> candidateGroups);

        /// <summary>
        /// Only select tasks that have the given tenant id.
        /// </summary>
        T taskTenantId(string tenantId);

        /// <summary>
        /// Only select tasks with a tenant id like the given one.
        /// </summary>
        T taskTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select tasks that do not have a tenant id.
        /// </summary>
        T taskWithoutTenantId();

        /// <summary>
        /// Only select tasks for the given process instance id.
        /// </summary>
        T processInstanceId(string processInstanceId);

        /// <summary>
        /// Only select tasks for the given process ids.
        /// </summary>
        T processInstanceIdIn(IList<string> processInstanceIds);

        /// <summary>
        /// Only select tasks foe the given business key </summary>
        T processInstanceBusinessKey(string processInstanceBusinessKey);

        /// <summary>
        /// Only select tasks with a business key like the given value The syntax is that of SQL: for example usage: processInstanceBusinessKeyLike("%activiti%").
        /// </summary>
        T processInstanceBusinessKeyLike(string processInstanceBusinessKeyLike);

        /// <summary>
        /// Only select tasks with a business key like the given value The syntax is that of SQL: for example usage: processInstanceBusinessKeyLike("%activiti%").
        /// 
        /// This method, unlike the <seealso cref="#processInstanceBusinessKeyLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the
        /// query is executed.
        /// </summary>
        T processInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase);

        /// <summary>
        /// Only select tasks for the given execution.
        /// </summary>
        T executionId(string executionId);

        /// <summary>
        /// Only select tasks that are created on the given date.
        /// </summary>
        T taskCreatedOn(DateTime? createTime);

        /// <summary>
        /// Only select tasks that are created before the given date.
        /// </summary>
        T taskCreatedBefore(DateTime? before);

        /// <summary>
        /// Only select tasks that are created after the given date.
        /// </summary>
        T taskCreatedAfter(DateTime? after);

        /// <summary>
        /// Only select tasks with the given category.
        /// </summary>
        T taskCategory(string category);

        /// <summary>
        /// Only select tasks with the given taskDefinitionKey. The task definition key is the id of the userTask: &lt;userTask id="xxx" .../&gt;
        /// 
        /// </summary>
        T taskDefinitionKey(string key);

        /// <summary>
        /// Only select tasks with a taskDefinitionKey that match the given parameter. The syntax is that of SQL: for example usage: taskDefinitionKeyLike("%activiti%"). The task definition key is the id of
        /// the userTask: &lt;userTask id="xxx" .../&gt;
        /// 
        /// </summary>
        T taskDefinitionKeyLike(string keyLike);

        /// <summary>
        /// Only select tasks with the given due date.
        /// </summary>
        T taskDueDate(DateTime? dueDate);

        /// <summary>
        /// Only select tasks which have a due date before the given date.
        /// </summary>
        T taskDueBefore(DateTime? dueDate);

        /// <summary>
        /// Only select tasks which have a due date after the given date.
        /// </summary>
        T taskDueAfter(DateTime? dueDate);

        /// <summary>
        /// Only select tasks with no due date.
        /// </summary>
        T withoutTaskDueDate();

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition key.
        /// </summary>
        T processDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition key like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T processDefinitionKeyLike(string processDefinitionKeyLike);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition key like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#processDefinitionKeyLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query
        /// is executed.
        /// </summary>
        T processDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase);

        /// <summary>
        /// Only select tasks that have a process definition for which the key is present in the given list * </summary>
        T processDefinitionKeyIn(IList<string> processDefinitionKeys);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition id.
        /// </summary>
        T processDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition name.
        /// </summary>
        T processDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition name like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T processDefinitionNameLike(string processDefinitionNameLike);

        /// <summary>
        /// Only select tasks which are part of a process instance whose definition belongs to the category which is present in the given list.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed category list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        /// <param name="processCategoryInList"> </param>
        T processCategoryIn(IList<string> processCategoryInList);

        /// <summary>
        /// Only select tasks which are part of a process instance whose definition does not belong to the category which is present in the given list.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed category list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        /// <param name="processCategoryNotInList"> </param>
        T processCategoryNotIn(IList<string> processCategoryNotInList);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given deployment id.
        /// </summary>
        T deploymentId(string deploymentId);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given deployment id.
        /// </summary>
        T deploymentIdIn(IList<string> deploymentIds);

        /// <summary>
        /// Only select tasks which have a local task variable with the given name set to the given value.
        /// </summary>
        T taskVariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which have at least one local task variable with the given value.
        /// </summary>
        T taskVariableValueEquals(object variableValue);

        /// <summary>
        /// Only select tasks which have a local string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T taskVariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a local task variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive
        /// type wrappers) are not supported.
        /// </summary>
        T taskVariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which have a local string variable with is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T taskVariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a local variable value greater than the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T taskVariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value greater than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T taskVariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value less than the passed value when the ended.Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T taskVariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value less than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T taskVariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value like the given value when they ended. This can be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        T taskVariableValueLike(string name, string value);

        /// <summary>
        /// Only select tasks which have a local variable value like the given value (case insensitive)
        /// when they ended. This can be used on string variables only. </summary>
        /// <param name="name"> cannot be null. </param>
        /// <param name="value"> cannot be null. The string can include the
        ///          wildcard character '%' to express like-strategy: starts with
        ///          (string%), ends with (%string) or contains (%string%).  </param>
        T taskVariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which are part of a process that has a variable with the given name set to the given value.
        /// </summary>
        T processVariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has at least one variable with the given value.
        /// </summary>
        T processVariableValueEquals(object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has a local string variable which is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T processVariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        T processVariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has a string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T processVariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a global variable value greater than the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T processVariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value greater than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive
        /// type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T processVariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value less than the passed value when the ended.Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T processVariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value less than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T processVariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value like the given value when they ended. This can be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        T processVariableValueLike(string name, string value);

        /// <summary>
        /// Only select tasks which have a global variable value like the given value (case insensitive)
        /// when they ended. This can be used on string variables only. </summary>
        /// <param name="name"> cannot be null. </param>
        /// <param name="value"> cannot be null. The string can include the
        ///          wildcard character '%' to express like-strategy: starts with
        ///          (string%), ends with (%string) or contains (%string%).  </param>
        T processVariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Include local task variables in the task query result
        /// </summary>
        T includeTaskLocalVariables();

        /// <summary>
        /// Include global task variables in the task query result
        /// </summary>
        T includeProcessVariables();

        /// <summary>
        /// Limit task variables
        /// </summary>
        T limitTaskVariables(int? taskVariablesLimit);

        /// <summary>
        /// Localize task name and description to specified locale.
        /// </summary>
        T locale(string locale);

        /// <summary>
        /// Instruct localization to fallback to more general locales including the default locale of the JVM if the specified locale is not found. 
        /// </summary>
        T withLocalizationFallback();

        /// <summary>
        /// All query clauses called will be added to a single or-statement. This or-statement will be included with the other already existing clauses in the query, joined by an 'and'.
        /// 
        /// Calling endOr() will add all clauses to the regular query again. Calling or() after endOr() has been called will result in an exception.
        /// </summary>
        T or();

        T endOr();

        // ORDERING

        /// <summary>
        /// Order by task id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskId();

        /// <summary>
        /// Order by task name (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskName();

        /// <summary>
        /// Order by description (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskDescription();

        /// <summary>
        /// Order by priority (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskPriority();

        /// <summary>
        /// Order by assignee (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskAssignee();

        /// <summary>
        /// Order by the time on which the tasks were created (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskCreateTime();

        /// <summary>
        /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByProcessInstanceId();

        /// <summary>
        /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByExecutionId();

        /// <summary>
        /// Order by process definition id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByProcessDefinitionId();

        /// <summary>
        /// Order by task due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskDueDate();

        /// <summary>
        /// Order by task owner (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskOwner();

        /// <summary>
        /// Order by task definition key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTaskDefinitionKey();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T orderByTenantId();

        /// <summary>
        /// Order by due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). If any of the tasks have null for the due date, these will be first in the result.
        /// </summary>
        T orderByDueDateNullsFirst();

        /// <summary>
        /// Order by due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). If any of the tasks have null for the due date, these will be last in the result.
        /// </summary>
        T orderByDueDateNullsLast();

    }

}