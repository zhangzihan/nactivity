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
namespace Sys.Workflow.Engine.Impl.Runtimes
{

    using Sys.Workflow.Engine.Runtime;

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
        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, object> _transientVariables;
        /// <summary>
        /// 
        /// </summary>
        protected internal string _initialFlowElementId;

        /// <inheritdoc />
        public ProcessInstanceBuilderImpl(RuntimeServiceImpl runtimeService)
        {
            this.runtimeService = runtimeService;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetProcessDefinitionId(string processDefinitionId)
        {
            this._processDefinitionId = processDefinitionId;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetProcessDefinitionKey(string processDefinitionKey)
        {
            this._processDefinitionKey = processDefinitionKey;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetMessageName(string messageName)
        {
            this._messageName = messageName;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetName(string processInstanceName)
        {
            this.processInstanceName = processInstanceName;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetBusinessKey(string businessKey)
        {
            this._businessKey = businessKey;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetTenantId(string tenantId)
        {
            this._tenantId = tenantId;
            return this;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetVariable(string variableName, object value)
        {
            if (this._variables == null)
            {
                this._variables = new Dictionary<string, object>();
            }
            this._variables[variableName] = value;
            return this;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetTransientVariable(string variableName, object value)
        {
            if (this._transientVariables == null)
            {
                this._transientVariables = new Dictionary<string, object>();
            }
            this._transientVariables[variableName] = value;
            return this;
        }

        /// <inheritdoc />
        public virtual IProcessInstance Start()
        {
            return runtimeService.StartProcessInstance(this);
        }

        /// <inheritdoc />
        public virtual IProcessInstanceBuilder SetInitialFlowElement(string initialFlowElementId)
        {
            _initialFlowElementId = initialFlowElementId;

            return this;
        }

        /// <inheritdoc />
        public virtual string ProcessDefinitionId
        {
            get
            {
                return _processDefinitionId;
            }
        }

        /// <inheritdoc />
        public virtual string ProcessDefinitionKey
        {
            get
            {
                return _processDefinitionKey;
            }
        }

        /// <inheritdoc />
        public virtual string MessageName
        {
            get
            {
                return _messageName;
            }
        }

        /// <inheritdoc />
        public virtual string ProcessInstanceName
        {
            get
            {
                return processInstanceName;
            }
        }

        /// <inheritdoc />
        public virtual string BusinessKey
        {
            get
            {
                return _businessKey;
            }
        }

        /// <inheritdoc />
        public virtual string TenantId
        {
            get
            {
                return _tenantId;
            }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return _variables;
            }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> TransientVariables
        {
            get
            {
                return _transientVariables;
            }
        }

        /// <inheritdoc />
        public virtual string InitialFlowElementId
        {
            get
            {
                return _initialFlowElementId;
            }
        }
    }
}