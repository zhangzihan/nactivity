using Microsoft.Extensions.Logging;
using org.activiti.engine.@delegate;
using org.activiti.engine.impl.bpmn.helper;
using org.activiti.engine.impl.persistence.entity;
using Sys.Workflow;
using System;

namespace org.activiti.engine.impl.bpmn.listener
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
