using System;

namespace Sys.Workflow.Engine.Impl.Agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Logging;
    using Sys.Workflow;

    /// <summary>
    /// Special operation when executing an instance of a multi-instance.
    /// It's similar to the <seealso cref="ContinueProcessOperation"/>, but simpler, as it doesn't need to 
    /// cater for as many use cases.
    /// </summary>
    public class ContinueMultiInstanceOperation : AbstractOperation
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<ContinueMultiInstanceOperation>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public ContinueMultiInstanceOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RunOperation()
        {
            try
            {
                FlowElement currentFlowElement = GetCurrentFlowElement(execution);
                if (currentFlowElement is FlowNode)
                {
                    ContinueThroughMultiInstanceFlowNode((FlowNode)currentFlowElement);
                }
                else
                {
                    throw new Exception("Programmatic error: no valid multi instance flow node, type: " + currentFlowElement + ". Halting.");
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
        /// <param name="flowNode"></param>
        protected internal virtual void ContinueThroughMultiInstanceFlowNode(FlowNode flowNode)
        {
            if (!flowNode.Asynchronous)
            {
                ExecuteSynchronous(flowNode);
            }
            else
            {
                ExecuteAsynchronous(flowNode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteSynchronous(FlowNode flowNode)
        {

            // Execution listener
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                ExecuteExecutionListeners(flowNode, BaseExecutionListenerFields.EVENTNAME_START);
            }

            commandContext.HistoryManager.RecordActivityStart(execution);

            // Execute actual behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;
            if (activityBehavior is object)
            {
                //logger.LogDebug($"Executing activityBehavior {activityBehavior.GetType()} on activity '{flowNode.Id}' with execution {execution.Id}");

                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                if (processEngineConfiguration is object && processEngineConfiguration.EventDispatcher.Enabled)
                {
                    processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_STARTED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
                }

                try
                {
                    activityBehavior.Execute(execution);
                }
                catch (BpmnError error)
                {
                    // re-throw business fault so that it can be caught by an Error Intermediate Event or Error Event Sub-Process in the process
                    ErrorPropagation.PropagateError(error, execution);
                }
                catch (Exception e)
                {
                    if (LogMDC.MDCEnabled)
                    {
                        LogMDC.PutMDCExecution(execution);
                    }
                    throw;
                }
            }
            else
            {
                logger.LogDebug($"No activityBehavior on activity '{flowNode.Id}' with execution {execution.Id}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteAsynchronous(FlowNode flowNode)
        {
            IJobEntity job = commandContext.JobManager.CreateAsyncJob(execution, flowNode.Exclusive);
            commandContext.JobManager.ScheduleAsyncJob(job);
        }
    }
}