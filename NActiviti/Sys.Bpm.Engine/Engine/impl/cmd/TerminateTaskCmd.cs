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

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow;
    using Sys.Workflow.Services.Api.Commands;

    /// 
    /// 
    [Serializable]
    public class TerminateTaskCmd : CompleteTaskCmd
    {
        private const long serialVersionUID = 1L;

        private readonly bool isTerminateExecution;

        private readonly object syncRoot = new object();

        public TerminateTaskCmd(
            string taskId,
            string terminateReason,
            bool isTerminateExecution,
            IDictionary<string, object> variables = null,
            IDictionary<string, object> transientVariables = null)
            : base(taskId, variables, transientVariables: transientVariables)
        {
            this.completeReason = terminateReason;
            this.isTerminateExecution = isTerminateExecution;
            this.variables = variables;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            return base.Execute(commandContext, task);
        }


        protected internal override void ExecuteTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            lock (syncRoot)
            {
                taskEntity.SetVariable(WorkflowVariable.GLOBAL_APPROVALED_VARIABLE, false);
                taskEntity.SetVariableLocal(WorkflowVariable.GLOBAL_OPERATOR_STATE, 4);
                // Task complete logic
                CompleteTask(commandContext, taskEntity, variables, localScope);

                if (!string.IsNullOrWhiteSpace(completeReason))
                {
                    commandContext.ProcessEngineConfiguration.commandExecutor
                        .Execute(new AddCommentCmd(taskId, taskEntity.ProcessInstanceId, completeReason));
                }

                IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;

                if (eventDispatcher.Enabled)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCustomTaskCompletedEvent(taskEntity, ActivitiEventType.TASK_TERMINATED));
                }

                // Continue process (if not a standalone task)
                if (taskEntity.ExecutionId is object && isTerminateExecution)
                {
                    IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ExecutionId);
                    executionEntity.SetVariableLocal(WorkflowVariable.GLOBAL_TERMINATE_TASK_VARNAME, true);
                    Context.Agenda.PlanTriggerExecutionOperation(executionEntity, variables ?? new Dictionary<string, object>());
                }
            }
        }
    }
}