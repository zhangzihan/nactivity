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

    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public class SuspendProcessDefinitionCmd : AbstractSetProcessDefinitionStateCmd
    {

        public SuspendProcessDefinitionCmd(IProcessDefinitionEntity processDefinitionEntity, bool includeProcessInstances, DateTime? executionDate, string tenantId) : base(processDefinitionEntity, includeProcessInstances, executionDate, tenantId)
        {
        }

        public SuspendProcessDefinitionCmd(string processDefinitionId, string processDefinitionKey, bool suspendProcessInstances, DateTime? suspensionDate, string tenantId) : base(processDefinitionId, processDefinitionKey, suspendProcessInstances, suspensionDate, tenantId)
        {
        }

        protected internal override ISuspensionState ProcessDefinitionSuspensionState
        {
            get
            {
                return SuspensionState_Fields.SUSPENDED;
            }
        }

        protected internal override string DelayedExecutionJobHandlerType
        {
            get
            {
                return TimerSuspendProcessDefinitionHandler.TYPE;
            }
        }

        protected internal override AbstractSetProcessInstanceStateCmd getProcessInstanceChangeStateCmd(IProcessInstance processInstance)
        {
            return new SuspendProcessInstanceCmd(processInstance.Id);
        }

    }

}