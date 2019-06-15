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
    /// Contains the possible properties which can be used in a <seealso cref="IHistoricDetailQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class HistoricDetailQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, HistoricDetailQueryProperty> properties = new Dictionary<string, HistoricDetailQueryProperty>();

        public static readonly HistoricDetailQueryProperty PROCESS_INSTANCE_ID = new HistoricDetailQueryProperty("PROC_INST_ID_");
        public static readonly HistoricDetailQueryProperty VARIABLE_NAME = new HistoricDetailQueryProperty("NAME_");
        public static readonly HistoricDetailQueryProperty VARIABLE_TYPE = new HistoricDetailQueryProperty("TYPE_");
        public static readonly HistoricDetailQueryProperty VARIABLE_REVISION = new HistoricDetailQueryProperty("REV_");
        public static readonly HistoricDetailQueryProperty TIME = new HistoricDetailQueryProperty("TIME_");

        private string name;

        public HistoricDetailQueryProperty(string name)
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

        public static HistoricDetailQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }
    }

}