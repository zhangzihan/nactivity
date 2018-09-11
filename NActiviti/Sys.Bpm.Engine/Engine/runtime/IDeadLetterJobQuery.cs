using System;

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

namespace org.activiti.engine.runtime
{

	using org.activiti.engine.query;

	/// <summary>
	/// Allows programmatic querying of <seealso cref="IJob"/>s.
	/// 
	/// 
	/// 
	/// </summary>
	public interface IDeadLetterJobQuery : IQuery<IDeadLetterJobQuery, IJob>
	{

	  /// <summary>
	  /// Only select jobs with the given id </summary>
	  IDeadLetterJobQuery jobId(string jobId);

	  /// <summary>
	  /// Only select jobs which exist for the given process instance. * </summary>
	  IDeadLetterJobQuery processInstanceId(string processInstanceId);

	  /// <summary>
	  /// Only select jobs which exist for the given execution </summary>
	  IDeadLetterJobQuery executionId(string executionId);

	  /// <summary>
	  /// Only select jobs which exist for the given process definition id </summary>
	  IDeadLetterJobQuery processDefinitionId(string processDefinitionid);

	  /// <summary>
	  /// Only select jobs which are executable, ie. duedate is null or duedate is in the past
	  /// 
	  /// </summary>
	  IDeadLetterJobQuery executable();

	  /// <summary>
	  /// Only select jobs that are timers. Cannot be used together with <seealso cref="#messages()"/>
	  /// </summary>
	  IDeadLetterJobQuery timers();

	  /// <summary>
	  /// Only select jobs that are messages. Cannot be used together with <seealso cref="#timers()"/>
	  /// </summary>
	  IDeadLetterJobQuery messages();

	  /// <summary>
	  /// Only select jobs where the duedate is lower than the given date. </summary>
	  IDeadLetterJobQuery duedateLowerThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs where the duedate is higher then the given date. </summary>
	  IDeadLetterJobQuery duedateHigherThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs that failed due to an exception. </summary>
	  IDeadLetterJobQuery withException();

	  /// <summary>
	  /// Only select jobs that failed due to an exception with the given message. </summary>
	  IDeadLetterJobQuery exceptionMessage(string exceptionMessage);

	  /// <summary>
	  /// Only select jobs that have the given tenant id.
	  /// </summary>
	  IDeadLetterJobQuery jobTenantId(string tenantId);

	  /// <summary>
	  /// Only select jobs with a tenant id like the given one.
	  /// </summary>
	  IDeadLetterJobQuery jobTenantIdLike(string tenantIdLike);

	  /// <summary>
	  /// Only select jobs that do not have a tenant id.
	  /// </summary>
	  IDeadLetterJobQuery jobWithoutTenantId();

	  // sorting //////////////////////////////////////////

	  /// <summary>
	  /// Order by job id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByJobId();

	  /// <summary>
	  /// Order by duedate (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByJobDuedate();

	  /// <summary>
	  /// Order by retries (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByJobRetries();

	  /// <summary>
	  /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByProcessInstanceId();

	  /// <summary>
	  /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByExecutionId();

	  /// <summary>
	  /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IDeadLetterJobQuery orderByTenantId();

	}

}