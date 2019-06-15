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
namespace org.activiti.engine.impl.asyncexecutor
{
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public interface IAsyncExecutor
    {

        /// <summary>
        /// Starts the Async Executor: jobs will be acquired and executed.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops executing jobs.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Offers the provided <seealso cref="IJobEntity"/> to this <seealso cref="IAsyncExecutor"/> instance
        /// to execute. If the offering does not work for some reason, false 
        /// will be returned (For example when the job queue is full in the <seealso cref="DefaultAsyncJobExecutor"/>). 
        /// </summary>
        bool ExecuteAsyncJob(IJob job);


        /* Getters and Setters */

        /// <summary>
        /// 
        /// </summary>
        ProcessEngineConfigurationImpl ProcessEngineConfiguration { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool AutoActivate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// 
        /// </summary>
        string LockOwner { get; }

        /// <summary>
        /// 
        /// </summary>
        int TimerLockTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int AsyncJobLockTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int DefaultTimerJobAcquireWaitTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int DefaultAsyncJobAcquireWaitTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int DefaultQueueSizeFullWaitTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int MaxAsyncJobsDuePerAcquisition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int MaxTimerJobsPerAcquisition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int RetryWaitTimeInMillis { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int ResetExpiredJobsInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int ResetExpiredJobsPageSize { get; set; }
    }
}