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
namespace Sys.Workflow.engine.impl.persistence.entity.data.impl
{

    using Sys.Workflow.engine.@event;
    using Sys.Workflow.engine.impl.cfg;

    /// 
    public class MybatisEventLogEntryDataManager : AbstractDataManager<IEventLogEntryEntity>, IEventLogEntryDataManager
    {

        public MybatisEventLogEntryDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(EventLogEntryEntityImpl);
            }
        }

        public override IEventLogEntryEntity Create()
        {
            return new EventLogEntryEntityImpl();
        }

        public virtual IList<IEventLogEntry> FindAllEventLogEntries()
        {
            return DbSqlSession.SelectList<EventLogEntryEntityImpl, IEventLogEntry>("selectAllEventLogEntries");
        }

        public virtual IList<IEventLogEntry> FindEventLogEntries(long startLogNr, long pageSize)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["startLogNr"] = startLogNr
            };
            if (pageSize > 0)
            {
                @params["endLogNr"] = startLogNr + pageSize + 1;
            }
            return DbSqlSession.SelectList<EventLogEntryEntityImpl, IEventLogEntry>("selectEventLogEntries", @params);
        }

        public virtual IList<IEventLogEntry> FindEventLogEntriesByProcessInstanceId(string processInstanceId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["processInstanceId"] = processInstanceId
            };
            return DbSqlSession.SelectList<EventLogEntryEntityImpl, IEventLogEntry>("selectEventLogEntriesByProcessInstanceId", @params);
        }

        public virtual void DeleteEventLogEntry(long logNr)
        {
            DbSqlSession.Delete("deleteEventLogEntry", new { logNr }, typeof(EventLogEntryEntityImpl));
        }
    }

}