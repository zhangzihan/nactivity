using Newtonsoft.Json;
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
    public class StartProcessInstanceCmd : Command
    {
        private readonly string id;
        private string processDefinitionKey;
        private string processDefinitionId;
        private IDictionary<string, object> variables;
        private string businessKey;

        public StartProcessInstanceCmd()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        [JsonConstructor]
        public StartProcessInstanceCmd([JsonProperty("ProcessDefinitionKey")]string processDefinitionKey,
            [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("Variables")]IDictionary<string, object> variables,
            [JsonProperty("BusinessKey")]string businessKey) : this()
        {
            this.processDefinitionKey = processDefinitionKey;
            this.processDefinitionId = processDefinitionId;
            this.variables = variables;
            this.businessKey = businessKey;
        }

        [JsonConstructor]
        public StartProcessInstanceCmd([JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("Variables")]IDictionary<string, object> variables) : this()
        {
            this.processDefinitionId = processDefinitionId;

            this.variables = variables;
        }

        [JsonConstructor]
        public StartProcessInstanceCmd([JsonProperty("ProcessDefinitionId")] string processDefinitionId) : this()
        {
            this.processDefinitionId = processDefinitionId;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
        }
    }

}