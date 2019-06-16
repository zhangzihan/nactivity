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
    /// Contains the possible properties that can be used in a <seealso cref="IExecutionQuery"/> .
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ExecutionQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, ExecutionQueryProperty> properties = new Dictionary<string, ExecutionQueryProperty>();

        public static readonly ExecutionQueryProperty PROCESS_INSTANCE_ID = new ExecutionQueryProperty("RES.ID_");
        public static readonly ExecutionQueryProperty PROCESS_DEFINITION_KEY = new ExecutionQueryProperty("ProcessDefinitionKey");
        public static readonly ExecutionQueryProperty PROCESS_DEFINITION_ID = new ExecutionQueryProperty("ProcessDefinitionId");
        public static readonly ExecutionQueryProperty TENANT_ID = new ExecutionQueryProperty("RES.TENANT_ID_");

        private string name;

        public ExecutionQueryProperty(string name)
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

        public static ExecutionQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}