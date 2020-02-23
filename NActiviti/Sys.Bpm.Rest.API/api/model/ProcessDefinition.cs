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

using Newtonsoft.Json;

namespace Sys.Workflow.Cloud.Services.Api.Model
{

    /// <summary>
    /// 流程定义DTO
    /// </summary>
    public class ProcessDefinition
    {
        private string id;
        private string name;
        private string description;
        private int version;
        private string category;
        private string key;
        private string deploymentId;
        private string tenantId;
        private string businessKey;
        private string businessPath;
        private string startForm;


        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcessDefinition()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">流程定义id</param>
        /// <param name="name">流程名称</param>
        /// <param name="description">流程描述</param>
        /// <param name="version">流程版本</param>
        /// <param name="category">流程目录</param>
        /// <param name="key">流程键值</param>
        /// <param name="deploymentId">部署id</param>
        /// <param name="tenantId">租户id</param>
        /// <param name="businessKey">业务键值</param>
        /// <param name="businessPath">业务路径</param>
        /// <param name="startForm">启动表单</param>
        //[JsonConstructor]
        public ProcessDefinition([JsonProperty("Id")]string id,
            [JsonProperty("Name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("Version")]int version,
            [JsonProperty("Category")]string category,
            [JsonProperty("Key")]string key,
            [JsonProperty("Name")]string deploymentId,
            [JsonProperty("TenantId")]string tenantId,
            [JsonProperty("BusinessKey")]string businessKey,
            [JsonProperty("BusinessPath")]string businessPath,
            [JsonProperty("StartForm")]string startForm)
        {
            this.id = id;
            this.name = name;
            this.version = version;
            this.description = description;
            this.category = category;
            this.deploymentId = deploymentId;
            this.key = key;
            this.tenantId = tenantId;
            this.businessKey = businessKey;
            this.businessPath = businessPath;
            this.startForm = startForm;
        }


        /// <summary>
        /// 流程id
        /// </summary>
        public virtual string Id
        {
            get => id;
            set => id = value;
        }


        /// <summary>
        /// 流程名称
        /// </summary>
        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// 流程描述
        /// </summary>

        public virtual string Description
        {
            get => description;
            set => description = value;
        }


        /// <summary>
        /// 流程目录
        /// </summary>
        public virtual string Category
        {
            get => category; set => category = value;
        }

        /// <summary>
        /// 流程键值
        /// </summary>

        public virtual string Key
        {
            get => key; set => key = value;
        }


        /// <summary>
        /// 业务键值
        /// </summary>
        public virtual string BusinessKey
        {
            get => businessKey;
            set => businessKey = value;
        }


        /// <summary>
        /// 业务路径
        /// </summary>
        public virtual string BusinessPath
        {
            get => businessPath;
            set => businessPath = value;
        }


        /// <summary>
        /// 启动表单
        /// </summary>
        public virtual string StartForm
        {
            get => startForm;
            set => startForm = value;
        }


        /// <summary>
        /// 部署id
        /// </summary>
        public virtual string DeploymentId
        {
            get => deploymentId; set => deploymentId = value;
        }


        /// <summary>
        /// 版本
        /// </summary>
        public virtual int Version
        {
            get => version;
            set => version = value;
        }


        /// <summary>
        /// 租户id
        /// </summary>
        public virtual string TenantId
        {
            get => tenantId;
            set => tenantId = value;
        }

        public override string ToString()
        {
            return $"Key={key} Name={name} Version={version} Id={id} TenantId={tenantId}";
        }
    }
}