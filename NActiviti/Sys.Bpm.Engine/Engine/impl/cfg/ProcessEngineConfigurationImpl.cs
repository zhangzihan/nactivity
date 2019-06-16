using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
namespace Sys.Workflow.engine.impl.cfg
{
    using DatabaseSchemaReader;
    using javax.transaction;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.cfg;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.asyncexecutor;
    using Sys.Workflow.engine.impl.bpmn.data;
    using Sys.Workflow.engine.impl.bpmn.deployer;
    using Sys.Workflow.engine.impl.bpmn.listener;
    using Sys.Workflow.engine.impl.bpmn.parser;
    using Sys.Workflow.engine.impl.bpmn.parser.factory;
    using Sys.Workflow.engine.impl.bpmn.parser.handler;
    using Sys.Workflow.engine.impl.bpmn.webservice;
    using Sys.Workflow.engine.impl.calendar;
    using Sys.Workflow.engine.impl.cfg.standalone;
    using Sys.Workflow.engine.impl.cmd;
    using Sys.Workflow.engine.impl.db;
    using Sys.Workflow.engine.impl.@delegate.invocation;
    using Sys.Workflow.engine.impl.el;
    using Sys.Workflow.engine.impl.@event;
    using Sys.Workflow.engine.impl.@event.logger;
    using Sys.Workflow.engine.impl.history;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence;
    using Sys.Workflow.engine.impl.persistence.cache;
    using Sys.Workflow.engine.impl.persistence.deploy;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.persistence.entity.data;
    using Sys.Workflow.engine.impl.persistence.entity.data.impl;
    using Sys.Workflow.engine.impl.persistence.entity.data.integration;
    using Sys.Workflow.engine.impl.persistence.entity.integration;
    using Sys.Workflow.engine.impl.scripting;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.impl.variable;
    using Sys.Workflow.engine.integration;
    using Sys.Workflow.engine.parse;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow.validation;
    using Sys;
    using Sys.Bpm;
    using Sys.Data;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System.IO;
    using System.Linq;

    /// <inheritdoc />
    public abstract class ProcessEngineConfigurationImpl : ProcessEngineConfiguration
    {
        private static readonly ILogger<ProcessEngineConfigurationImpl> log = ProcessEngineServiceProvider.LoggerService<ProcessEngineConfigurationImpl>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="historyService"></param>
        /// <param name="taskService"></param>
        /// <param name="dynamicBpmnService"></param>
        /// <param name="repositoryService"></param>
        /// <param name="runtimeService"></param>
        /// <param name="managementService"></param>
        /// <param name="asyncExecutor"></param>
        /// <param name="configuration"></param>
        public ProcessEngineConfigurationImpl(IHistoryService historyService,
            ITaskService taskService,
            IDynamicBpmnService dynamicBpmnService,
            IRepositoryService repositoryService,
            IRuntimeService runtimeService,
            IManagementService managementService,
            IAsyncExecutor asyncExecutor,
            IConfiguration configuration) : base(configuration)
        {
            this.historyService = historyService;
            if (historyService is ServiceImpl)
            {
                (historyService as ServiceImpl).processEngineConfiguration = this;
            }
            this.taskService = taskService;
            if (taskService is ServiceImpl)
            {
                (taskService as ServiceImpl).processEngineConfiguration = this;
            }
            this.dynamicBpmnService = dynamicBpmnService;
            if (dynamicBpmnService is ServiceImpl)
            {
                (dynamicBpmnService as ServiceImpl).processEngineConfiguration = this;
            }
            this.repositoryService = repositoryService;
            if (repositoryService is ServiceImpl)
            {
                (repositoryService as ServiceImpl).processEngineConfiguration = this;
            }
            this.runtimeService = runtimeService;
            if (runtimeService is ServiceImpl)
            {
                (runtimeService as ServiceImpl).processEngineConfiguration = this;
            }
            this.managementService = managementService;
            if (managementService is ServiceImpl)
            {
                (managementService as ServiceImpl).processEngineConfiguration = this;
            }
            this.asyncExecutor = asyncExecutor;
        }

        #region fields
        /// <summary>
        /// 
        /// </summary>
        public const string DEFAULT_WS_SYNC_FACTORY = "Sys.Workflow.engine.impl.webservice.CxfWebServiceClientFactory";

        /// <summary>
        /// 
        /// </summary>
        public const string DEFAULT_MYBATIS_MAPPING_FILE = "resources/db/mapping/mappings.xml";

        /// <summary>
        /// 
        /// </summary>
        public const int DEFAULT_GENERIC_MAX_LENGTH_STRING = 4000;

        /// <summary>
        /// 
        /// </summary>
        public const int DEFAULT_ORACLE_MAX_LENGTH_STRING = 2000;

        // SERVICES /////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        protected internal IRepositoryService repositoryService;

        /// <summary>
        /// 
        /// </summary>
        protected internal IRuntimeService runtimeService;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoryService historyService;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITaskService taskService;

        /// <summary>
        /// 
        /// </summary>
        protected internal IManagementService managementService;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDynamicBpmnService dynamicBpmnService;

        /// <summary>
        /// 
        /// </summary>
        protected internal IUserGroupLookupProxy userGroupLookupProxy;

        /// <summary>
        /// 
        /// </summary>
        private IIntegrationContextService integrationContextService;

        // COMMAND EXECUTORS ////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        protected internal CommandConfig defaultCommandConfig;

        /// <summary>
        /// 
        /// </summary>
        protected internal CommandConfig schemaCommandConfig;

        /// <summary>
        /// 
        /// </summary>
        protected internal ICommandInterceptor commandInvoker;

        /// <summary>
        /// the configurable list which will be to build the <seealso cref="CommandExecutor"/>
        /// </summary>
        protected internal IList<ICommandInterceptor> customPreCommandInterceptors;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<ICommandInterceptor> customPostCommandInterceptors;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<ICommandInterceptor> commandInterceptors;

        /// <summary>
        /// this will be initialized during the configurationComplete() </summary>
        protected internal interceptor.ICommandExecutor commandExecutor;

        // DATA MANAGERS /////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IAttachmentDataManager attachmentDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IByteArrayDataManager byteArrayDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ICommentDataManager commentDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeploymentDataManager deploymentDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IEventLogEntryDataManager eventLogEntryDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IEventSubscriptionDataManager eventSubscriptionDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IExecutionDataManager executionDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricActivityInstanceDataManager historicActivityInstanceDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricDetailDataManager historicDetailDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricIdentityLinkDataManager historicIdentityLinkDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricProcessInstanceDataManager historicProcessInstanceDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricTaskInstanceDataManager historicTaskInstanceDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricVariableInstanceDataManager historicVariableInstanceDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IIdentityLinkDataManager identityLinkDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IJobDataManager jobDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITimerJobDataManager timerJobDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ISuspendedJobDataManager suspendedJobDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeadLetterJobDataManager deadLetterJobDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IModelDataManager modelDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessDefinitionDataManager processDefinitionDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessDefinitionInfoDataManager processDefinitionInfoDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IPropertyDataManager propertyDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IResourceDataManager resourceDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITaskDataManager taskDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableInstanceDataManager variableInstanceDataManager;

        /// <summary>
        /// 
        /// </summary>
        private IIntegrationContextDataManager integrationContextDataManager;


        // ENTITY MANAGERS ///////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IAttachmentEntityManager attachmentEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IByteArrayEntityManager byteArrayEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ICommentEntityManager commentEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeploymentEntityManager deploymentEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IEventLogEntryEntityManager eventLogEntryEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IEventSubscriptionEntityManager eventSubscriptionEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IExecutionEntityManager executionEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricActivityInstanceEntityManager historicActivityInstanceEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricDetailEntityManager historicDetailEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricIdentityLinkEntityManager historicIdentityLinkEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricProcessInstanceEntityManager historicProcessInstanceEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricTaskInstanceEntityManager historicTaskInstanceEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoricVariableInstanceEntityManager historicVariableInstanceEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IIdentityLinkEntityManager identityLinkEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IJobEntityManager jobEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITimerJobEntityManager timerJobEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ISuspendedJobEntityManager suspendedJobEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeadLetterJobEntityManager deadLetterJobEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IModelEntityManager modelEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessDefinitionEntityManager processDefinitionEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessDefinitionInfoEntityManager processDefinitionInfoEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IPropertyEntityManager propertyEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IResourceEntityManager resourceEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITableDataManager tableDataManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITaskEntityManager taskEntityManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableInstanceEntityManager variableInstanceEntityManager;

        private IIntegrationContextManager integrationContextManager;

        // History Manager


        /// <summary>
        /// 
        /// </summary>
        protected internal IHistoryManager historyManager;

        // Job Manager


        /// <summary>
        /// 
        /// </summary>
        protected internal IJobManager jobManager;

        // SESSION FACTORIES /////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IList<ISessionFactory> customSessionFactories;

        /// <summary>
        /// 
        /// </summary>
        protected internal DbSqlSessionFactory dbSqlSessionFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<Type, ISessionFactory> sessionFactories;

        // CONFIGURATORS ////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal bool enableConfiguratorServiceLoader = false; // Enabled by default. In certain environments this should be set to false (eg osgi)

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IProcessEngineConfigurator> configurators; // The injected configurators

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IProcessEngineConfigurator> allConfigurators; // Including auto-discovered configurators

        // DEPLOYERS //////////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal BpmnDeployer bpmnDeployer;

        /// <summary>
        /// 
        /// </summary>
        protected internal BpmnParser bpmnParser;

        /// <summary>
        /// 
        /// </summary>
        protected internal ParsedDeploymentBuilderFactory parsedDeploymentBuilderFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal TimerManager timerManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal EventSubscriptionManager eventSubscriptionManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal BpmnDeploymentHelper bpmnDeploymentHelper;

        /// <summary>
        /// 
        /// </summary>
        protected internal CachingAndArtifactsManager cachingAndArtifactsManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IDeployer> customPreDeployers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IDeployer> customPostDeployers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IDeployer> deployers;

        /// <summary>
        /// 
        /// </summary>
        protected internal DeploymentManager deploymentManager;


        /// <summary>
        /// 
        /// </summary>
        protected internal int processDefinitionCacheLimit = -1; // By default, no limit

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeploymentCache<ProcessDefinitionCacheEntry> processDefinitionCache;


        /// <summary>
        /// 
        /// </summary>
        protected internal int processDefinitionInfoCacheLimit = -1; // By default, no limit

        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessDefinitionInfoCache processDefinitionInfoCache;


        /// <summary>
        /// 
        /// </summary>
        protected internal int knowledgeBaseCacheLimit = -1;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDeploymentCache<object> knowledgeBaseCache;

        // JOB EXECUTOR /////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IJobHandler> customJobHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, IJobHandler> jobHandlers;

        // HELPERS //////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessInstanceHelper processInstanceHelper;

        /// <summary>
        /// 
        /// </summary>
        protected internal ListenerNotificationHelper listenerNotificationHelper;

        // ASYNC EXECUTOR ///////////////////////////////////////////////////////////

        /// <summary>
        /// The number of retries for a job.
        /// </summary>
        protected internal int asyncExecutorNumberOfRetries = 3;

        /// <summary>
        /// The minimal number of threads that are kept alive in the threadpool for job
        /// execution. Default value = 2. (This property is only applicable when using
        /// the <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorCorePoolSize = 2;

        /// <summary>
        /// The maximum number of threads that are created in the threadpool for job
        /// execution. Default value = 10. (This property is only applicable when using
        /// the <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorMaxPoolSize = 10;

        /// <summary>
        /// The time (in milliseconds) a thread used for job execution must be kept
        /// alive before it is destroyed. Default setting is 5 seconds. Having a
        /// setting > 0 takes resources, but in the case of many job executions it
        /// avoids creating new threads all the time. If 0, threads will be destroyed
        /// after they've been used for job execution.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal long asyncExecutorThreadKeepAliveTime = 5000L;

        /// <summary>
        /// The size of the queue on which jobs to be executed are placed, before they
        /// are actually executed. Default value = 100. (This property is only
        /// applicable when using the <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorThreadPoolQueueSize = 100;

        /// <summary>
        /// The queue onto which jobs will be placed before they are actually executed.
        /// Threads form the async executor threadpool will take work from this queue.
        /// 
        /// By default null. If null, an  will be created of
        /// size <seealso cref="AsyncExecutorThreadPoolQueueSize"/>.
        /// 
        /// When the queue is full, the job will be executed by the calling thread
        /// (ThreadPoolExecutor.CallerRunsPolicy())
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal ConcurrentQueue<ThreadStart> asyncExecutorThreadPoolQueue;

        /// <summary>
        /// The time (in seconds) that is waited to gracefully shut down the threadpool
        /// used for job execution when the a shutdown on the executor (or process
        /// engine) is requested. Default value = 60.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal long asyncExecutorSecondsToWaitOnShutdown = 60L;

        /// <summary>
        /// The number of timer jobs that are acquired during one query (before a job
        /// is executed, an acquirement thread fetches jobs from the database and puts
        /// them on the queue).
        /// 
        /// Default value = 1, as this lowers the potential on optimistic locking
        /// exceptions. Change this value if you know what you are doing.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorMaxTimerJobsPerAcquisition = 1;

        /// <summary>
        /// The number of async jobs that are acquired during one query (before a job
        /// is executed, an acquirement thread fetches jobs from the database and puts
        /// them on the queue).
        /// 
        /// Default value = 1, as this lowers the potential on optimistic locking
        /// exceptions. Change this value if you know what you are doing.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorMaxAsyncJobsDuePerAcquisition = 1;

        /// <summary>
        /// The time (in milliseconds) the timer acquisition thread will wait to
        /// execute the next acquirement query. This happens when no new timer jobs
        /// were found or when less timer jobs have been fetched than set in
        /// <seealso cref="asyncExecutorMaxTimerJobsPerAcquisition"/>. Default value = 10
        /// seconds.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorDefaultTimerJobAcquireWaitTime = 10 * 1000;

        /// <summary>
        /// The time (in milliseconds) the async job acquisition thread will wait to
        /// execute the next acquirement query. This happens when no new async jobs
        /// were found or when less async jobs have been fetched than set in
        /// <seealso cref="asyncExecutorMaxAsyncJobsDuePerAcquisition"/>. Default value = 10
        /// seconds.
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorDefaultAsyncJobAcquireWaitTime = 10 * 1000;

        /// <summary>
        /// The time (in milliseconds) the async job (both timer and async continuations) acquisition thread will
        /// wait when the queueu is full to execute the next query. By default set to 0 (for backwards compatibility)
        /// </summary>
        protected internal int asyncExecutorDefaultQueueSizeFullWaitTime = 0;

        /// <summary>
        /// When a job is acquired, it is locked so other async executors can't lock
        /// and execute it. While doing this, the 'name' of the lock owner is written
        /// into a column of the job.
        /// 
        /// By default, a random UUID will be generated when the executor is created.
        /// 
        /// It is important that each async executor instance in a cluster of Activiti
        /// engines has a different name!
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal string asyncExecutorLockOwner;

        /// <summary>
        /// The amount of time (in milliseconds) a timer job is locked when acquired by
        /// the async executor. During this period of time, no other async executor
        /// will try to acquire and lock this job.
        /// 
        /// Default value = 5 minutes;
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorTimerLockTimeInMillis = 5 * 60 * 1000;

        /// <summary>
        /// The amount of time (in milliseconds) an async job is locked when acquired
        /// by the async executor. During this period of time, no other async executor
        /// will try to acquire and lock this job.
        /// 
        /// Default value = 5 minutes;
        /// 
        /// (This property is only applicable when using the
        /// <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal int asyncExecutorAsyncJobLockTimeInMillis = 5 * 60 * 1000;

        /// <summary>
        /// The amount of time (in milliseconds) that is between two consecutive checks
        /// of 'expired jobs'. Expired jobs are jobs that were locked (a lock owner + time
        /// was written by some executor, but the job was never completed).
        /// 
        /// During such a check, jobs that are expired are again made available,
        /// meaning the lock owner and lock time will be removed. Other executors
        /// will now be able to pick it up.
        /// 
        /// A job is deemed expired if the lock time is before the current date.
        /// 
        /// By default one minute.
        /// </summary>
        protected internal int asyncExecutorResetExpiredJobsInterval = 60 * 1000;

        /// <summary>
        /// The <seealso cref="IAsyncExecutor"/> has a 'cleanup' thread that resets expired jobs
        /// so they can be re-acquired by other executors. This setting defines the size
        /// of the page being used when fetching these expired jobs.
        /// </summary>
        protected internal int asyncExecutorResetExpiredJobsPageSize = 3;

        /// <summary>
        /// Experimental!
        /// 
        /// Set this to true when using the message queue based job executor.
        /// </summary>
        protected internal bool asyncExecutorMessageQueueMode;

        /// <summary>
        /// Allows to define a custom factory for creating the that is executed by the async executor.
        /// 
        /// (This property is only applicable when using the <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal IExecuteAsyncRunnableFactory asyncExecutorExecuteAsyncRunnableFactory;

        // MYBATIS SQL SESSION FACTORY //////////////////////////////////////////////

        //protected internal DefaultSqlSessionFactory sqlSessionFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITransactionFactory transactionFactory;


        /// <summary>
        /// 
        /// </summary>
        protected internal ISet<Type> customMybatisMappers;

        /// <summary>
        /// 
        /// </summary>
        protected internal ISet<string> customMybatisXMLMappers;

        // ID GENERATOR ///////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IIdGenerator idGenerator;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDataSource idGeneratorDataSource;

        // BPMN PARSER //////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IBpmnParseHandler> preBpmnParseHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IBpmnParseHandler> postBpmnParseHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IBpmnParseHandler> customDefaultBpmnParseHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IActivityBehaviorFactory activityBehaviorFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal IListenerFactory listenerFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal IBpmnParseFactory bpmnParseFactory;

        // PROCESS VALIDATION //////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessValidator processValidator;

        // OTHER //////////////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IVariableType> customPreVariableTypes;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IVariableType> customPostVariableTypes;

        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableTypes variableTypes;

        /// <summary>
        /// This flag determines whether variables of the type 'serializable' will be tracked.
        /// This means that, when true, in a JavaDelegate you can write
        /// 
        /// MySerializableVariable myVariable = (MySerializableVariable) execution.getVariable("myVariable");
        /// myVariable.setNumber(123);
        /// 
        /// And the changes to the java object will be reflected in the database.
        /// Otherwise, a manual call to setVariable will be needed.
        /// 
        /// By default true for backwards compatibility.
        /// </summary>
        protected internal bool serializableVariableTypeTrackDeserializedObjects = true;


        /// <summary>
        /// 
        /// </summary>
        protected internal ExpressionManager expressionManager;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<string> customScriptingEngineClasses;

        /// <summary>
        /// 
        /// </summary>
        protected internal ScriptingEngines scriptingEngines;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IResolverFactory> resolverFactories;


        /// <summary>
        /// 
        /// </summary>
        protected internal IBusinessCalendarManager businessCalendarManager;


        /// <summary>
        /// 
        /// </summary>
        protected internal int executionQueryLimit = 20000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int taskQueryLimit = 20000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int historicTaskQueryLimit = 20000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int historicProcessInstancesQueryLimit = 20000;

        /// <summary>
        /// 
        /// </summary>
        protected internal string wsSyncFactoryClassName = DEFAULT_WS_SYNC_FACTORY;

        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<string, Uri> wsOverridenEndpointAddresses = new ConcurrentDictionary<string, Uri>();

        /// <summary>
        /// 
        /// </summary>
        protected internal CommandContextFactory commandContextFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITransactionContextFactory transactionContextFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<object, object> beans;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDelegateInterceptor delegateInterceptor;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, IEventHandler> eventHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IEventHandler> customEventHandlers;

        /// <summary>
        /// 
        /// </summary>
        protected internal IFailedJobCommandFactory failedJobCommandFactory;

        /// <summary>
        /// Set this to true if you want to have extra checks on the BPMN xml that is parsed. See http://www.jorambarrez.be/blog/2013/02/19/uploading-a-funny-xml -can-bring-down-your-server/
        /// 
        /// Unfortunately, this feature is not available on some platforms (JDK 6, JBoss), hence the reason why it is disabled by default. If your platform allows the use of StaxSource during XML parsing, do
        /// enable it.
        /// </summary>
        protected internal bool enableSafeBpmnXml;

        /// <summary>
        /// The following settings will determine the amount of entities loaded at once when the engine needs to load multiple entities (eg. when suspending a process definition with all its process
        /// instances).
        /// 
        /// The default setting is quite low, as not to surprise anyone with sudden memory spikes. Change it to something higher if the environment Activiti runs in allows it.
        /// </summary>
        protected internal int batchSizeProcessInstances = 25;

        /// <summary>
        /// 
        /// </summary>
        protected internal int batchSizeTasks = 25;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool enableEventDispatcher = true;

        /// <summary>
        /// 
        /// </summary>
        protected internal IActivitiEventDispatcher eventDispatcher;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IActivitiEventListener> eventListeners;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, IList<IActivitiEventListener>> typedEventListeners;

        // Event logging to database

        /// <summary>
        /// 
        /// </summary>
        protected internal bool enableDatabaseEventLogging;

        /// <summary>
        /// Using field injection together with a delegate expression for a service
        /// task / execution listener / task listener is not thread-sade , see user
        /// guide section 'Field Injection' for more information.
        /// 
        /// Set this flag to false to throw an exception at runtime when a field is
        /// injected and a delegateExpression is used.
        /// 
        /// @since 5.21
        /// </summary>
        protected internal DelegateExpressionFieldInjectionMode delegateExpressionFieldInjectionMode = DelegateExpressionFieldInjectionMode.MIXED;

        /// <summary>
        ///  Define a max length for storing String variable types in the database.
        ///  Mainly used for the Oracle NVARCHAR2 limit of 2000 characters
        /// </summary>
        protected internal int maxLengthStringVariableType = -1;

        /// <summary>
        /// If set to true, enables bulk insert (grouping sql inserts together).
        /// Default true. For some databases (eg DB2 on Zos: https://activiti.atlassian.net/browse/ACT-4042) needs to be set to false
        /// </summary>
        protected internal bool isBulkInsertEnabled = true;

        /// <summary>
        /// Some databases have a limit of how many parameters one sql insert can have (eg SQL Server, 2000 params (!= insert statements) ).
        /// Tweak this parameter in case of exceptions indicating too much is being put into one bulk insert,
        /// or make it higher if your database can cope with it and there are inserts with a huge amount of data.
        /// 
        /// By default: 100 (75 for mssql server as it has a hard limit of 2000 parameters in a statement)
        /// </summary>
        protected internal int maxNrOfStatementsInBulkInsert = 100;


        /// <summary>
        /// 
        /// </summary>
        public int DEFAULT_MAX_NR_OF_STATEMENTS_BULK_INSERT_SQL_SERVER = 70; // currently Execution has most params (28). 2000 / 28 = 71.


        /// <summary>
        /// 
        /// </summary>
        protected internal ObjectMapper objectMapper = new ObjectMapper();

        /// <summary>
        /// Flag that can be set to configure or nota relational database is used.
        /// This is useful for custom implementations that do not use relational databases at all.
        /// 
        /// If true (default), the <seealso cref="ProcessEngineConfiguration.DatabaseSchemaUpdate"/> value will be used to determine
        /// what needs to happen wrt the database schema.
        /// 
        /// If false, no validation or schema creation will be done. That means that the database schema must have been
        /// created 'manually' before but the engine does not validate whether the schema is correct.
        /// The <seealso cref="ProcessEngineConfiguration.DatabaseSchemaUpdate"/> value will not be used.
        /// </summary>
        protected internal bool usingRelationalDatabase = true;

        /// <summary>
        /// Enabled a very verbose debug output of the execution tree whilst executing operations.
        /// Most useful for core engine developers or people fiddling around with the execution tree.
        /// </summary>
        protected internal bool enableVerboseExecutionTreeLogging;


        /// <summary>
        /// 
        /// </summary>
        protected internal PerformanceSettings performanceSettings = new PerformanceSettings();


        /// <summary>
        /// 
        /// </summary>
        private readonly object syncRoot = new object();
        #endregion

        // buildProcessEngine
        // ///////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        public override IProcessEngine BuildProcessEngine()
        {
            Init();

            ProcessEngineImpl processEngine = new ProcessEngineImpl(this);

            PostProcessEngineInitialisation();

            return processEngine;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            InitConfigurators();
            ConfiguratorsBeforeInit();
            InitHistoryLevel();
            InitExpressionManager();

            if (usingRelationalDatabase)
            {
                InitDataSource();
            }

            InitAgendaFactory();
            InitHelpers();
            InitVariableTypes();
            InitBeans();
            InitScriptingEngines();
            InitClock();
            InitBusinessCalendarManager();
            InitCommandContextFactory();
            InitTransactionContextFactory();
            //初始化命令执行器
            InitCommandExecutors();
            //服务初始化
            InitServices();
            InitIdGenerator();
            InitBehaviorFactory();
            InitListenerFactory();
            InitBpmnParser();
            InitProcessDefinitionCache();
            InitProcessDefinitionInfoCache();
            InitKnowledgeBaseCache();
            InitJobHandlers();
            InitJobManager();
            InitAsyncExecutor();

            InitTransactionFactory();

            if (usingRelationalDatabase)
            {
                InitSqlSessionFactory();
            }

            InitSessionFactories();
            InitDataManagers();
            InitEntityManagers();
            InitHistoryManager();
            InitDeployers();
            InitDelegateInterceptor();
            InitEventHandlers();
            InitFailedJobCommandFactory();
            InitEventDispatcher();
            InitProcessValidator();
            InitDatabaseEventLogging();
            InitGetBookmarkRuleProvider();
            ConfiguratorsAfterInit();

            AddDefaultEventListner();
        }

        private void AddDefaultEventListner()
        {
            this.eventDispatcher.AddEventListener(this.listenerFactory.CreateCustomTaskCompletedEventListener(), ActivitiEventType.TASK_TERMINATED);
            this.eventDispatcher.AddEventListener(this.listenerFactory.CreateCustomTaskCompletedEventListener(), ActivitiEventType.TASK_TRANSFERED);
        }

        private void InitGetBookmarkRuleProvider()
        {
            GetBookmarkRuleProvider = new GetBookmarkRuleProvider();
        }

        // failedJobCommandFactory
        // ////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual void InitFailedJobCommandFactory()
        {
            if (failedJobCommandFactory == null)
            {
                failedJobCommandFactory = new DefaultFailedJobCommandFactory();
            }
        }

        // command executors
        // ////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual void InitCommandExecutors()
        {
            InitDefaultCommandConfig();
            InitSchemaCommandConfig();
            //命令调用器
            InitCommandInvoker();
            //初始化命令拦截器
            InitCommandInterceptors();
            //初始化命令执行器
            InitCommandExecutor();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDefaultCommandConfig()
        {
            if (defaultCommandConfig == null)
            {
                defaultCommandConfig = new CommandConfig();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitSchemaCommandConfig()
        {
            if (schemaCommandConfig == null)
            {
                schemaCommandConfig = (new CommandConfig()).TransactionNotSupported();
            }
        }


        /// <summary>
        /// 命令调用器
        /// </summary>
        public virtual void InitCommandInvoker()
        {
            if (commandInvoker == null)
            {
                if (enableVerboseExecutionTreeLogging)
                {
                    commandInvoker = new DebugCommandInvoker();
                }
                else
                {
                    commandInvoker = new CommandInvoker();
                }
            }
        }

        /// <summary>
        /// 初始化命令拦截器
        /// </summary>
        public virtual void InitCommandInterceptors()
        {
            if (commandInterceptors == null)
            {
                commandInterceptors = new List<ICommandInterceptor>();
                if (customPreCommandInterceptors != null)
                {
                    //客户自定义前置拦截器 
                    ((List<ICommandInterceptor>)commandInterceptors).AddRange(customPreCommandInterceptors);
                }
                //默认拦截器
                ((List<ICommandInterceptor>)commandInterceptors).AddRange(DefaultCommandInterceptors);
                if (customPostCommandInterceptors != null)
                {
                    //后置拦截器
                    ((List<ICommandInterceptor>)commandInterceptors).AddRange(customPostCommandInterceptors);
                }

                //命令调用器，拦截器最后的一个，为调用具体的命令
                commandInterceptors.Add(commandInvoker);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<ICommandInterceptor> DefaultCommandInterceptors
        {
            get
            {
                IList<ICommandInterceptor> interceptors = new List<ICommandInterceptor>
                {
                    //添加一个日志拦截器
                    new LogInterceptor()
                };

                ICommandInterceptor transactionInterceptor = CreateTransactionInterceptor();
                if (transactionInterceptor != null)
                {
                    //添加一个事务控制拦截器
                    interceptors.Add(transactionInterceptor);
                }

                if (commandContextFactory != null)
                {
                    //CommandContext拦截器，进行命令的保存
                    interceptors.Add(new CommandContextInterceptor(commandContextFactory, this));
                }

                if (transactionContextFactory != null)
                {
                    interceptors.Add(new TransactionContextInterceptor(transactionContextFactory));
                }

                return interceptors;
            }
        }

        /// <summary>
        /// 初始化命令执行器
        /// </summary>
        public virtual void InitCommandExecutor()
        {
            if (commandExecutor == null)
            {
                //初始化命令拦截器链，并返回第一个拦截器
                ICommandInterceptor first = InitInterceptorChain(commandInterceptors);
                commandExecutor = new CommandExecutorImpl(DefaultCommandConfig, first);
            }
        }

        /// <summary>
        /// //构建拦截器链，即上一个拦截器中存放着下一个拦截器
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public virtual ICommandInterceptor InitInterceptorChain(IList<ICommandInterceptor> chain)
        {
            if (chain == null || chain.Count == 0)
            {
                throw new ActivitiException("invalid command interceptor chain configuration: " + chain);
            }
            for (int i = 0; i < chain.Count - 1; i++)
            {
                chain[i].Next = chain[i + 1];
            }
            return chain[0];
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract ICommandInterceptor CreateTransactionInterceptor();

        /// <summary>
        /// 服务初始化
        /// </summary>
        public virtual void InitServices()
        {
            InitService(repositoryService);
            InitService(runtimeService);
            InitService(historyService);
            InitService(taskService);
            InitService(managementService);
            InitService(dynamicBpmnService);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitService(object service)
        {
            if (service is ServiceImpl)
            {
                //将命令执行器set进runtimeService中
                ((ServiceImpl)service).CommandExecutor = commandExecutor;
            }
        }

        // DataSource
        // ///////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDataSource()
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                InitDatabaseType();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal static Properties databaseTypeMappings;

        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_H2 = "h2";
        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_HSQL = "hsql";
        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_MYSQL = "mysql";
        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_ORACLE = "oracle";
        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_POSTGRES = "postgres";
        /// <summary>
        /// 
        /// </summary>
        public const string DATABASE_TYPE_MSSQL = "mssql";

        static ProcessEngineConfigurationImpl()
        {
            databaseTypeMappings = new Properties
            {
                ["H2"] = DATABASE_TYPE_H2,
                ["HSQL Database Engine"] = DATABASE_TYPE_HSQL,
                ["MySQL"] = DATABASE_TYPE_MYSQL,
                ["Oracle"] = DATABASE_TYPE_ORACLE,
                ["PostgreSQL"] = DATABASE_TYPE_POSTGRES,
                ["Microsoft SQL Server"] = DATABASE_TYPE_MSSQL
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDatabaseType()
        {
            try
            {
                IDatabaseReader dbreader = ProcessEngineServiceProvider.Resolve<IDatabaseReader>();

                log.LogDebug($"database product name: '{dbreader.DatabaseSchema.Provider}'");
                // Special care for MSSQL, as it has a hard limit of 2000 params per statement (incl bulk statement).
                // Especially with executions, with 100 as default, this limit is passed.
                switch (dbreader.DatabaseSchema.Provider)
                {
                    case "System.Data.SqlClient":
                        databaseType = DATABASE_TYPE_MSSQL;
                        break;
                    case "System.Data.OracleClient":
                        databaseType = DATABASE_TYPE_ORACLE;
                        break;
                    case "Npgsql":
                        databaseType = DATABASE_TYPE_POSTGRES;
                        break;
                    case "MySql.Data.MySqlClient":
                        databaseType = DATABASE_TYPE_MYSQL;
                        break;
                    case "System.Data.H2":
                        databaseType = DATABASE_TYPE_H2;
                        break;

                }

                if (DATABASE_TYPE_MSSQL.Equals(databaseType))
                {
                    maxNrOfStatementsInBulkInsert = DEFAULT_MAX_NR_OF_STATEMENTS_BULK_INSERT_SQL_SERVER;
                }

            }
            catch (Exception e)
            {
                log.LogError(e, "Exception while initializing Database connection");
            }
        }

        // myBatis SqlSessionFactory
        // ////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitTransactionFactory()
        {
            if (transactionFactory == null)
            {
                if (transactionsExternallyManaged)
                {
                    transactionFactory = new ManagedTransactionFactory();
                }
                else
                {
                    transactionFactory = new JdbcTransactionFactory();
                }
            }
        }


        private Properties properties;
        /// <summary>
        /// 
        /// </summary>
        public override Properties GetProperties()
        {
            if (properties == null)
            {
                InitProperties();
            }

            return properties;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitProperties()
        {
            lock (syncRoot)
            {
                string wildcardEscapeClause = "";
                if ((!(databaseWildcardEscapeCharacter is null)) && (databaseWildcardEscapeCharacter.Length != 0))
                {
                    wildcardEscapeClause = " escape '" + databaseWildcardEscapeCharacter + "'";
                }

                properties = new Properties
                {
                    ["wildcardEscapeClause"] = wildcardEscapeClause,
                    //set default properties
                    ["limitBefore"] = "",
                    ["limitAfter"] = "",
                    ["limitBetween"] = "",
                    ["limitOuterJoinBetween"] = "",
                    ["limitBeforeNativeQuery"] = "",
                    ["orderBy"] = "",
                    ["blobType"] = "",
                    ["boolValue"] = "TRUE"
                };

                string codebase = AppDomain.CurrentDomain.BaseDirectory;

                string pFile = Path.Combine(new string[]{
                    Path.GetDirectoryName(codebase),
                        "resources",
                        "db",
                        "properties",
                        $"{databaseType}.json"
                });

                if (File.Exists(pFile) == false)
                {
                    throw new FileNotFoundException(pFile);
                }

                properties.Load(pFile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitSqlSessionFactory()
        {
            InitMybatisConfiguration(GetProperties());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitMybatisConfiguration(Properties properties)
        {
            InitMybatisTypeHandlers();
            InitCustomMybatisMappers();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitMybatisTypeHandlers()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitCustomMybatisMappers()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual Stream GetResourceAsStream(string resource)
        {
            return ReflectUtil.GetResourceAsStream(resource);
        }


        // Data managers ///////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDataManagers()
        {
            if (attachmentDataManager == null)
            {
                attachmentDataManager = new MybatisAttachmentDataManager(this);
            }
            if (byteArrayDataManager == null)
            {
                byteArrayDataManager = new MybatisByteArrayDataManager(this);
            }
            if (commentDataManager == null)
            {
                commentDataManager = new MybatisCommentDataManager(this);
            }
            if (deploymentDataManager == null)
            {
                deploymentDataManager = new MybatisDeploymentDataManager(this);
            }
            if (eventLogEntryDataManager == null)
            {
                eventLogEntryDataManager = new MybatisEventLogEntryDataManager(this);
            }
            if (eventSubscriptionDataManager == null)
            {
                eventSubscriptionDataManager = new MybatisEventSubscriptionDataManager(this);
            }
            if (executionDataManager == null)
            {
                executionDataManager = new MybatisExecutionDataManager(this);
            }
            if (historicActivityInstanceDataManager == null)
            {
                historicActivityInstanceDataManager = new MybatisHistoricActivityInstanceDataManager(this);
            }
            if (historicDetailDataManager == null)
            {
                historicDetailDataManager = new MybatisHistoricDetailDataManager(this);
            }
            if (historicIdentityLinkDataManager == null)
            {
                historicIdentityLinkDataManager = new MybatisHistoricIdentityLinkDataManager(this);
            }
            if (historicProcessInstanceDataManager == null)
            {
                historicProcessInstanceDataManager = new MybatisHistoricProcessInstanceDataManager(this);
            }
            if (historicTaskInstanceDataManager == null)
            {
                historicTaskInstanceDataManager = new MybatisHistoricTaskInstanceDataManager(this);
            }
            if (historicVariableInstanceDataManager == null)
            {
                historicVariableInstanceDataManager = new MybatisHistoricVariableInstanceDataManager(this);
            }
            if (identityLinkDataManager == null)
            {
                identityLinkDataManager = new MybatisIdentityLinkDataManager(this);
            }
            if (jobDataManager == null)
            {
                jobDataManager = new MybatisJobDataManager(this);
            }
            if (timerJobDataManager == null)
            {
                timerJobDataManager = new MybatisTimerJobDataManager(this);
            }
            if (suspendedJobDataManager == null)
            {
                suspendedJobDataManager = new MybatisSuspendedJobDataManager(this);
            }
            if (deadLetterJobDataManager == null)
            {
                deadLetterJobDataManager = new MybatisDeadLetterJobDataManager(this);
            }
            if (modelDataManager == null)
            {
                modelDataManager = new MybatisModelDataManager(this);
            }
            if (processDefinitionDataManager == null)
            {
                processDefinitionDataManager = new MybatisProcessDefinitionDataManager(this);
            }
            if (processDefinitionInfoDataManager == null)
            {
                processDefinitionInfoDataManager = new MybatisProcessDefinitionInfoDataManager(this);
            }
            if (propertyDataManager == null)
            {
                propertyDataManager = new MybatisPropertyDataManager(this);
            }
            if (resourceDataManager == null)
            {
                resourceDataManager = new MybatisResourceDataManager(this);
            }
            if (taskDataManager == null)
            {
                taskDataManager = new MybatisTaskDataManager(this);
            }
            if (variableInstanceDataManager == null)
            {
                variableInstanceDataManager = new MybatisVariableInstanceDataManager(this);
            }
        }

        // Entity managers //////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitEntityManagers()
        {
            if (attachmentEntityManager == null)
            {
                attachmentEntityManager = new AttachmentEntityManagerImpl(this, attachmentDataManager);
            }
            if (byteArrayEntityManager == null)
            {
                byteArrayEntityManager = new ByteArrayEntityManagerImpl(this, byteArrayDataManager);
            }
            if (commentEntityManager == null)
            {
                commentEntityManager = new CommentEntityManagerImpl(this, commentDataManager);
            }
            if (deploymentEntityManager == null)
            {
                deploymentEntityManager = new DeploymentEntityManagerImpl(this, deploymentDataManager);
            }
            if (eventLogEntryEntityManager == null)
            {
                eventLogEntryEntityManager = new EventLogEntryEntityManagerImpl(this, eventLogEntryDataManager);
            }
            if (eventSubscriptionEntityManager == null)
            {
                eventSubscriptionEntityManager = new EventSubscriptionEntityManagerImpl(this, eventSubscriptionDataManager);
            }
            if (executionEntityManager == null)
            {
                executionEntityManager = new ExecutionEntityManagerImpl(this, executionDataManager);
            }
            if (historicActivityInstanceEntityManager == null)
            {
                historicActivityInstanceEntityManager = new HistoricActivityInstanceEntityManagerImpl(this, historicActivityInstanceDataManager);
            }
            if (historicDetailEntityManager == null)
            {
                historicDetailEntityManager = new HistoricDetailEntityManagerImpl(this, historicDetailDataManager);
            }
            if (historicIdentityLinkEntityManager == null)
            {
                historicIdentityLinkEntityManager = new HistoricIdentityLinkEntityManagerImpl(this, historicIdentityLinkDataManager);
            }
            if (historicProcessInstanceEntityManager == null)
            {
                historicProcessInstanceEntityManager = new HistoricProcessInstanceEntityManagerImpl(this, historicProcessInstanceDataManager);
            }
            if (historicTaskInstanceEntityManager == null)
            {
                historicTaskInstanceEntityManager = new HistoricTaskInstanceEntityManagerImpl(this, historicTaskInstanceDataManager);
            }
            if (historicVariableInstanceEntityManager == null)
            {
                historicVariableInstanceEntityManager = new HistoricVariableInstanceEntityManagerImpl(this, historicVariableInstanceDataManager);
            }

            if (identityLinkEntityManager == null)
            {
                identityLinkEntityManager = new IdentityLinkEntityManagerImpl(this, identityLinkDataManager);
            }
            if (jobEntityManager == null)
            {
                jobEntityManager = new JobEntityManagerImpl(this, jobDataManager);
            }
            if (timerJobEntityManager == null)
            {
                timerJobEntityManager = new TimerJobEntityManagerImpl(this, timerJobDataManager);
            }
            if (suspendedJobEntityManager == null)
            {
                suspendedJobEntityManager = new SuspendedJobEntityManagerImpl(this, suspendedJobDataManager);
            }
            if (deadLetterJobEntityManager == null)
            {
                deadLetterJobEntityManager = new DeadLetterJobEntityManagerImpl(this, deadLetterJobDataManager);
            }
            if (modelEntityManager == null)
            {
                modelEntityManager = new ModelEntityManagerImpl(this, modelDataManager);
            }
            if (processDefinitionEntityManager == null)
            {
                processDefinitionEntityManager = new ProcessDefinitionEntityManagerImpl(this, processDefinitionDataManager);
            }
            if (processDefinitionInfoEntityManager == null)
            {
                processDefinitionInfoEntityManager = new ProcessDefinitionInfoEntityManagerImpl(this, processDefinitionInfoDataManager);
            }
            if (propertyEntityManager == null)
            {
                propertyEntityManager = new PropertyEntityManagerImpl(this, propertyDataManager);
            }
            if (resourceEntityManager == null)
            {
                resourceEntityManager = new ResourceEntityManagerImpl(this, resourceDataManager);
            }
            if (tableDataManager == null)
            {
                tableDataManager = new TableDataManagerImpl(this);
            }
            if (taskEntityManager == null)
            {
                taskEntityManager = new TaskEntityManagerImpl(this, taskDataManager);
            }
            if (variableInstanceEntityManager == null)
            {
                variableInstanceEntityManager = new VariableInstanceEntityManagerImpl(this, variableInstanceDataManager);
            }
        }

        // History manager ///////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitHistoryManager()
        {
            if (historyManager == null)
            {
                historyManager = new DefaultHistoryManager(this, historyLevel);
            }
        }

        // Job manager ///////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitJobManager()
        {
            if (jobManager == null)
            {
                jobManager = new DefaultJobManager(this);
            }

            jobManager.ProcessEngineConfiguration = this;
        }

        // session factories ////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitSessionFactories()
        {
            if (sessionFactories == null)
            {
                sessionFactories = new ConcurrentDictionary<Type, ISessionFactory>();

                if (usingRelationalDatabase)
                {
                    InitDbSqlSessionFactory();
                }

                AddSessionFactory(new GenericManagerFactory(typeof(IEntityCache), typeof(EntityCacheImpl)));
            }

            if (customSessionFactories != null)
            {
                foreach (ISessionFactory sessionFactory in customSessionFactories)
                {
                    AddSessionFactory(sessionFactory);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDbSqlSessionFactory()
        {
            if (dbSqlSessionFactory == null)
            {
                dbSqlSessionFactory = CreateDbSqlSessionFactory();
            }
            dbSqlSessionFactory.DatabaseType = databaseType;
            dbSqlSessionFactory.IdGenerator = idGenerator;
            //dbSqlSessionFactory.SqlSessionFactory = sqlSessionFactory;
            dbSqlSessionFactory.DbHistoryUsed = isDbHistoryUsed;
            dbSqlSessionFactory.DatabaseTablePrefix = databaseTablePrefix;
            dbSqlSessionFactory.TablePrefixIsSchema = tablePrefixIsSchema;
            dbSqlSessionFactory.DatabaseCatalog = databaseCatalog;
            dbSqlSessionFactory.DatabaseSchema = databaseSchema;
            dbSqlSessionFactory.SetBulkInsertEnabled(isBulkInsertEnabled, databaseType);
            dbSqlSessionFactory.MaxNrOfStatementsInBulkInsert = maxNrOfStatementsInBulkInsert;
            AddSessionFactory(dbSqlSessionFactory);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSqlSessionFactory CreateDbSqlSessionFactory()
        {
            return new DbSqlSessionFactory();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddSessionFactory(ISessionFactory sessionFactory)
        {
            sessionFactories.AddOrUpdate(sessionFactory.SessionType, sessionFactory, (key, old) => sessionFactory);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitConfigurators()
        {
            allConfigurators = new List<IProcessEngineConfigurator>();

            // Configurators that are explicitly added to the config
            if (configurators != null)
            {
                foreach (IProcessEngineConfigurator configurator in configurators)
                {
                    allConfigurators.Add(configurator);
                }
            }

            // Auto discovery through ServiceLoader
            if (enableConfiguratorServiceLoader)
            {
                ClassLoader classLoader = ClassLoader;
                if (classLoader == null)
                {
                    classLoader = ReflectUtil.ClassLoader;
                }

                IEnumerable<IProcessEngineConfigurator> configuratorServiceLoader = ServiceLoader.Load<IEnumerable<IProcessEngineConfigurator>>(classLoader);
                int nrOfServiceLoadedConfigurators = 0;
                foreach (IProcessEngineConfigurator configurator in configuratorServiceLoader)
                {
                    allConfigurators.Add(configurator);
                    nrOfServiceLoadedConfigurators++;
                }

                if (nrOfServiceLoadedConfigurators > 0)
                {
                    log.LogInformation($"Found {nrOfServiceLoadedConfigurators++} auto-discoverable Process Engine Configurator{(nrOfServiceLoadedConfigurators > 1 ? "s" : "")}");
                }

                if (allConfigurators.Count > 0)
                {
                    // Order them according to the priorities (useful for dependent
                    // configurator)
                    allConfigurators.OrderBy(x => x, new ComparatorAnonymousInnerClass(this));

                    // Execute the configurators
                    log.LogInformation($"Found {allConfigurators.Count} Process Engine Configurators in total:");
                    foreach (IProcessEngineConfigurator configurator in allConfigurators)
                    {
                        log.LogInformation($"{configurator.GetType()} (priority:{configurator.Priority})");
                    }
                }
            }
        }

        private class ComparatorAnonymousInnerClass : IComparer<IProcessEngineConfigurator>
        {
            private readonly ProcessEngineConfigurationImpl outerInstance;

            public ComparatorAnonymousInnerClass(ProcessEngineConfigurationImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual int Compare(IProcessEngineConfigurator configurator1, IProcessEngineConfigurator configurator2)
            {
                int priority1 = configurator1.Priority;
                int priority2 = configurator2.Priority;

                if (priority1 < priority2)
                {
                    return -1;
                }
                else if (priority1 > priority2)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ConfiguratorsBeforeInit()
        {
            foreach (IProcessEngineConfigurator configurator in allConfigurators)
            {
                log.LogInformation($"Executing beforeInit() of {configurator.GetType()} (priority:{configurator.Priority})");
                configurator.BeforeInit(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ConfiguratorsAfterInit()
        {
            foreach (IProcessEngineConfigurator configurator in allConfigurators)
            {
                log.LogInformation($"Executing configure() of {configurator.GetType()} (priority:{configurator.Priority})");
                configurator.Configure(this);
            }
        }

        // deployers
        // ////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitProcessDefinitionCache()
        {
            if (processDefinitionCache == null)
            {
                if (processDefinitionCacheLimit <= 0)
                {
                    processDefinitionCache = new DefaultDeploymentCache<ProcessDefinitionCacheEntry>();
                }
                else
                {
                    processDefinitionCache = new DefaultDeploymentCache<ProcessDefinitionCacheEntry>(processDefinitionCacheLimit);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitProcessDefinitionInfoCache()
        {
            if (processDefinitionInfoCache == null)
            {
                if (processDefinitionInfoCacheLimit <= 0)
                {
                    processDefinitionInfoCache = new ProcessDefinitionInfoCache(commandExecutor);
                }
                else
                {
                    processDefinitionInfoCache = new ProcessDefinitionInfoCache(commandExecutor, processDefinitionInfoCacheLimit);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitKnowledgeBaseCache()
        {
            if (knowledgeBaseCache == null)
            {
                if (knowledgeBaseCacheLimit <= 0)
                {
                    knowledgeBaseCache = new DefaultDeploymentCache<object>();
                }
                else
                {
                    knowledgeBaseCache = new DefaultDeploymentCache<object>(knowledgeBaseCacheLimit);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDeployers()
        {
            if (this.deployers == null)
            {
                this.deployers = new List<IDeployer>();
                if (customPreDeployers != null)
                {
                    ((List<IDeployer>)this.deployers).AddRange(customPreDeployers);
                }
              ((List<IDeployer>)this.deployers).AddRange(DefaultDeployers);
                if (customPostDeployers != null)
                {
                    ((List<IDeployer>)this.deployers).AddRange(customPostDeployers);
                }
            }

            if (deploymentManager == null)
            {
                deploymentManager = new DeploymentManager
                {
                    Deployers = deployers,
                    ProcessDefinitionCache = processDefinitionCache,
                    ProcessDefinitionInfoCache = processDefinitionInfoCache,
                    KnowledgeBaseCache = knowledgeBaseCache,
                    ProcessEngineConfiguration = this,
                    ProcessDefinitionEntityManager = processDefinitionEntityManager,
                    DeploymentEntityManager = deploymentEntityManager
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitBpmnDeployerDependencies()
        {
            if (parsedDeploymentBuilderFactory == null)
            {
                parsedDeploymentBuilderFactory = new ParsedDeploymentBuilderFactory();
            }
            if (parsedDeploymentBuilderFactory.BpmnParser == null)
            {
                parsedDeploymentBuilderFactory.BpmnParser = bpmnParser;
            }

            if (timerManager == null)
            {
                timerManager = new TimerManager();
            }

            if (eventSubscriptionManager == null)
            {
                eventSubscriptionManager = new EventSubscriptionManager();
            }

            if (bpmnDeploymentHelper == null)
            {
                bpmnDeploymentHelper = new BpmnDeploymentHelper();
            }
            if (bpmnDeploymentHelper.TimerManager == null)
            {
                bpmnDeploymentHelper.TimerManager = timerManager;
            }
            if (bpmnDeploymentHelper.EventSubscriptionManager == null)
            {
                bpmnDeploymentHelper.EventSubscriptionManager = eventSubscriptionManager;
            }

            if (cachingAndArtifactsManager == null)
            {
                cachingAndArtifactsManager = new CachingAndArtifactsManager();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<IDeployer> DefaultDeployers
        {
            get
            {
                IList<IDeployer> defaultDeployers = new List<IDeployer>();

                if (bpmnDeployer == null)
                {
                    bpmnDeployer = new BpmnDeployer();
                }

                InitBpmnDeployerDependencies();

                bpmnDeployer.IdGenerator = idGenerator;
                bpmnDeployer.ParsedDeploymentBuilderFactory = parsedDeploymentBuilderFactory;
                bpmnDeployer.BpmnDeploymentHelper = bpmnDeploymentHelper;
                bpmnDeployer.CachingAndArtifactsManager = cachingAndArtifactsManager;

                defaultDeployers.Add(bpmnDeployer);
                return defaultDeployers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitListenerFactory()
        {
            if (listenerFactory == null)
            {
                DefaultListenerFactory defaultListenerFactory = new DefaultListenerFactory
                {
                    ExpressionManager = expressionManager
                };
                listenerFactory = defaultListenerFactory;
            }
            else if ((listenerFactory is AbstractBehaviorFactory) && ((AbstractBehaviorFactory)listenerFactory).ExpressionManager == null)
            {
                ((AbstractBehaviorFactory)listenerFactory).ExpressionManager = expressionManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitBehaviorFactory()
        {
            if (activityBehaviorFactory == null)
            {
                DefaultActivityBehaviorFactory defaultActivityBehaviorFactory = new DefaultActivityBehaviorFactory
                {
                    ExpressionManager = expressionManager
                };
                activityBehaviorFactory = defaultActivityBehaviorFactory;
            }
            else if ((activityBehaviorFactory is AbstractBehaviorFactory) && ((AbstractBehaviorFactory)activityBehaviorFactory).ExpressionManager == null)
            {
                ((AbstractBehaviorFactory)activityBehaviorFactory).ExpressionManager = expressionManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitBpmnParser()
        {
            if (bpmnParser == null)
            {
                bpmnParser = new BpmnParser();
            }

            if (bpmnParseFactory == null)
            {
                bpmnParseFactory = ProcessEngineServiceProvider.Resolve<IBpmnParseFactory>();
            }

            bpmnParser.BpmnParseFactory = bpmnParseFactory;
            bpmnParser.ActivityBehaviorFactory = activityBehaviorFactory;
            bpmnParser.ListenerFactory = listenerFactory;

            IList<IBpmnParseHandler> parseHandlers = new List<IBpmnParseHandler>();
            if (PreBpmnParseHandlers != null)
            {
                ((List<IBpmnParseHandler>)parseHandlers).AddRange(PreBpmnParseHandlers);
            }
          ((List<IBpmnParseHandler>)parseHandlers).AddRange(DefaultBpmnParseHandlers);
            if (PostBpmnParseHandlers != null)
            {
                ((List<IBpmnParseHandler>)parseHandlers).AddRange(PostBpmnParseHandlers);
            }

            BpmnParseHandlers bpmnParseHandlers = new BpmnParseHandlers();
            bpmnParseHandlers.AddHandlers(parseHandlers);
            bpmnParser.BpmnParserHandlers = bpmnParseHandlers;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IBpmnParseHandler> DefaultBpmnParseHandlers
        {
            get
            {
                // Alphabetic list of default parse handler classes
                IList<IBpmnParseHandler> bpmnParserHandlers = new List<IBpmnParseHandler>
                {
                    new BoundaryEventParseHandler(),
                    new BusinessRuleParseHandler(),
                    new CallActivityParseHandler(),
                    new CancelEventDefinitionParseHandler(),
                    new CompensateEventDefinitionParseHandler(),
                    new EndEventParseHandler(),
                    new ErrorEventDefinitionParseHandler(),
                    new EventBasedGatewayParseHandler(),
                    new ExclusiveGatewayParseHandler(),
                    new InclusiveGatewayParseHandler(),
                    new IntermediateCatchEventParseHandler(),
                    new IntermediateThrowEventParseHandler(),
                    new ManualTaskParseHandler(),
                    new MessageEventDefinitionParseHandler(),
                    new ParallelGatewayParseHandler(),
                    new ProcessParseHandler(),
                    new ReceiveTaskParseHandler(),
                    new ScriptTaskParseHandler(),
                    new SendTaskParseHandler(),
                    new SequenceFlowParseHandler(),
                    new ServiceTaskParseHandler(),
                    new SignalEventDefinitionParseHandler(),
                    new StartEventParseHandler(),
                    new SubProcessParseHandler(),
                    new EventSubProcessParseHandler(),
                    new AdhocSubProcessParseHandler(),
                    new TaskParseHandler(),
                    new TimerEventDefinitionParseHandler(),
                    new TransactionParseHandler(),
                    new UserTaskParseHandler()
                };

                // Replace any default handler if the user wants to replace them
                if (customDefaultBpmnParseHandlers != null)
                {
                    IDictionary<Type, IBpmnParseHandler> customParseHandlerMap = new Dictionary<Type, IBpmnParseHandler>();
                    foreach (IBpmnParseHandler bpmnParseHandler in customDefaultBpmnParseHandlers)
                    {
                        foreach (Type handledType in bpmnParseHandler.HandledTypes)
                        {
                            customParseHandlerMap[handledType] = bpmnParseHandler;
                        }
                    }

                    for (int i = 0; i < bpmnParserHandlers.Count; i++)
                    {
                        // All the default handlers support only one type
                        IBpmnParseHandler defaultBpmnParseHandler = bpmnParserHandlers[i];
                        if (defaultBpmnParseHandler.HandledTypes.Count != 1)
                        {
                            StringBuilder supportedTypes = new StringBuilder();
                            foreach (Type type in defaultBpmnParseHandler.HandledTypes)
                            {
                                supportedTypes.Append(" ").Append(type.FullName).Append(" ");
                            }
                            throw new ActivitiException("The default BPMN parse handlers should only support one type, but " + defaultBpmnParseHandler.GetType() + " supports " + supportedTypes.ToString() + ". This is likely a programmatic error");
                        }
                        else
                        {
                            Type handledType = defaultBpmnParseHandler.HandledTypes.GetEnumerator().MoveNext() ? defaultBpmnParseHandler.HandledTypes.GetEnumerator().Current : null;
                            if (customParseHandlerMap.ContainsKey(handledType))
                            {
                                IBpmnParseHandler newBpmnParseHandler = customParseHandlerMap[handledType];
                                log.LogInformation("Replacing default BpmnParseHandler " + defaultBpmnParseHandler.GetType().FullName + " with " + newBpmnParseHandler.GetType().FullName);
                                bpmnParserHandlers[i] = newBpmnParseHandler;
                            }
                        }
                    }
                }

                return bpmnParserHandlers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitClock()
        {
            if (clock == null)
            {
                clock = new DefaultClockImpl();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitAgendaFactory()
        {
            if (this.engineAgendaFactory == null)
            {
                this.engineAgendaFactory = ProcessEngineServiceProvider.Resolve<IActivitiEngineAgendaFactory>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitJobHandlers()
        {
            jobHandlers = new Dictionary<string, IJobHandler>();

            AsyncContinuationJobHandler asyncContinuationJobHandler = new AsyncContinuationJobHandler();
            jobHandlers[asyncContinuationJobHandler.Type] = asyncContinuationJobHandler;

            TriggerTimerEventJobHandler triggerTimerEventJobHandler = new TriggerTimerEventJobHandler();
            jobHandlers[triggerTimerEventJobHandler.Type] = triggerTimerEventJobHandler;

            TimerStartEventJobHandler timerStartEvent = new TimerStartEventJobHandler();
            jobHandlers[timerStartEvent.Type] = timerStartEvent;

            TimerSuspendProcessDefinitionHandler suspendProcessDefinitionHandler = new TimerSuspendProcessDefinitionHandler();
            jobHandlers[suspendProcessDefinitionHandler.Type] = suspendProcessDefinitionHandler;

            TimerActivateProcessDefinitionHandler activateProcessDefinitionHandler = new TimerActivateProcessDefinitionHandler();
            jobHandlers[activateProcessDefinitionHandler.Type] = activateProcessDefinitionHandler;

            ProcessEventJobHandler processEventJobHandler = new ProcessEventJobHandler();
            jobHandlers[processEventJobHandler.Type] = processEventJobHandler;

            // if we have custom job handlers, register them
            if (CustomJobHandlers != null)
            {
                foreach (IJobHandler customJobHandler in CustomJobHandlers)
                {
                    jobHandlers[customJobHandler.Type] = customJobHandler;
                }
            }
        }

        // async executor
        // /////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitAsyncExecutor()
        {
            if (asyncExecutor == null)
            {
                DefaultAsyncJobExecutor defaultAsyncExecutor = new DefaultAsyncJobExecutor
                {
                    // Message queue mode
                    MessageQueueMode = asyncExecutorMessageQueueMode,

                    // Thread pool config
                    CorePoolSize = asyncExecutorCorePoolSize,
                    MaxPoolSize = asyncExecutorMaxPoolSize,
                    KeepAliveTime = asyncExecutorThreadKeepAliveTime
                };

                // Threadpool queue
                if (asyncExecutorThreadPoolQueue != null)
                {
                    defaultAsyncExecutor.ThreadPoolQueue = asyncExecutorThreadPoolQueue;
                }
                defaultAsyncExecutor.QueueSize = asyncExecutorThreadPoolQueueSize;

                // Acquisition wait time
                defaultAsyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis = asyncExecutorDefaultTimerJobAcquireWaitTime;
                defaultAsyncExecutor.DefaultAsyncJobAcquireWaitTimeInMillis = asyncExecutorDefaultAsyncJobAcquireWaitTime;

                // Queue full wait time
                defaultAsyncExecutor.DefaultQueueSizeFullWaitTimeInMillis = asyncExecutorDefaultQueueSizeFullWaitTime;

                // Job locking
                defaultAsyncExecutor.TimerLockTimeInMillis = asyncExecutorTimerLockTimeInMillis;
                defaultAsyncExecutor.AsyncJobLockTimeInMillis = asyncExecutorAsyncJobLockTimeInMillis;
                if (!(asyncExecutorLockOwner is null))
                {
                    defaultAsyncExecutor.LockOwner = asyncExecutorLockOwner;
                }

                // Reset expired
                defaultAsyncExecutor.ResetExpiredJobsInterval = asyncExecutorResetExpiredJobsInterval;
                defaultAsyncExecutor.ResetExpiredJobsPageSize = asyncExecutorResetExpiredJobsPageSize;

                // Shutdown
                defaultAsyncExecutor.SecondsToWaitOnShutdown = asyncExecutorSecondsToWaitOnShutdown;

                asyncExecutor = defaultAsyncExecutor;
            }

            asyncExecutor.ProcessEngineConfiguration = this;
            asyncExecutor.AutoActivate = asyncExecutorActivate;
        }

        // history
        // //////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitHistoryLevel()
        {
            if (historyLevel == null)
            {
                historyLevel = HistoryLevel.GetHistoryLevelForKey(History);
            }
        }

        // id generator
        // /////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitIdGenerator()
        {
            if (idGenerator == null)
            {
                idGenerator = ProcessEngineServiceProvider.Resolve<IIdGenerator>();
                //interceptor.ICommandExecutor idGeneratorCommandExecutor = CommandExecutor;

                //DbIdGenerator dbIdGenerator = new DbIdGenerator();
                //dbIdGenerator.IdBlockSize = idBlockSize;
                //dbIdGenerator.CommandExecutor = idGeneratorCommandExecutor;
                //dbIdGenerator.CommandConfig = DefaultCommandConfig.transactionRequiresNew();
                //idGenerator = dbIdGenerator;
            }
        }

        // OTHER
        // ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitCommandContextFactory()
        {
            if (commandContextFactory == null)
            {
                commandContextFactory = new CommandContextFactory();
            }
            commandContextFactory.ProcessEngineConfiguration = this;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitTransactionContextFactory()
        {
            if (transactionContextFactory == null)
            {
                transactionContextFactory = new StandaloneMybatisTransactionContextFactory();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitHelpers()
        {
            if (processInstanceHelper == null)
            {
                processInstanceHelper = new ProcessInstanceHelper();
            }
            if (listenerNotificationHelper == null)
            {
                listenerNotificationHelper = new ListenerNotificationHelper();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitVariableTypes()
        {
            if (variableTypes == null)
            {
                variableTypes = new DefaultVariableTypes();
                if (customPreVariableTypes != null)
                {
                    foreach (IVariableType customVariableType in customPreVariableTypes)
                    {
                        variableTypes.AddType(customVariableType);
                    }
                }
                variableTypes.AddType(new NullType());
                variableTypes.AddType(new StringType(MaxLengthString));
                variableTypes.AddType(new LongStringType(MaxLengthString + 1));
                variableTypes.AddType(new BooleanType());
                variableTypes.AddType(new ShortType());
                variableTypes.AddType(new IntegerType());
                variableTypes.AddType(new LongType());
                variableTypes.AddType(new DateType());
                variableTypes.AddType(new JodaDateType());
                variableTypes.AddType(new JodaDateTimeType());
                variableTypes.AddType(new DoubleType());
                variableTypes.AddType(new UUIDType());
                variableTypes.AddType(new JsonType(MaxLengthString, objectMapper));
                variableTypes.AddType(new LongJsonType(MaxLengthString + 1, objectMapper));
                variableTypes.AddType(new ByteArrayType());
                variableTypes.AddType(new SerializableType(serializableVariableTypeTrackDeserializedObjects));
                variableTypes.AddType(new CustomObjectType("item", typeof(ItemInstance)));
                variableTypes.AddType(new CustomObjectType("message", typeof(MessageInstance)));
                if (customPostVariableTypes != null)
                {
                    foreach (IVariableType customVariableType in customPostVariableTypes)
                    {
                        variableTypes.AddType(customVariableType);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxLengthString
        {
            get
            {
                if (maxLengthStringVariableType == -1)
                {
                    if ("oracle".Equals(databaseType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return DEFAULT_ORACLE_MAX_LENGTH_STRING;
                    }
                    else
                    {
                        return DEFAULT_GENERIC_MAX_LENGTH_STRING;
                    }
                }
                else
                {
                    return maxLengthStringVariableType;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitScriptingEngines()
        {
            if (resolverFactories == null)
            {
                resolverFactories = new List<IResolverFactory>
                {
                    new VariableScopeResolverFactory(),
                    new BeansResolverFactory()
                };
            }
            if (scriptingEngines == null)
            {
                scriptingEngines = new ScriptingEngines();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitExpressionManager()
        {
            if (expressionManager == null)
            {
                expressionManager = new ExpressionManager(beans);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitBusinessCalendarManager()
        {
            if (businessCalendarManager == null)
            {
                MapBusinessCalendarManager mapBusinessCalendarManager = new MapBusinessCalendarManager();
                mapBusinessCalendarManager.AddBusinessCalendar(DurationBusinessCalendar.NAME, new DurationBusinessCalendar(this.clock));
                mapBusinessCalendarManager.AddBusinessCalendar(DueDateBusinessCalendar.NAME, new DueDateBusinessCalendar(this.clock));
                mapBusinessCalendarManager.AddBusinessCalendar(CycleBusinessCalendar.NAME, new CycleBusinessCalendar(this.clock));

                businessCalendarManager = mapBusinessCalendarManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDelegateInterceptor()
        {
            if (delegateInterceptor == null)
            {
                delegateInterceptor = new DefaultDelegateInterceptor();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitEventHandlers()
        {
            if (eventHandlers == null)
            {
                eventHandlers = new Dictionary<string, IEventHandler>();

                SignalEventHandler signalEventHandler = new SignalEventHandler();
                eventHandlers[signalEventHandler.EventHandlerType] = signalEventHandler;

                CompensationEventHandler compensationEventHandler = new CompensationEventHandler();
                eventHandlers[compensationEventHandler.EventHandlerType] = compensationEventHandler;

                MessageEventHandler messageEventHandler = new MessageEventHandler();
                eventHandlers[messageEventHandler.EventHandlerType] = messageEventHandler;

            }
            if (customEventHandlers != null)
            {
                foreach (IEventHandler eventHandler in customEventHandlers)
                {
                    eventHandlers[eventHandler.EventHandlerType] = eventHandler;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitBeans()
        {
            if (beans == null)
            {
                beans = new Dictionary<object, object>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitEventDispatcher()
        {
            if (this.eventDispatcher == null)
            {
                this.eventDispatcher = new ActivitiEventDispatcherImpl();
            }

            this.eventDispatcher.Enabled = enableEventDispatcher;

            if (eventListeners != null)
            {
                foreach (IActivitiEventListener listenerToAdd in eventListeners)
                {
                    this.eventDispatcher.AddEventListener(listenerToAdd);
                }
            }

            if (typedEventListeners != null)
            {
                foreach (KeyValuePair<string, IList<IActivitiEventListener>> listenersToAdd in typedEventListeners.SetOfKeyValuePairs())
                {
                    // Extract types from the given string
                    ActivitiEventType[] types = ActivitiEventType.GetTypesFromString(listenersToAdd.Key);

                    foreach (IActivitiEventListener listenerToAdd in listenersToAdd.Value)
                    {
                        this.eventDispatcher.AddEventListener(listenerToAdd, types);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitProcessValidator()
        {
            if (this.processValidator == null)
            {
                this.processValidator = ProcessEngineServiceProvider.Resolve<IProcessValidator>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitDatabaseEventLogging()
        {
            if (enableDatabaseEventLogging)
            {
                // Database event logging uses the default logging mechanism and adds
                // a specific event listener to the list of event listeners
                EventDispatcher.AddEventListener(new EventLogger(clock, objectMapper));
            }
        }

        /// <summary>
        /// Called when the <seealso cref="IProcessEngine"/> is initialized, but before it is returned
        /// </summary>
        protected internal virtual void PostProcessEngineInitialisation()
        {
            if (performanceSettings.ValidateExecutionRelationshipCountConfigOnBoot)
            {
                commandExecutor.Execute(new ValidateExecutionRelatedEntityCountCfgCmd());
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual CommandConfig DefaultCommandConfig
        {
            get
            {
                return defaultCommandConfig;
            }
            set
            {
                this.defaultCommandConfig = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual CommandConfig SchemaCommandConfig
        {
            get
            {
                return schemaCommandConfig;
            }
            set
            {
                this.schemaCommandConfig = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual ICommandInterceptor CommandInvoker
        {
            get
            {
                return commandInvoker;
            }
            set
            {
                this.commandInvoker = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ICommandInterceptor> CustomPreCommandInterceptors
        {
            get
            {
                return customPreCommandInterceptors;
            }
            set
            {
                this.customPreCommandInterceptors = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ICommandInterceptor> CustomPostCommandInterceptors
        {
            get
            {
                return customPostCommandInterceptors;
            }
            set
            {
                this.customPostCommandInterceptors = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ICommandInterceptor> CommandInterceptors
        {
            get
            {
                return commandInterceptors;
            }
            set
            {
                this.commandInterceptors = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual interceptor.ICommandExecutor CommandExecutor
        {
            get
            {
                return commandExecutor;
            }
            set
            {
                this.commandExecutor = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IRepositoryService RepositoryService
        {
            get
            {
                return repositoryService;
            }
            protected set
            {
                this.repositoryService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IRuntimeService RuntimeService
        {
            get
            {
                return runtimeService;
            }
            protected set
            {
                this.runtimeService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IHistoryService HistoryService
        {
            get
            {
                return historyService;
            }
            protected set
            {
                this.historyService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override ITaskService TaskService
        {
            get
            {
                return taskService;
            }
            protected set
            {
                this.taskService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IManagementService ManagementService
        {
            get
            {
                return managementService;
            }
            protected set
            {
                this.managementService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDynamicBpmnService DynamicBpmnService
        {
            get
            {
                return dynamicBpmnService;
            }
            set
            {
                this.dynamicBpmnService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IUserGroupLookupProxy UserGroupLookupProxy
        {
            get
            {
                return userGroupLookupProxy;
            }
            set
            {
                this.userGroupLookupProxy = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override IIntegrationContextManager IntegrationContextManager
        {
            get
            {
                if (integrationContextManager == null)
                {
                    integrationContextManager = new IntegrationContextManagerImpl(this, IntegrationContextDataManager);
                }
                return integrationContextManager;
            }
        }

        private IIntegrationContextDataManager IntegrationContextDataManager
        {
            get
            {
                if (integrationContextDataManager == null)
                {
                    integrationContextDataManager = new MybatisIntegrationContextDataManager(this);
                }
                return integrationContextDataManager;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IIntegrationContextService IntegrationContextService
        {
            get
            {
                if (integrationContextService == null)
                {
                    integrationContextService = new IntegrationContextServiceImpl(commandExecutor);
                }
                return integrationContextService;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ConcurrentDictionary<Type, ISessionFactory> SessionFactories
        {
            get
            {
                return sessionFactories;
            }
            set
            {
                this.sessionFactories = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IProcessEngineConfigurator> Configurators
        {
            get
            {
                return configurators;
            }
            set
            {
                this.configurators = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl AddConfigurator(IProcessEngineConfigurator configurator)
        {
            if (this.configurators == null)
            {
                this.configurators = new List<IProcessEngineConfigurator>();
            }
            this.configurators.Add(configurator);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableConfiguratorServiceLoader
        {
            set
            {
                this.enableConfiguratorServiceLoader = value;
            }
            get
            {
                return enableConfiguratorServiceLoader;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IProcessEngineConfigurator> AllConfigurators
        {
            get
            {
                return allConfigurators;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual BpmnDeployer BpmnDeployer
        {
            get
            {
                return bpmnDeployer;
            }
            set
            {
                this.bpmnDeployer = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual BpmnParser BpmnParser
        {
            get
            {
                return bpmnParser;
            }
            set
            {
                this.bpmnParser = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ParsedDeploymentBuilderFactory ParsedDeploymentBuilderFactory
        {
            get
            {
                return parsedDeploymentBuilderFactory;
            }
            set
            {
                this.parsedDeploymentBuilderFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TimerManager TimerManager
        {
            get
            {
                return timerManager;
            }
            set
            {
                this.timerManager = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual EventSubscriptionManager EventSubscriptionManager
        {
            get
            {
                return eventSubscriptionManager;
            }
            set
            {
                this.eventSubscriptionManager = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual BpmnDeploymentHelper BpmnDeploymentHelper
        {
            get
            {
                return bpmnDeploymentHelper;
            }
            set
            {
                this.bpmnDeploymentHelper = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual CachingAndArtifactsManager CachingAndArtifactsManager
        {
            get
            {
                return cachingAndArtifactsManager;
            }
            set
            {
                this.cachingAndArtifactsManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IDeployer> Deployers
        {
            get
            {
                return deployers;
            }
            set
            {
                this.deployers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IIdGenerator IdGenerator
        {
            get
            {
                return idGenerator;
            }
            set
            {
                this.idGenerator = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string WsSyncFactoryClassName
        {
            get
            {
                return wsSyncFactoryClassName;
            }
            set
            {
                this.wsSyncFactoryClassName = value;
            }
        }

        /// <summary>
        /// Add or replace the address of the given web-service endpoint with the given value </summary>
        /// <param name="endpointName"> The endpoint name for which a new address must be set </param>
        /// <param name="address"> The new address of the endpoint </param>
        public virtual ProcessEngineConfiguration AddWsEndpointAddress(string endpointName, Uri address)
        {
            this.wsOverridenEndpointAddresses.AddOrUpdate(endpointName, address, (key, old) => address);

            return this;
        }

        /// <summary>
        /// Remove the address definition of the given web-service endpoint </summary>
        /// <param name="endpointName"> The endpoint name for which the address definition must be removed </param>
        public virtual ProcessEngineConfiguration RemoveWsEndpointAddress(string endpointName)
        {
            if (this.wsOverridenEndpointAddresses.TryRemove(endpointName, out _))
            {
                return this;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ConcurrentDictionary<string, Uri> WsOverridenEndpointAddresses
        {
            get
            {
                return this.wsOverridenEndpointAddresses;
            }
            set
            {
                this.wsOverridenEndpointAddresses.PutAll((IDictionary<string, Uri>)value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ScriptingEngines ScriptingEngines
        {
            get
            {
                return scriptingEngines;
            }
            set
            {
                this.scriptingEngines = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IVariableTypes VariableTypes
        {
            get
            {
                return variableTypes;
            }
            set
            {
                this.variableTypes = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool SerializableVariableTypeTrackDeserializedObjects
        {
            get
            {
                return serializableVariableTypeTrackDeserializedObjects;
            }
            set
            {
                this.serializableVariableTypeTrackDeserializedObjects = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ExpressionManager ExpressionManager
        {
            get
            {
                return expressionManager;
            }
            set
            {
                this.expressionManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IBusinessCalendarManager BusinessCalendarManager
        {
            get
            {
                return businessCalendarManager;
            }
            set
            {
                this.businessCalendarManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int ExecutionQueryLimit
        {
            get
            {
                return executionQueryLimit;
            }
            set
            {
                this.executionQueryLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int TaskQueryLimit
        {
            get
            {
                return taskQueryLimit;
            }
            set
            {
                this.taskQueryLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int HistoricTaskQueryLimit
        {
            get
            {
                return historicTaskQueryLimit;
            }
            set
            {
                this.historicTaskQueryLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int HistoricProcessInstancesQueryLimit
        {
            get
            {
                return historicProcessInstancesQueryLimit;
            }

            set
            {
                this.historicProcessInstancesQueryLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual CommandContextFactory CommandContextFactory
        {
            get
            {
                return commandContextFactory;
            }
            set
            {
                this.commandContextFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITransactionContextFactory TransactionContextFactory
        {
            get
            {
                return transactionContextFactory;
            }
            set
            {
                this.transactionContextFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IDeployer> CustomPreDeployers
        {
            get
            {
                return customPreDeployers;
            }
            set
            {
                this.customPreDeployers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IDeployer> CustomPostDeployers
        {
            get
            {
                return customPostDeployers;
            }
            set
            {
                this.customPostDeployers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, IJobHandler> JobHandlers
        {
            get
            {
                return jobHandlers;
            }
            set
            {
                this.jobHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstanceHelper ProcessInstanceHelper
        {
            get
            {
                return processInstanceHelper;
            }
            set
            {
                this.processInstanceHelper = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ListenerNotificationHelper ListenerNotificationHelper
        {
            get
            {
                return listenerNotificationHelper;
            }
            set
            {
                this.listenerNotificationHelper = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSqlSessionFactory DbSqlSessionFactory
        {
            get
            {
                return dbSqlSessionFactory;
            }
            set
            {
                this.dbSqlSessionFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITransactionFactory TransactionFactory
        {
            get
            {
                return transactionFactory;
            }
            set
            {
                this.transactionFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ISessionFactory> CustomSessionFactories
        {
            get
            {
                return customSessionFactories;
            }
            set
            {
                this.customSessionFactories = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IJobHandler> CustomJobHandlers
        {
            get
            {
                return customJobHandlers;
            }
            set
            {
                this.customJobHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<string> CustomScriptingEngineClasses
        {
            get
            {
                return customScriptingEngineClasses;
            }
            set
            {
                this.customScriptingEngineClasses = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IVariableType> CustomPreVariableTypes
        {
            get
            {
                return customPreVariableTypes;
            }
            set
            {
                this.customPreVariableTypes = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IVariableType> CustomPostVariableTypes
        {
            get
            {
                return customPostVariableTypes;
            }
            set
            {
                this.customPostVariableTypes = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IBpmnParseHandler> PreBpmnParseHandlers
        {
            get
            {
                return preBpmnParseHandlers;
            }
            set
            {
                this.preBpmnParseHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IBpmnParseHandler> CustomDefaultBpmnParseHandlers
        {
            get
            {
                return customDefaultBpmnParseHandlers;
            }
            set
            {
                this.customDefaultBpmnParseHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IBpmnParseHandler> PostBpmnParseHandlers
        {
            get
            {
                return postBpmnParseHandlers;
            }
            set
            {
                this.postBpmnParseHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IActivityBehaviorFactory ActivityBehaviorFactory
        {
            get
            {
                return activityBehaviorFactory;
            }
            set
            {
                this.activityBehaviorFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IListenerFactory ListenerFactory
        {
            get
            {
                return listenerFactory;
            }
            set
            {
                this.listenerFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IBpmnParseFactory BpmnParseFactory
        {
            get
            {
                return bpmnParseFactory;
            }
            set
            {
                this.bpmnParseFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<object, object> Beans
        {
            get
            {
                return beans;
            }
            set
            {
                this.beans = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IResolverFactory> ResolverFactories
        {
            get
            {
                return resolverFactories;
            }
            set
            {
                this.resolverFactories = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DeploymentManager DeploymentManager
        {
            get
            {
                return deploymentManager;
            }
            set
            {
                this.deploymentManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDelegateInterceptor DelegateInterceptor
        {
            get
            {
                return delegateInterceptor;
            }
            set
            {
                this.delegateInterceptor = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IEventHandler GetEventHandler(string eventType)
        {
            return eventHandlers[eventType];
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, IEventHandler> EventHandlers
        {
            get
            {
                return eventHandlers;
            }
            set
            {
                this.eventHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IEventHandler> CustomEventHandlers
        {
            get
            {
                return customEventHandlers;
            }
            set
            {
                this.customEventHandlers = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IFailedJobCommandFactory FailedJobCommandFactory
        {
            get
            {
                return failedJobCommandFactory;
            }
            set
            {
                this.failedJobCommandFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDataSource IdGeneratorDataSource
        {
            get
            {
                return idGeneratorDataSource;
            }
            set
            {
                this.idGeneratorDataSource = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int BatchSizeProcessInstances
        {
            get
            {
                return batchSizeProcessInstances;
            }
            set
            {
                this.batchSizeProcessInstances = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int BatchSizeTasks
        {
            get
            {
                return batchSizeTasks;
            }
            set
            {
                this.batchSizeTasks = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int ProcessDefinitionCacheLimit
        {
            get
            {
                return processDefinitionCacheLimit;
            }
            set
            {
                this.processDefinitionCacheLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeploymentCache<ProcessDefinitionCacheEntry> ProcessDefinitionCache
        {
            get
            {
                return processDefinitionCache;
            }
            set
            {
                this.processDefinitionCache = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int KnowledgeBaseCacheLimit
        {
            get
            {
                return knowledgeBaseCacheLimit;
            }
            set
            {
                this.knowledgeBaseCacheLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeploymentCache<object> KnowledgeBaseCache
        {
            get
            {
                return knowledgeBaseCache;
            }
            set
            {
                this.knowledgeBaseCache = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableSafeBpmnXml
        {
            get
            {
                return enableSafeBpmnXml;
            }
            set
            {
                this.enableSafeBpmnXml = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IActivitiEventDispatcher EventDispatcher
        {
            get
            {
                return eventDispatcher;
            }
            set
            {
                this.eventDispatcher = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl SetEnableEventDispatcher(bool enableEventDispatcher)
        {
            this.enableEventDispatcher = enableEventDispatcher;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, IList<IActivitiEventListener>> TypedEventListeners
        {
            get
            {
                return typedEventListeners;
            }
            set
            {
                this.typedEventListeners = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IActivitiEventListener> EventListeners
        {
            get
            {
                return eventListeners;
            }
            set
            {
                this.eventListeners = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessValidator ProcessValidator
        {
            get
            {
                return processValidator;
            }
            set
            {
                this.processValidator = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableEventDispatcher
        {
            get
            {
                return enableEventDispatcher;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableDatabaseEventLogging
        {
            get
            {
                return enableDatabaseEventLogging;
            }
            set
            {
                this.enableDatabaseEventLogging = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxLengthStringVariableType
        {
            get
            {
                return maxLengthStringVariableType;
            }
            set
            {
                this.maxLengthStringVariableType = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool BulkInsertEnabled
        {
            get
            {
                return isBulkInsertEnabled;
            }
            set
            {
                this.isBulkInsertEnabled = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool UsingRelationalDatabase
        {
            get
            {
                return usingRelationalDatabase;
            }
            set
            {
                this.usingRelationalDatabase = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableEagerExecutionTreeFetching
        {
            get
            {
                return this.performanceSettings.EnableEagerExecutionTreeFetching;
            }
            set
            {
                this.performanceSettings.EnableEagerExecutionTreeFetching = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableExecutionRelationshipCounts
        {
            get
            {
                return this.performanceSettings.EnableExecutionRelationshipCounts;
            }
            set
            {
                this.performanceSettings.EnableExecutionRelationshipCounts = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableLocalization
        {
            get
            {
                return this.performanceSettings.EnableLocalization;
            }
            set
            {
                this.performanceSettings.EnableLocalization = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxNrOfStatementsInBulkInsert
        {
            get
            {
                return maxNrOfStatementsInBulkInsert;
            }
            set
            {
                this.maxNrOfStatementsInBulkInsert = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableVerboseExecutionTreeLogging
        {
            get
            {
                return enableVerboseExecutionTreeLogging;
            }
            set
            {
                this.enableVerboseExecutionTreeLogging = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual PerformanceSettings PerformanceSettings
        {
            get
            {
                return performanceSettings;
            }
            set
            {
                this.performanceSettings = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IAttachmentDataManager AttachmentDataManager
        {
            get
            {
                return attachmentDataManager;
            }
            set
            {
                this.attachmentDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IByteArrayDataManager ByteArrayDataManager
        {
            get
            {
                return byteArrayDataManager;
            }
            set
            {
                this.byteArrayDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICommentDataManager CommentDataManager
        {
            get
            {
                return commentDataManager;
            }
            set
            {
                this.commentDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeploymentDataManager DeploymentDataManager
        {
            get
            {
                return deploymentDataManager;
            }
            set
            {
                this.deploymentDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IEventLogEntryDataManager EventLogEntryDataManager
        {
            get
            {
                return eventLogEntryDataManager;
            }
            set
            {
                this.eventLogEntryDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IEventSubscriptionDataManager EventSubscriptionDataManager
        {
            get
            {
                return eventSubscriptionDataManager;
            }
            set
            {
                this.eventSubscriptionDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionDataManager ExecutionDataManager
        {
            get
            {
                return executionDataManager;
            }
            set
            {
                this.executionDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricActivityInstanceDataManager HistoricActivityInstanceDataManager
        {
            get
            {
                return historicActivityInstanceDataManager;
            }
            set
            {
                this.historicActivityInstanceDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricDetailDataManager HistoricDetailDataManager
        {
            get
            {
                return historicDetailDataManager;
            }
            set
            {
                this.historicDetailDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricIdentityLinkDataManager HistoricIdentityLinkDataManager
        {
            get
            {
                return historicIdentityLinkDataManager;
            }
            set
            {
                this.historicIdentityLinkDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricProcessInstanceDataManager HistoricProcessInstanceDataManager
        {
            get
            {
                return historicProcessInstanceDataManager;
            }
            set
            {
                this.historicProcessInstanceDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricTaskInstanceDataManager HistoricTaskInstanceDataManager
        {
            get
            {
                return historicTaskInstanceDataManager;
            }
            set
            {
                this.historicTaskInstanceDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricVariableInstanceDataManager HistoricVariableInstanceDataManager
        {
            get
            {
                return historicVariableInstanceDataManager;
            }
            set
            {
                this.historicVariableInstanceDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IIdentityLinkDataManager IdentityLinkDataManager
        {
            get
            {
                return identityLinkDataManager;
            }
            set
            {
                this.identityLinkDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IJobDataManager JobDataManager
        {
            get
            {
                return jobDataManager;
            }
            set
            {
                this.jobDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITimerJobDataManager TimerJobDataManager
        {
            get
            {
                return timerJobDataManager;
            }
            set
            {
                this.timerJobDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ISuspendedJobDataManager SuspendedJobDataManager
        {
            get
            {
                return suspendedJobDataManager;
            }
            set
            {
                this.suspendedJobDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeadLetterJobDataManager DeadLetterJobDataManager
        {
            get
            {
                return deadLetterJobDataManager;
            }
            set
            {
                this.deadLetterJobDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IModelDataManager ModelDataManager
        {
            get
            {
                return modelDataManager;
            }
            set
            {
                this.modelDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessDefinitionDataManager ProcessDefinitionDataManager
        {
            get
            {
                return processDefinitionDataManager;
            }
            set
            {
                this.processDefinitionDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessDefinitionInfoDataManager ProcessDefinitionInfoDataManager
        {
            get
            {
                return processDefinitionInfoDataManager;
            }
            set
            {
                this.processDefinitionInfoDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPropertyDataManager PropertyDataManager
        {
            get
            {
                return propertyDataManager;
            }
            set
            {
                this.propertyDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IResourceDataManager ResourceDataManager
        {
            get
            {
                return resourceDataManager;
            }
            set
            {
                this.resourceDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITaskDataManager TaskDataManager
        {
            get
            {
                return taskDataManager;
            }
            set
            {
                this.taskDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IVariableInstanceDataManager VariableInstanceDataManager
        {
            get
            {
                return variableInstanceDataManager;
            }
            set
            {
                this.variableInstanceDataManager = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IAttachmentEntityManager AttachmentEntityManager
        {
            get
            {
                return attachmentEntityManager;
            }
            set
            {
                this.attachmentEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IByteArrayEntityManager ByteArrayEntityManager
        {
            get
            {
                return byteArrayEntityManager;
            }
            set
            {
                this.byteArrayEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICommentEntityManager CommentEntityManager
        {
            get
            {
                return commentEntityManager;
            }
            set
            {
                this.commentEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeploymentEntityManager DeploymentEntityManager
        {
            get
            {
                return deploymentEntityManager;
            }
            set
            {
                this.deploymentEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IEventLogEntryEntityManager EventLogEntryEntityManager
        {
            get
            {
                return eventLogEntryEntityManager;
            }
            set
            {
                this.eventLogEntryEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IEventSubscriptionEntityManager EventSubscriptionEntityManager
        {
            get
            {
                return eventSubscriptionEntityManager;
            }
            set
            {
                this.eventSubscriptionEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntityManager ExecutionEntityManager
        {
            get
            {
                return executionEntityManager;
            }
            set
            {
                this.executionEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricActivityInstanceEntityManager HistoricActivityInstanceEntityManager
        {
            get
            {
                return historicActivityInstanceEntityManager;
            }
            set
            {
                this.historicActivityInstanceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricDetailEntityManager HistoricDetailEntityManager
        {
            get
            {
                return historicDetailEntityManager;
            }
            set
            {
                this.historicDetailEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricIdentityLinkEntityManager HistoricIdentityLinkEntityManager
        {
            get
            {
                return historicIdentityLinkEntityManager;
            }
            set
            {
                this.historicIdentityLinkEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricProcessInstanceEntityManager HistoricProcessInstanceEntityManager
        {
            get
            {
                return historicProcessInstanceEntityManager;
            }
            set
            {
                this.historicProcessInstanceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricTaskInstanceEntityManager HistoricTaskInstanceEntityManager
        {
            get
            {
                return historicTaskInstanceEntityManager;
            }
            set
            {
                this.historicTaskInstanceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoricVariableInstanceEntityManager HistoricVariableInstanceEntityManager
        {
            get
            {
                return historicVariableInstanceEntityManager;
            }
            set
            {
                this.historicVariableInstanceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IIdentityLinkEntityManager IdentityLinkEntityManager
        {
            get
            {
                return identityLinkEntityManager;
            }
            set
            {
                this.identityLinkEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IJobEntityManager JobEntityManager
        {
            get
            {
                return jobEntityManager;
            }
            set
            {
                this.jobEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITimerJobEntityManager TimerJobEntityManager
        {
            get
            {
                return timerJobEntityManager;
            }
            set
            {

                this.timerJobEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ISuspendedJobEntityManager SuspendedJobEntityManager
        {
            get
            {
                return suspendedJobEntityManager;
            }
            set
            {

                this.suspendedJobEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeadLetterJobEntityManager DeadLetterJobEntityManager
        {
            get
            {
                return deadLetterJobEntityManager;
            }
            set
            {
                this.deadLetterJobEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IModelEntityManager ModelEntityManager
        {
            get
            {
                return modelEntityManager;
            }
            set
            {
                this.modelEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessDefinitionEntityManager ProcessDefinitionEntityManager
        {
            get
            {
                return processDefinitionEntityManager;
            }
            set
            {
                this.processDefinitionEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessDefinitionInfoEntityManager ProcessDefinitionInfoEntityManager
        {
            get
            {
                return processDefinitionInfoEntityManager;
            }
            set
            {
                this.processDefinitionInfoEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPropertyEntityManager PropertyEntityManager
        {
            get
            {
                return propertyEntityManager;
            }
            set
            {
                this.propertyEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IResourceEntityManager ResourceEntityManager
        {
            get
            {
                return resourceEntityManager;
            }
            set
            {
                this.resourceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITaskEntityManager TaskEntityManager
        {
            get
            {
                return taskEntityManager;
            }
            set
            {
                this.taskEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IVariableInstanceEntityManager VariableInstanceEntityManager
        {
            get
            {
                return variableInstanceEntityManager;
            }
            set
            {
                this.variableInstanceEntityManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ITableDataManager TableDataManager
        {
            get
            {
                return tableDataManager;
            }
            set
            {
                this.tableDataManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IHistoryManager HistoryManager
        {
            get
            {
                return historyManager;
            }
            set
            {
                this.historyManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IJobManager JobManager
        {
            get
            {
                return jobManager;
            }
            set
            {
                this.jobManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IClock Clock
        {
            get
            {
                return this.clock;
            }
            set
            {
                if (this.clock == null)
                {
                    this.clock = value;
                }
                else
                {
                    this.clock.CurrentCalendar = clock.CurrentCalendar;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ResetClock()
        {
            if (this.clock != null)
            {
                clock.Reset();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DelegateExpressionFieldInjectionMode DelegateExpressionFieldInjectionMode
        {
            get
            {
                return delegateExpressionFieldInjectionMode;
            }
            set
            {
                this.delegateExpressionFieldInjectionMode = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ObjectMapper ObjectMapper
        {
            get
            {
                return objectMapper;
            }
            set
            {
                this.objectMapper = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorCorePoolSize
        {
            get
            {
                return asyncExecutorCorePoolSize;
            }
            set
            {
                this.asyncExecutorCorePoolSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorNumberOfRetries
        {
            get
            {
                return asyncExecutorNumberOfRetries;
            }
            set
            {
                this.asyncExecutorNumberOfRetries = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorMaxPoolSize
        {
            get
            {
                return asyncExecutorMaxPoolSize;
            }
            set
            {
                this.asyncExecutorMaxPoolSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long AsyncExecutorThreadKeepAliveTime
        {
            get
            {
                return asyncExecutorThreadKeepAliveTime;
            }
            set
            {
                this.asyncExecutorThreadKeepAliveTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorThreadPoolQueueSize
        {
            get
            {
                return asyncExecutorThreadPoolQueueSize;
            }
            set
            {
                this.asyncExecutorThreadPoolQueueSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ConcurrentQueue<ThreadStart> AsyncExecutorThreadPoolQueue
        {
            get
            {
                return asyncExecutorThreadPoolQueue;
            }
            set
            {
                this.asyncExecutorThreadPoolQueue = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long AsyncExecutorSecondsToWaitOnShutdown
        {
            get
            {
                return asyncExecutorSecondsToWaitOnShutdown;
            }
            set
            {
                this.asyncExecutorSecondsToWaitOnShutdown = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorMaxTimerJobsPerAcquisition
        {
            get
            {
                return asyncExecutorMaxTimerJobsPerAcquisition;
            }
            set
            {
                this.asyncExecutorMaxTimerJobsPerAcquisition = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorMaxAsyncJobsDuePerAcquisition
        {
            get
            {
                return asyncExecutorMaxAsyncJobsDuePerAcquisition;
            }
            set
            {
                this.asyncExecutorMaxAsyncJobsDuePerAcquisition = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorDefaultTimerJobAcquireWaitTime
        {
            get
            {
                return asyncExecutorDefaultTimerJobAcquireWaitTime;
            }
            set
            {
                this.asyncExecutorDefaultTimerJobAcquireWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorDefaultAsyncJobAcquireWaitTime
        {
            get
            {
                return asyncExecutorDefaultAsyncJobAcquireWaitTime;
            }
            set
            {
                this.asyncExecutorDefaultAsyncJobAcquireWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorDefaultQueueSizeFullWaitTime
        {
            get
            {
                return asyncExecutorDefaultQueueSizeFullWaitTime;
            }
            set
            {
                this.asyncExecutorDefaultQueueSizeFullWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string AsyncExecutorLockOwner
        {
            get
            {
                return asyncExecutorLockOwner;
            }
            set
            {
                this.asyncExecutorLockOwner = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorTimerLockTimeInMillis
        {
            get
            {
                return asyncExecutorTimerLockTimeInMillis;
            }
            set
            {
                this.asyncExecutorTimerLockTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorAsyncJobLockTimeInMillis
        {
            get
            {
                return asyncExecutorAsyncJobLockTimeInMillis;
            }
            set
            {
                this.asyncExecutorAsyncJobLockTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorResetExpiredJobsInterval
        {
            get
            {
                return asyncExecutorResetExpiredJobsInterval;
            }
            set
            {
                this.asyncExecutorResetExpiredJobsInterval = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecuteAsyncRunnableFactory AsyncExecutorExecuteAsyncRunnableFactory
        {
            get
            {
                return asyncExecutorExecuteAsyncRunnableFactory;
            }
            set
            {
                this.asyncExecutorExecuteAsyncRunnableFactory = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncExecutorResetExpiredJobsPageSize
        {
            get
            {
                return asyncExecutorResetExpiredJobsPageSize;
            }
            set
            {
                this.asyncExecutorResetExpiredJobsPageSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AsyncExecutorIsMessageQueueMode
        {
            get
            {
                return asyncExecutorMessageQueueMode;
            }
            set
            {
                this.asyncExecutorMessageQueueMode = value;
            }
        }
    }
}