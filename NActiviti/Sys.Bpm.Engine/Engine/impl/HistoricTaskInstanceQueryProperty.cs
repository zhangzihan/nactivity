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

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.Query;

    /// 
    [Serializable]
    public class HistoricTaskInstanceQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, HistoricTaskInstanceQueryProperty> properties = new Dictionary<string, HistoricTaskInstanceQueryProperty>();

        public static readonly HistoricTaskInstanceQueryProperty HISTORIC_TASK_INSTANCE_ID = new HistoricTaskInstanceQueryProperty("RES.ID_");
        public static readonly HistoricTaskInstanceQueryProperty PROCESS_DEFINITION_ID = new HistoricTaskInstanceQueryProperty("RES.PROC_DEF_ID_");
        public static readonly HistoricTaskInstanceQueryProperty PROCESS_INSTANCE_ID = new HistoricTaskInstanceQueryProperty("RES.PROC_INST_ID_");
        public static readonly HistoricTaskInstanceQueryProperty EXECUTION_ID = new HistoricTaskInstanceQueryProperty("RES.EXECUTION_ID_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_NAME = new HistoricTaskInstanceQueryProperty("RES.NAME_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_BUSINESSKEY = new HistoricTaskInstanceQueryProperty("RES.BUSINESS_KEY_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_DESCRIPTION = new HistoricTaskInstanceQueryProperty("RES.DESCRIPTION_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_ASSIGNEE = new HistoricTaskInstanceQueryProperty("RES.ASSIGNEE_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_OWNER = new HistoricTaskInstanceQueryProperty("RES.OWNER_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_DEFINITION_KEY = new HistoricTaskInstanceQueryProperty("RES.TASK_DEF_KEY_");
        public static readonly HistoricTaskInstanceQueryProperty DELETE_REASON = new HistoricTaskInstanceQueryProperty("RES.DELETE_REASON_");
        public static readonly HistoricTaskInstanceQueryProperty START = new HistoricTaskInstanceQueryProperty("RES.START_TIME_");
        public static readonly HistoricTaskInstanceQueryProperty END = new HistoricTaskInstanceQueryProperty("RES.END_TIME_");
        public static readonly HistoricTaskInstanceQueryProperty DURATION = new HistoricTaskInstanceQueryProperty("RES.DURATION_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_PRIORITY = new HistoricTaskInstanceQueryProperty("RES.PRIORITY_");
        public static readonly HistoricTaskInstanceQueryProperty TASK_DUE_DATE = new HistoricTaskInstanceQueryProperty("RES.DUE_DATE_");
        public static readonly HistoricTaskInstanceQueryProperty TENANT_ID_ = new HistoricTaskInstanceQueryProperty("RES.TENANT_ID_");
        public static readonly HistoricTaskInstanceQueryProperty INCLUDED_VARIABLE_TIME = new HistoricTaskInstanceQueryProperty("VAR.LAST_UPDATED_TIME_");

        private readonly string name;

        public HistoricTaskInstanceQueryProperty(string name)
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

        public static HistoricTaskInstanceQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }
    }

}