using System;
using System.Runtime.Serialization;

namespace org.activiti.engine.impl.persistence.entity
{
    [Serializable]
    public class ExistsProcessInstanceException : Exception
    {
        public ExistsProcessInstanceException(string processName) : base($"{processName}流程已存在运行记录")
        {
        }
    }
}