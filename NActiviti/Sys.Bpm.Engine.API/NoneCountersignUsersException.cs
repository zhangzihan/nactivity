using System;
using System.Runtime.Serialization;

namespace Sys.Workflow
{
    /// <summary>
    /// 无法找到会签人员
    /// </summary>
    [Serializable]
    public class NoneCountersignUsersException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public string ExecutionName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionName"></param>
        public NoneCountersignUsersException(string executionName) : base($"节点{executionName}没有会签人员")
        {
            this.ExecutionName = executionName;
        }

        protected NoneCountersignUsersException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}