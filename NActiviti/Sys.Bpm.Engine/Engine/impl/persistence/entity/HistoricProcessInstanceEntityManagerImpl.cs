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
    using org.activiti.engine.impl.persistence.entity.data;

    /// 
    /// 
    public class HistoricProcessInstanceEntityManagerImpl : AbstractEntityManager<IHistoricProcessInstanceEntity>, IHistoricProcessInstanceEntityManager
    {

        protected internal IHistoricProcessInstanceDataManager historicProcessInstanceDataManager;

        public HistoricProcessInstanceEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricProcessInstanceDataManager historicProcessInstanceDataManager) : base(processEngineConfiguration)
        {
            this.historicProcessInstanceDataManager = historicProcessInstanceDataManager;
        }

        protected internal override IDataManager<IHistoricProcessInstanceEntity> DataManager
        {
            get
            {
                return historicProcessInstanceDataManager;
            }
        }

        public virtual IHistoricProcessInstanceEntity create(IExecutionEntity processInstanceExecutionEntity)
        {
            return historicProcessInstanceDataManager.create(processInstanceExecutionEntity);
        }

        public virtual void deleteHistoricProcessInstanceByProcessDefinitionId(string processDefinitionId)
        {
            if (HistoryManager.HistoryEnabled)
            {
                IList<string> historicProcessInstanceIds = historicProcessInstanceDataManager.findHistoricProcessInstanceIdsByProcessDefinitionId(processDefinitionId);
                foreach (string historicProcessInstanceId in historicProcessInstanceIds)
                {
                    delete(new KeyValuePair<string, object>("id", historicProcessInstanceId));
                }
            }
        }

        public override void delete(KeyValuePair<string, object> id)
        {
            string historicProcessInstanceId = id.Value?.ToString();

            if (HistoryManager.HistoryEnabled)
            {
                if (historicProcessInstanceId == null)
                {
                    return;
                }

                IHistoricProcessInstanceEntity historicProcessInstance = findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", historicProcessInstanceId));

                HistoricDetailEntityManager.deleteHistoricDetailsByProcessInstanceId(historicProcessInstanceId);
                HistoricVariableInstanceEntityManager.deleteHistoricVariableInstanceByProcessInstanceId(historicProcessInstanceId);
                HistoricActivityInstanceEntityManager.deleteHistoricActivityInstancesByProcessInstanceId(historicProcessInstanceId);
                HistoricTaskInstanceEntityManager.deleteHistoricTaskInstancesByProcessInstanceId(historicProcessInstanceId);
                HistoricIdentityLinkEntityManager.deleteHistoricIdentityLinksByProcInstance(historicProcessInstanceId);
                CommentEntityManager.deleteCommentsByProcessInstanceId(historicProcessInstanceId);

                delete(historicProcessInstance, false);

                // Also delete any sub-processes that may be active (ACT-821)

                IList<IHistoricProcessInstanceEntity> selectList = historicProcessInstanceDataManager.findHistoricProcessInstancesBySuperProcessInstanceId(historicProcessInstanceId);
                foreach (IHistoricProcessInstanceEntity child in selectList)
                {
                    delete(new KeyValuePair<string, object>("id", child.Id)); // NEEDS to be by id, to come again through this method!
                }
            }
        }

        public virtual long findHistoricProcessInstanceCountByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.findHistoricProcessInstanceCountByQueryCriteria(historicProcessInstanceQuery);
            }
            return 0;
        }

        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.findHistoricProcessInstancesByQueryCriteria(historicProcessInstanceQuery);
            }
            return new List<IHistoricProcessInstance>();
        }
        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesAndVariablesByQueryCriteria(HistoricProcessInstanceQueryImpl historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.findHistoricProcessInstancesAndVariablesByQueryCriteria(historicProcessInstanceQuery);
            }
            return new List<IHistoricProcessInstance>();
        }

        public virtual IList<IHistoricProcessInstance> findHistoricProcessInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicProcessInstanceDataManager.findHistoricProcessInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricProcessInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicProcessInstanceDataManager.findHistoricProcessInstanceCountByNativeQuery(parameterMap);
        }

        public virtual IHistoricProcessInstanceDataManager HistoricProcessInstanceDataManager
        {
            get
            {
                return historicProcessInstanceDataManager;
            }
            set
            {
                this.historicProcessInstanceDataManager = value;
            }
        }


    }

}