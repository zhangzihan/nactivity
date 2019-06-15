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
        /// <summary>
        /// 
        /// </summary>
        protected internal RuntimeServiceImpl runtimeService;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _processDefinitionId;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _processDefinitionKey;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _messageName;
        /// <summary>
        /// 
        /// </summary>
        protected internal string processInstanceName;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _businessKey;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _tenantId;
        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, object> _variables;
        protected internal IDictionary<string, object> _transientVariables;

        public ProcessInstanceBuilderImpl(RuntimeServiceImpl runtimeService)
        {
            this.runtimeService = runtimeService;
        }

        public virtual IProcessInstanceBuilder SetProcessDefinitionId(string processDefinitionId)
        {
            this._processDefinitionId = processDefinitionId;
            return this;
        }

        public virtual IProcessInstanceBuilder SetProcessDefinitionKey(string processDefinitionKey)
        {
            this._processDefinitionKey = processDefinitionKey;
            return this;
        }

        public virtual IProcessInstanceBuilder SetMessageName(string messageName)
        {
            this._messageName = messageName;
            return this;
        }

        public virtual IProcessInstanceBuilder SetName(string processInstanceName)
        {
            this.processInstanceName = processInstanceName;
            return this;
        }

        public virtual IProcessInstanceBuilder SetBusinessKey(string businessKey)
        {
            this._businessKey = businessKey;
            return this;
        }

        public virtual IProcessInstanceBuilder SetTenantId(string tenantId)
        {
            this._tenantId = tenantId;
            return this;
        }

        public virtual IProcessInstanceBuilder SetVariables(IDictionary<string, object> variables)
        {
            if (this._variables == null)
            {
                this._variables = new Dictionary<string, object>();
            }
            if (variables != null)
            {
                foreach (string variableName in variables.Keys)
                {
                    this._variables[variableName] = variables[variableName];
                }
            }
            return this;
        }

        public virtual IProcessInstanceBuilder SetVariable(string variableName, object value)
        {
            if (this._variables == null)
            {
                this._variables = new Dictionary<string, object>();
            }
            this._variables[variableName] = value;
            return this;
        }

        public virtual IProcessInstanceBuilder SetTransientVariables(IDictionary<string, object> transientVariables)
        {
            if (this._transientVariables == null)
            {
                this._transientVariables = new Dictionary<string, object>();
            }
            if (transientVariables != null)
            {
                foreach (string variableName in transientVariables.Keys)
                {
                    this._transientVariables[variableName] = transientVariables[variableName];
                }
            }
            return this;
        }

        public virtual IProcessInstanceBuilder SetTransientVariable(string variableName, object value)
        {
            if (this._transientVariables == null)
            {
                this._transientVariables = new Dictionary<string, object>();
            }
            this._transientVariables[variableName] = value;
            return this;
        }

        public virtual IProcessInstance Start()
        {
            return runtimeService.StartProcessInstance(this);
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return _processDefinitionId;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return _processDefinitionKey;
            }
        }

        public virtual string MessageName
        {
            get
            {
                return _messageName;
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
                return _businessKey;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return _tenantId;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return _variables;
            }
        }

        public virtual IDictionary<string, object> TransientVariables
        {
            get
            {
                return _transientVariables;
            }
        }

    }

}