using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Engine.Repository
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StartFormUniqueException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="startForm"></param>
        public StartFormUniqueException(string processName, string startForm) :
            base($"Start form must unique, but {startForm} exists {processName}")
        {
        }

        protected StartFormUniqueException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}