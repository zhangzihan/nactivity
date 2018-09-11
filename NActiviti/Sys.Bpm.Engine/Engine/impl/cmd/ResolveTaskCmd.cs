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
    using org.activiti.engine.task;

    /// 
    /// 
    [Serializable]
    public class ResolveTaskCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal IDictionary<string, object> variables;
        protected internal IDictionary<string, object> transientVariables;

        public ResolveTaskCmd(string taskId, IDictionary<string, object> variables) : base(taskId)
        {
            this.variables = variables;
        }

        public ResolveTaskCmd(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables) : this(taskId, variables)
        {
            this.transientVariables = transientVariables;
        }

        protected internal override object execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (variables != null)
            {
                task.Variables = variables;
            }
            if (transientVariables != null)
            {
                task.TransientVariables = transientVariables;
            }

            task.DelegationState = DelegationState.RESOLVED;
            commandContext.TaskEntityManager.changeTaskAssignee(task, task.Owner);

            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot resolve a suspended task";
            }
        }

    }

}