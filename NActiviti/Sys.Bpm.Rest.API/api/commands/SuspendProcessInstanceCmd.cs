namespace Sys.Workflow.cloud.services.api.commands
{
    using Newtonsoft.Json;
    using System;


    /// <summary>
    /// 挂起流程实例
    /// </summary>
    public class SuspendProcessInstanceCmd : ICommand
    {

        private readonly string id = "suspendProcessInstanceCmd";
        private string processInstanceId;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        //[JsonConstructor]
        public SuspendProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
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
            get
            {
                return processInstanceId;
            }
            set => processInstanceId = value;
        }
    }

}