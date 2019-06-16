using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Bpmn.Helper;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow;
using System;

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
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
