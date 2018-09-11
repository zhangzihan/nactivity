using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.impl
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;

    /// 
    /// 
    /// 
    public class HistoryServiceImpl : ServiceImpl, IHistoryService
	{

	  public HistoryServiceImpl()
	  {

	  }

	  public HistoryServiceImpl(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
	  {
	  }

	  public virtual IHistoricProcessInstanceQuery createHistoricProcessInstanceQuery()
	  {
		return new HistoricProcessInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricActivityInstanceQuery createHistoricActivityInstanceQuery()
	  {
		return new HistoricActivityInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricTaskInstanceQuery createHistoricTaskInstanceQuery()
	  {
		return new HistoricTaskInstanceQueryImpl(commandExecutor, processEngineConfiguration.DatabaseType);
	  }

	  public virtual IHistoricDetailQuery createHistoricDetailQuery()
	  {
		return new HistoricDetailQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricDetailQuery createNativeHistoricDetailQuery()
	  {
		return new NativeHistoricDetailQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricVariableInstanceQuery createHistoricVariableInstanceQuery()
	  {
		return new HistoricVariableInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricVariableInstanceQuery createNativeHistoricVariableInstanceQuery()
	  {
		return new NativeHistoricVariableInstanceQueryImpl(commandExecutor);
	  }

	  public virtual void deleteHistoricTaskInstance(string taskId)
	  {
		commandExecutor.execute(new DeleteHistoricTaskInstanceCmd(taskId));
	  }

	  public virtual void deleteHistoricProcessInstance(string processInstanceId)
	  {
		commandExecutor.execute(new DeleteHistoricProcessInstanceCmd(processInstanceId));
	  }

	  public virtual INativeHistoricProcessInstanceQuery createNativeHistoricProcessInstanceQuery()
	  {
		return new NativeHistoricProcessInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricTaskInstanceQuery createNativeHistoricTaskInstanceQuery()
	  {
		return new NativeHistoricTaskInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricActivityInstanceQuery createNativeHistoricActivityInstanceQuery()
	  {
		return new NativeHistoricActivityInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IList<IHistoricIdentityLink> getHistoricIdentityLinksForProcessInstance(string processInstanceId)
	  {
		return commandExecutor.execute(new GetHistoricIdentityLinksForTaskCmd(null, processInstanceId));
	  }

	  public virtual IList<IHistoricIdentityLink> getHistoricIdentityLinksForTask(string taskId)
	  {
		return commandExecutor.execute(new GetHistoricIdentityLinksForTaskCmd(taskId, null));
	  }

	  public virtual IProcessInstanceHistoryLogQuery createProcessInstanceHistoryLogQuery(string processInstanceId)
	  {
		return new ProcessInstanceHistoryLogQueryImpl(commandExecutor, processInstanceId);
	  }

	}

}