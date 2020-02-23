using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.Engine.Repository
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StartFormNullException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deployName"></param>
        public StartFormNullException(string deployName) :
            base($"{deployName} Process start form is null")
        {
        }

        protected StartFormNullException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}