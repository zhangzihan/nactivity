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
namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.db;
    using Sys.Workflow.engine.task;

    /// 
    /// 
    /// 
    /// 
    public interface ITaskEntity : IVariableScope, ITask, IDelegateTask, IEntity, IHasRevision
    {
        new string TenantId { get; set; }

        new int? Priority { get; set; }

        new string Id { get; set; }

        new DateTime? DueDate { get; set; }

        new string Name { get; set; }

        new string Description { get; set; }

        new IExecutionEntity Execution { get; set; }

        new string ExecutionId { get; set; }


        IList<IIdentityLinkEntity> IdentityLinks { get; }

        IDictionary<string, object> ExecutionVariables { set; }

        new DateTime? CreateTime { get; set; }

        new string ProcessDefinitionId { get; set; }

        new string EventName { get; set; }

        new ActivitiListener CurrentActivitiListener { get; set; }

        new DelegationState? DelegationState { get; set; }

        IExecutionEntity ProcessInstance { get; }

        new string ProcessInstanceId { get; set; }

        int SuspensionState { get; set; }

        new string TaskDefinitionKey { get; set; }

        IDictionary<string, IVariableInstanceEntity> VariableInstanceEntities { get; }

        void ForceUpdate();

        new bool Deleted { get; set; }

        new DateTime? ClaimTime { get; set; }


        bool Canceled { get; set; }
        new bool Suspended { get; set; }

        new string Assignee { get; set; }

        new string AssigneeUser { get; set; }

        new string Owner { get; set; }

        new string Category { get; set; }

        new string FormKey { get; set; }

        new string ParentTaskId { get; set; }

        void IsRuntimeAssignee();
    }
}