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
    public interface ITimerJobQuery : IQuery<ITimerJobQuery, IJob>
    {

        /// <summary>
        /// Only select jobs with the given id </summary>
        ITimerJobQuery SetJobId(string jobId);

        /// <summary>
        /// Only select jobs which exist for the given process instance. * </summary>
        ITimerJobQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select jobs which exist for the given execution </summary>
        ITimerJobQuery SetExecutionId(string executionId);

        /// <summary>
        /// Only select jobs which exist for the given process definition id </summary>
        ITimerJobQuery SetProcessDefinitionId(string processDefinitionid);

        /// <summary>
        /// Only select jobs which are executable, ie. duedate is null or duedate is in the past
        /// 
        /// </summary>
        ITimerJobQuery SetExecutable();

        /// <summary>
        /// Only select jobs that are timers. Cannot be used together with <seealso cref="#messages()"/>
        /// </summary>
        ITimerJobQuery SetTimers();

        /// <summary>
        /// Only select jobs that are messages. Cannot be used together with <seealso cref="#timers()"/>
        /// </summary>
        ITimerJobQuery SetMessages();

        /// <summary>
        /// Only select jobs where the duedate is lower than the given date. </summary>
        ITimerJobQuery SetDuedateLowerThan(DateTime? date);

        /// <summary>
        /// Only select jobs where the duedate is higher then the given date. </summary>
        ITimerJobQuery SetDuedateHigherThan(DateTime? date);

        /// <summary>
        /// Only select jobs that failed due to an exception. </summary>
        ITimerJobQuery SetWithException();

        /// <summary>
        /// Only select jobs that failed due to an exception with the given message. </summary>
        ITimerJobQuery SetExceptionMessage(string exceptionMessage);

        /// <summary>
        /// Only select jobs that have the given tenant id.
        /// </summary>
        ITimerJobQuery SetJobTenantId(string tenantId);

        /// <summary>
        /// Only select jobs with a tenant id like the given one.
        /// </summary>
        ITimerJobQuery SetJobTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select jobs that do not have a tenant id.
        /// </summary>
        ITimerJobQuery SetJobWithoutTenantId();

        // sorting //////////////////////////////////////////

        /// <summary>
        /// Order by job id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByJobId();

        /// <summary>
        /// Order by duedate (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByJobDuedate();

        /// <summary>
        /// Order by retries (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByJobRetries();

        /// <summary>
        /// Order by process instance id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by execution id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByExecutionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        ITimerJobQuery OrderByTenantId();
    }
}