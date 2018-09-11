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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// 
    /// 
    [Serializable]
    public class SetTaskVariablesCmd<T1> : NeedsActiveTaskCmd<T1>
    {

        private const long serialVersionUID = 1L;
        protected internal IDictionary<string, T1> variables;
        protected internal bool isLocal;

        public SetTaskVariablesCmd(string taskId, IDictionary<string, T1> variables, bool isLocal) : base(taskId)
        {
            this.taskId = taskId;
            this.variables = variables;
            this.isLocal = isLocal;
        }

        protected internal override T1 execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (isLocal)
            {
                if (variables != null)
                {
                    foreach (string variableName in variables.Keys)
                    {
                        task.setVariableLocal(variableName, variables[variableName], false);
                    }
                }

            }
            else
            {
                if (variables != null)
                {
                    foreach (string variableName in variables.Keys)
                    {
                        task.setVariable(variableName, variables[variableName], false);
                    }
                }
            }

            // ACT-1887: Force an update of the task's revision to prevent
            // simultaneous inserts of the same variable. If not, duplicate variables may occur since optimistic
            // locking doesn't work on inserts
            task.forceUpdate();

            return default(T1);
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot add variables to a suspended task";
            }
        }

    }

}