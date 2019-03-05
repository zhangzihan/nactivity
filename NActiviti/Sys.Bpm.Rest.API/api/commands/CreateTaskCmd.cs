using Newtonsoft.Json;
using System;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.cloud.services.api.commands
{
    public class CreateTaskCmd : ICommand
    {
        private readonly string id = "createTaskCmd";

        ////[JsonConstructor]
        public CreateTaskCmd([JsonProperty("Name")] string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("DueDate")] DateTime? dueDate,
            [JsonProperty("Priority")]int? priority,
            [JsonProperty("Assignee")]string assignee,
            [JsonProperty("ParentTaskId")]string parentTaskId)
        {
            this.id = Guid.NewGuid().ToString();
            this.Name = name;
            this.Description = description;
            this.DueDate = dueDate;
            this.Priority = priority;
            this.Assignee = assignee;
            this.ParentTaskId = parentTaskId;
        }

        public virtual string Id
        {
            get => id;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual DateTime? DueDate
        {
            get;
            set;
        }

        public virtual int? Priority
        {
            get;
            set;
        }

        public virtual string Assignee
        {
            get;
            set;
        }

        public virtual string ParentTaskId
        {
            get;
            set;
        }
    }

}