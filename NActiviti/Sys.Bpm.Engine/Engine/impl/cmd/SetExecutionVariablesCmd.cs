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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class SetExecutionVariablesCmd : NeedsActiveExecutionCmd<object>
    {
        private const long serialVersionUID = 1L;
        protected internal IDictionary<string, object> variables;
        protected internal bool isLocal;

        public SetExecutionVariablesCmd(string executionId, IDictionary<string, object> variables, bool isLocal) : base(executionId)
        {
            this.variables = variables;
            this.isLocal = isLocal;
        }

        protected internal override object Execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (variables is null || variables.Count == 0)
            {
                return null;
            }

            if (isLocal)
            {
                foreach (string variableName in variables.Keys)
                {
                    execution.SetVariableLocal(variableName, variables[variableName], false);
                }
            }
            else
            {
                foreach (string variableName in variables.Keys)
                {
                    execution.SetVariable(variableName, variables[variableName], false);
                }
            }

            // ACT-1887: Force an update of the execution's revision to prevent
            // simultaneous inserts of the same
            // variable. If not, duplicate variables may occur since optimistic
            // locking doesn't work on inserts
            execution.ForceUpdate();

            return null;
        }

        protected internal override string SuspendedExceptionMessage
        {
            get
            {
                return "Cannot set variables because execution '" + executionId + "' is suspended";
            }
        }

    }

}