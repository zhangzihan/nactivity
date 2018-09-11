using Newtonsoft.Json;
using System;

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

namespace org.activiti.cloud.services.api.model
{
    public class ProcessInstance
    {
        public enum ProcessInstanceStatus
        {
            RUNNING,
            SUSPENDED,
            COMPLETED
        }

        private string id;
        private string name;
        private string description;
        private string processDefinitionId;
        private string processDefinitionKey;
        private string initiator;
        private DateTime? startDate;
        private string businessKey;
        private string status;

        public ProcessInstance()
        {
        }

        [JsonConstructor]
        public ProcessInstance([JsonProperty("Id")]string id,
            [JsonProperty("Name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("Initiator")]string initiator,
            [JsonProperty("StartDate")]DateTime? startDate,
            [JsonProperty("BusinessKey")]string businessKey,
            [JsonProperty("Status")]string status,
            [JsonProperty("ProcessDefinitionKey")]string processDefinitionKey)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.startDate = startDate;
            this.initiator = initiator;
            this.businessKey = businessKey;
            this.status = status;
            this.processDefinitionId = processDefinitionId;
            this.processDefinitionKey = processDefinitionKey;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual string Description
        {
            get
            {
                return description;
            }
        }

        public virtual DateTime? StartDate
        {
            get
            {
                return startDate;
            }
        }

        public virtual string Initiator
        {
            get
            {
                return initiator;
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
        }

        public virtual string Status
        {
            get
            {
                return status;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey;
            }
        }
    }
}