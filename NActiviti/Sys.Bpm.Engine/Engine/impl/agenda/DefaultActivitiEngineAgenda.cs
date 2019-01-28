using System.Collections.Generic;
using System.Threading;

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
namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;
    using System;

    /// 
    /// 
    public class DefaultActivitiEngineAgenda : IActivitiEngineAgenda
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<DefaultActivitiEngineAgenda>();

        protected internal LinkedList<AbstractOperation> operations = new LinkedList<AbstractOperation>();
        protected internal ICommandContext commandContext;

        public DefaultActivitiEngineAgenda(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        public virtual bool Empty
        {
            get
            {
                return operations.Count == 0;
            }
        }

        public virtual AbstractOperation NextOperation()
        {
            if (operations.First != null)
            {
                var operation = operations.First.Value;

                operations.RemoveFirst();

                return operation;
            }

            return null;
        }

        //public void planOperation(Action operation)
        //{

        //}

        /// <summary>
        /// Generic method to plan a <seealso cref="Runnable"/>.
        /// </summary>
        public virtual void planOperation(AbstractOperation operation)
        {
            operations.AddLast(operation);

            if (operation is AbstractOperation)
            {
                IExecutionEntity execution = operation.Execution;
                if (execution != null)
                {
                    commandContext.addInvolvedExecution(execution);
                }
            }

            logger.LogDebug($"Operation {operation.GetType()} added to agenda");
        }

        public virtual void planContinueProcessOperation(IExecutionEntity execution)
        {
            planOperation(new ContinueProcessOperation(commandContext, execution));
        }

        public virtual void planContinueProcessSynchronousOperation(IExecutionEntity execution)
        {
            planOperation(new ContinueProcessOperation(commandContext, execution, true, false));
        }

        public virtual void planContinueProcessInCompensation(IExecutionEntity execution)
        {
            planOperation(new ContinueProcessOperation(commandContext, execution, false, true));
        }

        public virtual void planContinueMultiInstanceOperation(IExecutionEntity execution)
        {
            planOperation(new ContinueMultiInstanceOperation(commandContext, execution));
        }

        public virtual void planTakeOutgoingSequenceFlowsOperation(IExecutionEntity execution, bool evaluateConditions)
        {
            planOperation(new TakeOutgoingSequenceFlowsOperation(commandContext, execution, evaluateConditions));
        }

        public virtual void planEndExecutionOperation(IExecutionEntity execution)
        {
            planOperation(new EndExecutionOperation(commandContext, execution));
        }

        public virtual void planTriggerExecutionOperation(IExecutionEntity execution)
        {
            planOperation(new TriggerExecutionOperation(commandContext, execution));
        }

        public virtual void planDestroyScopeOperation(IExecutionEntity execution)
        {
            planOperation(new DestroyScopeOperation(commandContext, execution));
        }

        public virtual void planExecuteInactiveBehaviorsOperation()
        {
            planOperation(new ExecuteInactiveBehaviorsOperation(commandContext));
        }
    }

}