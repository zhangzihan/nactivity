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

namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Programmatic querying for <seealso cref="IHistoricActivityInstance"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricActivityInstanceQuery : IQuery<IHistoricActivityInstanceQuery, IHistoricActivityInstance>
    {

        /// <summary>
        /// Only select historic activity instances with the given id (primary key within history tables).
        /// </summary>
        IHistoricActivityInstanceQuery SetActivityInstanceId(string activityInstanceId);

        /// <summary>
        /// Only select historic activity instances with the given process instance. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricActivityInstanceQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic activity instances for the given process definition </summary>
        IHistoricActivityInstanceQuery SetProcessDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select historic activity instances for the given execution </summary>
        IHistoricActivityInstanceQuery SetExecutionId(string executionId);

        /// <summary>
        /// Only select historic activity instances for the given activity (id from BPMN 2.0 XML)
        /// </summary>
        IHistoricActivityInstanceQuery SetActivityId(string activityId);

        /// <summary>
        /// Only select historic activity instances for activities with the given name
        /// </summary>
        IHistoricActivityInstanceQuery SetActivityName(string activityName);

        /// <summary>
        /// Only select historic activity instances for activities with the given activity type
        /// </summary>
        IHistoricActivityInstanceQuery SetActivityType(string activityType);

        /// <summary>
        /// Only select historic activity instances for userTask activities assigned to the given user
        /// </summary>
        IHistoricActivityInstanceQuery SetTaskAssignee(string userId);

        /// <summary>
        /// Only select historic activity instances that are finished. </summary>
        IHistoricActivityInstanceQuery SetFinished();

        /// <summary>
        /// Only select historic activity instances that are not finished yet. </summary>
        IHistoricActivityInstanceQuery SetUnfinished();

        /// <summary>
        /// Obly select historic activity instances with a specific delete reason. </summary>
        IHistoricActivityInstanceQuery SetDeleteReason(string deleteReason);

        /// <summary>
        /// Obly select historic activity instances with a delete reason that matches the provided parameter. </summary>
        IHistoricActivityInstanceQuery SetDeleteReasonLike(string deleteReasonLike);

        /// <summary>
        /// Only select historic activity instances that have the given tenant id. </summary>
        IHistoricActivityInstanceQuery SetActivityTenantId(string tenantId);

        /// <summary>
        /// Only select historic activity instances with a tenant id like the given one.
        /// </summary>
        IHistoricActivityInstanceQuery SetActivityTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select historic activity instances that do not have a tenant id. </summary>
        IHistoricActivityInstanceQuery SetActivityWithoutTenantId();

        // ordering
        // /////////////////////////////////////////////////////////////////
        /// <summary>
        /// Order by id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). </summary>
        IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceId();

        /// <summary>
        /// Order by processInstanceId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByProcessInstanceId();

        /// <summary>
        /// Order by executionId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByExecutionId();

        /// <summary>
        /// Order by activityId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByActivityId();

        /// <summary>
        /// Order by activityName (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByActivityName();

        /// <summary>
        /// Order by activityType (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByActivityType();

        /// <summary>
        /// Order by start (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/> ).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceStartTime();

        /// <summary>
        /// Order by end (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceEndTime();

        /// <summary>
        /// Order by duration (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByHistoricActivityInstanceDuration();

        /// <summary>
        /// Order by processDefinitionId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByProcessDefinitionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery OrderByTenantId();
    }
}