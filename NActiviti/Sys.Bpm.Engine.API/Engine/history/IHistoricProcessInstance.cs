using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.engine.history
{

    using Sys.Workflow.engine.runtime;

    /// <summary>
    /// A single execution of a whole process definition that is stored permanently.
    /// 
    /// 
    /// </summary>
    public interface IHistoricProcessInstance
    {

        /// <summary>
        /// The process instance id (== as the id for the runtime <seealso cref="IProcessInstance process instance"/>).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The user provided unique reference to this process instance. </summary>
        string BusinessKey { get; }

        /// <summary>
        /// The process definition reference. </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// The name of the process definition of the process instance. </summary>
        string ProcessDefinitionName { get; }

        /// <summary>
        /// The key of the process definition of the process instance. </summary>
        string ProcessDefinitionKey { get; }

        /// <summary>
        /// The version of the process definition of the process instance. </summary>
        int? ProcessDefinitionVersion { get; }

        /// <summary>
        /// The deployment id of the process definition of the process instance.
        /// </summary>
        string DeploymentId { get; }

        /// <summary>
        /// The time the process was started. </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// The time the process was ended. </summary>
        DateTime? EndTime { get; }

        /// <summary>
        /// The difference between <seealso cref="EndTime"/> and <seealso cref="StartTime"/> .
        /// </summary>
        long? DurationInMillis { get; }

        /// <summary>
        /// Reference to the activity in which this process instance ended. Note that a process instance can have multiple end events, in this case it might not be deterministic which activity id will be
        /// referenced here. Use a <seealso cref="IHistoricActivityInstanceQuery"/> instead to query for end events of the process instance (use the activityTYpe attribute)
        /// 
        /// </summary>
        string EndActivityId { get; }

        /// <summary>
        /// The authenticated user that started this process instance.
        /// </summary>
        /// <seealso cref="IdentityService.SetAuthenticatedUserId(string)"> </seealso>
        string StartUserId { get; }

        /// <summary>
        /// The authenticated user that started this process instance.
        /// </summary>
        /// <seealso cref="IdentityService.SetAuthenticatedUserId(String)"></seealso>
        string StartUser { get; }

        /// <summary>
        /// The start activity. </summary>
        string StartActivityId { get; }

        /// <summary>
        /// Obtains the reason for the process instance's deletion. </summary>
        string DeleteReason { get; }

        /// <summary>
        /// The process instance id of a potential super process instance or null if no super process instance exists
        /// </summary>
        string SuperProcessInstanceId { get; }

        /// <summary>
        /// The tenant identifier for the process instance.
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// The name for the process instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description for the process instance.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Returns the process variables if requested in the process instance query </summary>
        IDictionary<string, object> ProcessVariables { get; }
    }

}