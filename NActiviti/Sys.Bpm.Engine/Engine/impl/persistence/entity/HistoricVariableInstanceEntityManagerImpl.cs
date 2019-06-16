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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Histories;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;

    /// 
    /// 
    public class HistoricVariableInstanceEntityManagerImpl : AbstractEntityManager<IHistoricVariableInstanceEntity>, IHistoricVariableInstanceEntityManager
    {

        protected internal IHistoricVariableInstanceDataManager historicVariableInstanceDataManager;

        public HistoricVariableInstanceEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IHistoricVariableInstanceDataManager historicVariableInstanceDataManager) : base(processEngineConfiguration)
        {
            this.historicVariableInstanceDataManager = historicVariableInstanceDataManager;
        }

        protected internal override IDataManager<IHistoricVariableInstanceEntity> DataManager
        {
            get
            {
                return historicVariableInstanceDataManager;
            }
        }

        public virtual IHistoricVariableInstanceEntity CopyAndInsert(IVariableInstanceEntity variableInstance)
        {
            IHistoricVariableInstanceEntity historicVariableInstance = historicVariableInstanceDataManager.Create();
            historicVariableInstance.Id = variableInstance.Id;
            historicVariableInstance.ProcessInstanceId = variableInstance.ProcessInstanceId;
            historicVariableInstance.ExecutionId = variableInstance.ExecutionId;
            historicVariableInstance.TaskId = variableInstance.TaskId;
            historicVariableInstance.Revision = variableInstance.Revision;
            historicVariableInstance.Name = variableInstance.Name;
            historicVariableInstance.VariableType = variableInstance.Type;

            CopyVariableValue(historicVariableInstance, variableInstance);

            DateTime time = Clock.CurrentTime;
            historicVariableInstance.CreateTime = time;
            historicVariableInstance.LastUpdatedTime = time;

            Insert(historicVariableInstance);

            return historicVariableInstance;
        }

        public virtual void CopyVariableValue(IHistoricVariableInstanceEntity historicVariableInstance, IVariableInstanceEntity variableInstance)
        {
            historicVariableInstance.TextValue = variableInstance.TextValue;
            historicVariableInstance.TextValue2 = variableInstance.TextValue2;
            historicVariableInstance.DoubleValue = variableInstance.DoubleValue;
            historicVariableInstance.LongValue = variableInstance.LongValue;

            historicVariableInstance.VariableType = variableInstance.Type;
            if (variableInstance.ByteArrayRef != null)
            {
                historicVariableInstance.Bytes = variableInstance.Bytes;
            }

            historicVariableInstance.LastUpdatedTime = Clock.CurrentTime;
        }

        public override void Delete(IHistoricVariableInstanceEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, fireDeleteEvent);

            if (entity.ByteArrayRef != null)
            {
                entity.ByteArrayRef.Delete();
            }
        }

        public virtual void DeleteHistoricVariableInstanceByProcessInstanceId(string historicProcessInstanceId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IList<IHistoricVariableInstanceEntity> historicProcessVariables = historicVariableInstanceDataManager.FindHistoricVariableInstancesByProcessInstanceId(historicProcessInstanceId);
                foreach (IHistoricVariableInstanceEntity historicProcessVariable in historicProcessVariables)
                {
                    Delete(historicProcessVariable);
                }
            }
        }

        public virtual long FindHistoricVariableInstanceCountByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery)
        {
            return historicVariableInstanceDataManager.FindHistoricVariableInstanceCountByQueryCriteria(historicProcessVariableQuery);
        }

        public virtual IList<IHistoricVariableInstance> FindHistoricVariableInstancesByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery, Page page)
        {
            return historicVariableInstanceDataManager.FindHistoricVariableInstancesByQueryCriteria(historicProcessVariableQuery, page);
        }

        public virtual IHistoricVariableInstanceEntity FindHistoricVariableInstanceByVariableInstanceId(string variableInstanceId)
        {
            return historicVariableInstanceDataManager.FindHistoricVariableInstanceByVariableInstanceId(variableInstanceId);
        }

        public virtual void DeleteHistoricVariableInstancesByTaskId(string taskId)
        {
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
            {
                IList<IHistoricVariableInstanceEntity> historicProcessVariables = historicVariableInstanceDataManager.FindHistoricVariableInstancesByTaskId(taskId);
                foreach (IHistoricVariableInstanceEntity historicProcessVariable in historicProcessVariables)
                {
                    Delete(historicProcessVariable);
                }
            }
        }

        public virtual IList<IHistoricVariableInstance> FindHistoricVariableInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return historicVariableInstanceDataManager.FindHistoricVariableInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindHistoricVariableInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return historicVariableInstanceDataManager.FindHistoricVariableInstanceCountByNativeQuery(parameterMap);
        }


        public virtual IHistoricVariableInstanceDataManager HistoricVariableInstanceDataManager
        {
            get
            {
                return historicVariableInstanceDataManager;
            }
            set
            {
                this.historicVariableInstanceDataManager = value;
            }
        }



    }

}