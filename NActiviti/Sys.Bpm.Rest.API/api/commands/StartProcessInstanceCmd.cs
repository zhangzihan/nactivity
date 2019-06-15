using Newtonsoft.Json;
using org.activiti.services.api.commands;
using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace org.activiti.cloud.services.api.commands
{

    /// <summary>
    /// 流程启动命令
    /// </summary>
    public class StartProcessInstanceCmd : ICommand, IStartProcessInstanceCmd
    {
        private readonly string id = "startProcessInstanceCmd";
        private string processDefinitionKey;
        private string processDefinitionId;
        private string processInstanceName;
        private WorkflowVariable variables;
        private string businessKey;
        private string tenantId;
        private string startForm;


        /// <summary>
        /// 构造函数
        /// </summary>
        public StartProcessInstanceCmd()
        {
        }

        //[JsonConstructor]
        //public StartProcessInstanceCmd([JsonProperty("ProcessDefinitionKey")]string processDefinitionKey,
        //    [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
        //    [JsonProperty("Variables")]IDictionary<string, object> variables,
        //    [JsonProperty("BusinessKey")]string businessKey) : this()
        //{
        //    this.processDefinitionKey = processDefinitionKey;
        //    this.processDefinitionId = processDefinitionId;
        //    this.variables = variables;
        //    this.businessKey = businessKey;
        //}


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processDefinitionId">流程定义id</param>
        /// <param name="variables">流程变量</param>
        //[JsonConstructor]
        public StartProcessInstanceCmd(
            [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("Variables")]WorkflowVariable variables) : this()
        {
            this.processDefinitionId = processDefinitionId;

            this.variables = variables;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processDefinitionId">流程定义id</param>
        //[JsonConstructor]
        public StartProcessInstanceCmd(
            [JsonProperty("ProcessDefinitionId")] string processDefinitionId) : this()
        {
            this.processDefinitionId = processDefinitionId;
        }

        /// <summary>
        /// 命令id
        /// </summary>

        public virtual string Id
        {
            get => id;
        }

        /// <summary>
        /// 流程定义key
        /// </summary>
        public virtual string ProcessDefinitionKey
        {
            get => processDefinitionKey;
            set => processDefinitionKey = value;
        }


        /// <summary>
        /// 流程实例业务名称,方便查询显示.
        /// </summary>
        public string ProcessInstanceName
        {
            get => processInstanceName;
            set => processInstanceName = value;
        }

        /// <summary>
        /// 租户id
        /// </summary>

        public string TenantId
        {
            get => tenantId;
            set => tenantId = value;
        }

        /// <summary>
        /// 流程定义id
        /// </summary>
        public virtual string ProcessDefinitionId
        {
            get => processDefinitionId;
            set => processDefinitionId = value;
        }

        /// <summary>
        /// 流程启动变量
        /// </summary>
        public virtual WorkflowVariable Variables
        {
            get => variables;
            set => variables = value;
        }

        /// <inheritdoc />
        public virtual string ProcessName
        {
            get; set;
        }

        /// <summary>
        /// 业务键值,主要用来保存启动流程时的业务主键,可以是主键id，
        /// 也可以是表单编号,同一流程只要保持唯一性就可以了
        /// </summary>
        public virtual string BusinessKey
        {
            get => businessKey;
            set => businessKey = value;
        }

        /// <summary>
        /// 启动表单,根据表单判断
        /// </summary>
        public string StartForm
        {
            get => startForm;
            set => startForm = value;
        }
    }

}