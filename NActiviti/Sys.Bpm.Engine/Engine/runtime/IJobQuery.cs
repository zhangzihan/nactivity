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
	public interface IJobQuery : IQuery<IJobQuery, IJob>
	{

	  /// <summary>
	  /// Only select jobs with the given id </summary>
	  IJobQuery jobId(string jobId);

	  /// <summary>
	  /// Only select jobs which exist for the given process instance. * </summary>
	  IJobQuery processInstanceId(string processInstanceId);

	  /// <summary>
	  /// Only select jobs which exist for the given execution </summary>
	  IJobQuery executionId(string executionId);

	  /// <summary>
	  /// Only select jobs which exist for the given process definition id </summary>
	  IJobQuery processDefinitionId(string processDefinitionid);

	  /// <summary>
	  /// Only select jobs that are timers. Cannot be used together with <seealso cref="#messages()"/>
	  /// </summary>
	  IJobQuery timers();

	  /// <summary>
	  /// Only select jobs that are messages. Cannot be used together with <seealso cref="#timers()"/>
	  /// </summary>
	  IJobQuery messages();

	  /// <summary>
	  /// Only select jobs where the duedate is lower than the given date. </summary>
	  IJobQuery duedateLowerThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs where the duedate is higher then the given date. </summary>
	  IJobQuery duedateHigherThan(DateTime? date);

	  /// <summary>
	  /// Only select jobs that failed due to an exception. </summary>
	  IJobQuery withException();

	  /// <summary>
	  /// Only select jobs that failed due to an exception with the given message. </summary>
	  IJobQuery exceptionMessage(string exceptionMessage);

	  /// <summary>
	  /// Only select jobs that have the given tenant id.
	  /// </summary>
	  IJobQuery jobTenantId(string tenantId);

	  /// <summary>
	  /// Only select jobs with a tenant id like the given one.
	  /// </summary>
	  IJobQuery jobTenantIdLike(string tenantIdLike);

	  /// <summary>
	  /// Only select jobs that do not have a tenant id.
	  /// </summary>
	  IJobQuery jobWithoutTenantId();

	  /// <summary>
	  /// Only return jobs that are locked (i.e. they are acquired by an executor).
	  /// </summary>
	  IJobQuery locked();

	  /// <summary>
	  /// Only return jobs that are not locked.
	  /// </summary>
	  IJobQuery unlocked();

	  // sorting //////////////////////////////////////////

	  /// <summary>
	  /// Order by job id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByJobId();

	  /// <summary>
	  /// Order by duedate (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByJobDuedate();

	  /// <summary>
	  /// Order by retries (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByJobRetries();

	  /// <summary>
	  /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByProcessInstanceId();

	  /// <summary>
	  /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByExecutionId();

	  /// <summary>
	  /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
	  /// </summary>
	  IJobQuery orderByTenantId();

	}

}