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
namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.impl.asyncexecutor;
    using Sys.Workflow.engine.impl.cmd;
    using Sys.Workflow.engine.runtime;

    /// <summary>
    /// <seealso cref="EntityManager"/> responsible for the <seealso cref="IJobEntity"/> class.
    /// 
    /// 
    /// </summary>
    public interface IJobEntityManager : IEntityManager<IJobEntity>
    {

        /// <summary>
        /// Insert the <seealso cref="IJobEntity"/>, similar to <seealso cref="#insert(JobEntity)"/>,
        /// but returns a boolean in case the insert did not go through.
        /// This could happen if the execution related to the <seealso cref="IJobEntity"/> has been removed. 
        /// </summary>
        bool InsertJobEntity(IJobEntity timerJobEntity);

        /// <summary>
        /// Returns <seealso cref="IJobEntity"/> that are eligble to be executed.
        /// 
        /// For example used by the default <seealso cref="AcquireJobsCmd"/> command used by 
        /// the default <seealso cref="AcquireTimerJobsRunnable"/> implementation to get async jobs 
        /// that can be executed.
        /// </summary>
        IList<IJobEntity> FindJobsToExecute(Page page);

        /// <summary>
        /// Returns all <seealso cref="IJobEntity"/> instances related to on <seealso cref="IExecutionEntity"/>. 
        /// </summary>
        IList<IJobEntity> FindJobsByExecutionId(string executionId);

        /// <summary>
        /// Returns all <seealso cref="IJobEntity"/> instances related to on <seealso cref="IProcessDefinitionEntity"/>.
        /// </summary>
        IList<IJobEntity> FindJobsByProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Returns all <seealso cref="IJobEntity"/> instances related to on <seealso cref="IProcessDefinitionEntity"/>.
        /// </summary>
        IList<IJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobTypeTimer, string id);

        /// <summary>
        /// Returns all <seealso cref="IJobEntity"/> instances related to one process instance <seealso cref="IExecutionEntity"/>. 
        /// </summary>
        IList<IJobEntity> FindJobsByProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Returns all <seealso cref="IJobEntity"/> instance which are expired, which means 
        /// that the lock time of the <seealso cref="IJobEntity"/> is past a certain configurable
        /// date and is deemed to be in error. 
        /// </summary>
        IList<IJobEntity> FindExpiredJobs(Page page);

        /// <summary>
        /// Executes a <seealso cref="IJobQuery"/> and returns the matching <seealso cref="IJobEntity"/> instances.
        /// </summary>
        IList<IJob> FindJobsByQueryCriteria(IJobQuery jobQuery, Page page);

        /// <summary>
        /// Same as <seealso cref="#FindJobsByQueryCriteria(IJobQuery, Page)"/>, but only returns a count 
        /// and not the instances itself.
        /// </summary>
        long FindJobCountByQueryCriteria(IJobQuery jobQuery);

        /// <summary>
        /// Resets an expired job. These are jobs that were locked, but not completed.
        /// Resetting these will make them available for being picked up by other executors.
        /// </summary>
        void ResetExpiredJob(string jobId);

        /// <summary>
        /// Changes the tenantId for all jobs related to a given <seealso cref="IDeploymentEntity"/>. 
        /// </summary>
        void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId);
    }
}