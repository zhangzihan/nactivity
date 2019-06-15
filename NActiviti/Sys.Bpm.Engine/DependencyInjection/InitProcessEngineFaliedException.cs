using System;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
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
    }
}
