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
namespace Sys.Workflow.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow;
    using System.Collections.Concurrent;

    /// 
    /// 
    public class DefaultActivitiEngineAgenda : IActivitiEngineAgenda
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<DefaultActivitiEngineAgenda>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentStack<AbstractOperation> operations = new ConcurrentStack<AbstractOperation>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ICommandContext commandContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public DefaultActivitiEngineAgenda(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Empty
        {
            get
            {
                return operations.IsEmpty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual AbstractOperation NextOperation()
        {
            if (operations.IsEmpty == false)
            {
                if (operations.TryPop(out var operation))//.First.Value;)
                {
                    //operations.RemoveFirst();

                    return operation;
                }
            }

            return null;
        }

        //public void planOperation(Action operation)
        //{

        //}

        /// <summary>
        /// Generic method to plan a <seealso cref="Runnable"/>.
        /// </summary>
        public virtual void PlanOperation(AbstractOperation operation)
        {
            operations.Push(operation);

            if (operation is AbstractOperation)
            {
                IExecutionEntity execution = operation.Execution;
                if (execution != null)
                {
                    commandContext.AddInvolvedExecution(execution);
                }
            }

            logger.LogDebug($"Operation {operation.GetType()} added to agenda");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanContinueProcessOperation(IExecutionEntity execution)
        {
            PlanOperation(new ContinueProcessOperation(commandContext, execution));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanContinueProcessSynchronousOperation(IExecutionEntity execution)
        {
            PlanOperation(new ContinueProcessOperation(commandContext, execution, true, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanContinueProcessInCompensation(IExecutionEntity execution)
        {
            PlanOperation(new ContinueProcessOperation(commandContext, execution, false, true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanContinueMultiInstanceOperation(IExecutionEntity execution)
        {
            PlanOperation(new ContinueMultiInstanceOperation(commandContext, execution));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="evaluateConditions"></param>
        public virtual void PlanTakeOutgoingSequenceFlowsOperation(IExecutionEntity execution, bool evaluateConditions)
        {
            PlanOperation(new TakeOutgoingSequenceFlowsOperation(commandContext, execution, evaluateConditions));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanEndExecutionOperation(IExecutionEntity execution)
        {
            PlanOperation(new EndExecutionOperation(commandContext, execution));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanTriggerExecutionOperation(IExecutionEntity execution)
        {
            PlanOperation(new TriggerExecutionOperation(commandContext, execution));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanTriggerExecutionOperation(IExecutionEntity execution, object signalData)
        {
            PlanOperation(new TriggerExecutionOperation(commandContext, execution, signalData));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void PlanDestroyScopeOperation(IExecutionEntity execution)
        {
            PlanOperation(new DestroyScopeOperation(commandContext, execution));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void PlanExecuteInactiveBehaviorsOperation()
        {
            PlanOperation(new ExecuteInactiveBehaviorsOperation(commandContext));
        }
    }
}