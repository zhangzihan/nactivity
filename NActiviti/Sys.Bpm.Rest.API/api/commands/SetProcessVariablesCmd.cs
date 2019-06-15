using Newtonsoft.Json;
using org.activiti.services.api.commands;
using System;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.commands
{

    /// <summary>
    /// 设置流程变量命令
    /// </summary>
    public class SetProcessVariablesCmd : ICommand
    {

        private readonly string id = "setProcessVariablesCmd";
        private string processId;
        private WorkflowVariable variables;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processId">实例id</param>
        /// <param name="variables">变量列表</param>
        ////[JsonConstructor]
        public SetProcessVariablesCmd([JsonProperty("ProcessId")] string processId,
            [JsonProperty("Variables")] WorkflowVariable variables)
        {
            this.id = Guid.NewGuid().ToString();
            this.processId = processId;
            this.variables = variables;
        }


        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get => id;
        }


        /// <summary>
        /// 流程id
        /// </summary>
        public virtual string ProcessId
        {
            get
            {
                return processId;
            }
            set => processId = value;
        }


        /// <summary>
        /// 变量列表
        /// </summary>
        public virtual WorkflowVariable Variables
        {
            get
            {
                return variables;
            }
            set => variables = value;
        }
    }
}