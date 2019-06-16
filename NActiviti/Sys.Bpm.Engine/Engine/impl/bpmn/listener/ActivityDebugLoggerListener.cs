using Microsoft.Extensions.Logging;
using Sys.Workflow.engine.@delegate;
using Sys.Workflow.engine.impl.bpmn.helper;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow;
using System;

namespace Sys.Workflow.engine.impl.bpmn.listener
{
    public class ActivityDebugLoggerListener : ITaskListener, IExecutionListener
    {
        private static readonly ILogger<ActivityDebugLoggerListener> logger = ProcessEngineServiceProvider.LoggerService<ActivityDebugLoggerListener>();

        public void Notify(IDelegateTask delegateTask)
        {
            try
            {
                delegateTask.Execution.WriteDebugLog();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public void Notify(IExecutionEntity execution)
        {
            try
            {
                execution.WriteDebugLog();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
