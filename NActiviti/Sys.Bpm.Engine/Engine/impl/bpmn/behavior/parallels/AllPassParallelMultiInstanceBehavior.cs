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
namespace Sys.Workflow.engine.impl.bpmn.behavior
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.bpmn.helper;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;

    /// 
    /// 
    [Serializable]
    public class AllPassParallelMultiInstanceBehavior : ParallelMultiInstanceBehavior
    {
        private const long serialVersionUID = 1L;

        public AllPassParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior originalActivityBehavior) : base(activity, originalActivityBehavior)
        {
            completedPolicy = new AllPassCompletedPolicy();
        }

        /// <summary>
        /// Handles the parallel case of spawning the instances. Will create child executions accordingly for every instance needed.
        /// </summary>
        protected internal override int CreateInstances(IExecutionEntity execution)
        {
            int nrOfInstances = base.CreateInstances(execution);

            return nrOfInstances;
        }

        /// <summary>
        /// Called when the wrapped <seealso cref="ActivityBehavior"/> calls the <seealso cref="AbstractBpmnActivityBehavior#leave(ActivityExecution)"/> method. Handles the completion of one of the parallel instances
        /// </summary>
        public override void Leave(IExecutionEntity execution)
        {
            Leave(execution, null);
        }

        public override void Leave(IExecutionEntity execution, object signalData)
        {
            base.Leave(execution, signalData);
        }

        protected internal override void LockFirstParentScope(IExecutionEntity execution)
        {
            base.LockFirstParentScope(execution);
        }

        // TODO: can the ExecutionManager.deleteChildExecution not be used?
        protected internal override void DeleteChildExecutions(IExecutionEntity parentExecution, bool deleteExecution, ICommandContext commandContext)
        {
            base.DeleteChildExecutions(parentExecution, deleteExecution, commandContext);
        }
    }
}