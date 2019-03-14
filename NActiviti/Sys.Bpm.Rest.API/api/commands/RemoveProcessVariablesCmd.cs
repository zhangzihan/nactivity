using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.commands
{

    /// <summary>
    /// 删除流程数据变量命令
    /// </summary>
    public class RemoveProcessVariablesCmd : ICommand
    {
        private readonly string id = "removeProcessVariablesCmd";
        private string processId;
        private IList<string> variableNames;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="variableNames"></param>
        ////[JsonConstructor]
        public RemoveProcessVariablesCmd([JsonProperty("ProcessId")] string processId,
            [JsonProperty("Variables")]IList<string> variableNames)
        {
            this.processId = processId;
            this.variableNames = variableNames;
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
            get => processId;
            set => processId = value;
        }


        /// <summary>
        /// 带移除变量列表
        /// </summary>
        public virtual IList<string> VariableNames
        {
            get => variableNames;
            set => variableNames = value;
        }
    }
}