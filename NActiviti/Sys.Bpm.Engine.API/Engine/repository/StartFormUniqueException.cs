using System;
using System.Runtime.Serialization;

namespace org.activiti.engine.repository
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
    }
}