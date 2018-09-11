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

namespace org.activiti.engine.impl.persistence.entity
{
    using Newtonsoft.Json;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.runtime;

    public interface IExecutionEntity : IVariableScope, IExecution, IProcessInstance, IEntity, IHasRevision
    {




        /// <summary>
        /// Unique id of this path of execution that can be used as a handle to provide external signals back into the engine after wait states.
        /// </summary>
        new string Id { get; set; }

        /// <summary>
        /// Reference to the overall process instance </summary>
        new string ProcessInstanceId { get; set; }

        /// <summary>
        /// The 'root' process instance. When using call activity for example, the processInstance
        /// set will not always be the root. This method returns the topmost process instance.
        /// </summary>
        new string RootProcessInstanceId { get; set; }

        /// <summary>
        /// Will contain the event name in case this execution is passed in for an <seealso cref="IExecutionListener"/>.
        /// </summary>
        string EventName { get; set; }


        /// <summary>
        /// The business key for the process instance this execution is associated with.
        /// </summary>
        string ProcessInstanceBusinessKey { get; }

        /// <summary>
        /// The process definition key for the process instance this execution is associated with.
        /// </summary>
        new string ProcessDefinitionId { get; set; }

        /// <summary>
        /// Gets the id of the parent of this execution. If null, the execution represents a process-instance.
        /// </summary>
        new string ParentId { get; set; }

        /// <summary>
        /// Gets the id of the calling execution. If not null, the execution is part of a subprocess.
        /// </summary>
        new string SuperExecutionId { get; }

        /// <summary>
        /// Gets the id of the current activity.
        /// </summary>
        string CurrentActivityId { get; }

        /// <summary>
        /// Returns the tenant id, if any is set before on the process definition or process instance.
        /// </summary>
        new string TenantId { get; set; }

        /// <summary>
        /// The BPMN element where the execution currently is at. 
        /// </summary>
        [JsonIgnore]
        FlowElement CurrentFlowElement { get; set; }


        /// <summary>
        /// Returns the <seealso cref="ActivitiListener"/> instance matching an <seealso cref="IExecutionListener"/>
        /// if currently an execution listener is being execution. 
        /// Returns null otherwise.
        /// </summary>
        [JsonIgnore]
        ActivitiListener CurrentActivitiListener { get; set; }


        /* Execution management */

        /// <summary>
        /// returns the parent of this execution, or null if there no parent.
        /// </summary>
        IExecutionEntity Parent { get; set; }

        /// <summary>
        /// returns the list of execution of which this execution the parent of.
        /// </summary>
        IList<IExecutionEntity> Executions { get; }

        /* State management */

        /// <summary>
        /// makes this execution active or inactive.
        /// </summary>
        bool IsActive { set; get; }


        /// <summary>
        /// returns whether this execution has ended or not.
        /// </summary>
        new bool Ended { get; set; }

        /// <summary>
        /// changes the concurrent indicator on this execution.
        /// </summary>
        bool IsConcurrent { set; get; }


        /// <summary>
        /// returns whether this execution is a process instance or not.
        /// </summary>
        bool ProcessInstanceType { get; }

        /// <summary>
        /// Inactivates this execution. This is useful for example in a join: the execution still exists, but it is not longer active.
        /// </summary>
        void inactivate();

        /// <summary>
        /// Returns whether this execution is a scope.
        /// </summary>
        bool IsScope { get; set; }


        /// <summary>
        /// Returns whather this execution is the root of a multi instance execution.
        /// </summary>
        bool IsMultiInstanceRoot { get; set; }

        new string BusinessKey { get; set; }

        new string ProcessDefinitionKey { get; set; }

        new string ProcessDefinitionName { get; set; }

        new int? ProcessDefinitionVersion { get; set; }

        new string DeploymentId { get; set; }

        IExecutionEntity ProcessInstance { get; set; }


        IExecutionEntity SuperExecution { get; set; }


        IExecutionEntity SubProcessInstance { get; set; }

        IExecutionEntity RootProcessInstance { get; set; }


        void addChildExecution(IExecutionEntity executionEntity);

        IList<ITaskEntity> Tasks { get; }

        IList<IEventSubscriptionEntity> EventSubscriptions { get; }

        IList<IJobEntity> Jobs { get; }

        IList<ITimerJobEntity> TimerJobs { get; }

        IList<IIdentityLinkEntity> IdentityLinks { get; }

        string DeleteReason { get; set; }

        int SuspensionState { get; set; }

        bool IsEventScope { get; set; }

        new string Name { get; set; }

        new string Description { set; }

        new string LocalizedName { set; }

        new string LocalizedDescription { set; }

        DateTime? LockTime { get; set; }

        new bool Deleted { get; set; }

        void forceUpdate();

        new string StartUserId { get; set; }

        new DateTime StartTime { get; set; }
    }
}