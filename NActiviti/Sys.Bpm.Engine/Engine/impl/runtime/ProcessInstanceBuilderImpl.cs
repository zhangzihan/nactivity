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
namespace org.activiti.engine.impl.runtime
{

    using org.activiti.engine.runtime;

    /// 
    /// 
    public class ProcessInstanceBuilderImpl : IProcessInstanceBuilder
    {

        protected internal RuntimeServiceImpl runtimeService;
        protected internal string processDefinitionId_Renamed;
        protected internal string processDefinitionKey_Renamed;
        protected internal string messageName_Renamed;
        protected internal string processInstanceName;
        protected internal string businessKey_Renamed;
        protected internal string tenantId_Renamed;
        protected internal IDictionary<string, object> variables_Renamed;
        protected internal IDictionary<string, object> transientVariables_Renamed;

        public ProcessInstanceBuilderImpl(RuntimeServiceImpl runtimeService)
        {
            this.runtimeService = runtimeService;
        }

        public virtual IProcessInstanceBuilder processDefinitionId(string processDefinitionId)
        {
            this.processDefinitionId_Renamed = processDefinitionId;
            return this;
        }

        public virtual IProcessInstanceBuilder processDefinitionKey(string processDefinitionKey)
        {
            this.processDefinitionKey_Renamed = processDefinitionKey;
            return this;
        }

        public virtual IProcessInstanceBuilder messageName(string messageName)
        {
            this.messageName_Renamed = messageName;
            return this;
        }

        public virtual IProcessInstanceBuilder name(string processInstanceName)
        {
            this.processInstanceName = processInstanceName;
            return this;
        }

        public virtual IProcessInstanceBuilder businessKey(string businessKey)
        {
            this.businessKey_Renamed = businessKey;
            return this;
        }

        public virtual IProcessInstanceBuilder tenantId(string tenantId)
        {
            this.tenantId_Renamed = tenantId;
            return this;
        }

        public virtual IProcessInstanceBuilder variables(IDictionary<string, object> variables)
        {
            if (this.variables_Renamed == null)
            {
                this.variables_Renamed = new Dictionary<string, object>();
            }
            if (variables != null)
            {
                foreach (string variableName in variables.Keys)
                {
                    this.variables_Renamed[variableName] = variables[variableName];
                }
            }
            return this;
        }

        public virtual IProcessInstanceBuilder variable(string variableName, object value)
        {
            if (this.variables_Renamed == null)
            {
                this.variables_Renamed = new Dictionary<string, object>();
            }
            this.variables_Renamed[variableName] = value;
            return this;
        }

        public virtual IProcessInstanceBuilder transientVariables(IDictionary<string, object> transientVariables)
        {
            if (this.transientVariables_Renamed == null)
            {
                this.transientVariables_Renamed = new Dictionary<string, object>();
            }
            if (transientVariables != null)
            {
                foreach (string variableName in transientVariables.Keys)
                {
                    this.transientVariables_Renamed[variableName] = transientVariables[variableName];
                }
            }
            return this;
        }

        public virtual IProcessInstanceBuilder transientVariable(string variableName, object value)
        {
            if (this.transientVariables_Renamed == null)
            {
                this.transientVariables_Renamed = new Dictionary<string, object>();
            }
            this.transientVariables_Renamed[variableName] = value;
            return this;
        }

        public virtual IProcessInstance start()
        {
            return runtimeService.startProcessInstance(this);
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_Renamed;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_Renamed;
            }
        }

        public virtual string MessageName
        {
            get
            {
                return messageName_Renamed;
            }
        }

        public virtual string ProcessInstanceName
        {
            get
            {
                return processInstanceName;
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey_Renamed;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId_Renamed;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables_Renamed;
            }
        }

        public virtual IDictionary<string, object> TransientVariables
        {
            get
            {
                return transientVariables_Renamed;
            }
        }

    }

}