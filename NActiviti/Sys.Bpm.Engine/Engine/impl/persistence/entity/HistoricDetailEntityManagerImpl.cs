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

	  public virtual IHistoricDetailVariableInstanceUpdateEntity copyAndInsertHistoricDetailVariableInstanceUpdateEntity(IVariableInstanceEntity variableInstance)
	  {
		IHistoricDetailVariableInstanceUpdateEntity historicVariableUpdate = historicDetailDataManager.createHistoricDetailVariableInstanceUpdate();
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

		insert(historicVariableUpdate);
		return historicVariableUpdate;
	  }

	  public override void delete(IHistoricDetailEntity entity, bool fireDeleteEvent)
	  {
		base.delete(entity, fireDeleteEvent);

		if (entity is IHistoricDetailVariableInstanceUpdateEntity)
		{
		  IHistoricDetailVariableInstanceUpdateEntity historicDetailVariableInstanceUpdateEntity = ((IHistoricDetailVariableInstanceUpdateEntity) entity);
		  if (historicDetailVariableInstanceUpdateEntity.ByteArrayRef != null)
		  {
			historicDetailVariableInstanceUpdateEntity.ByteArrayRef.delete();
		  }
		}
	  }

	  public virtual void deleteHistoricDetailsByProcessInstanceId(string historicProcessInstanceId)
	  {
		if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.AUDIT))
		{
		  IList<IHistoricDetailEntity> historicDetails = historicDetailDataManager.findHistoricDetailsByProcessInstanceId(historicProcessInstanceId);

		  foreach (IHistoricDetailEntity historicDetail in historicDetails)
		  {
			delete(historicDetail);
		  }
		}
	  }

	  public virtual long findHistoricDetailCountByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery)
	  {
		return historicDetailDataManager.findHistoricDetailCountByQueryCriteria(historicVariableUpdateQuery);
	  }

	  public virtual IList<IHistoricDetail> findHistoricDetailsByQueryCriteria(IHistoricDetailQuery historicVariableUpdateQuery, Page page)
	  {
		return historicDetailDataManager.findHistoricDetailsByQueryCriteria(historicVariableUpdateQuery, page);
	  }

	  public virtual void deleteHistoricDetailsByTaskId(string taskId)
	  {
		if (HistoryManager.isHistoryLevelAtLeast(HistoryLevel.FULL))
		{
		  IList<IHistoricDetailEntity> details = historicDetailDataManager.findHistoricDetailsByTaskId(taskId);
		  foreach (IHistoricDetail detail in details)
		  {
			delete((IHistoricDetailEntity) detail);
		  }
		}
	  }

	  public virtual IList<IHistoricDetail> findHistoricDetailsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
	  {
		return historicDetailDataManager.findHistoricDetailsByNativeQuery(parameterMap, firstResult, maxResults);
	  }

	  public virtual long findHistoricDetailCountByNativeQuery(IDictionary<string, object> parameterMap)
	  {
		return historicDetailDataManager.findHistoricDetailCountByNativeQuery(parameterMap);
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