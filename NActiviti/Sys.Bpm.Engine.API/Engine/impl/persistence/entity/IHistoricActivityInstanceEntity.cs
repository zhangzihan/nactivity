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

namespace Sys.Workflow.engine.impl.persistence.entity
{
    using Sys.Workflow.engine.history;
    using System;

    /// 
    /// 
    public interface IHistoricActivityInstanceEntity : IHistoricActivityInstance, IHistoricScopeInstanceEntity
    {
        new string Id { get; set; }

        new string ProcessDefinitionId { get; set; }

        new string ProcessInstanceId { get; set; }

        new string ActivityId { get; set; }

        new DateTime? StartTime { get; set; }

        /// <summary>
        /// Time when the activity instance ended </summary>
        new DateTime? EndTime { get; set; }

        new string ActivityName { get; set; }

        new string ActivityType { get; set; }

        new string ExecutionId { get; set; }

        new string Assignee { get; set; }

        new string AssigneeUser { get; set; }

        new string TaskId { get; set; }

        new string CalledProcessInstanceId { get; set; }

        new string TenantId { get; set; }

    }

}