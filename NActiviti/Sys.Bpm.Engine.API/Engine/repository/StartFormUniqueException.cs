using System;
using System.Runtime.Serialization;

namespace org.activiti.engine.repository
{
    [Serializable]
    public class StartFormUniqueException : Exception
    {
        public StartFormUniqueException(string processName, string startForm) : 
            base($"Start form must unique, but {startForm} exists {processName}")
        {
        }
    }
}