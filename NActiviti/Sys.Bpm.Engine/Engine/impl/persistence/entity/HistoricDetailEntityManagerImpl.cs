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

    public class HistoricDetailEntityManagerImpl : AbstractEntityManager<IHistoricDetailEntity>, IHistoricDetailEntityManager
    {

        protected internal IHistoricDetailDataManager historicDetailDataManager;

        public HistoricDetailEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricDetailDataManager historicDetailDataManager) : base(processEngineConfiguration)
        {
            this.historicDetailDataManager = historicDetailDataManager;
        }

        protected internal override IDataManager<IHistoricDetailEntity> DataManager
        {
            get
            {
                return historicDetailDataManager;
            }
        }

        public virtual IHistoricDetailVariableInstanceUpdateEntity CopyAndInsertHistoricDetailVariableInstanceUpdateEntity(IVariableInstanceEntity variableInstance)
        {
            IHistoricDetailVariableInstanceUpdateEntity historicVariableUpdate = historicDetailDataManager.CreateHistoricDetailVariableInstanceUpdate();
            historicVariableUpdate.ProcessInstanceId = variableInstance.ProcessInstanceId;
            historicVariableUpdate.ExecutionId = variableInstance.ExecutionId;
            historicVariableUpdate.TaskId = variableInstance.TaskId;
            historicVariableUpdate.Time = Clock.CurrentTime;
            historicVariableUpdate.Revision = variableInstance.Revision;
            historicVariableUpdate.Name = variableInstance.Name;
            historicVariableUpdate.VariableType = variableInstance.Type;
            historicVariableUpdate.TextValue = variableInstance.TextValue;
            historicVariableUpdate.TextValue2 = variableInstance.TextValue2;
            historicVariableUpdate.DoubleValue = variableInstance.DoubleValue;
            historicVariableUpdate.LongValue = variableInstance.LongValue;

            if (variableInstance.Bytes != null)
            {
                historicVariableUpdate.Bytes = variableInstance.Bytes;
            }

            Insert(historicVariableUpdate);
            return historicVariableUpdate;
        }

        public override void Delete(IHistoricDetailEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, fireDeleteEvent);

            if (entity is IHistoricDetailVariableInstanceUpdateEntity historicDetailVariableInstanceUpdateEntity)
            {
                if (historicDetailVariableInstanceUpdateEntity.ByteArrayRef != null)
                {
                    historicDetailVariableInstanceUpdateEntity.ByteArrayRef.Delete();
                }
            }
        }

        public virtual void DeleteHistoricDetailsByProcessInstanceId(string historicProcessInstanceId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                IList<IHistoricDetailEntity> historicDetails = historicDetailDataManager.FindHistoricDetailsByProcessInstanceId(historicProcessInstanceId);

                foreach (IHistoricDetailEntity historicDetail in historicDetails)
                {
                    Delete(historicDetail);
                }
            }
        }

        public virtual long FindHistoricDetailCountByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery)
        {
            return historicDetailDataManager.FindHistoricDetailCountByQueryCriteria(historicVariableUpdateQuery);
        }

        public virtual IList<IHistoricDetail> FindHistoricDetailsByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery, Page page)
        {
            return historicDetailDataManager.FindHistoricDetailsByQueryCriteria(historicVariableUpdateQuery, page);
        }

        public virtual void DeleteHistoricDetailsByTaskId(string taskId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.FULL))
            {
                IList<IHistoricDetailEntity> details = historicDetailDataManager.FindHistoricDetailsByTaskId(taskId);
                foreach (IHistoricDetail detail in details)
                {
                    Delete((IHistoricDetailEntity)detail);
                }
            }
        }

        public virtual IList<IHistoricDetail> FindHistoricDetailsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicDetailDataManager.FindHistoricDetailsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricDetailCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicDetailDataManager.FindHistoricDetailCountByNativeQuery(parameterMap);
        }

        public virtual IHistoricDetailDataManager HistoricDetailDataManager
        {
            get
            {
                return historicDetailDataManager;
            }
            set
            {
                this.historicDetailDataManager = value;
            }
        }


    }

}