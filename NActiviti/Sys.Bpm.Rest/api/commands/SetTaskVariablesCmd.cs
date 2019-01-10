using Newtonsoft.Json;
using System.Collections.Generic;

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
    public class SetTaskVariablesCmd : Command
    {
        private readonly string id;
        private string taskId;
        private IDictionary<string, object> variables;

        ////[JsonConstructor]
        public SetTaskVariablesCmd([JsonProperty("TaskId")] string taskId, [JsonProperty("Variables")] IDictionary<string, object> variables)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.taskId = taskId;
            this.variables = variables;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
        }
    }

}