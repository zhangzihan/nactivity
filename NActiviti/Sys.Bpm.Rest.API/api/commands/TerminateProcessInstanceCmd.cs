namespace Sys.Workflow.Cloud.Services.Api.Commands
{
    using Newtonsoft.Json;
    using Sys.Workflow.Services.Api.Commands;
    using System;


    /// <summary>
    /// 终止流程实例
    /// </summary>
    public class TerminateProcessInstanceCmd : ICommand
    {
        private readonly string id = "terminateProcessInstanceCmd";

        public TerminateProcessInstanceCmd()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <param name="reason">终止原因</param>
        //[JsonConstructor]
        public TerminateProcessInstanceCmd([JsonProperty("ProcessInstanceId")] string processInstanceId,
            [JsonProperty("BusinessKey")] string businessKey,
            [JsonProperty("Reason")] string reason,
            [JsonProperty("OutputVariables")] WorkflowVariable outputVariables)
        {
            BusinessKey = businessKey;
            ProcessInstanceId = processInstanceId;
            Reason = reason;
            OutputVariables = outputVariables;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get => id;
        }


        /// <summary>
        /// 流程实例id
        /// </summary>
        public virtual string ProcessInstanceId
        {
            get;
            set;
        }


        /// <summary>
        /// 业务主键
        /// </summary>
        public virtual string BusinessKey
        {
            get;
            set;
        }

        /// <summary>
        /// 终止原因
        /// </summary>
        public virtual string Reason
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorkflowVariable OutputVariables { get; set; }
    }

}