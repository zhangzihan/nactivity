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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// <summary>
    /// 一票通过全部否决，多任务场景下如果有人提交了该任务，并且提交条件为true则当前多任务就算完成，并删除所有其它未完成的子任务，否则需要等待其他人得投票.
    /// </summary>
    [Serializable]
    public class OnePassParallelMultiInstanceBehavior : ParallelMultiInstanceBehavior
    {
        private const long serialVersionUID = 1L;

        public OnePassParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior originalActivityBehavior) : base(activity, originalActivityBehavior)
        {
            completedPolicy = new OnePassCompletedPolicy();
        }

        public override void Completed(IExecutionEntity execution)
        {
            base.Completed(execution);
        }

        public override void Completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
            base.Completing(execution, subProcessInstance);
        }

        protected internal override int CreateInstances(IExecutionEntity execution)
        {
            int nrOfInstances = base.CreateInstances(execution);

            return nrOfInstances;
        }

        /// <summary>
        /// 如果一票通过或一票拒绝，就删除当前所有子任务,并初始化删除原因
        /// </summary>
        /// <param name="parentExecution"></param>
        /// <param name="deleteExecution"></param>
        /// <param name="commandContext"></param>
        protected internal override void DeleteChildExecutions(IExecutionEntity parentExecution, bool deleteExecution, ICommandContext commandContext)
        {
            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(parentExecution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    DeleteChildExecutions(childExecution, true, commandContext);
                }
            }

            if (deleteExecution)
            {
                executionEntityManager.DeleteExecutionAndRelatedData(parentExecution, $"任务已一票通过", false);
            }
        }

        //public override void Trigger(IExecutionEntity execution, string signalName, object signalData)
        //{
        //    completedPolicy.LeaveExection = execution;
        //    if (completedPolicy.CompletionConditionSatisfied(execution, this))
        //    {
        //        Leave(execution);
        //        return;
        //    }

        //    base.Trigger(execution, signalName, signalData, throwError);
        //}

        public override void Execute(IExecutionEntity execution)
        {
            base.Execute(execution);
        }

        public override void LastExecutionEnded(IExecutionEntity execution)
        {
            base.LastExecutionEnded(execution);
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
    }
}