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

namespace Sys.Workflow.Engine.Runtime
{

    using Sys.Workflow.Engine.Query;

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
        IJobQuery SetJobId(string jobId);

        /// <summary>
        /// Only select jobs which exist for the given process instance. * </summary>
        IJobQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select jobs which exist for the given execution </summary>
        IJobQuery SetExecutionId(string executionId);

        /// <summary>
        /// Only select jobs which exist for the given process definition id </summary>
        IJobQuery SetProcessDefinitionId(string processDefinitionid);

        /// <summary>
        /// Only select jobs that are timers. Cannot be used together with <seealso cref="#messages()"/>
        /// </summary>
        IJobQuery SetTimers();

        /// <summary>
        /// Only select jobs that are messages. Cannot be used together with <seealso cref="#timers()"/>
        /// </summary>
        IJobQuery SetMessages();

        /// <summary>
        /// Only select jobs where the duedate is lower than the given date. </summary>
        IJobQuery SetDuedateLowerThan(DateTime? date);

        /// <summary>
        /// Only select jobs where the duedate is higher then the given date. </summary>
        IJobQuery SetDuedateHigherThan(DateTime? date);

        /// <summary>
        /// Only select jobs that failed due to an exception. </summary>
        IJobQuery SetWithException();

        /// <summary>
        /// Only select jobs that failed due to an exception with the given message. </summary>
        IJobQuery SetExceptionMessage(string exceptionMessage);

        /// <summary>
        /// Only select jobs that have the given tenant id.
        /// </summary>
        IJobQuery SetJobTenantId(string tenantId);

        /// <summary>
        /// Only select jobs with a tenant id like the given one.
        /// </summary>
        IJobQuery SetJobTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select jobs that do not have a tenant id.
        /// </summary>
        IJobQuery SetJobWithoutTenantId();

        /// <summary>
        /// Only return jobs that are locked (i.e. they are acquired by an executor).
        /// </summary>
        IJobQuery SetLocked();

        /// <summary>
        /// Only return jobs that are not locked.
        /// </summary>
        IJobQuery SetUnlocked();

        // sorting //////////////////////////////////////////

        /// <summary>
        /// Order by job id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByJobId();

        /// <summary>
        /// Order by duedate (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByJobDuedate();

        /// <summary>
        /// Order by retries (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByJobRetries();

        /// <summary>
        /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByExecutionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IJobQuery OrderByTenantId();
    }
}