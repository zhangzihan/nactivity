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
    public class SignalCmd : Command
    {
        private readonly string id;
        private string name;
        private IDictionary<string, object> inputVariables;

        public SignalCmd()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        [JsonConstructor]
        public SignalCmd([JsonProperty("Name")]string name,
            [JsonProperty("InputVariables")]IDictionary<string, object> inputVariables) : this()
        {
            this.name = name;
            this.inputVariables = inputVariables;
        }

        [JsonConstructor]
        public SignalCmd([JsonProperty("Name")]string name) : this()
        {
            this.name = name;
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

        public virtual IDictionary<string, object> InputVariables
        {
            get
            {
                return inputVariables;
            }
        }
    }
}