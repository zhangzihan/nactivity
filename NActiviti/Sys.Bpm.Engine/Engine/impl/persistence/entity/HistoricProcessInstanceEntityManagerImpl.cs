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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;

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

        public virtual IHistoricProcessInstanceEntity Create(IExecutionEntity processInstanceExecutionEntity)
        {
            return historicProcessInstanceDataManager.Create(processInstanceExecutionEntity);
        }

        public virtual void DeleteHistoricProcessInstanceByProcessDefinitionId(string processDefinitionId)
        {
            if (HistoryManager.HistoryEnabled)
            {
                IList<string> historicProcessInstanceIds = historicProcessInstanceDataManager.FindHistoricProcessInstanceIdsByProcessDefinitionId(processDefinitionId);
                foreach (string historicProcessInstanceId in historicProcessInstanceIds)
                {
                    Delete(new KeyValuePair<string, object>("id", historicProcessInstanceId));
                }
            }
        }

        public override void Delete(KeyValuePair<string, object> id)
        {
            string historicProcessInstanceId = id.Value?.ToString();

            if (HistoryManager.HistoryEnabled)
            {
                if (historicProcessInstanceId is null)
                {
                    return;
                }

                IHistoricProcessInstanceEntity historicProcessInstance = FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", historicProcessInstanceId));

                HistoricDetailEntityManager.DeleteHistoricDetailsByProcessInstanceId(historicProcessInstanceId);
                HistoricVariableInstanceEntityManager.DeleteHistoricVariableInstanceByProcessInstanceId(historicProcessInstanceId);
                HistoricActivityInstanceEntityManager.DeleteHistoricActivityInstancesByProcessInstanceId(historicProcessInstanceId);
                HistoricTaskInstanceEntityManager.DeleteHistoricTaskInstancesByProcessInstanceId(historicProcessInstanceId);
                HistoricIdentityLinkEntityManager.DeleteHistoricIdentityLinksByProcInstance(historicProcessInstanceId);
                CommentEntityManager.DeleteCommentsByProcessInstanceId(historicProcessInstanceId);

                Delete(historicProcessInstance, false);

                // Also delete any sub-processes that may be active (ACT-821)

                IList<IHistoricProcessInstanceEntity> selectList = historicProcessInstanceDataManager.FindHistoricProcessInstancesBySuperProcessInstanceId(historicProcessInstanceId);
                foreach (IHistoricProcessInstanceEntity child in selectList)
                {
                    Delete(new KeyValuePair<string, object>("id", child.Id)); // NEEDS to be by id, to come again through this method!
                }
            }
        }

        public virtual long FindHistoricProcessInstanceCountByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.FindHistoricProcessInstanceCountByQueryCriteria(historicProcessInstanceQuery);
            }
            return 0;
        }

        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.FindHistoricProcessInstancesByQueryCriteria(historicProcessInstanceQuery);
            }
            return new List<IHistoricProcessInstance>();
        }
        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesAndVariablesByQueryCriteria(IHistoricProcessInstanceQuery historicProcessInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicProcessInstanceDataManager.FindHistoricProcessInstancesAndVariablesByQueryCriteria(historicProcessInstanceQuery);
            }
            return new List<IHistoricProcessInstance>();
        }

        public virtual IList<IHistoricProcessInstance> FindHistoricProcessInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicProcessInstanceDataManager.FindHistoricProcessInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricProcessInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicProcessInstanceDataManager.FindHistoricProcessInstanceCountByNativeQuery(parameterMap);
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