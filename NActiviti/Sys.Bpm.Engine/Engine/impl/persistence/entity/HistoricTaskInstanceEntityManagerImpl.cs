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
    public class HistoricTaskInstanceEntityManagerImpl : AbstractEntityManager<IHistoricTaskInstanceEntity>, IHistoricTaskInstanceEntityManager
    {

        protected internal IHistoricTaskInstanceDataManager historicTaskInstanceDataManager;

        public HistoricTaskInstanceEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricTaskInstanceDataManager historicTaskInstanceDataManager) : base(processEngineConfiguration)
        {
            this.historicTaskInstanceDataManager = historicTaskInstanceDataManager;
        }

        protected internal override IDataManager<IHistoricTaskInstanceEntity> DataManager
        {
            get
            {
                return historicTaskInstanceDataManager;
            }
        }

        public virtual IHistoricTaskInstanceEntity create(ITaskEntity task, IExecutionEntity execution)
        {
            return historicTaskInstanceDataManager.create(task, execution);
        }

        public virtual void deleteHistoricTaskInstancesByProcessInstanceId(string processInstanceId)
        {
            if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IList<IHistoricTaskInstanceEntity> taskInstances = historicTaskInstanceDataManager.findHistoricTaskInstanceByProcessInstanceId(processInstanceId);
                foreach (IHistoricTaskInstanceEntity historicTaskInstanceEntity in taskInstances)
                {
                    delete(new KeyValuePair<string, object>("id", historicTaskInstanceEntity.Id)); // Needs to be by id (since that method is overridden, see below !)
                }
            }
        }

        public virtual long findHistoricTaskInstanceCountByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.findHistoricTaskInstanceCountByQueryCriteria(historicTaskInstanceQuery);
            }
            return 0;
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.findHistoricTaskInstancesByQueryCriteria(historicTaskInstanceQuery);
            }
            return new List<IHistoricTaskInstance>();
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesAndVariablesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.findHistoricTaskInstancesAndVariablesByQueryCriteria(historicTaskInstanceQuery);
            }
            return new List<IHistoricTaskInstance>();
        }

        public override void delete(KeyValuePair<string, object> id)
        {
            if (HistoryManager.HistoryEnabled)
            {
                IHistoricTaskInstanceEntity historicTaskInstance = findById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", id.Value));
                if (historicTaskInstance != null)
                {
                    IList<IHistoricTaskInstanceEntity> subTasks = historicTaskInstanceDataManager.findHistoricTasksByParentTaskId(historicTaskInstance.Id);
                    foreach (IHistoricTaskInstance subTask in subTasks)
                    {
                        delete(new KeyValuePair<string, object>("id", subTask.Id));
                    }

                    var sid = id.Value.ToString();
                    HistoricDetailEntityManager.deleteHistoricDetailsByTaskId(sid);
                    HistoricVariableInstanceEntityManager.deleteHistoricVariableInstancesByTaskId(sid);
                    CommentEntityManager.deleteCommentsByTaskId(sid);
                    AttachmentEntityManager.deleteAttachmentsByTaskId(sid);
                    HistoricIdentityLinkEntityManager.deleteHistoricIdentityLinksByTaskId(sid);

                    delete(historicTaskInstance);
                }
            }
        }

        public virtual IList<IHistoricTaskInstance> findHistoricTaskInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicTaskInstanceDataManager.findHistoricTaskInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findHistoricTaskInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicTaskInstanceDataManager.findHistoricTaskInstanceCountByNativeQuery(parameterMap);
        }

        public virtual IHistoricTaskInstanceDataManager HistoricTaskInstanceDataManager
        {
            get
            {
                return historicTaskInstanceDataManager;
            }
            set
            {
                this.historicTaskInstanceDataManager = value;
            }
        }


    }

}