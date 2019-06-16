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

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cmd;

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

	  public virtual IHistoricProcessInstanceQuery CreateHistoricProcessInstanceQuery()
	  {
		return new HistoricProcessInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricActivityInstanceQuery CreateHistoricActivityInstanceQuery()
	  {
		return new HistoricActivityInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricTaskInstanceQuery CreateHistoricTaskInstanceQuery()
	  {
		return new HistoricTaskInstanceQueryImpl(commandExecutor, processEngineConfiguration.DatabaseType);
	  }

	  public virtual IHistoricDetailQuery CreateHistoricDetailQuery()
	  {
		return new HistoricDetailQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricDetailQuery CreateNativeHistoricDetailQuery()
	  {
		return new NativeHistoricDetailQueryImpl(commandExecutor);
	  }

	  public virtual IHistoricVariableInstanceQuery CreateHistoricVariableInstanceQuery()
	  {
		return new HistoricVariableInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricVariableInstanceQuery CreateNativeHistoricVariableInstanceQuery()
	  {
		return new NativeHistoricVariableInstanceQueryImpl(commandExecutor);
	  }

	  public virtual void DeleteHistoricTaskInstance(string taskId)
	  {
		commandExecutor.Execute(new DeleteHistoricTaskInstanceCmd(taskId));
	  }

	  public virtual void DeleteHistoricProcessInstance(string processInstanceId)
	  {
		commandExecutor.Execute(new DeleteHistoricProcessInstanceCmd(processInstanceId));
	  }

	  public virtual INativeHistoricProcessInstanceQuery CreateNativeHistoricProcessInstanceQuery()
	  {
		return new NativeHistoricProcessInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricTaskInstanceQuery CreateNativeHistoricTaskInstanceQuery()
	  {
		return new NativeHistoricTaskInstanceQueryImpl(commandExecutor);
	  }

	  public virtual INativeHistoricActivityInstanceQuery CreateNativeHistoricActivityInstanceQuery()
	  {
		return new NativeHistoricActivityInstanceQueryImpl(commandExecutor);
	  }

	  public virtual IList<IHistoricIdentityLink> GetHistoricIdentityLinksForProcessInstance(string processInstanceId)
	  {
		return commandExecutor.Execute(new GetHistoricIdentityLinksForTaskCmd(null, processInstanceId));
	  }

	  public virtual IList<IHistoricIdentityLink> GetHistoricIdentityLinksForTask(string taskId)
	  {
		return commandExecutor.Execute(new GetHistoricIdentityLinksForTaskCmd(taskId, null));
	  }

	  public virtual IProcessInstanceHistoryLogQuery CreateProcessInstanceHistoryLogQuery(string processInstanceId)
	  {
		return new ProcessInstanceHistoryLogQueryImpl(commandExecutor, processInstanceId);
	  }

	}

}