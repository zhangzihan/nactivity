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

namespace Sys.Workflow.Engine.Impl.Cmd
{

	using Sys.Workflow.Engine.Impl.Interceptor;
	using Sys.Workflow.Engine.Impl.Persistence.Entity;
	using Sys.Workflow.Engine.Runtime;
	using Sys.Workflow.Engine.Tasks;
	using System.Linq;

	/// 
	[Serializable]
	public class GetProcessInstanceByBusinessKeyCmd : ICommand<IProcessInstance>
	{
		private const long serialVersionUID = 1L;
		private readonly string businessKey;
		private readonly ISuspensionState suspensionState;

		public GetProcessInstanceByBusinessKeyCmd(string businessKey, ISuspensionState suspensionState = null)
		{
			this.businessKey = businessKey;
			this.suspensionState = suspensionState;
		}

		public virtual IProcessInstance Execute(ICommandContext commandContext)
		{
			var runtimeService = commandContext.ProcessEngineConfiguration.RuntimeService;

			IProcessInstanceQuery query = runtimeService.CreateProcessInstanceQuery();
			query = query.SetProcessInstanceBusinessKey(businessKey);
			if (suspensionState is not null)
			{
				query.Suspended();
			}

			IProcessInstance processInstance = query.List().FirstOrDefault();

			if (processInstance is not null)
			{
				ExecutionEntityImpl.EnsureStarterInitialized(new ExecutionEntityImpl[] { processInstance as ExecutionEntityImpl });
			}

			return processInstance;
		}
	}
}