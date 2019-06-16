using System;
using System.Runtime.Serialization;

namespace Sys.Workflow.engine.impl.persistence.entity
{
    [Serializable]
    public class ExistsProcessInstanceException : Exception
    {
        public ExistsProcessInstanceException(string processName) : base($"{processName}流程已存在运行记录")
        {
        }
    }
}