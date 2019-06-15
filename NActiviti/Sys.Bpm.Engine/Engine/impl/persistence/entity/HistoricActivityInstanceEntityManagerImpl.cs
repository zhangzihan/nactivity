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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.persistence.entity.data;

    /// 
    /// 
    public class HistoricActivityInstanceEntityManagerImpl : AbstractEntityManager<IHistoricActivityInstanceEntity>, IHistoricActivityInstanceEntityManager
    {

        protected internal IHistoricActivityInstanceDataManager historicActivityInstanceDataManager;


        public HistoricActivityInstanceEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricActivityInstanceDataManager historicActivityInstanceDataManager) : base(processEngineConfiguration)
        {
            this.historicActivityInstanceDataManager = historicActivityInstanceDataManager;
        }

        protected internal override IDataManager<IHistoricActivityInstanceEntity> DataManager
        {
            get
            {
                return historicActivityInstanceDataManager;
            }
        }

        public virtual IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByExecutionAndActivityId(string executionId, string activityId)
        {
            return historicActivityInstanceDataManager.FindUnfinishedHistoricActivityInstancesByExecutionAndActivityId(executionId, activityId);
        }

        public virtual IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByProcessInstanceId(string processInstanceId)
        {
            return historicActivityInstanceDataManager.FindUnfinishedHistoricActivityInstancesByProcessInstanceId(processInstanceId);
        }

        public virtual void DeleteHistoricActivityInstancesByProcessInstanceId(string historicProcessInstanceId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                historicActivityInstanceDataManager.DeleteHistoricActivityInstancesByProcessInstanceId(historicProcessInstanceId);
            }
        }

        public virtual long FindHistoricActivityInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicActivityInstanceDataManager.FindHistoricActivityInstanceCountByNativeQuery(parameterMap);
        }

        public virtual long FindHistoricActivityInstanceCountByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery)
        {
            return historicActivityInstanceDataManager.FindHistoricActivityInstanceCountByQueryCriteria(historicActivityInstanceQuery);
        }

        public virtual IList<IHistoricActivityInstance> FindHistoricActivityInstancesByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery, Page page)
        {
            return historicActivityInstanceDataManager.FindHistoricActivityInstancesByQueryCriteria(historicActivityInstanceQuery, page);
        }

        public virtual IList<IHistoricActivityInstance> FindHistoricActivityInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicActivityInstanceDataManager.FindHistoricActivityInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual IHistoricActivityInstanceDataManager HistoricActivityInstanceDataManager
        {
            get
            {
                return historicActivityInstanceDataManager;
            }
            set
            {
                this.historicActivityInstanceDataManager = value;
            }
        }


    }

}