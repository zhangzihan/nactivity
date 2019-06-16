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

namespace Sys.Workflow.engine.runtime
{

    /// <summary>
    /// Represents one job (timer, async job, etc.).
    /// 
    /// 
    /// </summary>
    public interface IJob
    {

        /// <summary>
        /// Returns the unique identifier for this job.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Returns the date on which this job is supposed to be processed.
        /// </summary>
        DateTime? Duedate { get; }

        /// <summary>
        /// Returns the id of the process instance which execution created the job.
        /// </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// Returns the specific execution on which the job was created.
        /// </summary>
        string ExecutionId { get; }

        /// <summary>
        /// Returns the specific process definition on which the job was created
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// Returns the number of retries this job has left. Whenever the jobexecutor fails to execute the job, this value is decremented. When it hits zero, the job is supposed to be dead and not retried
        /// again (ie a manual retry is required then).
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Returns the message of the exception that occurred, the last time the job was executed. Returns null when no exception occurred.
        /// 
        /// To get the full exception stacktrace, use <seealso cref="IManagementService.GetJobExceptionStacktrace(string)"/>
        /// </summary>
        string ExceptionMessage { get; }

        /// <summary>
        /// Get the tenant identifier for this job.
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// Is the job exclusive?
        /// </summary>
        bool Exclusive { get; }

        /// <summary>
        /// Get the job type for this job.
        /// </summary>
        string JobType { get; }

        /// <summary>
        /// Get the job handler type.
        /// </summary>
        string JobHandlerType { get; }

        /// <summary>
        /// Get the job configuration.
        /// </summary>
        string JobHandlerConfiguration { get; }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class JobFields
    {
        /// <summary>
        /// 
        /// </summary>
        public const string JOB_TYPE_TIMER = "timer";

        /// <summary>
        /// 
        /// </summary>
        public const string JOB_TYPE_MESSAGE = "message";

        /// <summary>
        /// 
        /// </summary>
        public const bool DEFAULT_EXCLUSIVE = true;

        /// <summary>
        /// 
        /// </summary>
        public const int MAX_EXCEPTION_MESSAGE_LENGTH = 255;
    }
}