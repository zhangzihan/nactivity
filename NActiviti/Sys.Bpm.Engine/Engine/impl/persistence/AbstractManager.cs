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

namespace org.activiti.engine.impl.persistence
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System;

    /// 
    /// 
    public abstract class AbstractManager
    {

        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        public AbstractManager(ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.processEngineConfiguration = processEngineConfiguration;
        }

        // Command scoped 

        protected internal virtual ICommandContext CommandContext
        {
            get
            {
                return Context.CommandContext;
            }
        }

        protected internal virtual T getSession<T>() where T : ISession
        {
            return CommandContext.getSession<T>();
        }

        // Engine scoped

        protected internal virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
        }

        protected internal virtual ICommandExecutor CommandExecutor
        {
            get
            {
                return ProcessEngineConfiguration.CommandExecutor;
            }
        }

        protected internal virtual IClock Clock
        {
            get
            {
                return ProcessEngineConfiguration.Clock;
            }
        }

        protected internal virtual IAsyncExecutor AsyncExecutor
        {
            get
            {
                return ProcessEngineConfiguration.AsyncExecutor;
            }
        }

        protected internal virtual IActivitiEventDispatcher EventDispatcher
        {
            get
            {
                return ProcessEngineConfiguration.EventDispatcher;
            }
        }

        protected internal virtual IHistoryManager HistoryManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoryManager;
            }
        }

        protected internal virtual IJobManager JobManager
        {
            get
            {
                return ProcessEngineConfiguration.JobManager;
            }
        }

        protected internal virtual IDeploymentEntityManager DeploymentEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.DeploymentEntityManager;
            }
        }

        protected internal virtual IResourceEntityManager ResourceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ResourceEntityManager;
            }
        }

        protected internal virtual IByteArrayEntityManager ByteArrayEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ByteArrayEntityManager;
            }
        }

        protected internal virtual IProcessDefinitionEntityManager ProcessDefinitionEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ProcessDefinitionEntityManager;
            }
        }

        protected internal virtual IProcessDefinitionInfoEntityManager ProcessDefinitionInfoEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ProcessDefinitionInfoEntityManager;
            }
        }

        protected internal virtual IModelEntityManager ModelEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ModelEntityManager;
            }
        }

        protected internal virtual IExecutionEntityManager ExecutionEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.ExecutionEntityManager;
            }
        }

        protected internal virtual ITaskEntityManager TaskEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.TaskEntityManager;
            }
        }

        protected internal virtual IdentityLinkEntityManager IdentityLinkEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.IdentityLinkEntityManager;
            }
        }

        protected internal virtual IEventSubscriptionEntityManager EventSubscriptionEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.EventSubscriptionEntityManager;
            }
        }

        protected internal virtual IVariableInstanceEntityManager VariableInstanceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.VariableInstanceEntityManager;
            }
        }

        protected internal virtual IJobEntityManager JobEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.JobEntityManager;
            }
        }

        protected internal virtual ITimerJobEntityManager TimerJobEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.TimerJobEntityManager;
            }
        }

        protected internal virtual ISuspendedJobEntityManager SuspendedJobEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.SuspendedJobEntityManager;
            }
        }

        protected internal virtual IDeadLetterJobEntityManager DeadLetterJobEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.DeadLetterJobEntityManager;
            }
        }

        protected internal virtual IHistoricProcessInstanceEntityManager HistoricProcessInstanceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricProcessInstanceEntityManager;
            }
        }

        protected internal virtual IHistoricDetailEntityManager HistoricDetailEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricDetailEntityManager;
            }
        }

        protected internal virtual IHistoricActivityInstanceEntityManager HistoricActivityInstanceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricActivityInstanceEntityManager;
            }
        }

        protected internal virtual IHistoricVariableInstanceEntityManager HistoricVariableInstanceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricVariableInstanceEntityManager;
            }
        }

        protected internal virtual IHistoricTaskInstanceEntityManager HistoricTaskInstanceEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricTaskInstanceEntityManager;
            }
        }

        protected internal virtual IHistoricIdentityLinkEntityManager HistoricIdentityLinkEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.HistoricIdentityLinkEntityManager;
            }
        }

        protected internal virtual IAttachmentEntityManager AttachmentEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.AttachmentEntityManager;
            }
        }

        protected internal virtual ICommentEntityManager CommentEntityManager
        {
            get
            {
                return ProcessEngineConfiguration.CommentEntityManager;
            }
        }
    }

}