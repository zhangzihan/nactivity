using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.bpmn.behavior;
    using Sys.Workflow.engine.impl.bpmn.helper;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.@delegate;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public EndExecutionOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RunOperation()
        {
            try
            {
                if (execution.ProcessInstanceType)
                {
                    HandleProcessInstanceExecution(execution);
                }
                else
                {
                    HandleRegularExecution();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceExecution"></param>
        protected internal virtual void HandleProcessInstanceExecution(IExecutionEntity processInstanceExecution)
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
                    subProcessActivityBehavior.Completing(superExecution, processInstanceExecution);
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

            int activeExecutions = GetNumberOfActiveChildExecutionsForProcessInstance(executionEntityManager, processInstanceId);
            if (activeExecutions == 0)
            {
                logger.LogDebug($"No active executions found. Ending process instance {processInstanceId} ");

                // note the use of execution here vs processinstance execution for getting the flowelement
                executionEntityManager.DeleteProcessInstanceExecutionEntity(processInstanceId, execution.CurrentFlowElement?.Id, null, false, false);
            }
            else
            {
                logger.LogDebug($"Active executions found. Process instance {processInstanceId} will not be ended.");
            }

            Process process = ProcessDefinitionUtil.GetProcess(processInstanceExecution.ProcessDefinitionId);

            // Execute execution listeners for process end.
            if (CollectionUtil.IsNotEmpty(process.ExecutionListeners))
            {
                ExecuteExecutionListeners(process, processInstanceExecution, BaseExecutionListenerFields.EVENTNAME_END);
            }

            // and trigger execution afterwards if doing a call activity
            if (superExecution != null)
            {
                superExecution.SubProcessInstance = null;
                try
                {
                    subProcessActivityBehavior.Completed(superExecution);
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

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void HandleRegularExecution()
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            // There will be a parent execution (or else we would be in the process instance handling method)
            IExecutionEntity parentExecution = executionEntityManager.FindById<IExecutionEntity>(execution.ParentId);

            // If the execution is a scope, all the child executions must be deleted first.
            if (execution.IsScope)
            {
                executionEntityManager.DeleteChildExecutions(execution, null, false);
            }

            // Delete current execution
            logger.LogDebug($"Ending execution {execution.Id}");
            executionEntityManager.DeleteExecutionAndRelatedData(execution, null, false);

            logger.LogDebug($"Parent execution found. Continuing process using execution {parentExecution.Id}");

            // When ending an execution in a multi instance subprocess , special care is needed
            if (IsEndEventInMultiInstanceSubprocess(execution))
            {
                HandleMultiInstanceSubProcess(executionEntityManager, parentExecution);
                return;
            }

            SubProcess subProcess = execution.CurrentFlowElement.SubProcess;

            // If there are no more active child executions, the process can be continued
            // If not (eg an embedded subprocess still has active elements, we cannot continue)
            if (GetNumberOfActiveChildExecutionsForExecution(executionEntityManager, parentExecution.Id) == 0 || IsAllEventScopeExecutions(executionEntityManager, parentExecution))
            {
                IExecutionEntity executionToContinue = null;

                if (subProcess != null)
                {

                    // In case of ending a subprocess: go up in the scopes and continue via the parent scope
                    // unless its a compensation, then we don't need to do anything and can just end it

                    if (subProcess.ForCompensation)
                    {
                        Context.Agenda.PlanEndExecutionOperation(parentExecution);
                    }
                    else
                    {
                        executionToContinue = HandleSubProcessEnd(executionEntityManager, parentExecution, subProcess);
                    }
                }
                else
                {

                    // In the 'regular' case (not being in a subprocess), we use the parent execution to
                    // continue process instance execution

                    executionToContinue = HandleRegularExecutionEnd(executionEntityManager, parentExecution);
                }

                if (executionToContinue != null)
                {
                    // only continue with outgoing sequence flows if the execution is
                    // not the process instance root execution (otherwise the process instance is finished)
                    if (executionToContinue.ProcessInstanceType)
                    {
                        HandleProcessInstanceExecution(executionToContinue);
                    }
                    else
                    {
                        Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(executionToContinue, true);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="parentExecution"></param>
        /// <param name="subProcess"></param>
        /// <returns></returns>
        protected internal virtual IExecutionEntity HandleSubProcessEnd(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution, SubProcess subProcess)
        {
            // create a new execution to take the outgoing sequence flows
            IExecutionEntity executionToContinue = executionEntityManager.CreateChildExecution(parentExecution.Parent);
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
                    if (subElement is Activity subActivity)
                    {
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
                ScopeUtil.CreateCopyOfSubProcessExecutionForCompensation(parentExecution);
            }

            executionEntityManager.DeleteChildExecutions(parentExecution, null, false);
            executionEntityManager.DeleteExecutionAndRelatedData(parentExecution, null, false);

            Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_COMPLETED, subProcess.Id, subProcess.Name, parentExecution.Id, parentExecution.ProcessInstanceId, parentExecution.ProcessDefinitionId, subProcess));
            return executionToContinue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="parentExecution"></param>
        /// <returns></returns>
        protected internal virtual IExecutionEntity HandleRegularExecutionEnd(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            if (!parentExecution.ProcessInstanceType && !(parentExecution.CurrentFlowElement is SubProcess))
            {
                parentExecution.CurrentFlowElement = execution.CurrentFlowElement;
            }

            IExecutionEntity executionToContinue;
            if (execution.CurrentFlowElement is SubProcess currentSubProcess)
            {
                if (currentSubProcess.OutgoingFlows.Count > 0)
                {
                    // create a new execution to take the outgoing sequence flows
                    executionToContinue = executionEntityManager.CreateChildExecution(parentExecution);
                    executionToContinue.CurrentFlowElement = execution.CurrentFlowElement;
                }
                else
                {
                    if (!parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        // create a new execution to take the outgoing sequence flows
                        executionToContinue = executionEntityManager.CreateChildExecution(parentExecution.Parent);
                        executionToContinue.CurrentFlowElement = parentExecution.CurrentFlowElement;

                        executionEntityManager.DeleteChildExecutions(parentExecution, null, false);
                        executionEntityManager.DeleteExecutionAndRelatedData(parentExecution, null, false);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="parentExecution"></param>
        protected internal virtual void HandleMultiInstanceSubProcess(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            IList<IExecutionEntity> activeChildExecutions = GetActiveChildExecutionsForExecution(executionEntityManager, parentExecution.Id);
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

                ScopeUtil.CreateCopyOfSubProcessExecutionForCompensation(parentExecution);
                Context.Agenda.PlanDestroyScopeOperation(parentExecution);

                SubProcess subProcess = execution.CurrentFlowElement.SubProcess;
                MultiInstanceActivityBehavior multiInstanceBehavior = (MultiInstanceActivityBehavior)subProcess.Behavior;
                parentExecution.CurrentFlowElement = subProcess;
                multiInstanceBehavior.Leave(parentExecution);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <returns></returns>
        protected internal virtual bool IsEndEventInMultiInstanceSubprocess(IExecutionEntity executionEntity)
        {
            if (executionEntity.CurrentFlowElement is EndEvent)
            {
                SubProcess subProcess = ((EndEvent)execution.CurrentFlowElement).SubProcess;
                return !executionEntity.Parent.ProcessInstanceType && subProcess != null && subProcess.LoopCharacteristics != null && subProcess.Behavior is MultiInstanceActivityBehavior;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        protected internal virtual int GetNumberOfActiveChildExecutionsForProcessInstance(IExecutionEntityManager executionEntityManager, string processInstanceId)
        {
            ICollection<IExecutionEntity> executions = executionEntityManager.FindChildExecutionsByProcessInstanceId(processInstanceId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="executionId"></param>
        /// <returns></returns>
        protected internal virtual int GetNumberOfActiveChildExecutionsForExecution(IExecutionEntityManager executionEntityManager, string executionId)
        {
            IList<IExecutionEntity> executions = executionEntityManager.FindChildExecutionsByParentExecutionId(executionId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="executionId"></param>
        /// <returns></returns>
        protected internal virtual IList<IExecutionEntity> GetActiveChildExecutionsForExecution(IExecutionEntityManager executionEntityManager, string executionId)
        {
            IList<IExecutionEntity> activeChildExecutions = new List<IExecutionEntity>();
            IList<IExecutionEntity> executions = executionEntityManager.FindChildExecutionsByParentExecutionId(executionId);

            foreach (IExecutionEntity activeExecution in executions)
            {
                if (!(activeExecution.CurrentFlowElement is BoundaryEvent))
                {
                    activeChildExecutions.Add(activeExecution);
                }
            }

            return activeChildExecutions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntityManager"></param>
        /// <param name="parentExecution"></param>
        /// <returns></returns>
        protected internal virtual bool IsAllEventScopeExecutions(IExecutionEntityManager executionEntityManager, IExecutionEntity parentExecution)
        {
            bool allEventScopeExecutions = true;
            IList<IExecutionEntity> executions = executionEntityManager.FindChildExecutionsByParentExecutionId(parentExecution.Id);
            foreach (IExecutionEntity childExecution in executions)
            {
                if (childExecution.IsEventScope)
                {
                    executionEntityManager.DeleteExecutionAndRelatedData(childExecution, null, false);
                }
                else
                {
                    allEventScopeExecutions = false;
                    break;
                }
            }
            return allEventScopeExecutions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecutionEntity"></param>
        /// <param name="executionEntityToIgnore"></param>
        /// <returns></returns>
        protected internal virtual bool AllChildExecutionsEnded(IExecutionEntity parentExecutionEntity, IExecutionEntity executionEntityToIgnore)
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
                        if (!AllChildExecutionsEnded(childExecutionEntity, executionEntityToIgnore))
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