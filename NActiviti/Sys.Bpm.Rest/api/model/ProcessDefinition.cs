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

        public ProcessDefinition()
        {
        }

        //[JsonConstructor]
        public ProcessDefinition([JsonProperty("Id")]string id,
            [JsonProperty("Name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("Version")]int version)
        {
            this.id = id;
            this.name = name;
            this.version = version;
            this.description = description;
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

        public virtual int Version
        {
            get
            {
                return version;
            }
        }
    }
}