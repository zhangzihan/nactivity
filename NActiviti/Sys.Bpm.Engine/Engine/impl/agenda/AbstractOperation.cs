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
namespace Sys.Workflow.Engine.Impl.Agenda
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract superclass for all operation interfaces (which are <seealso cref="Runnable"/> instances),
    /// exposing some shared helper methods and member fields to subclasses.
    /// 
    /// An operations is a <seealso cref="Runnable"/> instance that is put on the <seealso cref="IAgenda"/> during
    /// the execution of a <seealso cref="ICommand&lt;T&gt;"/>.
    /// 
    /// 
    /// </summary>
    public abstract class AbstractOperation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ICommandContext commandContext;

        /// <summary>
        /// 
        /// </summary>
        protected internal IAgenda agenda;

        /// <summary>
        /// 
        /// </summary>
        protected internal IExecutionEntity execution;

        public object SignalData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected internal Action Run { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void RunOperation();

        /// <summary>
        /// 
        /// </summary>
        public AbstractOperation()
        {
            Run = new Action(RunOperation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public AbstractOperation(ICommandContext commandContext, IExecutionEntity execution) : this(commandContext, execution, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public AbstractOperation(ICommandContext commandContext, IExecutionEntity execution, object signalData) : this()
        {
            this.commandContext = commandContext;
            this.execution = execution;
            this.agenda = commandContext.Agenda;
            SignalData = signalData;
        }

        /// <summary>
        /// Helper method to match the activityId of an execution with a FlowElement of the process definition referenced by the execution.
        /// </summary>
        protected internal virtual FlowElement GetCurrentFlowElement(IExecutionEntity execution)
        {
            if (execution.CurrentFlowElement != null)
            {
                return execution.CurrentFlowElement;
            }
            else if (execution.CurrentActivityId is object)
            {
                string processDefinitionId = execution.ProcessDefinitionId;
                Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
                string activityId = execution.CurrentActivityId;
                FlowElement currentFlowElement = process.GetFlowElement(activityId, true);
                return currentFlowElement;
            }
            return null;
        }

        /// <summary>
        /// Executes the execution listeners defined on the given element, with the given event type.
        /// Uses the <seealso cref="#execution"/> of this operation instance as argument for the execution listener.
        /// </summary>
        protected internal virtual void ExecuteExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, string eventType)
        {
            ExecuteExecutionListeners(elementWithExecutionListeners, execution, eventType);
        }

        /// <summary>
        /// Executes the execution listeners defined on the given element, with the given event type,
        /// and passing the provided execution to the <seealso cref="IExecutionListener"/> instances.
        /// </summary>
        protected internal virtual void ExecuteExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, IExecutionEntity executionEntity, string eventType)
        {
            commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.ExecuteExecutionListeners(elementWithExecutionListeners, executionEntity, eventType);
        }

        /// <summary>
        /// Returns the first parent execution of the provided execution that is a scope.
        /// </summary>
        protected internal virtual IExecutionEntity FindFirstParentScopeExecution(IExecutionEntity executionEntity)
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(executionEntity.ParentId);
            while (currentlyExaminedExecution != null && parentScopeExecution == null)
            {
                if (currentlyExaminedExecution.IsScope)
                {
                    parentScopeExecution = currentlyExaminedExecution;
                }
                else
                {
                    currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(currentlyExaminedExecution.ParentId);
                }
            }
            return parentScopeExecution;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICommandContext CommandContext
        {
            get
            {
                return commandContext;
            }
            set
            {
                this.commandContext = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IAgenda Agenda
        {
            get
            {
                return agenda;
            }
            set => agenda = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity Execution
        {
            get => execution;
            set => execution = value;
        }
    }
}