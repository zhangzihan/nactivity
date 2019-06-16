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

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.task;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;

    /// 
    /// 
    [Serializable]
    public class ResolveTaskCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal IDictionary<string, object> variables;
        protected internal IDictionary<string, object> transientVariables;

        private readonly IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

        public ResolveTaskCmd(string taskId, IDictionary<string, object> variables) : base(taskId)
        {
            this.variables = variables;
        }

        public ResolveTaskCmd(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables) : this(taskId, variables)
        {
            this.transientVariables = transientVariables;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
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
            string ownerName = null;
            if (string.IsNullOrWhiteSpace(task.Owner) == false)
            {
                ownerName = AsyncHelper.RunSync(() => userService.GetUser(task.Owner))?.FullName;
            }
            commandContext.TaskEntityManager.ChangeTaskAssignee(task, task.Owner, ownerName);

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