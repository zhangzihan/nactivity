using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Debug;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Bpmn.Listener
{
    /// <summary>
    /// 调试器事件日志
    /// </summary>
    public class WorkflowDebuggerEvent : IActivitiEvent
    {
        /// <inheritdoc />
        public ActivitiEventType Type => ActivitiEventType.DEBUG_LOGGER;

        /// <inheritdoc />
        public string ExecutionId => Execution.Id;

        /// <inheritdoc />
        public string ProcessInstanceId => Execution.ProcessInstanceId;

        /// <inheritdoc />
        public string ProcessDefinitionId => Execution.ProcessDefinitionId;

        public Exception Exception { get; }

        public IExecutionEntity Execution { get; }
        public LogLevel LogLevel { get; }
        public string ExecutionTrace { get; }

        /// <inheritdoc />
        public WorkflowDebuggerEvent(IExecutionEntity execution, Exception exception = null)
        {
            this.Execution = execution;
            this.Exception = exception;

            if (execution != null)
            {
                ExecutionTree eTree = ExecutionTreeUtil.BuildExecutionTree(execution);
                ExecutionTrace = eTree.ToString();
            }
            if (exception != null)
            {
                LogLevel = LogLevel.Error;
            }
            else
            {
                LogLevel = LogLevel.Information;
            }
        }

        /// <inheritdoc />
        public WorkflowDebuggerEvent(IExecutionEntity execution, LogLevel logLevel, Exception exception = null) : this(execution, exception)
        {
            LogLevel = logLevel;
        }
    }
}
