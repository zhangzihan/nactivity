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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.persistence.entity.data;

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

	  public virtual IHistoricVariableInstanceEntity copyAndInsert(IVariableInstanceEntity variableInstance)
	  {
		IHistoricVariableInstanceEntity historicVariableInstance = historicVariableInstanceDataManager.create();
		historicVariableInstance.Id = variableInstance.Id;
		historicVariableInstance.ProcessInstanceId = variableInstance.ProcessInstanceId;
		historicVariableInstance.ExecutionId = variableInstance.ExecutionId;
		historicVariableInstance.TaskId = variableInstance.TaskId;
		historicVariableInstance.Revision = variableInstance.Revision;
		historicVariableInstance.Name = variableInstance.Name;
		historicVariableInstance.VariableType = variableInstance.Type;

		copyVariableValue(historicVariableInstance, variableInstance);

		DateTime time = Clock.CurrentTime;
		historicVariableInstance.CreateTime = time;
		historicVariableInstance.LastUpdatedTime = time;

		insert(historicVariableInstance);

		return historicVariableInstance;
	  }

	  public virtual void copyVariableValue(IHistoricVariableInstanceEntity historicVariableInstance, IVariableInstanceEntity variableInstance)
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

	  public override void delete(IHistoricVariableInstanceEntity entity, bool fireDeleteEvent)
	  {
		base.delete(entity, fireDeleteEvent);

		if (entity.ByteArrayRef != null)
		{
		  entity.ByteArrayRef.delete();
		}
	  }

	  public virtual void deleteHistoricVariableInstanceByProcessInstanceId(string historicProcessInstanceId)
	  {
		if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
		{
		  IList<IHistoricVariableInstanceEntity> historicProcessVariables = historicVariableInstanceDataManager.findHistoricVariableInstancesByProcessInstanceId(historicProcessInstanceId);
		  foreach (IHistoricVariableInstanceEntity historicProcessVariable in historicProcessVariables)
		  {
			delete(historicProcessVariable);
		  }
		}
	  }

	  public virtual long findHistoricVariableInstanceCountByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery)
	  {
		return historicVariableInstanceDataManager.findHistoricVariableInstanceCountByQueryCriteria(historicProcessVariableQuery);
	  }

	  public virtual IList<IHistoricVariableInstance> findHistoricVariableInstancesByQueryCriteria(IHistoricVariableInstanceQuery historicProcessVariableQuery, Page page)
	  {
		return historicVariableInstanceDataManager.findHistoricVariableInstancesByQueryCriteria(historicProcessVariableQuery, page);
	  }

	  public virtual IHistoricVariableInstanceEntity findHistoricVariableInstanceByVariableInstanceId(string variableInstanceId)
	  {
		return historicVariableInstanceDataManager.findHistoricVariableInstanceByVariableInstanceId(variableInstanceId);
	  }

	  public virtual void deleteHistoricVariableInstancesByTaskId(string taskId)
	  {
		if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.ACTIVITY))
		{
		  IList<IHistoricVariableInstanceEntity> historicProcessVariables = historicVariableInstanceDataManager.findHistoricVariableInstancesByTaskId(taskId);
		  foreach (IHistoricVariableInstanceEntity historicProcessVariable in historicProcessVariables)
		  {
			delete((IHistoricVariableInstanceEntity) historicProcessVariable);
		  }
		}
	  }

	  public virtual IList<IHistoricVariableInstance> findHistoricVariableInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
	  {
		return historicVariableInstanceDataManager.findHistoricVariableInstancesByNativeQuery(parameterMap, firstResult, maxResults);
	  }

	  public virtual long findHistoricVariableInstanceCountByNativeQuery(IDictionary<string, object> parameterMap)
	  {
		return historicVariableInstanceDataManager.findHistoricVariableInstanceCountByNativeQuery(parameterMap);
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