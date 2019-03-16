using System;

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
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    [Serializable]
	public class SetProcessInstanceNameCmd : ICommand<object>
	{

	  private const long serialVersionUID = 1L;

	  protected internal string processInstanceId;
	  protected internal string name;

	  public SetProcessInstanceNameCmd(string processInstanceId, string name)
	  {
		this.processInstanceId = processInstanceId;
		this.name = name;
	  }

	  public  virtual object  execute(ICommandContext commandContext)
	  {
		if (ReferenceEquals(processInstanceId, null))
		{
		  throw new ActivitiIllegalArgumentException("processInstanceId is null");
		}

		IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(processInstanceId);

		if (execution == null)
		{
		  throw new ActivitiObjectNotFoundException("process instance " + processInstanceId + " doesn't exist", typeof(IProcessInstance));
		}

		if (!execution.ProcessInstanceType)
		{
		  throw new ActivitiObjectNotFoundException("process instance " + processInstanceId + " doesn't exist, the given ID references an execution, though", typeof(IProcessInstance));
		}

		if (execution.Suspended)
		{
		  throw new ActivitiException("process instance " + processInstanceId + " is suspended, cannot set name");
		}

		// Actually set the name
		execution.Name = name;

		// Record the change in history
		commandContext.HistoryManager.recordProcessInstanceNameChange(processInstanceId, name);

		return null;
	  }

	}

}