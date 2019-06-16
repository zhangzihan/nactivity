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
    using Sys.Workflow.engine.task;

    /// <summary>
    /// Contains the possible properties that can be used in a <seealso cref="ITaskQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class TaskQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, TaskQueryProperty> properties = new Dictionary<string, TaskQueryProperty>();

        public static readonly TaskQueryProperty TASK_ID = new TaskQueryProperty("RES.ID_");
        public static readonly TaskQueryProperty NAME = new TaskQueryProperty("RES.NAME_");
        public static readonly TaskQueryProperty DESCRIPTION = new TaskQueryProperty("RES.DESCRIPTION_");
        public static readonly TaskQueryProperty PRIORITY = new TaskQueryProperty("RES.PRIORITY_");
        public static readonly TaskQueryProperty ASSIGNEE = new TaskQueryProperty("RES.ASSIGNEE_");
        public static readonly TaskQueryProperty OWNER = new TaskQueryProperty("RES.OWNER_");
        public static readonly TaskQueryProperty CREATE_TIME = new TaskQueryProperty("RES.CREATE_TIME_");
        public static readonly TaskQueryProperty PROCESS_INSTANCE_ID = new TaskQueryProperty("RES.PROC_INST_ID_");
        public static readonly TaskQueryProperty EXECUTION_ID = new TaskQueryProperty("RES.EXECUTION_ID_");
        public static readonly TaskQueryProperty PROCESS_DEFINITION_ID = new TaskQueryProperty("RES.PROC_DEF_ID_");
        public static readonly TaskQueryProperty DUE_DATE = new TaskQueryProperty("RES.DUE_DATE_");
        public static readonly TaskQueryProperty TENANT_ID = new TaskQueryProperty("RES.TENANT_ID_");
        public static readonly TaskQueryProperty TASK_DEFINITION_KEY = new TaskQueryProperty("RES.TASK_DEF_KEY_");

        private string name;

        public TaskQueryProperty(string name)
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

        public static TaskQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}