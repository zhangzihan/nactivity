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
        public string ExecutionName { get; }

        public NoneCountersignUsersException(string executionName) : base($"节点{executionName}没有会签人员")
        {
            this.ExecutionName = executionName;
        }
    }
}