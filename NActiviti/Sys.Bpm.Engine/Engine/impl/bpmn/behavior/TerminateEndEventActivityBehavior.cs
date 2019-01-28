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
namespace org.activiti.engine.impl.bpmn.behavior
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

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

        public override void execute(IExecutionEntity execution)
        {

            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            if (terminateAll)
            {
                terminateAllBehaviour(execution, commandContext, executionEntityManager);
            }
            else if (terminateMultiInstance)
            {
                terminateMultiInstanceRoot(execution, commandContext, executionEntityManager);
            }
            else
            {
                defaultTerminateEndEventBehaviour(execution, commandContext, executionEntityManager);
            }
        }

        protected internal virtual void terminateAllBehaviour(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {
            IExecutionEntity rootExecutionEntity = executionEntityManager.findByRootProcessInstanceId(execution.RootProcessInstanceId);
            string deleteReason = createDeleteReason(execution.CurrentActivityId);
            deleteExecutionEntities(executionEntityManager, rootExecutionEntity, deleteReason);
            endAllHistoricActivities(rootExecutionEntity.Id, deleteReason);
            commandContext.HistoryManager.recordProcessInstanceEnd(rootExecutionEntity.Id, deleteReason, execution.CurrentActivityId);
        }

        protected internal virtual void defaultTerminateEndEventBehaviour(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {

            IExecutionEntity scopeExecutionEntity = executionEntityManager.findFirstScope(execution);
            sendProcessInstanceCancelledEvent(scopeExecutionEntity, execution.CurrentFlowElement);

            // If the scope is the process instance, we can just terminate it all
            // Special treatment is needed when the terminated activity is a subprocess (embedded/callactivity/..)
            // The subprocess is destroyed, but the execution calling it, continues further on.
            // In case of a multi-instance subprocess, only one instance is terminated, the other instances continue to exist.

            string deleteReason = createDeleteReason(execution.CurrentActivityId);

            if (scopeExecutionEntity.ProcessInstanceType && ReferenceEquals(scopeExecutionEntity.SuperExecutionId, null))
            {

                endAllHistoricActivities(scopeExecutionEntity.Id, deleteReason);
                deleteExecutionEntities(executionEntityManager, scopeExecutionEntity, deleteReason);
                commandContext.HistoryManager.recordProcessInstanceEnd(scopeExecutionEntity.Id, deleteReason, execution.CurrentActivityId);

            }
            else if (scopeExecutionEntity.CurrentFlowElement != null && scopeExecutionEntity.CurrentFlowElement is SubProcess)
            { // SubProcess

                SubProcess subProcess = (SubProcess)scopeExecutionEntity.CurrentFlowElement;

                scopeExecutionEntity.DeleteReason = deleteReason;
                if (subProcess.hasMultiInstanceLoopCharacteristics())
                {

                    Context.Agenda.planDestroyScopeOperation(scopeExecutionEntity);
                    MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)subProcess.Behavior;
                    multiInstanceBehavior.leave(scopeExecutionEntity);

                }
                else
                {
                    Context.Agenda.planDestroyScopeOperation(scopeExecutionEntity);
                    IExecutionEntity outgoingFlowExecution = executionEntityManager.createChildExecution(scopeExecutionEntity.Parent);
                    outgoingFlowExecution.CurrentFlowElement = scopeExecutionEntity.CurrentFlowElement;
                    Context.Agenda.planTakeOutgoingSequenceFlowsOperation(outgoingFlowExecution, true);
                }

            }
            else if (ReferenceEquals(scopeExecutionEntity.ParentId, null) && !ReferenceEquals(scopeExecutionEntity.SuperExecutionId, null))
            { // CallActivity

                IExecutionEntity callActivityExecution = scopeExecutionEntity.SuperExecution;
                CallActivity callActivity = (CallActivity)callActivityExecution.CurrentFlowElement;

                if (callActivity.hasMultiInstanceLoopCharacteristics())
                {

                    MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)callActivity.Behavior;
                    multiInstanceBehavior.leave(callActivityExecution);
                    executionEntityManager.deleteProcessInstanceExecutionEntity(scopeExecutionEntity.Id, execution.CurrentFlowElement.Id, "terminate end event", false, false);

                }
                else
                {

                    executionEntityManager.deleteProcessInstanceExecutionEntity(scopeExecutionEntity.Id, execution.CurrentFlowElement.Id, "terminate end event", false, false);
                    IExecutionEntity superExecutionEntity = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", scopeExecutionEntity.SuperExecutionId));
                    Context.Agenda.planTakeOutgoingSequenceFlowsOperation(superExecutionEntity, true);

                }

            }
        }

        protected internal virtual void endAllHistoricActivities(string processInstanceId, string deleteReason)
        {

            if (!Context.ProcessEngineConfiguration.HistoryLevel.isAtLeast(HistoryLevel.ACTIVITY))
            {
                return;
            }

            IList<IHistoricActivityInstanceEntity> historicActivityInstances = Context.CommandContext.HistoricActivityInstanceEntityManager.findUnfinishedHistoricActivityInstancesByProcessInstanceId(processInstanceId);

            foreach (IHistoricActivityInstanceEntity historicActivityInstance in historicActivityInstances)
            {
                historicActivityInstance.markEnded(deleteReason);

                // Fire event
                ProcessEngineConfigurationImpl config = Context.ProcessEngineConfiguration;
                if (config != null && config.EventDispatcher.Enabled)
                {
                    config.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.HISTORIC_ACTIVITY_INSTANCE_ENDED, historicActivityInstance));
                }
            }

        }

        protected internal virtual void terminateMultiInstanceRoot(IExecutionEntity execution, ICommandContext commandContext, IExecutionEntityManager executionEntityManager)
        {

            // When terminateMultiInstance is 'true', we look for the multi instance root and delete it from there.
            IExecutionEntity miRootExecutionEntity = executionEntityManager.findFirstMultiInstanceRoot(execution);
            if (miRootExecutionEntity != null)
            {
                // Create sibling execution to continue process instance execution before deletion
                IExecutionEntity siblingExecution = executionEntityManager.createChildExecution(miRootExecutionEntity.Parent);
                siblingExecution.CurrentFlowElement = miRootExecutionEntity.CurrentFlowElement;

                deleteExecutionEntities(executionEntityManager, miRootExecutionEntity, createDeleteReason(miRootExecutionEntity.ActivityId));

                Context.Agenda.planTakeOutgoingSequenceFlowsOperation(siblingExecution, true);
            }
            else
            {
                defaultTerminateEndEventBehaviour(execution, commandContext, executionEntityManager);
            }
        }

        protected internal virtual void deleteExecutionEntities(IExecutionEntityManager executionEntityManager, IExecutionEntity rootExecutionEntity, string deleteReason)
        {

            IList<IExecutionEntity> childExecutions = executionEntityManager.collectChildren(rootExecutionEntity);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                executionEntityManager.deleteExecutionAndRelatedData(childExecutions[i], deleteReason, false);
            }
            executionEntityManager.deleteExecutionAndRelatedData(rootExecutionEntity, deleteReason, false);
        }

        protected internal virtual void sendProcessInstanceCancelledEvent(IExecutionEntity execution, FlowElement terminateEndEvent)
        {
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                if ((execution.ProcessInstanceType && ReferenceEquals(execution.SuperExecutionId, null)) || (ReferenceEquals(execution.ParentId, null) && !ReferenceEquals(execution.SuperExecutionId, null)))
                {

                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createCancelledEvent(execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, execution.CurrentFlowElement));
                }
            }

            dispatchExecutionCancelled(execution, terminateEndEvent);
        }

        protected internal virtual void dispatchExecutionCancelled(IExecutionEntity execution, FlowElement terminateEndEvent)
        {

            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

            // subprocesses
            foreach (IExecutionEntity subExecution in executionEntityManager.findChildExecutionsByParentExecutionId(execution.Id))
            {
                dispatchExecutionCancelled(subExecution, terminateEndEvent);
            }

            // call activities
            IExecutionEntity subProcessInstance = Context.CommandContext.ExecutionEntityManager.findSubProcessInstanceBySuperExecutionId(execution.Id);
            if (subProcessInstance != null)
            {
                dispatchExecutionCancelled(subProcessInstance, terminateEndEvent);
            }

            // activity with message/signal boundary events
            FlowElement currentFlowElement = execution.CurrentFlowElement;
            if (currentFlowElement is FlowNode)
            {
                dispatchActivityCancelled(execution, terminateEndEvent);
            }
        }

        protected internal virtual void dispatchActivityCancelled(IExecutionEntity execution, FlowElement terminateEndEvent)
        {
            Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityCancelledEvent(execution.CurrentFlowElement.Id, execution.CurrentFlowElement.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, parseActivityType((FlowNode)execution.CurrentFlowElement), terminateEndEvent));
        }

        protected internal virtual string createDeleteReason(string activityId)
        {
            return engine.history.DeleteReason_Fields.TERMINATE_END_EVENT + " (" + activityId + ")";
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