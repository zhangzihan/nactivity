using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Engine.Exceptions
{
    [Serializable]
    public class TerminateProcessInstanceException : Exception
    {

        public TerminateProcessInstanceException(string message) : base(message)
        {
        }

        protected TerminateProcessInstanceException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}