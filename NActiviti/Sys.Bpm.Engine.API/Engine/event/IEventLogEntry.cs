using System;

namespace Sys.Workflow.engine.@event
{

    /// 
    public interface IEventLogEntry
    {
        /// <summary>
        /// 
        /// </summary>
        long LogNumber { get; }

        /// <summary>
        /// 
        /// </summary>
        string Type { get; }

        /// <summary>
        /// 
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// 
        /// </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// 
        /// </summary>
        string ExecutionId { get; }

        /// <summary>
        /// 
        /// </summary>
        string TaskId { get; }

        /// <summary>
        /// 
        /// </summary>
        DateTime? TimeStamp { get; }

        /// <summary>
        /// 
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// 
        /// </summary>
        byte[] Data { get; }
    }
}