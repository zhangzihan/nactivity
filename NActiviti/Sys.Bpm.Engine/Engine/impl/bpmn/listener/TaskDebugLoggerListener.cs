using org.activiti.engine.@delegate;
using System;
using System.Collections.Generic;
using System.Text;
using org.activiti.engine.impl.bpmn.helper;
using Microsoft.Extensions.Logging;
using Sys;

namespace org.activiti.engine.impl.bpmn.listener
{
    public class TaskDebugLoggerListener : ITaskListener
    {
        private static readonly ILogger<TaskDebugLoggerListener> logger = ProcessEngineServiceProvider.LoggerService<TaskDebugLoggerListener>();

        public void notify(IDelegateTask delegateTask)
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
    }
}
