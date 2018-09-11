using System;

namespace org.activiti.engine.impl.agenda
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.logging;

    /// <summary>
    /// Special operation when executing an instance of a multi-instance.
    /// It's similar to the <seealso cref="ContinueProcessOperation"/>, but simpler, as it doesn't need to 
    /// cater for as many use cases.
    /// 
    /// 
    /// 
    /// </summary>
    public class ContinueMultiInstanceOperation : AbstractOperation
    {
        public ContinueMultiInstanceOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        protected override void run()
        {
            FlowElement currentFlowElement = getCurrentFlowElement(execution);
            if (currentFlowElement is FlowNode)
            {
                continueThroughMultiInstanceFlowNode((FlowNode)currentFlowElement);
            }
            else
            {
                throw new Exception("Programmatic error: no valid multi instance flow node, type: " + currentFlowElement + ". Halting.");
            }
        }

        protected internal virtual void continueThroughMultiInstanceFlowNode(FlowNode flowNode)
        {
            if (!flowNode.Asynchronous)
            {
                executeSynchronous(flowNode);
            }
            else
            {
                executeAsynchronous(flowNode);
            }
        }

        protected internal virtual void executeSynchronous(FlowNode flowNode)
        {

            // Execution listener
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                executeExecutionListeners(flowNode, BaseExecutionListener_Fields.EVENTNAME_START);
            }

            commandContext.HistoryManager.recordActivityStart(execution);

            // Execute actual behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;
            if (activityBehavior != null)
            {
                //logger.debug("Executing activityBehavior {} on activity '{}' with execution {}", activityBehavior.GetType(), flowNode.Id, execution.Id);

                if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
                {
                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityEvent(ActivitiEventType.ACTIVITY_STARTED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
                }

                try
                {
                    activityBehavior.execute(execution);
                }
                catch (BpmnError error)
                {
                    // re-throw business fault so that it can be caught by an Error Intermediate Event or Error Event Sub-Process in the process
                    ErrorPropagation.propagateError(error, execution);
                }
                catch (Exception e)
                {
                    if (LogMDC.MDCEnabled)
                    {
                        LogMDC.putMDCExecution(execution);
                    }
                    throw e;
                }
            }
            else
            {
                //logger.debug("No activityBehavior on activity '{}' with execution {}", flowNode.Id, execution.Id);
            }
        }

        protected internal virtual void executeAsynchronous(FlowNode flowNode)
        {
            IJobEntity job = commandContext.JobManager.createAsyncJob(execution, flowNode.Exclusive);
            commandContext.JobManager.scheduleAsyncJob(job);
        }
    }

}