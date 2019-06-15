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
    public class GetTaskVariablesCmd : ICommand<IDictionary<string, object>>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal ICollection<string> variableNames;
        protected internal bool isLocal;

        public GetTaskVariablesCmd(string taskId, ICollection<string> variableNames, bool isLocal)
        {
            this.taskId = taskId;
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        public virtual IDictionary<string, object> Execute(ICommandContext commandContext)
        {
            if (taskId is null)
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("task " + taskId + " doesn't exist", typeof(ITask));
            }

            if (variableNames == null)
            {

                if (isLocal)
                {
                    return task.VariablesLocal;
                }
                else
                {
                    return task.Variables;
                }

            }
            else
            {

                if (isLocal)
                {
                    return task.GetVariablesLocal(variableNames, false);
                }
                else
                {
                    return task.GetVariables(variableNames, false);
                }

            }

        }
    }

}