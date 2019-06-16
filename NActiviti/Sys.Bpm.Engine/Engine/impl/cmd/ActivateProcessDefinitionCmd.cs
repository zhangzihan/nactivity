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
namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;

    /// 
    /// 
    public class ActivateProcessDefinitionCmd : AbstractSetProcessDefinitionStateCmd
    {

        public ActivateProcessDefinitionCmd(IProcessDefinitionEntity processDefinitionEntity, bool includeProcessInstances, DateTime? executionDate, string tenantId) : base(processDefinitionEntity, includeProcessInstances, executionDate, tenantId)
        {
        }

        public ActivateProcessDefinitionCmd(string processDefinitionId, string processDefinitionKey, bool includeProcessInstances, DateTime executionDate, string tenantId) : base(processDefinitionId, processDefinitionKey, includeProcessInstances, executionDate, tenantId)
        {
        }

        protected internal override ISuspensionState ProcessDefinitionSuspensionState
        {
            get
            {
                return SuspensionStateProvider.ACTIVE;
            }
        }

        protected internal override string DelayedExecutionJobHandlerType
        {
            get
            {
                return TimerActivateProcessDefinitionHandler.TYPE;
            }
        }

        protected internal override AbstractSetProcessInstanceStateCmd GetProcessInstanceChangeStateCmd(IProcessInstance processInstance)
        {
            return new ActivateProcessInstanceCmd(processInstance.Id);
        }
    }
}