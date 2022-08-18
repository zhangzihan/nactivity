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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    [Serializable]
    public class CompleteTaskCmd : AbstractCompleteTaskCmd
    {
        private const long serialVersionUID = 1L;
        protected IDictionary<string, object> variables;
        protected IDictionary<string, object> transientVariables;
        protected bool localScope;
        private readonly object syncRoot = new object();

        public CompleteTaskCmd(string taskId, IDictionary<string, object> variables) : base(taskId)
        {
            this.variables = variables ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public CompleteTaskCmd(string taskId, IDictionary<string, object> variables, bool localScope) : this(taskId, variables, null, localScope)
        {
        }

        public CompleteTaskCmd(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables, bool localScope = false, string completeReason = null) : this(taskId, variables)
        {
            this.localScope = localScope;
            this.transientVariables = transientVariables;
            this.completeReason = completeReason;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (variables is object)
            {
                if (localScope)
                {
                    task.VariablesLocal = variables;
                }
                else if (task.ExecutionId is not null)
                {
                    task.ExecutionVariables = variables;
                }
                else
                {
                    task.Variables = variables;
                }
            }

            if (transientVariables is object)
            {
                if (localScope)
                {
                    task.TransientVariablesLocal = transientVariables;
                }
                else
                {
                    task.TransientVariables = transientVariables;
                }
            }

            ExecuteTaskComplete(commandContext, task, variables, localScope);

            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot complete a suspended task";
            }
        }

    }

}