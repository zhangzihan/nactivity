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

namespace org.activiti.engine.history
{
    using org.activiti.engine.query;

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
        IHistoricActivityInstanceQuery activityInstanceId(string activityInstanceId);

        /// <summary>
        /// Only select historic activity instances with the given process instance. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricActivityInstanceQuery processInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic activity instances for the given process definition </summary>
        IHistoricActivityInstanceQuery processDefinitionId(string processDefinitionId);

        /// <summary>
        /// Only select historic activity instances for the given execution </summary>
        IHistoricActivityInstanceQuery executionId(string executionId);

        /// <summary>
        /// Only select historic activity instances for the given activity (id from BPMN 2.0 XML)
        /// </summary>
        IHistoricActivityInstanceQuery activityId(string activityId);

        /// <summary>
        /// Only select historic activity instances for activities with the given name
        /// </summary>
        IHistoricActivityInstanceQuery activityName(string activityName);

        /// <summary>
        /// Only select historic activity instances for activities with the given activity type
        /// </summary>
        IHistoricActivityInstanceQuery activityType(string activityType);

        /// <summary>
        /// Only select historic activity instances for userTask activities assigned to the given user
        /// </summary>
        IHistoricActivityInstanceQuery taskAssignee(string userId);

        /// <summary>
        /// Only select historic activity instances that are finished. </summary>
        IHistoricActivityInstanceQuery finished();

        /// <summary>
        /// Only select historic activity instances that are not finished yet. </summary>
        IHistoricActivityInstanceQuery unfinished();

        /// <summary>
        /// Obly select historic activity instances with a specific delete reason. </summary>
        IHistoricActivityInstanceQuery deleteReason(string deleteReason);

        /// <summary>
        /// Obly select historic activity instances with a delete reason that matches the provided parameter. </summary>
        IHistoricActivityInstanceQuery deleteReasonLike(string deleteReasonLike);

        /// <summary>
        /// Only select historic activity instances that have the given tenant id. </summary>
        IHistoricActivityInstanceQuery activityTenantId(string tenantId);

        /// <summary>
        /// Only select historic activity instances with a tenant id like the given one.
        /// </summary>
        IHistoricActivityInstanceQuery activityTenantIdLike(string tenantIdLike);

        /// <summary>
        /// Only select historic activity instances that do not have a tenant id. </summary>
        IHistoricActivityInstanceQuery activityWithoutTenantId();

        // ordering
        // /////////////////////////////////////////////////////////////////
        /// <summary>
        /// Order by id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>). </summary>
        IHistoricActivityInstanceQuery orderByHistoricActivityInstanceId();

        /// <summary>
        /// Order by processInstanceId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByProcessInstanceId();

        /// <summary>
        /// Order by executionId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByExecutionId();

        /// <summary>
        /// Order by activityId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByActivityId();

        /// <summary>
        /// Order by activityName (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByActivityName();

        /// <summary>
        /// Order by activityType (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByActivityType();

        /// <summary>
        /// Order by start (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/> ).
        /// </summary>
        IHistoricActivityInstanceQuery orderByHistoricActivityInstanceStartTime();

        /// <summary>
        /// Order by end (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByHistoricActivityInstanceEndTime();

        /// <summary>
        /// Order by duration (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByHistoricActivityInstanceDuration();

        /// <summary>
        /// Order by processDefinitionId (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByProcessDefinitionId();

        /// <summary>
        /// Order by tenant id (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricActivityInstanceQuery orderByTenantId();

    }

}