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
namespace Sys.Workflow.engine.task
{

    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Interface containing shared methods between the <seealso cref="ITaskQuery"/> and the <seealso cref="IHistoricTaskInstanceQuery"/>.
    /// 
    /// 
    /// </summary>
    public interface ITaskInfoQuery<T, V> : IQuery<T, V> where V : ITaskInfo
    {

        T SetTaskIdNotIn(string[] ids);

        /// <summary>
        /// Only select tasks with the given task id (in practice, there will be maximum one of this kind)
        /// </summary>
        T SetTaskId(string taskId);

        /// <summary>
        /// Only select tasks with the given name </summary>
        T SetTaskName(string name);

        /// <summary>
        /// Only select tasks with a name that is in the given list
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T SetTaskNameIn(IList<string> nameList);

        /// <summary>
        /// Only select tasks with a name that is in the given list
        /// 
        /// This method, unlike the <seealso cref="#taskNameIn(List)"/> method will not take in account the upper/lower case: both the input parameters as the column value are lowercased when the query is executed.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T SetTaskNameInIgnoreCase(IList<string> nameList);

        /// <summary>
        /// Only select tasks with a name matching the parameter. The syntax is that of SQL: for example usage: nameLike(%activiti%)
        /// </summary>
        T SetTaskNameLike(string nameLike);

        /// <summary>
        /// Only select tasks with a name matching the parameter. The syntax is that of SQL: for example usage: nameLike(%activiti%)
        /// 
        /// This method, unlike the <seealso cref="#taskNameLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is executed.
        /// </summary>
        T SetTaskNameLikeIgnoreCase(string nameLike);

        /// <summary>
        /// Only select tasks with the given description. </summary>
        T SetTaskDescription(string description);

        /// <summary>
        /// Only select tasks with a description matching the parameter . The syntax is that of SQL: for example usage: descriptionLike(%activiti%)
        /// </summary>
        T SetTaskDescriptionLike(string descriptionLike);

        /// <summary>
        /// Only select tasks with a description matching the parameter . The syntax is that of SQL: for example usage: descriptionLike(%activiti%)
        /// 
        /// This method, unlike the <seealso cref="#taskDescriptionLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T SetTaskDescriptionLikeIgnoreCase(string descriptionLike);

        /// <summary>
        /// Only select tasks with the given priority. </summary>
        T SetTaskPriority(int? priority);

        /// <summary>
        /// Only select tasks with the given priority or higher. </summary>
        T SetTaskMinPriority(int? minPriority);

        /// <summary>
        /// Only select tasks with the given priority or lower. </summary>
        T SetTaskMaxPriority(int? maxPriority);

        /// <summary>
        /// Only select tasks which are assigned to the given user. </summary>
        T SetTaskAssignee(string assignee);

        /// <summary>
        /// Only select tasks which were last assigned to an assignee like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T SetTaskAssigneeLike(string assigneeLike);

        /// <summary>
        /// Only select tasks which were last assigned to an assignee like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#taskAssigneeLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T SetTaskAssigneeLikeIgnoreCase(string assigneeLikeIgnoreCase);

        /// <summary>
        /// Only select tasks with an assignee that is in the given list
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed name list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        T SetTaskAssigneeIds(IList<string> assigneeListIds);

        /// <summary>
        /// Only select tasks for which the given user is the owner. </summary>
        T SetTaskOwner(string owner);

        /// <summary>
        /// Only select tasks which were last assigned to an owner like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T SetTaskOwnerLike(string ownerLike);

        /// <summary>
        /// Only select tasks which were last assigned to an owner like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#taskOwnerLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query is
        /// executed.
        /// </summary>
        T SetTaskOwnerLikeIgnoreCase(string ownerLikeIgnoreCase);


        /// <summary>
        /// Only select tasks for which the given user is a candidate. If identity service is available then also through user's groups. </summary>
        T SetTaskCandidateUser(string candidateUser);

        /// <summary>
        /// Only select tasks for which the given user is a candidate. </summary>
        T SetTaskCandidateUser(string candidateUser, IList<string> usersGroups);

        /// <summary>
        /// Only select tasks for which there exist an <seealso cref="IIdentityLink"/> with the given user, including tasks which have been assigned to the given user (assignee) or owned by the given user (owner).
        /// </summary>
        T SetTaskInvolvedUser(string involvedUser);

        /// <summary>
        /// Only select tasks for users involved in the given groups </summary>
        T SetTaskInvolvedGroupsIn(IList<string> involvedGroups);

        /// <summary>
        /// Only select tasks for which users in the given group are candidates. </summary>
        T SetTaskCandidateGroup(string candidateGroup);

        /// <summary>
        /// Only select tasks for which the 'candidateGroup' is one of the given groups.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When query is executed and <seealso cref="#taskCandidateGroup(String)"/> or <seealso cref="#taskCandidateUser(String)"/> has been executed on the query instance. When passed group list is empty or
        ///           <code>null</code>. </exception>
        T SetTaskCandidateGroupIn(IList<string> candidateGroups);

        /// <summary>
        /// Only select tasks that have the given tenant id.
        /// </summary>
        T SetTaskTenantId(string tenantId);

        /// <summary>
        /// Only select tasks with a tenant id like the given one.
        /// </summary>
        T SetTaskTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select tasks that do not have a tenant id.
        /// </summary>
        T TaskWithoutTenantId();

        /// <summary>
        /// Only select tasks for the given process instance id.
        /// </summary>
        T SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select tasks for the given process ids.
        /// </summary>
        T SetProcessInstanceIdIn(string[] processInstanceIds);

        /// <summary>
        /// Only select tasks foe the given business key </summary>
        T SetProcessInstanceBusinessKey(string processInstanceBusinessKey);

        /// <summary>
        /// Only select tasks with a business key like the given value The syntax is that of SQL: for example usage: processInstanceBusinessKeyLike("%activiti%").
        /// </summary>
        T SetProcessInstanceBusinessKeyLike(string processInstanceBusinessKeyLike);

        /// <summary>
        /// Only select tasks with a business key like the given value The syntax is that of SQL: for example usage: processInstanceBusinessKeyLike("%activiti%").
        /// 
        /// This method, unlike the <seealso cref="#processInstanceBusinessKeyLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the
        /// query is executed.
        /// </summary>
        T SetProcessInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase);

        /// <summary>
        /// Only select tasks for the given execution.
        /// </summary>
        T SetExecutionId(string executionId);

        /// <summary>
        /// Only select tasks for the given execution ids.
        /// </summary>
        /// <param name="executionIds"></param>
        /// <returns></returns>
        T SetExecutionIdIn(string[] executionIds);

        /// <summary>
        /// Only select tasks that are created on the given date.
        /// </summary>
        T SetTaskCreatedOn(DateTime? createTime);

        /// <summary>
        /// Only select tasks that are created before the given date.
        /// </summary>
        T SetTaskCreatedBefore(DateTime? before);

        /// <summary>
        /// Only select tasks that are created after the given date.
        /// </summary>
        T SetTaskCreatedAfter(DateTime? after);

        /// <summary>
        /// Only select tasks with the given category.
        /// </summary>
        T SetTaskCategory(string category);

        /// <summary>
        /// Only select tasks with the given taskDefinitionKey. The task definition key is the id of the userTask: &lt;userTask id="xxx" .../&gt;
        /// 
        /// </summary>
        T SetTaskDefinitionKey(string key);

        /// <summary>
        /// Only select tasks with a taskDefinitionKey that match the given parameter. The syntax is that of SQL: for example usage: taskDefinitionKeyLike("%activiti%"). The task definition key is the id of
        /// the userTask: &lt;userTask id="xxx" .../&gt;
        /// 
        /// </summary>
        T SetTaskDefinitionKeyLike(string keyLike);

        /// <summary>
        /// Only select tasks with the given due date.
        /// </summary>
        T SetTaskDueDate(DateTime? dueDate);

        /// <summary>
        /// Only select tasks which have a due date before the given date.
        /// </summary>
        T SetTaskDueBefore(DateTime? dueDate);

        /// <summary>
        /// Only select tasks which have a due date after the given date.
        /// </summary>
        T SetTaskDueAfter(DateTime? dueDate);

        /// <summary>
        /// Only select tasks with no due date.
        /// </summary>
        T SetWithoutTaskDueDate();

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition key.
        /// </summary>
        T SetProcessDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition key like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T SetProcessDefinitionKeyLike(string processDefinitionKeyLike);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition key like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// 
        /// This method, unlike the <seealso cref="#processDefinitionKeyLike(String)"/> method will not take in account the upper/lower case: both the input parameter as the column value are lowercased when the query
        /// is executed.
        /// </summary>
        T SetProcessDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase);

        /// <summary>
        /// Only select tasks that have a process definition for which the key is present in the given list * </summary>
        T SetProcessDefinitionKeyIn(IList<string> processDefinitionKeys);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition id.
        /// </summary>
        T SetProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given process definition name.
        /// </summary>
        T SetProcessDefinitionName(string processDefinitionName);

        /// <summary>
        /// Only select tasks which are part of a process instance which has a process definition name like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        T SetProcessDefinitionNameLike(string processDefinitionNameLike);

        /// <summary>
        /// Only select tasks which are part of a process instance whose definition belongs to the category which is present in the given list.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed category list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        /// <param name="processCategoryInList"> </param>
        T SetProcessCategoryIn(IList<string> processCategoryInList);

        /// <summary>
        /// Only select tasks which are part of a process instance whose definition does not belong to the category which is present in the given list.
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           When passed category list is empty or <code>null</code> or contains <code>null String</code>. </exception>
        /// <param name="processCategoryNotInList"> </param>
        T SetProcessCategoryNotIn(IList<string> processCategoryNotInList);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given deployment id.
        /// </summary>
        T SetDeploymentId(string deploymentId);

        /// <summary>
        /// Only select tasks which are part of a process instance which has the given deployment id.
        /// </summary>
        T SetDeploymentIdIn(IList<string> deploymentIds);

        /// <summary>
        /// Only select tasks which have a local task variable with the given name set to the given value.
        /// </summary>
        T SetTaskVariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which have at least one local task variable with the given value.
        /// </summary>
        T SetTaskVariableValueEquals(object variableValue);

        /// <summary>
        /// Only select tasks which have a local string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T SetTaskVariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a local task variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive
        /// type wrappers) are not supported.
        /// </summary>
        T SetTaskVariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which have a local string variable with is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T SetTaskVariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a local variable value greater than the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetTaskVariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value greater than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetTaskVariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value less than the passed value when the ended.Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetTaskVariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value less than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetTaskVariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a local variable value like the given value when they ended. This can be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        T SetTaskVariableValueLike(string name, string value);

        /// <summary>
        /// Only select tasks which have a local variable value like the given value (case insensitive)
        /// when they ended. This can be used on string variables only. </summary>
        /// <param name="name"> cannot be null. </param>
        /// <param name="value"> cannot be null. The string can include the
        ///          wildcard character '%' to express like-strategy: starts with
        ///          (string%), ends with (%string) or contains (%string%).  </param>
        T SetTaskVariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which are part of a process that has a variable with the given name set to the given value.
        /// </summary>
        T SetProcessVariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has at least one variable with the given value.
        /// </summary>
        T SetProcessVariableValueEquals(object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has a local string variable which is not the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T SetProcessVariableValueEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a variable with the given name, but with a different value than the passed value. Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        T SetProcessVariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// Only select tasks which are part of a process that has a string variable with the given value, case insensitive.
        /// <para>
        /// This method only works if your database has encoding/collation that supports case-sensitive queries. For example, use "collate UTF-8" on MySQL and for MSSQL, select one of the case-sensitive
        /// Collations available (<a href="http://msdn.microsoft.com/en-us/library/ms144250(v=sql.105).aspx" >MSDN Server Collation Reference</a>).
        /// </para>
        /// </summary>
        T SetProcessVariableValueNotEqualsIgnoreCase(string name, string value);

        /// <summary>
        /// Only select tasks which have a global variable value greater than the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers)
        /// are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetProcessVariableValueGreaterThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value greater than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive
        /// type wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetProcessVariableValueGreaterThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value less than the passed value when the ended.Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type wrappers) are
        /// not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetProcessVariableValueLessThan(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value less than or equal to the passed value when they ended. Booleans, Byte-arrays and <seealso cref="Serializable"/> objects (which are not primitive type
        /// wrappers) are not supported.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. </param>
        T SetProcessVariableValueLessThanOrEqual(string name, object value);

        /// <summary>
        /// Only select tasks which have a global variable value like the given value when they ended. This can be used on string variables only.
        /// </summary>
        /// <param name="name">
        ///          cannot be null. </param>
        /// <param name="value">
        ///          cannot be null. The string can include the wildcard character '%' to express like-strategy: starts with (string%), ends with (%string) or contains (%string%). </param>
        T SetProcessVariableValueLike(string name, string value);

        /// <summary>
        /// Only select tasks which have a global variable value like the given value (case insensitive)
        /// when they ended. This can be used on string variables only. </summary>
        /// <param name="name"> cannot be null. </param>
        /// <param name="value"> cannot be null. The string can include the
        ///          wildcard character '%' to express like-strategy: starts with
        ///          (string%), ends with (%string) or contains (%string%).  </param>
        T SetProcessVariableValueLikeIgnoreCase(string name, string value);

        /// <summary>
        /// Include local task variables in the task query result
        /// </summary>
        T SetIncludeTaskLocalVariables();

        /// <summary>
        /// Include global task variables in the task query result
        /// </summary>
        T SetIncludeProcessVariables();

        /// <summary>
        /// Limit task variables
        /// </summary>
        T SetLimitTaskVariables(int? taskVariablesLimit);

        /// <summary>
        /// Localize task name and description to specified locale.
        /// </summary>
        T SetLocale(string locale);

        /// <summary>
        /// Instruct localization to fallback to more general locales including the default locale of the JVM if the specified locale is not found. 
        /// </summary>
        T WithLocalizationFallback();

        /// <summary>
        /// All query clauses called will be added to a single or-statement. This or-statement will be included with the other already existing clauses in the query, joined by an 'and'.
        /// 
        /// Calling endOr() will add all clauses to the regular query again. Calling or() after endOr() has been called will result in an exception.
        /// </summary>
        T Or();

        T EndOr();

        // ORDERING

        /// <summary>
        /// Order by task id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskId();

        /// <summary>
        /// Order by task name (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskName();

        /// <summary>
        /// Order by description (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskDescription();

        /// <summary>
        /// Order by priority (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskPriority();

        /// <summary>
        /// Order by assignee (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskAssignee();

        /// <summary>
        /// Order by the time on which the tasks were created (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskCreateTime();

        /// <summary>
        /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByProcessInstanceId();

        /// <summary>
        /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByExecutionId();

        /// <summary>
        /// Order by process definition id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByProcessDefinitionId();

        /// <summary>
        /// Order by task due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskDueDate();

        /// <summary>
        /// Order by task owner (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskOwner();

        /// <summary>
        /// Order by task definition key (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTaskDefinitionKey();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        T OrderByTenantId();

        /// <summary>
        /// Order by due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). If any of the tasks have null for the due date, these will be first in the result.
        /// </summary>
        T OrderByDueDateNullsFirst();

        /// <summary>
        /// Order by due date (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). If any of the tasks have null for the due date, these will be last in the result.
        /// </summary>
        T OrderByDueDateNullsLast();
    }
}