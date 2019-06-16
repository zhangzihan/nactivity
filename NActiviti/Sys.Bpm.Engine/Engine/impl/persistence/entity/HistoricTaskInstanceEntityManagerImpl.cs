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
    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.history;
    using Sys.Workflow.engine.impl.persistence.entity.data;

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

        public virtual IHistoricTaskInstanceEntity Create(ITaskEntity task, IExecutionEntity execution)
        {
            return historicTaskInstanceDataManager.Create(task, execution);
        }

        public virtual void DeleteHistoricTaskInstancesByProcessInstanceId(string processInstanceId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IList<IHistoricTaskInstanceEntity> taskInstances = historicTaskInstanceDataManager.FindHistoricTaskInstanceByProcessInstanceId(processInstanceId);
                foreach (IHistoricTaskInstanceEntity historicTaskInstanceEntity in taskInstances)
                {
                    Delete(new KeyValuePair<string, object>("id", historicTaskInstanceEntity.Id)); // Needs to be by id (since that method is overridden, see below !)
                }
            }
        }

        public virtual long FindHistoricTaskInstanceCountByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.FindHistoricTaskInstanceCountByQueryCriteria(historicTaskInstanceQuery);
            }
            return 0;
        }

        public virtual IList<IHistoricTaskInstance> FindHistoricTaskInstancesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.FindHistoricTaskInstancesByQueryCriteria(historicTaskInstanceQuery);
            }
            return new List<IHistoricTaskInstance>();
        }

        public virtual IList<IHistoricTaskInstance> FindHistoricTaskInstancesAndVariablesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery)
        {
            if (HistoryManager.HistoryEnabled)
            {
                return historicTaskInstanceDataManager.FindHistoricTaskInstancesAndVariablesByQueryCriteria(historicTaskInstanceQuery);
            }
            return new List<IHistoricTaskInstance>();
        }

        public override void Delete(KeyValuePair<string, object> id)
        {
            if (HistoryManager.HistoryEnabled)
            {
                IHistoricTaskInstanceEntity historicTaskInstance = FindById<IHistoricTaskInstanceEntity>(new KeyValuePair<string, object>("historicTaskInstanceId", id.Value));
                if (historicTaskInstance != null)
                {
                    IList<IHistoricTaskInstanceEntity> subTasks = historicTaskInstanceDataManager.FindHistoricTasksByParentTaskId(historicTaskInstance.Id);
                    foreach (IHistoricTaskInstance subTask in subTasks)
                    {
                        Delete(new KeyValuePair<string, object>("id", subTask.Id));
                    }

                    var sid = id.Value.ToString();
                    HistoricDetailEntityManager.DeleteHistoricDetailsByTaskId(sid);
                    HistoricVariableInstanceEntityManager.DeleteHistoricVariableInstancesByTaskId(sid);
                    CommentEntityManager.DeleteCommentsByTaskId(sid);
                    AttachmentEntityManager.DeleteAttachmentsByTaskId(sid);
                    HistoricIdentityLinkEntityManager.DeleteHistoricIdentityLinksByTaskId(sid);

                    Delete(historicTaskInstance);
                }
            }
        }

        public virtual IList<IHistoricTaskInstance> FindHistoricTaskInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicTaskInstanceDataManager.FindHistoricTaskInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricTaskInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicTaskInstanceDataManager.FindHistoricTaskInstanceCountByNativeQuery(parameterMap);
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