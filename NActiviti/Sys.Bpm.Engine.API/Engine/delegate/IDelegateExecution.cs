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

using Sys.Workflow.Bpmn.Models;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Delegate
{
    /**
     * Execution used in {@link JavaDelegate}s and {@link ExecutionListener}s.
     * 
     */
    public interface IDelegateExecution : IVariableScope
    {

        /**
         * Unique id of this path of execution that can be used as a handle to provide external signals back into the engine after wait states.
         */
        string Id { get; }

        /** Reference to the overall process instance */
        string ProcessInstanceId { get; }

        /**
         * The 'root' process instance. When using call activity for example, the processInstance
         * set will not always be the root. This method returns the topmost process instance.
         */
        string RootProcessInstanceId { get; }

        /**
         * Will contain the event name in case this execution is passed in for an {@link ExecutionListener}.
         */
        string EventName { get; set; }

        /**
         * The business key for the process instance this execution is associated with.
         */
        string ProcessInstanceBusinessKey { get; }

        /**
         * The process definition key for the process instance this execution is associated with.
         */
        string ProcessDefinitionId { get; }

        /**
         * Gets the id of the parent of this execution. If null, the execution represents a process-instance.
         */
        string ParentId { get; }

        /**
         * Gets the id of the calling execution. If not null, the execution is part of a subprocess.
         */
        string SuperExecutionId { get; }

        /**
         * Gets the id of the current activity.
         */
        string CurrentActivityId { get; }

        /**
         * Returns the tenant id, if any is set before on the process definition or process instance.
         */
        string TenantId { get; }

        /**
         * The BPMN element where the execution currently is at. 
         */
        FlowElement CurrentFlowElement { get; }

        /**
         * Change the current BPMN element the execution is at. 
         */
        void SetCurrentFlowElement(FlowElement flowElement);

        /**
         * Returns the {@link ActivitiListener} instance matching an {@link ExecutionListener}
         * if currently an execution listener is being execution. 
         * Returns null otherwise.
         */
        ActivitiListener CurrentActivitiListener { get; }

        /**
         * Called when an {@link ExecutionListener} is being executed. 
         */
        void SetCurrentActivitiListener(ActivitiListener currentActivitiListener);

        /* Execution management */

        /**
         * returns the parent of this execution, or null if there no parent.
         */
        IDelegateExecution Parent { get; }

        /**
         * returns the list of execution of which this execution the parent of.
         */
        IList<IDelegateExecution> Executions { get; }

        /* State management */

        /**
         * makes this execution active or inactive.
         */
        bool IsActive { get; }

        /**
         * returns whether this execution has ended or not.
         */
        bool IsEnded { get; }

        /**
         * changes the concurrent indicator on this execution.
         */
        bool IsConcurrent { get; set; }

        /**
         * returns whether this execution is a process instance or not.
         */
        bool IsProcessInstanceType { get; }

        /**
         * Inactivates this execution. This is useful for example in a join: the execution still exists, but it is not longer active.
         */
        void Inactivate();

        /**
         * Returns whether this execution is a scope.
         */
        bool IsScope { get; set; }


        /**
         * Returns whather this execution is the root of a multi instance execution.
         */
        bool IsMultiInstanceRoot { get; set; }
    }
}
