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
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;

    /// 
    [Serializable]
    public abstract class NeedsActiveProcessDefinitionCmd<T> : ICommand<T>
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId;

        public NeedsActiveProcessDefinitionCmd(string processDefinitionId)
        {
            this.processDefinitionId = processDefinitionId;
        }

        public  virtual T  Execute(ICommandContext  commandContext)
        {
            IProcessDefinitionEntity processDefinition = ProcessDefinitionUtil.GetProcessDefinitionFromDatabase(processDefinitionId);

            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("No process definition found for id = '" + processDefinitionId + "'", typeof(IProcessDefinition));
            }

            if (processDefinition.Suspended)
            {
                throw new ActivitiException("Cannot execute operation because process definition '" + processDefinition.Name + "' (id=" + processDefinition.Id + ") is suspended");
            }

            return Execute(commandContext, processDefinition);
        }

        /// <summary>
        /// Subclasses should implement this. The provided <seealso cref="IProcessDefinition"/> is guaranteed to be an active process definition (ie. not suspended).
        /// </summary>
        protected  internal abstract T  Execute(ICommandContext  commandContext, IProcessDefinitionEntity processDefinition);

    }

}