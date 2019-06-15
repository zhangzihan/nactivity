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

namespace org.activiti.engine.impl
{

    using org.activiti.engine.query;
    using org.activiti.engine.repository;

    /// <summary>
    /// Contains the possible properties that can be used in a <seealso cref="IProcessDefinitionQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ProcessDefinitionQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, ProcessDefinitionQueryProperty> properties = new Dictionary<string, ProcessDefinitionQueryProperty>();

        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_KEY = new ProcessDefinitionQueryProperty("RES.KEY_");
        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_CATEGORY = new ProcessDefinitionQueryProperty("RES.CATEGORY_");
        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_ID = new ProcessDefinitionQueryProperty("RES.ID_");
        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_VERSION = new ProcessDefinitionQueryProperty("RES.VERSION_");
        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_NAME = new ProcessDefinitionQueryProperty("RES.NAME_");
        public static readonly ProcessDefinitionQueryProperty DEPLOYMENT_ID = new ProcessDefinitionQueryProperty("RES.DEPLOYMENT_ID_");
        public static readonly ProcessDefinitionQueryProperty PROCESS_DEFINITION_TENANT_ID = new ProcessDefinitionQueryProperty("RES.TENANT_ID_");

        private readonly string name;

        public ProcessDefinitionQueryProperty(string name)
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

        public static ProcessDefinitionQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}