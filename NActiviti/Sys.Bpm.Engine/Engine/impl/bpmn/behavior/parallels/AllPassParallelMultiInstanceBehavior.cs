using System;

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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// 一票否决全部通过，多任务场景下如果有人提交了该任务，并且提交条件为false则当前多任务就算完成，并删除所有其它未完成的子任务，否则需要等待其他人得投票.
    /// </summary>
    [Serializable]
    public class AllPassParallelMultiInstanceBehavior : ParallelMultiInstanceBehavior
    {
        private const long serialVersionUID = 1L;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="originalActivityBehavior"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="signalData"></param>
        public override void Leave(IExecutionEntity execution, object signalData)
        {
            base.Leave(execution, signalData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        protected internal override void LockFirstParentScope(IExecutionEntity execution)
        {
            base.LockFirstParentScope(execution);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecution"></param>
        /// <param name="deleteExecution"></param>
        /// <param name="commandContext"></param>
        // TODO: can the ExecutionManager.deleteChildExecution not be used?
        protected internal override void DeleteChildExecutions(IExecutionEntity parentExecution, bool deleteExecution, ICommandContext commandContext)
        {
            base.DeleteChildExecutions(parentExecution, deleteExecution, commandContext);
        }
    }
}