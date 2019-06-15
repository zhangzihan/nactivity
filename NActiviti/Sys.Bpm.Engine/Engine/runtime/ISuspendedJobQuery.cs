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
	public interface ISuspendedJobQuery : IQuery<ISuspendedJobQuery, IJob>
	{

	  /// <summary>
	  /// Only select jobs with the given id </summary>
	  ISuspendedJobQuery SetJobId(string jobId);

	  /// <summary>
	  /// Only select jobs which exist for the given process instance. * </summary>
	  ISuspendedJobQuery SetProcessInstanceId(string processInstanceId);

	  /// <summary>
	  /// Only select jobs which exist for the given execution </summary>
	  ISuspendedJobQuery SetExecutionId(string executionId);

	  /// <summary>
	  /// Only select jobs which exist for the given process definition id </summary>
	  ISuspendedJobQuery SetProcessDefinitionId(string processDefinitionid);

	  /// <summary>
	  /// Only select jobs which have retries left </summary>
	  ISuspendedJobQuery SetWithRetriesLeft();

	  /// <summary>
	  /// Only select jobs which have no retries left </summary>
	  ISuspendedJobQuery SetNoRetriesLeft();

	  /// <summary>
	  /// Only select jobs which are executable, ie. retries &gt; 0 and duedate is null or duedate is in the past
	  /// 
	  /// </summary>
	  ISuspendedJobQuery SetExecutable();

	  /// <summary>
	  /// Only select jobs that are timers. Cannot be used together with <seealso cref="#messages()"/>
	  /// </summary>
	  ISuspendedJobQuery SetTimers();

	  /// <summary>
	  /// Only select jobs that are messages. Cannot be used together with <seealso cref="#timers()"/>
	  /// </summary>
	  ISuspendedJobQuery SetMessages();

	  /// <summary>
	  /// Only select jobs where the duedate is lower than the given date. </summary>
	  ISuspendedJobQuery SetDuedateLowerThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs where the duedate is higher then the given date. </summary>
	  ISuspendedJobQuery SetDuedateHigherThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs that failed due to an exception. </summary>
	  ISuspendedJobQuery SetWithException();

	  /// <summary>
	  /// Only select jobs that failed due to an exception with the given message. </summary>
	  ISuspendedJobQuery SetExceptionMessage(string exceptionMessage);

	  /// <summary>
	  /// Only select jobs that have the given tenant id.
	  /// </summary>
	  ISuspendedJobQuery SetJobTenantId(string tenantId);

	  /// <summary>
	  /// Only select jobs with a tenant id like the given one.
	  /// </summary>
	  ISuspendedJobQuery SetJobTenantIdLike(string tenantIdLike);

	  /// <summary>
	  /// Only select jobs that do not have a tenant id.
	  /// </summary>
	  ISuspendedJobQuery SetJobWithoutTenantId();

	  // sorting //////////////////////////////////////////

	  /// <summary>
	  /// Order by job id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByJobId();

	  /// <summary>
	  /// Order by duedate (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByJobDuedate();

	  /// <summary>
	  /// Order by retries (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByJobRetries();

	  /// <summary>
	  /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByProcessInstanceId();

	  /// <summary>
	  /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByExecutionId();

	  /// <summary>
	  /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  ISuspendedJobQuery OrderByTenantId();

	}

}