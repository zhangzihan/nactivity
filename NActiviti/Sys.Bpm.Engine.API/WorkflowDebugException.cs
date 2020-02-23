using Sys.Workflow.Engine.Impl.Persistence.Entity;
using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Exceptions
{
    [Serializable]
    public class WorkflowDebugException : Exception
    {
        public WorkflowDebugException(string userId, IExecutionEntity execution, Exception innerException) : base("发生错误,查看InnerExeception", innerException)
        {
            UserId = userId;
            Execution = execution;
        }

        public string UserId { get; }

        public IExecutionEntity Execution { get; }

        protected WorkflowDebugException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}