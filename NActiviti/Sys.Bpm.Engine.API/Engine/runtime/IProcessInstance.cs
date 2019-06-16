using System;
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
namespace Sys.Workflow.engine.runtime
{

    using Sys.Workflow.engine.repository;
    using Sys.Workflow;

    /// <summary>
    /// Represents one execution of a <seealso cref="IProcessDefinition"/>.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IProcessInstance : IExecution
    {

        /// <summary>
        /// The id of the process definition of the process instance.
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// The name of the process definition of the process instance.
        /// </summary>
        string ProcessDefinitionName { get; }

        /// <summary>
        /// The key of the process definition of the process instance.
        /// </summary>
        string ProcessDefinitionKey { get; }

        /// <summary>
        /// The version of the process definition of the process instance.
        /// </summary>
        int? ProcessDefinitionVersion { get; }

        /// <summary>
        /// The deployment id of the process definition of the process instance.
        /// </summary>
        string DeploymentId { get; }

        /// <summary>
        /// The business key of this process instance.
        /// </summary>
        string BusinessKey { get; }

        /// <summary>
        /// returns true if the process instance is suspended
        /// </summary>
        new bool Suspended { get; }

        /// <summary>
        /// Returns the process variables if requested in the process instance query
        /// </summary>
        IDictionary<string, object> ProcessVariables { get; }

        /// <summary>
        /// The tenant identifier of this process instance
        /// </summary>
        new string TenantId { get; }

        /// <summary>
        /// Returns the name of this process instance.
        /// </summary>
        new string Name { get; }

        /// <summary>
        /// Returns the description of this process instance.
        /// </summary>
        new string Description { get; }

        /// <summary>
        /// Returns the localized name of this process instance.
        /// </summary>
        string LocalizedName { get; }

        /// <summary>
        /// Returns the localized description of this process instance.
        /// </summary>
        string LocalizedDescription { get; }

        /// <summary>
        /// Returns the start time of this process instance.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Returns the user id of this process instance.
        /// </summary>
        string StartUserId { get; }

        /// <summary>
        /// Returns the user id of this process instance.
        /// </summary>
        string StartUser { get; }

        /// <summary>
        /// Returns the user id of this process instance.
        /// </summary>
        IUserInfo Starter { get; }
    }

}