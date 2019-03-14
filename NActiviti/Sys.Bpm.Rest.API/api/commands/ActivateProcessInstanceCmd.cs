using Newtonsoft.Json;
using System;

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    /// 激活流程实例命令
    /// </summary>
    public class ActivateProcessInstanceCmd : ICommand
    {

        private readonly string id = "activateProcessInstanceCmd";

        private string processInstanceId;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processInstanceId">实例id</param>
        ////[JsonConstructor]
        public ActivateProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get
            {
                return id;
            }
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
        }
    }

}