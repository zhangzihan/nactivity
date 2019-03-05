using System;
using System.Runtime.Serialization;

namespace org.activiti.engine.repository
{
    [Serializable]
    public class StartFormNullException : Exception
    {
        public StartFormNullException(string deployName) :
            base($"{deployName} Process start form is null")
        {
        }
    }
}