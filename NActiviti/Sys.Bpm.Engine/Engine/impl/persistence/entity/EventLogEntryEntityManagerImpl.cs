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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.@event;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.persistence.entity.data;

    /// 
    public class EventLogEntryEntityManagerImpl : AbstractEntityManager<IEventLogEntryEntity>, IEventLogEntryEntityManager
    {

        protected internal IEventLogEntryDataManager eventLogEntryDataManager;

        public EventLogEntryEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IEventLogEntryDataManager eventLogEntryDataManager) : base(processEngineConfiguration)
        {
            this.eventLogEntryDataManager = eventLogEntryDataManager;
        }

        protected internal override IDataManager<IEventLogEntryEntity> DataManager
        {
            get
            {
                return eventLogEntryDataManager;
            }
        }

        public virtual IList<IEventLogEntry> FindAllEventLogEntries()
        {
            return eventLogEntryDataManager.FindAllEventLogEntries();
        }

        public virtual IList<IEventLogEntry> FindEventLogEntries(long startLogNr, long pageSize)
        {
            return eventLogEntryDataManager.FindEventLogEntries(startLogNr, pageSize);
        }

        public virtual IList<IEventLogEntry> FindEventLogEntriesByProcessInstanceId(string processInstanceId)
        {
            return eventLogEntryDataManager.FindEventLogEntriesByProcessInstanceId(processInstanceId);
        }

        public virtual void DeleteEventLogEntry(long logNr)
        {
            eventLogEntryDataManager.DeleteEventLogEntry(logNr);
        }

        public virtual IEventLogEntryDataManager EventLogEntryDataManager
        {
            get
            {
                return eventLogEntryDataManager;
            }
            set
            {
                this.eventLogEntryDataManager = value;
            }
        }
    }
}