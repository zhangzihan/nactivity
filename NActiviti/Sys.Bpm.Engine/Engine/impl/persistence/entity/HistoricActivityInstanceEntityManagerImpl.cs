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

        public virtual IList<IHistoricActivityInstanceEntity> findUnfinishedHistoricActivityInstancesByExecutionAndActivityId(string executionId, string activityId)
        {
            return historicActivityInstanceDataManager.findUnfinishedHistoricActivityInstancesByExecutionAndActivityId(executionId, activityId);
        }

        public virtual IList<IHistoricActivityInstanceEntity> findUnfinishedHistoricActivityInstancesByProcessInstanceId(string processInstanceId)
        {
            return historicActivityInstanceDataManager.findUnfinishedHistoricActivityInstancesByProcessInstanceId(processInstanceId);
        }

        public virtual void deleteHistoricActivityInstancesByProcessInstanceId(string historicProcessInstanceId)
        {
            if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                historicActivityInstanceDataManager.deleteHistoricActivityInstancesByProcessInstanceId(historicProcessInstanceId);
            }
        }

        public virtual long findHistoricActivityInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicActivityInstanceDataManager.findHistoricActivityInstanceCountByNativeQuery(parameterMap);
        }

        public virtual long findHistoricActivityInstanceCountByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery)
        {
            return historicActivityInstanceDataManager.findHistoricActivityInstanceCountByQueryCriteria(historicActivityInstanceQuery);
        }

        public virtual IList<IHistoricActivityInstance> findHistoricActivityInstancesByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery, Page page)
        {
            return historicActivityInstanceDataManager.findHistoricActivityInstancesByQueryCriteria(historicActivityInstanceQuery, page);
        }

        public virtual IList<IHistoricActivityInstance> findHistoricActivityInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicActivityInstanceDataManager.findHistoricActivityInstancesByNativeQuery(parameterMap, firstResult, maxResults);
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