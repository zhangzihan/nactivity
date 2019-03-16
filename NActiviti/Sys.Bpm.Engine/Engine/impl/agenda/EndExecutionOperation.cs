using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using Sys;

    /// <summary>
    /// This operations ends an execution and follows the typical BPMN rules to continue the process (if possible).
    /// <para>
    /// This operations is typically not scheduled from an <seealso cref="IActivityBehavior"/>, but rather from
    /// another operation. This happens when the conditions are so that the process can't continue via the regular
    /// ways and an execution cleanup needs to happen, potentially opening up new ways of continuing the process instance.
    /// </para>
    /// </summary>
    public class EndExecutionOperation : AbstractOperation
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<EndExecutionOperation>();

        public EndExecutionOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        protected override void run()
        {
            if (execution.ProcessInstanceType)
            {
                handleProcessInstanceExecution(execution);
            }
            else
            {
                handleRegularExecution();
            }
        }

        protected internal virtual void handleProcessInstanceExecution(IExecutionEntity processInstanceExecution)
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            string processInstanceId = processInstanceExecution.Id; 
            // No parent execution == process instance id
            logger.LogDebug($"No parent execution found. Verifying if process instance {processInstanceId} can be stopped.");

            IExecutionEntity superExecution = processInstanceExecution.SuperExecution;
            @delegate.ISubProcessActivityBehavior subProcessActivityBehavior = null;

            // copy variables before destroying the ended sub process instance (call activity)
            if (superExecution != null)
            {
                FlowNode superExecutionElement = (FlowNode)superExecution.CurrentFlowElement;
                subProcessActivityBehavior = (@delegate.ISubProcessActivityBehavior)superExecutionElement.Behavior;
                try
                {
                    subProcessActivityBehavior.completing(superExecution, processInstanceExecution);
                }
                //catch (Exception e)
                //{
                //    //logger.error("Error while completing sub process of execution {}", processInstanceExecution, e);
                //    throw e;
                //}
                catch (Exception e)
                {
                    logger.LogError($"Error while completing sub process of execution {processInstanceExecution}.Exception Message: {e.Message}");
                    throw new ActivitiException("Error while completing sub process of execution " + processInstanceExecution, e);
                }
            }

            int activeExecutions = getNumberOfActiveChildExecutionsForProcessInstance(executionEntityManager, processInstanceId);
            if (activeExecutions == 0)
            {
                logger.LogDebug($"No active executions found. Ending process instance {processInstanceId} ");

                // note the use of execution here vs processinstance execution for getting the flowelement
                executionEntityManager.deleteProcessInstanceExecutionEntity(processInstanceId, execution.CurrentFlowElement != null ? execution.CurrentFlowElement.Id : null, null, false, false);
            }
            else
            {
                logger.LogDebug($"Active executions found. Process instance {processInstanceId} will not be ended.");
            }

            Process process = ProcessDefinitionUtil.getProcess(processInstanceExecution.ProcessDefinitionId);

            // Execute execution listeners for process end.
            if (CollectionUtil.IsNotEmpty(process.ExecutionListeners))
            {
                executeExecutionListeners(process, processInstanceExecution, BaseExecutionListener_Fields.EVENTNAME_END);
            }

            // and trigger execution afterwards if doing a call activity
            if (superExecution != null)
            {
                superExecution.SubProcessInstance = null;
                try
                {
                    subProcessActivityBehavior.completed(superExecution);
                }
                //catch (Exception e)
                //{
                //    logger.error("Error while completing sub process of execution {}", processInstanceExecution, e);
                //    throw e;
                //}
                catch (Exception e)
                {
                    logger.LogError($"Error while completing sub process of execution {processInstanceExecution}. Exception Messgae: {e.Message}.");
                    throw new ActivitiException("Error while completing sub process of execution " + processInstanceExecution, e);
                }
            }
        }

        protected internal virtual void handleRegularExecution()
        {

            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            // There will be a parent execution (or else we would be in the process instance handling method)
            IExecutionEntity parentExecution = executionEntityManager.findById<IExecutionEntity>(execution.ParentId);

            // If the execution is a scope, all the child executions must be deleted first.
            if (execution.IsScope)
            {
                executionEntityManager.deleteChildExecutions(execution, null, false);
            }

            // Delete current execution
            logger.LogDebug($"Ending execution {execution.Id}");
            executionEntityManager.deleteExecutionAndRelatedData(execution, null, false);

            logger.LogDebug($"Parent execution found. Continuing process using execution {parentExecution.Id}");

            // When ending an execution in a multi instance subprocess , special care is needed
            if (isEndEventInMultiInstanceSubprocess(execution))
            {
                handleMultiInstanceSubProcess(executionEntityManager, parentExecution);
                return;
            }

            SubProcess subProcess = execution.CurrentFlowElement.SubProcess;

            // If there are no more active child executions, the process can be continued
            // If not (eg an embedded subprocess still has active elements, we cannot continue)
            if (getNumberOfActiveChildExecutionsForExecution(executionEntityManager, parentExecution.Id) == 0 || isAllEventScopeExecutions(executionEntityManager, parentExecution))
            {

                IExecutionEntity executionToContinue = null;

                if (subProcess != null)
                {

                    // In case of ending a subprocess: go up in the scopes and continue via the parent scope
                    // unless its a compensation, then we don't need to do anything and can just end it

                    if (subProcess.ForCompensation)
                    {
                        Context.Agenda.planEndExecutionOperation(parentExecution);
                    }
                    else
                    {
                        executionToContinue = handleSubProcessEnd(executionEntityManager, parentExecution, subProcess);
                    }
                }
                else
                {

                    // In the 'regular' case (not being in a subprocess), we use the parent execution to
                    // continue process instance execution

                    executionToContinue = handleRegularExecutionEnd(executionEntityManager, parentExecution);
                }

                if (executionToContinue != null)
                {
                    // only continue with outgoing sequence flows if the execution is
                    // not the process instance root execution (otherwise the process instance is finished)
                    if (executionToContinue.ProcessInstanceType)
                    {
                        handleProcessInstanceExecution(executionToContinue);
                    }
                    else
                    {
                        Context.Agenda.planTakeOutgoingSequenceFlowsOperation(executionToContinue, true);
                    }
                }
            }
        }

        protected internal virtual IExecutionEntity handleSubProcessEnd(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution, SubProcess subProcess)
        {

            IExecutionEntity executionToContinue = null;
            // create a new execution to take the outgoing sequence flows
            executionToContinue = executionEntityManager.createChildExecution(parentExecution.Parent);
            executionToContinue.CurrentFlowElement = subProcess;

            bool hasCompensation = false;
            if (subProcess is Transaction)
            {
                hasCompensation = true;
            }
            else
            {
                foreach (FlowElement subElement in subProcess.FlowElements)
                {
                    if (subElement is Activity)
                    {
                        Activity subActivity = (Activity)subElement;
                        if (CollectionUtil.IsNotEmpty(subActivity.BoundaryEvents))
                        {
                            foreach (BoundaryEvent boundaryEvent in subActivity.BoundaryEvents)
                            {
                                if (CollectionUtil.IsNotEmpty(boundaryEvent.EventDefinitions) && boundaryEvent.EventDefinitions[0] is CompensateEventDefinition)
                                {
                                    hasCompensation = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // All executions will be cleaned up afterwards. However, for compensation we need
            // a copy of these executions so we can use them later on when the compensation is thrown.
            // The following method does exactly that, and moves the executions beneath the process instance.
            if (hasCompensation)
            {
                ScopeUtil.createCopyOfSubProcessExecutionForCompensation(parentExecution);
            }

            executionEntityManager.deleteChildExecutions(parentExecution, null, false);
            executionEntityManager.deleteExecutionAndRelatedData(parentExecution, null, false);

            Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityEvent(ActivitiEventType.ACTIVITY_COMPLETED, subProcess.Id, subProcess.Name, parentExecution.Id, parentExecution.ProcessInstanceId, parentExecution.ProcessDefinitionId, subProcess));
            return executionToContinue;
        }

        protected internal virtual IExecutionEntity handleRegularExecutionEnd(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            IExecutionEntity executionToContinue = null;

            if (!parentExecution.ProcessInstanceType && !(parentExecution.CurrentFlowElement is SubProcess))
            {
                parentExecution.CurrentFlowElement = execution.CurrentFlowElement;
            }

            if (execution.CurrentFlowElement is SubProcess)
            {
                SubProcess currentSubProcess = (SubProcess)execution.CurrentFlowElement;
                if (currentSubProcess.OutgoingFlows.Count > 0)
                {
                    // create a new execution to take the outgoing sequence flows
                    executionToContinue = executionEntityManager.createChildExecution(parentExecution);
                    executionToContinue.CurrentFlowElement = execution.CurrentFlowElement;
                }
                else
                {
                    if (!parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        // create a new execution to take the outgoing sequence flows
                        executionToContinue = executionEntityManager.createChildExecution(parentExecution.Parent);
                        executionToContinue.CurrentFlowElement = parentExecution.CurrentFlowElement;

                        executionEntityManager.deleteChildExecutions(parentExecution, null, false);
                        executionEntityManager.deleteExecutionAndRelatedData(parentExecution, null, false);
                    }
                    else
                    {
                        executionToContinue = parentExecution;
                    }
                }
            }
            else
            {
                executionToContinue = parentExecution;
            }
            return executionToContinue;
        }

        protected internal virtual void handleMultiInstanceSubProcess(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            IList<IExecutionEntity> activeChildExecutions = getActiveChildExecutionsForExecution(executionEntityManager, parentExecution.Id);
            bool containsOtherChildExecutions = false;
            foreach (IExecutionEntity activeExecution in activeChildExecutions)
            {
                if (!activeExecution.Id.Equals(execution.Id))
                {
                    containsOtherChildExecutions = true;
                }
            }

            if (!containsOtherChildExecutions)
            {

                // Destroy the current scope (subprocess) and leave via the subprocess

                ScopeUtil.createCopyOfSubProcessExecutionForCompensation(parentExecution);
                Context.Agenda.planDestroyScopeOperation(parentExecution);

                SubProcess subProcess = execution.CurrentFlowElement.SubProcess;
                MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)subProcess.Behavior;
                parentExecution.CurrentFlowElement = subProcess;
                multiInstanceBehavior.leave(parentExecution);
            }
        }

        protected internal virtual bool isEndEventInMultiInstanceSubprocess(IExecutionEntity executionEntity)
        {
            if (executionEntity.CurrentFlowElement is EndEvent)
            {
                SubProcess subProcess = ((EndEvent)execution.CurrentFlowElement).SubProcess;
                return !executionEntity.Parent.ProcessInstanceType && subProcess != null && subProcess.LoopCharacteristics != null && subProcess.Behavior is MultiInstanceActivityBehavior;
            }
            return false;
        }

        protected internal virtual int getNumberOfActiveChildExecutionsForProcessInstance(IExecutionEntityManager executionEntityManager, string processInstanceId)
        {
            ICollection<IExecutionEntity> executions = executionEntityManager.findChildExecutionsByProcessInstanceId(processInstanceId);
            int activeExecutions = 0;
            foreach (IExecutionEntity execution in executions)
            {
                if (execution.IsActive && !processInstanceId.Equals(execution.Id))
                {
                    activeExecutions++;
                }
            }
            return activeExecutions;
        }

        protected internal virtual int getNumberOfActiveChildExecutionsForExecution(IExecutionEntityManager executionEntityManager, string executionId)
        {
            IList<IExecutionEntity> executions = executionEntityManager.findChildExecutionsByParentExecutionId(executionId);
            int activeExecutions = 0;

            // Filter out the boundary events
            foreach (IExecutionEntity activeExecution in executions)
            {
                if (!(activeExecution.CurrentFlowElement is BoundaryEvent))
                {
                    activeExecutions++;
                }
            }

            return activeExecutions;
        }

        protected internal virtual IList<IExecutionEntity> getActiveChildExecutionsForExecution(IExecutionEntityManager executionEntityManager, string executionId)
        {
            IList<IExecutionEntity> activeChildExecutions = new List<IExecutionEntity>();
            IList<IExecutionEntity> executions = executionEntityManager.findChildExecutionsByParentExecutionId(executionId);

            foreach (IExecutionEntity activeExecution in executions)
            {
                if (!(activeExecution.CurrentFlowElement is BoundaryEvent))
                {
                    activeChildExecutions.Add(activeExecution);
                }
            }

            return activeChildExecutions;
        }

        protected internal virtual bool isAllEventScopeExecutions(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            bool allEventScopeExecutions = true;
            IList<IExecutionEntity> executions = executionEntityManager.findChildExecutionsByParentExecutionId(parentExecution.Id);
            foreach (IExecutionEntity childExecution in executions)
            {
                if (childExecution.IsEventScope)
                {
                    executionEntityManager.deleteExecutionAndRelatedData(childExecution, null, false);
                }
                else
                {
                    allEventScopeExecutions = false;
                    break;
                }
            }
            return allEventScopeExecutions;
        }

        protected internal virtual bool allChildExecutionsEnded(IExecutionEntity parentExecutionEntity, IExecutionEntity executionEntityToIgnore)
        {
            foreach (IExecutionEntity childExecutionEntity in parentExecutionEntity.Executions)
            {
                if (executionEntityToIgnore == null || !executionEntityToIgnore.Id.Equals(childExecutionEntity.Id))
                {
                    if (!childExecutionEntity.Ended)
                    {
                        return false;
                    }
                    if (childExecutionEntity.Executions != null && childExecutionEntity.Executions.Count > 0)
                    {
                        if (!allChildExecutionsEnded(childExecutionEntity, executionEntityToIgnore))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

}