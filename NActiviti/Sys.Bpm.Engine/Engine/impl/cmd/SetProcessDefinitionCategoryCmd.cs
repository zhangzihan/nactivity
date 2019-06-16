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

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;

    /// 
    public class SetProcessDefinitionCategoryCmd : ICommand<object>
    {

        protected internal string processDefinitionId;
        protected internal string category;

        public SetProcessDefinitionCategoryCmd(string processDefinitionId, string category)
        {
            this.processDefinitionId = processDefinitionId;
            this.category = category;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {

            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Process definition id is null");
            }

            IProcessDefinitionEntity processDefinition = commandContext.ProcessDefinitionEntityManager.FindById<IProcessDefinitionEntity>(processDefinitionId);

            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("No process definition found for id = '" + processDefinitionId + "'", typeof(IProcessDefinition));
            }

            // Update category
            processDefinition.Category = category;

            // Remove process definition from cache, it will be refetched later
            IDeploymentCache<ProcessDefinitionCacheEntry> processDefinitionCache = commandContext.ProcessEngineConfiguration.ProcessDefinitionCache;
            if (processDefinitionCache != null)
            {
                processDefinitionCache.Remove(processDefinitionId);
            }

            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, processDefinition));
            }

            return null;
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


    }

}