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

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow.engine.task;
    using System.Linq;

    /// 
    [Serializable]
    public class GetProcessInstanceByIdCmd : ICommand<IProcessInstance>
    {
        private const long serialVersionUID = 1L;
        protected internal string instanceId;

        public GetProcessInstanceByIdCmd(string instanceId)
        {
            this.instanceId = instanceId;
        }

        public virtual IProcessInstance Execute(ICommandContext commandContext)
        {
            var runtimeService = commandContext.ProcessEngineConfiguration.RuntimeService;

            IProcessInstanceQuery query = runtimeService.CreateProcessInstanceQuery();
            query = query.SetProcessInstanceId(instanceId);
            IProcessInstance processInstance = query.SingleResult();

            if (processInstance != null)
            {
                ExecutionEntityImpl.EnsureStarterInitialized(new ExecutionEntityImpl[] { processInstance as ExecutionEntityImpl });
            }

            return processInstance;
        }
    }
}