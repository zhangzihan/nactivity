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
    using Sys.Workflow.Engine.Tasks;

    [Serializable]
    public class GetTaskVariableInstancesCmd : ICommand<IDictionary<string, IVariableInstance>>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal ICollection<string> variableNames;
        protected internal bool isLocal;

        public GetTaskVariableInstancesCmd(string taskId, ICollection<string> variableNames, bool isLocal)
        {
            this.taskId = taskId;
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        public virtual IDictionary<string, IVariableInstance> Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(taskId, null))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(taskId);

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("task " + taskId + " doesn't exist", typeof(ITask));
            }

            IDictionary<string, IVariableInstance> variables = null;
            if (variableNames == null)
            {

                if (isLocal)
                {
                    variables = task.VariableInstancesLocal;
                }
                else
                {
                    variables = task.VariableInstances;
                }

            }
            else
            {
                if (isLocal)
                {
                    variables = task.GetVariableInstancesLocal(variableNames, false);
                }
                else
                {
                    variables = task.GetVariableInstances(variableNames, false);
                }

            }

            return variables;
        }
    }

}