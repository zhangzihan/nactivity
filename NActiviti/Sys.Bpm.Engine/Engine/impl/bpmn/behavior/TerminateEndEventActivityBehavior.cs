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
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Histories;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class TerminateEndEventActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal bool terminateAll;
        protected internal bool terminateMultiInstance;

        public TerminateEndEventActivityBehavior()
        {

        }

        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            if (terminateAll)
            {
                TerminateAllBehaviour(execution, commandContext, executionEntityManager);
            }
            else if (terminateMultiInstance)
            {
                TerminateMultiInstanceRoot(execution, commandContext, executionEntityManager);
            }
            else
            {
                DefaultTerminateEndEventBehaviour(execution, commandContext, executionEntityManager);
            }
        }

        protected internal virtual void TerminateAllBehaviour(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {
            IExecutionEntity rootExecutionEntity = executionEntityManager.FindByRootProcessInstanceId(execution.RootProcessInstanceId);
            string deleteReason = CreateDeleteReason(execution.CurrentActivityId);
            DeleteExecutionEntities(executionEntityManager, rootExecutionEntity, deleteReason);
            EndAllHistoricActivities(rootExecutionEntity.Id, deleteReason);
            commandContext.HistoryManager.RecordProcessInstanceEnd(rootExecutionEntity.Id, deleteReason, execution.CurrentActivityId);
        }

        protected internal virtual void DefaultTerminateEndEventBehaviour(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {

            IExecutionEntity scopeExecutionEntity = executionEntityManager.FindFirstScope(execution);
            SendProcessInstanceCancelledEvent(scopeExecutionEntity, execution.CurrentFlowElement);

            // If the scope is the process instance, we can just terminate it all
            // Special treatment is needed when the terminated activity is a subprocess (embedded/callactivity/..)
            // The subprocess is destroyed, but the execution calling it, continues further on.
            // In case of a multi-instance subprocess, only one instance is terminated, the other instances continue to exist.

            string deleteReason = CreateDeleteReason(execution.CurrentActivityId);

            if (scopeExecutionEntity.ProcessInstanceType && scopeExecutionEntity.SuperExecutionId is null)
            {
                EndAllHistoricActivities(scopeExecutionEntity.Id, deleteReason);
                DeleteExecutionEntities(executionEntityManager, scopeExecutionEntity, deleteReason);
                commandContext.HistoryManager.RecordProcessInstanceEnd(scopeExecutionEntity.Id, deleteReason, execution.CurrentActivityId);

            }
            else if (scopeExecutionEntity.CurrentFlowElement != null && scopeExecutionEntity.CurrentFlowElement is SubProcess)
            { // SubProcess

                SubProcess subProcess = (SubProcess)scopeExecutionEntity.CurrentFlowElement;

                scopeExecutionEntity.DeleteReason = deleteReason;
                if (subProcess.HasMultiInstanceLoopCharacteristics())
                {

                    Context.Agenda.PlanDestroyScopeOperation(scopeExecutionEntity);
                    MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)subProcess.Behavior;
                    multiInstanceBehavior.Leave(scopeExecutionEntity);
                }
                else
                {
                    Context.Agenda.PlanDestroyScopeOperation(scopeExecutionEntity);
                    IExecutionEntity outgoingFlowExecution = executionEntityManager.CreateChildExecution(scopeExecutionEntity.Parent);
                    outgoingFlowExecution.CurrentFlowElement = scopeExecutionEntity.CurrentFlowElement;
                    Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(outgoingFlowExecution, true);
                }

            }
            else if (scopeExecutionEntity.ParentId is null && !(scopeExecutionEntity.SuperExecutionId is null))
            { // CallActivity

                IExecutionEntity callActivityExecution = scopeExecutionEntity.SuperExecution;
                CallActivity callActivity = (CallActivity)callActivityExecution.CurrentFlowElement;

                if (callActivity.HasMultiInstanceLoopCharacteristics())
                {

                    MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)callActivity.Behavior;
                    multiInstanceBehavior.Leave(callActivityExecution);
                    executionEntityManager.DeleteProcessInstanceExecutionEntity(scopeExecutionEntity.Id, execution.CurrentFlowElement.Id, "terminate end event", false, false);
                }
                else
                {

                    executionEntityManager.DeleteProcessInstanceExecutionEntity(scopeExecutionEntity.Id, execution.CurrentFlowElement.Id, "terminate end event", false, false);
                    IExecutionEntity superExecutionEntity = executionEntityManager.FindById<IExecutionEntity>(scopeExecutionEntity.SuperExecutionId);
                    Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(superExecutionEntity, true);
                }
            }
        }

        protected internal virtual void EndAllHistoricActivities(string processInstanceId, string deleteReason)
        {

            if (!Context.ProcessEngineConfiguration.HistoryLevel.IsAtLeast(HistoryLevel.ACTIVITY))
            {
                return;
            }

            IList<IHistoricActivityInstanceEntity> historicActivityInstances = Context.CommandContext.HistoricActivityInstanceEntityManager.FindUnfinishedHistoricActivityInstancesByProcessInstanceId(processInstanceId);

            foreach (IHistoricActivityInstanceEntity historicActivityInstance in historicActivityInstances)
            {
                historicActivityInstance.markEnded(deleteReason);

                // Fire event
                ProcessEngineConfigurationImpl config = Context.ProcessEngineConfiguration;
                if (config != null && config.EventDispatcher.Enabled)
                {
                    config.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_ENDED, historicActivityInstance));
                }
            }

        }

        protected internal virtual void TerminateMultiInstanceRoot(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {
            // When terminateMultiInstance is 'true', we look for the multi instance root and delete it from there.
            IExecutionEntity miRootExecutionEntity = executionEntityManager.FindFirstMultiInstanceRoot(execution);
            if (miRootExecutionEntity != null)
            {
                // Create sibling execution to continue process instance execution before deletion
                IExecutionEntity siblingExecution = executionEntityManager.CreateChildExecution(miRootExecutionEntity.Parent);
                siblingExecution.CurrentFlowElement = miRootExecutionEntity.CurrentFlowElement;

                DeleteExecutionEntities(executionEntityManager, miRootExecutionEntity, CreateDeleteReason(miRootExecutionEntity.ActivityId));

                Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(siblingExecution, true);
            }
            else
            {
                DefaultTerminateEndEventBehaviour(execution, commandContext, executionEntityManager);
            }
        }

        protected internal virtual void DeleteExecutionEntities(IExecutionEntityManager executionEntityManager, IExecutionEntity rootExecutionEntity, string deleteReason)
        {
            IList<IExecutionEntity> childExecutions = executionEntityManager.CollectChildren(rootExecutionEntity);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                executionEntityManager.DeleteExecutionAndRelatedData(childExecutions[i], deleteReason, false);
            }
            executionEntityManager.DeleteExecutionAndRelatedData(rootExecutionEntity, deleteReason, false);
        }

        protected internal virtual void SendProcessInstanceCancelledEvent(IExecutionEntity execution, FlowElement terminateEndEvent)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                if ((execution.ProcessInstanceType && execution.SuperExecutionId is null) || (execution.ParentId is null && !(execution.SuperExecutionId is null)))
                {
                    Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCancelledEvent(execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, execution.CurrentFlowElement));
                }
            }

            DispatchExecutionCancelled(execution, terminateEndEvent);
        }

        protected internal virtual void DispatchExecutionCancelled(IExecutionEntity execution, FlowElement terminateEndEvent)
        {
            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

            // subprocesses
            foreach (IExecutionEntity subExecution in executionEntityManager.FindChildExecutionsByParentExecutionId(execution.Id))
            {
                DispatchExecutionCancelled(subExecution, terminateEndEvent);
            }

            // call activities
            IExecutionEntity subProcessInstance = Context.CommandContext.ExecutionEntityManager.FindSubProcessInstanceBySuperExecutionId(execution.Id);
            if (subProcessInstance != null)
            {
                DispatchExecutionCancelled(subProcessInstance, terminateEndEvent);
            }

            // activity with message/signal boundary events
            FlowElement currentFlowElement = execution.CurrentFlowElement;
            if (currentFlowElement is FlowNode)
            {
                DispatchActivityCancelled(execution, terminateEndEvent);
            }
        }

        protected internal virtual void DispatchActivityCancelled(IExecutionEntity execution, FlowElement terminateEndEvent)
        {
            Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(execution.CurrentFlowElement.Id, execution.CurrentFlowElement.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, ParseActivityType((FlowNode)execution.CurrentFlowElement), terminateEndEvent));
        }

        protected internal virtual string CreateDeleteReason(string activityId)
        {
            return History.DeleteReasonFields.TERMINATE_END_EVENT + " (" + activityId + ")";
        }

        public virtual bool TerminateAll
        {
            get
            {
                return terminateAll;
            }
            set
            {
                this.terminateAll = value;
            }
        }


        public virtual bool TerminateMultiInstance
        {
            get
            {
                return terminateMultiInstance;
            }
            set
            {
                this.terminateMultiInstance = value;
            }
        }
    }
}