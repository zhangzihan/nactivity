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

namespace org.activiti.cloud.services.api.model
{
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

        public ProcessDefinition()
        {
        }

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

        public virtual string Id
        {
            get => id;
            set => id = value;
        }

        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        public virtual string Description
        {
            get => description;
            set => description = value;
        }

        public virtual string Category
        {
            get => category; set => category = value;
        }

        public virtual string Key
        {
            get => key; set => key = value;
        }

        public virtual string BusinessKey
        {
            get => businessKey;
            set => businessKey = value;
        }

        public virtual string BusinessPath
        {
            get => businessPath;
            set => businessPath = value;
        }

        public virtual string StartForm
        {
            get => startForm;
            set => startForm = value;
        }

        public virtual string DeploymentId
        {
            get => deploymentId; set => deploymentId = value;
        }

        public virtual int Version
        {
            get => version;
            set => version = value;
        }

        public virtual string TenantId
        {
            get => tenantId;
            set => tenantId = value;
        }
    }
}