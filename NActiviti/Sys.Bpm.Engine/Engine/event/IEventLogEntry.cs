using System;

namespace org.activiti.engine.@event
{

    /// 
    public interface IEventLogEntry
    {

        long LogNumber { get; }

        string Type { get; }

        string ProcessDefinitionId { get; }

        string ProcessInstanceId { get; }

        string ExecutionId { get; }

        string TaskId { get; }

        DateTime? TimeStamp { get; }

        string UserId { get; }

        byte[] Data { get; }

    }

}