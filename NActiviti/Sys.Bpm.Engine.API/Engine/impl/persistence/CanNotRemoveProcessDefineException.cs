using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Engine.Impl.Persistence
{
    [Serializable]
    public class CanNotRemoveProcessDefineException : Exception
    {
        public CanNotRemoveProcessDefineException()
        {
        }

        public CanNotRemoveProcessDefineException(string message) : base(message)
        {
        }

        public CanNotRemoveProcessDefineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CanNotRemoveProcessDefineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}