using System;
using Newtonsoft.Json;

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
    public class TaskModel
    {
        public enum TaskStatus
        {
            CREATED,
            ASSIGNED,
            SUSPENDED,
            CANCELLED
        }

        private string id;
        private string owner;
        private string assignee;
        private string name;
        private string description;
        private DateTime? createdDate;
        private DateTime? claimedDate;
        private DateTime? dueDate;
        private int? priority;
        private string processDefinitionId;
        private string processInstanceId;
        private string parentTaskId;
        private TaskStatus status;
        private string formKey;

        public TaskModel()
        {
        }

        //[JsonConstructor]
        public TaskModel([JsonProperty("Id")]string id,
            [JsonProperty("Owner")]string owner,
            [JsonProperty("Assignee")]string assignee,
            [JsonProperty("name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("CreatedDate")]DateTime? createdDate,
            [JsonProperty("ClaimedDate")]DateTime? claimedDate,
            [JsonProperty("DueDate")]DateTime? dueDate,
            [JsonProperty("Priority")]int? priority,
            [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("ProcessInstanceId")]string processInstanceId,
            [JsonProperty("ParentTaskId")]string parentTaskId,
            [JsonProperty("FormKey")]string formKey,
            [JsonProperty("Status")]TaskStatus status)
        {
            this.id = id;
            this.owner = owner;
            this.assignee = assignee;
            this.name = name;
            this.description = description;
            this.createdDate = createdDate;
            this.claimedDate = claimedDate;
            this.dueDate = dueDate;
            this.priority = priority;
            this.processDefinitionId = processDefinitionId;
            this.processInstanceId = processInstanceId;
            this.parentTaskId = parentTaskId;
            this.formKey = formKey;
            this.status = status;
        }

        public virtual string Id
        {
            get => id;
            set => id = value;
        }


        public virtual string Owner
        {
            get => owner;
            set => owner = value;
        }

        public virtual string Assignee
        {
            get => assignee;
            set => assignee = value;
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

        public virtual DateTime? CreatedDate
        {
            get => createdDate;
            set => createdDate = value;
        }

        public virtual DateTime? ClaimedDate
        {
            get => claimedDate;
            set => claimedDate = value;
        }

        public virtual DateTime? DueDate
        {
            get => dueDate;
            set => dueDate = value;
        }

        public virtual int? Priority
        {
            get => priority;
            set => priority = value;
        }

        public virtual string ProcessDefinitionId
        {
            get => processDefinitionId;
            set => processDefinitionId = value;
        }

        public virtual string ProcessInstanceId
        {
            get => processInstanceId;
            set => processInstanceId = value;
        }

        public virtual TaskStatus Status
        {
            get => status;
            set => status = value;
        }

        public virtual string ParentTaskId
        {
            get => parentTaskId;
            set => parentTaskId = value;
        }

        public virtual string FormKey
        {
            get => formKey;
            set => formKey = value;
        }
    }
}