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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.runtime;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using System.Linq;

    /// 
    /// 
    public class StartProcessInstanceByActivityCmd : ICommand<IProcessInstance>
    {

        protected internal string messageName;
        protected internal string businessKey;
        protected internal IDictionary<string, object> processVariables;
        protected internal IDictionary<string, object> transientVariables;
        protected internal string tenantId;
        private readonly string activityId;
        private readonly string processDefinitionId;
        private readonly string processInstanceName;
        private readonly bool startProcessInstance;

        public StartProcessInstanceByActivityCmd(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> processVariables, string tenantId, IDictionary<string, object> transientVariables, bool startProcesInstance = true, string processInstanceName = null)
        {
            this.processDefinitionId = processDefinitionId;
            this.activityId = activityId;
            this.businessKey = businessKey;
            this.processVariables = processVariables;
            this.tenantId = tenantId;
            this.transientVariables = transientVariables;
            this.processInstanceName = processInstanceName;
            this.startProcessInstance = startProcessInstance;
        }

        public virtual IProcessInstance Execute(ICommandContext commandContext)
        {
            DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;

            IProcessDefinition processDefinition = deploymentCache.FindDeployedProcessDefinitionById(processDefinitionId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("No process definition found for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
            }

            ProcessInstanceHelper processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;

            // Get model from cache
            Process process = ProcessDefinitionUtil.GetProcess(processDefinition.Id);
            if (process == null)
            {
                throw new ActivitiException("Cannot start process instance. Process model " + processDefinition.Name + " (id = " + processDefinition.Id + ") could not be found");
            }

            FlowElement initialFlowElement = process.FlowElements.FirstOrDefault(x => x.Id == activityId);

            IProcessInstance processInstance = processInstanceHelper.CreateAndStartProcessInstanceWithInitialFlowElement(processDefinition, businessKey, processInstanceName, initialFlowElement, process, processVariables, transientVariables, startProcessInstance);

            return processInstance;
        }

    }

}