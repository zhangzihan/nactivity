using System;
using System.Runtime.Serialization;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class InitProcessEngineFaliedException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public InitProcessEngineFaliedException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public InitProcessEngineFaliedException(string message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InitProcessEngineFaliedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private InitProcessEngineFaliedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
