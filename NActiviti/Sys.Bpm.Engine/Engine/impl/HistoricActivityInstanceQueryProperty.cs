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

    using org.activiti.engine.history;
    using org.activiti.engine.query;

    /// <summary>
    /// Contains the possible properties which can be used in a <seealso cref="IHistoricActivityInstanceQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class HistoricActivityInstanceQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, HistoricActivityInstanceQueryProperty> properties = new Dictionary<string, HistoricActivityInstanceQueryProperty>();

        public static readonly HistoricActivityInstanceQueryProperty HISTORIC_ACTIVITY_INSTANCE_ID = new HistoricActivityInstanceQueryProperty("ID_");
        public static readonly HistoricActivityInstanceQueryProperty PROCESS_INSTANCE_ID = new HistoricActivityInstanceQueryProperty("PROC_INST_ID_");
        public static readonly HistoricActivityInstanceQueryProperty EXECUTION_ID = new HistoricActivityInstanceQueryProperty("EXECUTION_ID_");
        public static readonly HistoricActivityInstanceQueryProperty ACTIVITY_ID = new HistoricActivityInstanceQueryProperty("ACT_ID_");
        public static readonly HistoricActivityInstanceQueryProperty ACTIVITY_NAME = new HistoricActivityInstanceQueryProperty("ACT_NAME_");
        public static readonly HistoricActivityInstanceQueryProperty ACTIVITY_TYPE = new HistoricActivityInstanceQueryProperty("ACT_TYPE_");
        public static readonly HistoricActivityInstanceQueryProperty PROCESS_DEFINITION_ID = new HistoricActivityInstanceQueryProperty("PROC_DEF_ID_");
        public static readonly HistoricActivityInstanceQueryProperty START = new HistoricActivityInstanceQueryProperty("START_TIME_");
        public static readonly HistoricActivityInstanceQueryProperty END = new HistoricActivityInstanceQueryProperty("END_TIME_");
        public static readonly HistoricActivityInstanceQueryProperty DURATION = new HistoricActivityInstanceQueryProperty("DURATION_");
        public static readonly HistoricActivityInstanceQueryProperty TENANT_ID = new HistoricActivityInstanceQueryProperty("TENANT_ID_");

        private string name;

        public HistoricActivityInstanceQueryProperty(string name)
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

        public static HistoricActivityInstanceQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }
    }

}