using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
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
 */

namespace Sys.Workflow.engine.impl
{

    using Sys.Workflow.engine.query;
    using Sys.Workflow.engine.runtime;

    /// <summary>
    /// Contains the possible properties that can be used in a <seealso cref="IProcessInstanceQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ProcessInstanceQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, ProcessInstanceQueryProperty> properties = new Dictionary<string, ProcessInstanceQueryProperty>();

        public static readonly ProcessInstanceQueryProperty PROCESS_INSTANCE_ID = new ProcessInstanceQueryProperty("RES.ID_");
        public static readonly ProcessInstanceQueryProperty PROCESS_DEFINITION_KEY = new ProcessInstanceQueryProperty("ProcessDefinitionKey");
        public static readonly ProcessInstanceQueryProperty PROCESS_DEFINITION_ID = new ProcessInstanceQueryProperty("ProcessDefinitionId");
        public static readonly ProcessInstanceQueryProperty TENANT_ID = new ProcessInstanceQueryProperty("RES.TENANT_ID_");
        public static readonly ProcessInstanceQueryProperty PROCESS_EXECUTION_NAME = new ProcessInstanceQueryProperty("RES.NAME_");
        public static readonly ProcessInstanceQueryProperty PROCESS_EXECUTION_STARTDATE = new ProcessInstanceQueryProperty("RES.START_TIME_");

        private string name;

        public ProcessInstanceQueryProperty(string name)
        {
            this.name = name;
            properties[name] = this;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public static ProcessInstanceQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}