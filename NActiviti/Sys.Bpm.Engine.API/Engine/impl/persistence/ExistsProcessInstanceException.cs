using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    [Serializable]
    public class ExistsProcessInstanceException : Exception
    {
        public ExistsProcessInstanceException(string processName) : base($"{processName}流程已存在运行记录")
        {
        }

        protected ExistsProcessInstanceException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}