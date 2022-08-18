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
namespace Sys.Workflow.Engine.Impl
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Asyncexecutor;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow;
    using System.Collections.Concurrent;

    public class ProcessEngineImpl : IProcessEngine
    {
        protected internal string name;
        protected internal IRepositoryService repositoryService;
        protected internal IRuntimeService runtimeService;
        protected internal IHistoryService historicDataService;
        protected internal ITaskService taskService;
        protected internal IManagementService managementService;
        protected internal IDynamicBpmnService dynamicBpmnService;
        protected internal IAsyncExecutor asyncExecutor;
        protected internal ICommandExecutor commandExecutor;
        protected internal ConcurrentDictionary<Type, ISessionFactory> sessionFactories;
        protected internal ITransactionContextFactory transactionContextFactory;
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        private readonly ILogger<ProcessEngineImpl> log = ProcessEngineServiceProvider.LoggerService<ProcessEngineImpl>();

        public ProcessEngineImpl(ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.processEngineConfiguration = processEngineConfiguration;
            this.name = processEngineConfiguration.ProcessEngineName;
            this.repositoryService = processEngineConfiguration.RepositoryService;
            this.runtimeService = processEngineConfiguration.RuntimeService;
            this.historicDataService = processEngineConfiguration.HistoryService;
            this.taskService = processEngineConfiguration.TaskService;
            this.managementService = processEngineConfiguration.ManagementService;
            this.dynamicBpmnService = processEngineConfiguration.DynamicBpmnService;
            this.asyncExecutor = processEngineConfiguration.AsyncExecutor;
            this.commandExecutor = processEngineConfiguration.CommandExecutor;
            this.sessionFactories = processEngineConfiguration.SessionFactories;
            this.transactionContextFactory = processEngineConfiguration.TransactionContextFactory;

            if (processEngineConfiguration.UsingRelationalDatabase && processEngineConfiguration.DatabaseSchemaUpdate is not null)
            {
                commandExecutor.Execute(processEngineConfiguration.SchemaCommandConfig, new SchemaOperationsProcessEngineBuild());
            }

            if (name is null)
            {
                log.LogInformation("default activiti ProcessEngine created");
            }
            else
            {
                log.LogInformation($"ProcessEngine {name} created");
            }

            ProcessEngineFactory.RegisterProcessEngine(this);

            if (asyncExecutor is object && asyncExecutor.AutoActivate)
            {
                asyncExecutor.Start();
            }

            if (processEngineConfiguration.ProcessEngineLifecycleListener is object)
            {
                processEngineConfiguration.ProcessEngineLifecycleListener.OnProcessEngineBuilt(this);
            }

            processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateGlobalEvent(ActivitiEventType.ENGINE_CREATED));
        }

        public virtual void Close()
        {
            ProcessEngineFactory.Unregister(this);
            if (asyncExecutor is object && asyncExecutor.Active)
            {
                asyncExecutor.Shutdown();
            }

            commandExecutor.Execute(processEngineConfiguration.SchemaCommandConfig, new SchemaOperationProcessEngineClose());

            if (processEngineConfiguration.ProcessEngineLifecycleListener is object)
            {
                processEngineConfiguration.ProcessEngineLifecycleListener.OnProcessEngineClosed(this);
            }

            processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateGlobalEvent(ActivitiEventType.ENGINE_CLOSED));
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual IManagementService ManagementService
        {
            get
            {
                return managementService;
            }
        }

        public virtual ITaskService TaskService
        {
            get
            {
                return taskService;
            }
        }

        public virtual IHistoryService HistoryService
        {
            get
            {
                return historicDataService;
            }
        }

        public virtual IRuntimeService RuntimeService
        {
            get
            {
                return runtimeService;
            }
        }

        public virtual IRepositoryService RepositoryService
        {
            get
            {
                return repositoryService;
            }
        }

        public virtual IDynamicBpmnService DynamicBpmnService
        {
            get
            {
                return dynamicBpmnService;
            }
        }

        public virtual ProcessEngineConfiguration ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
        }
    }

}