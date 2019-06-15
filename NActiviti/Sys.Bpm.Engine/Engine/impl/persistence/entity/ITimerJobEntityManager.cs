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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.runtime;

    /// <summary>
    /// <seealso cref="EntityManager"/> responsible for <seealso cref="ITimerJobEntity"/> instances.
    /// 
    /// 
    /// 
    /// </summary>
    public interface ITimerJobEntityManager : IEntityManager<ITimerJobEntity>
    {

        /// <summary>
        /// Insert the <seealso cref="ITimerJobEntity"/>, similar to <seealso cref="#insert(TimerJobEntity)"/>,
        /// but returns a boolean in case the insert did not go through.
        /// This could happen if the execution related to the <seealso cref="ITimerJobEntity"/>
        /// has been removed (for example due to a task complete for a timer boundary on that task). 
        /// </summary>
        bool InsertTimerJobEntity(ITimerJobEntity timerJobEntity);

        /// <summary>
        /// Returns the <seealso cref="ITimerJobEntity"/> instances that are elegible to execute,
        /// meaning the due date of the timer has been passed.
        /// </summary>
        IList<ITimerJobEntity> FindTimerJobsToExecute(Page page);

        /// <summary>
        /// Returns the <seealso cref="ITimerJobEntity"/> for a given process definition.
        /// 
        /// This is for example used when deleting a process definition: it finds
        /// the <seealso cref="ITimerJobEntity"/> representing the timer start events.
        /// </summary>
        IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionId(string type, string processDefinitionId);

        /// <summary>
        /// The same as <seealso cref="#findJobsByTypeAndProcessDefinitionId(String, String)"/>, but
        /// by key and for a specific tenantId. 
        /// </summary>
        IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyAndTenantId(string type, string processDefinitionKey, string tenantId);

        /// <summary>
        /// The same as <seealso cref="#findJobsByTypeAndProcessDefinitionId(String, String)"/>, but
        /// by key and specifically for the 'no tenant' mode.
        /// </summary>
        IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyNoTenantId(string type, string processDefinitionKey);

        /// <summary>
        /// Returns all <seealso cref="ITimerJobEntity"/> instances related to on <seealso cref="IExecutionEntity"/>.
        /// </summary>
        IList<ITimerJobEntity> FindJobsByExecutionId(string id);

        /// <summary>
        /// Returns all <seealso cref="ITimerJobEntity"/> instances related to on <seealso cref="IExecutionEntity"/>. 
        /// </summary>
        IList<ITimerJobEntity> FindJobsByProcessInstanceId(string id);

        /// <summary>
        /// Executes a <seealso cref="ITimerJobQuery"/> and returns the matching <seealso cref="ITimerJobEntity"/> instances.
        /// </summary>
        IList<IJob> FindJobsByQueryCriteria(ITimerJobQuery jobQuery, Page page);

        /// <summary>
        /// Same as <seealso cref="#FindJobsByQueryCriteria(ITimerJobQuery, Page)"/>, but only returns a count 
        /// and not the instances itself.
        /// </summary>
        long FindJobCountByQueryCriteria(ITimerJobQuery jobQuery);

        /// <summary>
        /// Creates a new <seealso cref="ITimerJobEntity"/>, typically when a timer is used in a 
        /// repeating way. The returns <seealso cref="ITimerJobEntity"/> is not yet inserted.
        /// 
        /// Returns null if the timer has finished its repetitions.
        /// </summary>
        ITimerJobEntity CreateAndCalculateNextTimer(IJobEntity timerEntity, IVariableScope variableScope);

        /// <summary>
        /// Changes the tenantId for all jobs related to a given <seealso cref="IDeploymentEntity"/>.
        /// </summary>
        void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId);
    }
}