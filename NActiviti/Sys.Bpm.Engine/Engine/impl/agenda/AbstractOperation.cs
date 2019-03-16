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
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract superclass for all operation interfaces (which are <seealso cref="Runnable"/> instances),
    /// exposing some shared helper methods and member fields to subclasses.
    /// 
    /// An operations is a <seealso cref="Runnable"/> instance that is put on the <seealso cref="IAgenda"/> during
    /// the execution of a <seealso cref="Command"/>.
    /// 
    /// 
    /// </summary>
    public abstract class AbstractOperation
    {
        protected internal ICommandContext commandContext;
        protected internal IAgenda agenda;
        protected internal IExecutionEntity execution;

        protected internal Action Run { get; set; }

        public AbstractOperation()
        {
            Run = new Action(run);
        }

        protected abstract void run();

        public AbstractOperation(ICommandContext commandContext, IExecutionEntity execution) : this()
        {
            this.commandContext = commandContext;
            this.execution = execution;
            this.agenda = commandContext.Agenda;
        }

        /// <summary>
        /// Helper method to match the activityId of an execution with a FlowElement of the process definition referenced by the execution.
        /// </summary>
        protected internal virtual FlowElement getCurrentFlowElement(IExecutionEntity execution)
        {
            if (execution.CurrentFlowElement != null)
            {
                return execution.CurrentFlowElement;
            }
            else if (!ReferenceEquals(execution.CurrentActivityId, null))
            {
                string processDefinitionId = execution.ProcessDefinitionId;
                org.activiti.bpmn.model.Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
                string activityId = execution.CurrentActivityId;
                FlowElement currentFlowElement = process.getFlowElement(activityId, true);
                return currentFlowElement;
            }
            return null;
        }

        /// <summary>
        /// Executes the execution listeners defined on the given element, with the given event type.
        /// Uses the <seealso cref="#execution"/> of this operation instance as argument for the execution listener.
        /// </summary>
        protected internal virtual void executeExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, string eventType)
        {
            executeExecutionListeners(elementWithExecutionListeners, execution, eventType);
        }

        /// <summary>
        /// Executes the execution listeners defined on the given element, with the given event type,
        /// and passing the provided execution to the <seealso cref="IExecutionListener"/> instances.
        /// </summary>
        protected internal virtual void executeExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, IExecutionEntity executionEntity, string eventType)
        {
            commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.executeExecutionListeners(elementWithExecutionListeners, executionEntity, eventType);
        }

        /// <summary>
        /// Returns the first parent execution of the provided execution that is a scope.
        /// </summary>
        protected internal virtual IExecutionEntity findFirstParentScopeExecution(IExecutionEntity executionEntity)
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(executionEntity.ParentId);
            while (currentlyExaminedExecution != null && parentScopeExecution == null)
            {
                if (currentlyExaminedExecution.IsScope)
                {
                    parentScopeExecution = currentlyExaminedExecution;
                }
                else
                {
                    currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(currentlyExaminedExecution.ParentId);
                }
            }
            return parentScopeExecution;
        }

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


        public virtual IAgenda getAgenda()
        {
            return agenda;
        }

        public virtual void setAgenda(DefaultActivitiEngineAgenda agenda)
        {
            this.agenda = agenda;
        }

        public virtual IExecutionEntity Execution
        {
            get
            {
                return execution;
            }
            set
            {
                this.execution = value;
            }
        }


    }

}