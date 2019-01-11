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
namespace org.activiti.engine.impl.cfg
{
    using DatabaseSchemaReader;
    using javax.transaction;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.cfg;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.bpmn.data;
    using org.activiti.engine.impl.bpmn.deployer;
    using org.activiti.engine.impl.bpmn.listener;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.bpmn.parser.factory;
    using org.activiti.engine.impl.bpmn.parser.handler;
    using org.activiti.engine.impl.bpmn.webservice;
    using org.activiti.engine.impl.calendar;
    using org.activiti.engine.impl.cfg.standalone;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.@delegate.invocation;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.@event;
    using org.activiti.engine.impl.@event.logger;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence;
    using org.activiti.engine.impl.persistence.cache;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.impl.persistence.entity.data.impl;
    using org.activiti.engine.impl.persistence.entity.data.integration;
    using org.activiti.engine.impl.persistence.entity.integration;
    using org.activiti.engine.impl.scripting;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.impl.variable;
    using org.activiti.engine.integration;
    using org.activiti.engine.parse;
    using org.activiti.engine.runtime;
    using org.activiti.validation;
    using Sys;
    using Sys.Bpm;
    using Sys.Data;
    using System.IO;
    using System.Linq;

    public abstract class ProcessEngineConfigurationImpl : ProcessEngineConfiguration
    {
        private ILogger<ProcessEngineConfigurationImpl> log = ProcessEngineServiceProvider.LoggerService<ProcessEngineConfigurationImpl>();

        public ProcessEngineConfigurationImpl(IHistoryService historyService,
            ITaskService taskService,
            IDynamicBpmnService dynamicBpmnService,
            IRepositoryService repositoryService,
            IRuntimeService runtimeService,
            IManagementService managementService,
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
        }

        #region fields

        public const string DB_SCHEMA_UPDATE_CREATE = "create";
        public const string DB_SCHEMA_UPDATE_DROP_CREATE = "drop-create";

        public const string DEFAULT_WS_SYNC_FACTORY = "org.activiti.engine.impl.webservice.CxfWebServiceClientFactory";

        public const string DEFAULT_MYBATIS_MAPPING_FILE = "resources/db/mapping/mappings.xml";

        public const int DEFAULT_GENERIC_MAX_LENGTH_STRING = 4000;
        public const int DEFAULT_ORACLE_MAX_LENGTH_STRING = 2000;

        // SERVICES /////////////////////////////////////////////////////////////////

        protected internal IRepositoryService repositoryService;
        protected internal IRuntimeService runtimeService;
        protected internal IHistoryService historyService;
        protected internal ITaskService taskService;
        protected internal IManagementService managementService;
        protected internal IDynamicBpmnService dynamicBpmnService;

        protected internal IUserGroupLookupProxy userGroupLookupProxy;
        private IIntegrationContextService integrationContextService;

        // COMMAND EXECUTORS ////////////////////////////////////////////////////////

        protected internal CommandConfig defaultCommandConfig;
        protected internal CommandConfig schemaCommandConfig;

        protected internal ICommandInterceptor commandInvoker;

        /// <summary>
        /// the configurable list which will be <seealso cref="#initInterceptorChain(java.util.List) processed"/> to build the <seealso cref="#commandExecutor"/>
        /// </summary>
        protected internal IList<ICommandInterceptor> customPreCommandInterceptors;
        protected internal IList<ICommandInterceptor> customPostCommandInterceptors;

        protected internal IList<ICommandInterceptor> commandInterceptors;

        /// <summary>
        /// this will be initialized during the configurationComplete() </summary>
        protected internal interceptor.ICommandExecutor commandExecutor;

        // DATA MANAGERS /////////////////////////////////////////////////////////////

        protected internal IAttachmentDataManager attachmentDataManager;
        protected internal IByteArrayDataManager byteArrayDataManager;
        protected internal ICommentDataManager commentDataManager;
        protected internal IDeploymentDataManager deploymentDataManager;
        protected internal IEventLogEntryDataManager eventLogEntryDataManager;
        protected internal IEventSubscriptionDataManager eventSubscriptionDataManager;
        protected internal IExecutionDataManager executionDataManager;
        protected internal IHistoricActivityInstanceDataManager historicActivityInstanceDataManager;
        protected internal IHistoricDetailDataManager historicDetailDataManager;
        protected internal IHistoricIdentityLinkDataManager historicIdentityLinkDataManager;
        protected internal IHistoricProcessInstanceDataManager historicProcessInstanceDataManager;
        protected internal IHistoricTaskInstanceDataManager historicTaskInstanceDataManager;
        protected internal IHistoricVariableInstanceDataManager historicVariableInstanceDataManager;
        protected internal IdentityLinkDataManager identityLinkDataManager;
        protected internal IJobDataManager jobDataManager;
        protected internal ITimerJobDataManager timerJobDataManager;
        protected internal ISuspendedJobDataManager suspendedJobDataManager;
        protected internal IDeadLetterJobDataManager deadLetterJobDataManager;
        protected internal IModelDataManager modelDataManager;
        protected internal IProcessDefinitionDataManager processDefinitionDataManager;
        protected internal IProcessDefinitionInfoDataManager processDefinitionInfoDataManager;
        protected internal IPropertyDataManager propertyDataManager;
        protected internal IResourceDataManager resourceDataManager;
        protected internal ITaskDataManager taskDataManager;
        protected internal IVariableInstanceDataManager variableInstanceDataManager;
        private IIntegrationContextDataManager integrationContextDataManager;


        // ENTITY MANAGERS ///////////////////////////////////////////////////////////

        protected internal IAttachmentEntityManager attachmentEntityManager;
        protected internal IByteArrayEntityManager byteArrayEntityManager;
        protected internal ICommentEntityManager commentEntityManager;
        protected internal IDeploymentEntityManager deploymentEntityManager;
        protected internal IEventLogEntryEntityManager eventLogEntryEntityManager;
        protected internal IEventSubscriptionEntityManager eventSubscriptionEntityManager;
        protected internal IExecutionEntityManager executionEntityManager;
        protected internal IHistoricActivityInstanceEntityManager historicActivityInstanceEntityManager;
        protected internal IHistoricDetailEntityManager historicDetailEntityManager;
        protected internal IHistoricIdentityLinkEntityManager historicIdentityLinkEntityManager;
        protected internal IHistoricProcessInstanceEntityManager historicProcessInstanceEntityManager;
        protected internal IHistoricTaskInstanceEntityManager historicTaskInstanceEntityManager;
        protected internal IHistoricVariableInstanceEntityManager historicVariableInstanceEntityManager;
        protected internal IdentityLinkEntityManager identityLinkEntityManager;
        protected internal IJobEntityManager jobEntityManager;
        protected internal ITimerJobEntityManager timerJobEntityManager;
        protected internal ISuspendedJobEntityManager suspendedJobEntityManager;
        protected internal IDeadLetterJobEntityManager deadLetterJobEntityManager;
        protected internal IModelEntityManager modelEntityManager;
        protected internal IProcessDefinitionEntityManager processDefinitionEntityManager;
        protected internal IProcessDefinitionInfoEntityManager processDefinitionInfoEntityManager;
        protected internal IPropertyEntityManager propertyEntityManager;
        protected internal IResourceEntityManager resourceEntityManager;
        protected internal ITableDataManager tableDataManager;
        protected internal ITaskEntityManager taskEntityManager;
        protected internal IVariableInstanceEntityManager variableInstanceEntityManager;
        private IIntegrationContextManager integrationContextManager;

        // History Manager

        protected internal IHistoryManager historyManager;

        // Job Manager

        protected internal IJobManager jobManager;

        // SESSION FACTORIES /////////////////////////////////////////////////////////

        protected internal IList<ISessionFactory> customSessionFactories;
        protected internal DbSqlSessionFactory dbSqlSessionFactory;
        protected internal IDictionary<Type, ISessionFactory> sessionFactories;

        // CONFIGURATORS ////////////////////////////////////////////////////////////

        protected internal bool enableConfiguratorServiceLoader = false; // Enabled by default. In certain environments this should be set to false (eg osgi)
        protected internal IList<IProcessEngineConfigurator> configurators; // The injected configurators
        protected internal IList<IProcessEngineConfigurator> allConfigurators; // Including auto-discovered configurators

        // DEPLOYERS //////////////////////////////////////////////////////////////////

        protected internal BpmnDeployer bpmnDeployer;
        protected internal BpmnParser bpmnParser;
        protected internal ParsedDeploymentBuilderFactory parsedDeploymentBuilderFactory;
        protected internal TimerManager timerManager;
        protected internal EventSubscriptionManager eventSubscriptionManager;
        protected internal BpmnDeploymentHelper bpmnDeploymentHelper;
        protected internal CachingAndArtifactsManager cachingAndArtifactsManager;
        protected internal IList<IDeployer> customPreDeployers;
        protected internal IList<IDeployer> customPostDeployers;
        protected internal IList<IDeployer> deployers;
        protected internal DeploymentManager deploymentManager;

        protected internal int processDefinitionCacheLimit = -1; // By default, no limit
        protected internal IDeploymentCache<ProcessDefinitionCacheEntry> processDefinitionCache;

        protected internal int processDefinitionInfoCacheLimit = -1; // By default, no limit
        protected internal ProcessDefinitionInfoCache processDefinitionInfoCache;

        protected internal int knowledgeBaseCacheLimit = -1;
        protected internal IDeploymentCache<object> knowledgeBaseCache;

        // JOB EXECUTOR /////////////////////////////////////////////////////////////

        protected internal IList<IJobHandler> customJobHandlers;
        protected internal IDictionary<string, IJobHandler> jobHandlers;

        // HELPERS //////////////////////////////////////////////////////////////////
        protected internal ProcessInstanceHelper processInstanceHelper;
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
        /// By default null. If null, an <seealso cref="ArrayBlockingQueue"/> will be created of
        /// size <seealso cref="#asyncExecutorThreadPoolQueueSize"/>.
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
        /// <seealso cref="#asyncExecutorMaxTimerJobsPerAcquisition"/>. Default value = 10
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
        /// <seealso cref="#asyncExecutorMaxAsyncJobsDuePerAcquisition"/>. Default value = 10
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
        /// Allows to define a custom factory for creating the <seealso cref="Runnable"/> that is executed by the async executor.
        /// 
        /// (This property is only applicable when using the <seealso cref="DefaultAsyncJobExecutor"/>).
        /// </summary>
        protected internal IExecuteAsyncRunnableFactory asyncExecutorExecuteAsyncRunnableFactory;

        // MYBATIS SQL SESSION FACTORY //////////////////////////////////////////////

        //protected internal DefaultSqlSessionFactory sqlSessionFactory;
        protected internal ITransactionFactory transactionFactory;

        protected internal ISet<Type> customMybatisMappers;
        protected internal ISet<string> customMybatisXMLMappers;

        // ID GENERATOR ///////////////////////////////////////////////////////////////

        protected internal IIdGenerator idGenerator;
        protected internal IDataSource idGeneratorDataSource;

        // BPMN PARSER //////////////////////////////////////////////////////////////

        protected internal IList<IBpmnParseHandler> preBpmnParseHandlers;
        protected internal IList<IBpmnParseHandler> postBpmnParseHandlers;
        protected internal IList<IBpmnParseHandler> customDefaultBpmnParseHandlers;
        protected internal IActivityBehaviorFactory activityBehaviorFactory;
        protected internal IListenerFactory listenerFactory;
        protected internal IBpmnParseFactory bpmnParseFactory;

        // PROCESS VALIDATION //////////////////////////////////////////////////////////////

        protected internal IProcessValidator processValidator;

        // OTHER //////////////////////////////////////////////////////////////////////

        protected internal IList<IVariableType> customPreVariableTypes;
        protected internal IList<IVariableType> customPostVariableTypes;
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

        protected internal ExpressionManager expressionManager;
        protected internal IList<string> customScriptingEngineClasses;
        //protected internal ScriptingEngines scriptingEngines;
        protected internal IList<IResolverFactory> resolverFactories;

        protected internal IBusinessCalendarManager businessCalendarManager;

        protected internal int executionQueryLimit = 20000;
        protected internal int taskQueryLimit = 20000;
        protected internal int historicTaskQueryLimit = 20000;
        protected internal int historicProcessInstancesQueryLimit = 20000;

        protected internal string wsSyncFactoryClassName = DEFAULT_WS_SYNC_FACTORY;
        protected internal ConcurrentDictionary<string, Uri> wsOverridenEndpointAddresses = new ConcurrentDictionary<string, Uri>();

        protected internal CommandContextFactory commandContextFactory;
        protected internal ITransactionContextFactory transactionContextFactory;

        protected internal IDictionary<object, object> beans;

        protected internal IDelegateInterceptor delegateInterceptor;

        protected internal IDictionary<string, IEventHandler> eventHandlers;
        protected internal IList<IEventHandler> customEventHandlers;

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
        protected internal int batchSizeTasks = 25;

        protected internal bool enableEventDispatcher = true;
        protected internal IActivitiEventDispatcher eventDispatcher;
        protected internal IList<IActivitiEventListener> eventListeners;
        protected internal IDictionary<string, IList<IActivitiEventListener>> typedEventListeners;

        // Event logging to database
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

        public int DEFAULT_MAX_NR_OF_STATEMENTS_BULK_INSERT_SQL_SERVER = 70; // currently Execution has most params (28). 2000 / 28 = 71.

        protected internal ObjectMapper objectMapper = new ObjectMapper();

        /// <summary>
        /// Flag that can be set to configure or nota relational database is used.
        /// This is useful for custom implementations that do not use relational databases at all.
        /// 
        /// If true (default), the <seealso cref="ProcessEngineConfiguration#getDatabaseSchemaUpdate()"/> value will be used to determine
        /// what needs to happen wrt the database schema.
        /// 
        /// If false, no validation or schema creation will be done. That means that the database schema must have been
        /// created 'manually' before but the engine does not validate whether the schema is correct.
        /// The <seealso cref="ProcessEngineConfiguration#getDatabaseSchemaUpdate()"/> value will not be used.
        /// </summary>
        protected internal bool usingRelationalDatabase = true;

        /// <summary>
        /// Enabled a very verbose debug output of the execution tree whilst executing operations.
        /// Most useful for core engine developers or people fiddling around with the execution tree.
        /// </summary>
        protected internal bool enableVerboseExecutionTreeLogging;

        protected internal PerformanceSettings performanceSettings = new PerformanceSettings();

        #endregion

        // buildProcessEngine
        // ///////////////////////////////////////////////////////

        public override IProcessEngine buildProcessEngine()
        {
            init();

            ProcessEngineImpl processEngine = new ProcessEngineImpl(this);

            postProcessEngineInitialisation();

            return processEngine;
        }

        
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void init()
        {
            initConfigurators();
            configuratorsBeforeInit();
            initHistoryLevel();
            initExpressionManager();

            if (usingRelationalDatabase)
            {
                initDataSource();
            }

            initAgendaFactory();
            initHelpers();
            initVariableTypes();
            initBeans();
            initScriptingEngines();
            initClock();
            initBusinessCalendarManager();
            initCommandContextFactory();
            initTransactionContextFactory();
            //初始化命令执行器
            initCommandExecutors();
            //服务初始化
            initServices();
            initIdGenerator();
            initBehaviorFactory();
            initListenerFactory();
            initBpmnParser();
            initProcessDefinitionCache();
            initProcessDefinitionInfoCache();
            initKnowledgeBaseCache();
            initJobHandlers();
            initJobManager();
            initAsyncExecutor();

            initTransactionFactory();

            if (usingRelationalDatabase)
            {
                initSqlSessionFactory();
            }

            initSessionFactories();
            initDataManagers();
            initEntityManagers();
            initHistoryManager();
            initDeployers();
            initDelegateInterceptor();
            initEventHandlers();
            initFailedJobCommandFactory();
            initEventDispatcher();
            initProcessValidator();
            initDatabaseEventLogging();
            configuratorsAfterInit();
        }

        // failedJobCommandFactory
        // ////////////////////////////////////////////////////////

        public virtual void initFailedJobCommandFactory()
        {
            if (failedJobCommandFactory == null)
            {
                failedJobCommandFactory = new DefaultFailedJobCommandFactory();
            }
        }

        // command executors
        // ////////////////////////////////////////////////////////

        public virtual void initCommandExecutors()
        {
            initDefaultCommandConfig();
            initSchemaCommandConfig();
            //命令调用器
            initCommandInvoker();
            //初始化命令拦截器
            initCommandInterceptors();
            //初始化命令执行器
            initCommandExecutor();
        }

        public virtual void initDefaultCommandConfig()
        {
            if (defaultCommandConfig == null)
            {
                defaultCommandConfig = new CommandConfig();
            }
        }

        public virtual void initSchemaCommandConfig()
        {
            if (schemaCommandConfig == null)
            {
                schemaCommandConfig = (new CommandConfig()).transactionNotSupported();
            }
        }


        /// <summary>
        /// 命令调用器
        /// </summary>
        public virtual void initCommandInvoker()
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
        public virtual void initCommandInterceptors()
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

        public virtual ICollection<ICommandInterceptor> DefaultCommandInterceptors
        {
            get
            {
                IList<ICommandInterceptor> interceptors = new List<ICommandInterceptor>();
                //添加一个日志拦截器
                interceptors.Add(new LogInterceptor());

                ICommandInterceptor transactionInterceptor = createTransactionInterceptor();
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
        public virtual void initCommandExecutor()
        {
            if (commandExecutor == null)
            {
                //初始化命令拦截器链，并返回第一个拦截器
                ICommandInterceptor first = initInterceptorChain(commandInterceptors);
                commandExecutor = new CommandExecutorImpl(DefaultCommandConfig, first);
            }
        }

        /// <summary>
        /// //构建拦截器链，即上一个拦截器中存放着下一个拦截器
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public virtual ICommandInterceptor initInterceptorChain(IList<ICommandInterceptor> chain)
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

        public abstract ICommandInterceptor createTransactionInterceptor();

        /// <summary>
        /// 服务初始化
        /// </summary>
        public virtual void initServices()
        {
            initService(repositoryService);
            initService(runtimeService);
            initService(historyService);
            initService(taskService);
            initService(managementService);
            initService(dynamicBpmnService);
        }

        public virtual void initService(object service)
        {
            if (service is ServiceImpl)
            {
                //将命令执行器set进runtimeService中
                ((ServiceImpl)service).CommandExecutor = commandExecutor;
            }
        }

        // DataSource
        // ///////////////////////////////////////////////////////////////

        public virtual void initDataSource()
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                initDatabaseType();
            }
        }

        protected internal static Properties databaseTypeMappings = DefaultDatabaseTypeMappings;

        public const string DATABASE_TYPE_H2 = "h2";
        public const string DATABASE_TYPE_HSQL = "hsql";
        public const string DATABASE_TYPE_MYSQL = "mysql";
        public const string DATABASE_TYPE_ORACLE = "oracle";
        public const string DATABASE_TYPE_POSTGRES = "postgres";
        public const string DATABASE_TYPE_MSSQL = "mssql";

        public static Properties DefaultDatabaseTypeMappings
        {
            get
            {
                databaseTypeMappings = new Properties();

                databaseTypeMappings["H2"] = DATABASE_TYPE_H2;
                databaseTypeMappings["HSQL Database Engine"] = DATABASE_TYPE_HSQL;
                databaseTypeMappings["MySQL"] = DATABASE_TYPE_MYSQL;
                databaseTypeMappings["Oracle"] = DATABASE_TYPE_ORACLE;
                databaseTypeMappings["PostgreSQL"] = DATABASE_TYPE_POSTGRES;
                databaseTypeMappings["Microsoft SQL Server"] = DATABASE_TYPE_MSSQL;

                return databaseTypeMappings;
            }
        }

        public virtual void initDatabaseType()
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

        public virtual void initTransactionFactory()
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

        public virtual void initSqlSessionFactory()
        {
            string wildcardEscapeClause = "";
            if ((!string.ReferenceEquals(databaseWildcardEscapeCharacter, null)) && (databaseWildcardEscapeCharacter.Length != 0))
            {
                wildcardEscapeClause = " escape '" + databaseWildcardEscapeCharacter + "'";
            }

            Properties properties = new Properties();
            properties["wildcardEscapeClause"] = wildcardEscapeClause;
            //set default properties
            properties["limitBefore"] = "";
            properties["limitAfter"] = "";
            properties["limitBetween"] = "";
            properties["limitOuterJoinBetween"] = "";
            properties["limitBeforeNativeQuery"] = "";
            properties["orderBy"] = "";
            properties["blobType"] = "";
            properties["boolValue"] = "TRUE";

            string codebase = new Uri(this.GetType().Assembly.CodeBase).LocalPath;

            properties.load(Path.Combine(Path.GetDirectoryName(codebase), $@"resources\db\properties\{databaseType}.json"));

            initMybatisConfiguration(properties);
        }

        public virtual void initMybatisConfiguration(/*Environment environment, StreamReader reader,*/ Properties properties)
        {
            SqlMapper.Variables = properties;

            initMybatisTypeHandlers();
            initCustomMybatisMappers();
        }

        public virtual void initMybatisTypeHandlers()
        {

        }

        public virtual void initCustomMybatisMappers()
        {
        }

        protected internal virtual System.IO.Stream getResourceAsStream(string resource)
        {
            return ReflectUtil.getResourceAsStream(resource);
        }


        // Data managers ///////////////////////////////////////////////////////////

        public virtual void initDataManagers()
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

        public virtual void initEntityManagers()
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

        public virtual void initHistoryManager()
        {
            if (historyManager == null)
            {
                historyManager = new DefaultHistoryManager(this, historyLevel);
            }
        }

        // Job manager ///////////////////////////////////////////////////////////

        public virtual void initJobManager()
        {
            if (jobManager == null)
            {
                jobManager = new DefaultJobManager(this);
            }

            jobManager.ProcessEngineConfiguration = this;
        }

        // session factories ////////////////////////////////////////////////////////

        public virtual void initSessionFactories()
        {
            if (sessionFactories == null)
            {
                sessionFactories = new Dictionary<Type, ISessionFactory>();

                if (usingRelationalDatabase)
                {
                    initDbSqlSessionFactory();
                }

                addSessionFactory(new GenericManagerFactory(typeof(IEntityCache), typeof(EntityCacheImpl)));
            }

            if (customSessionFactories != null)
            {
                foreach (ISessionFactory sessionFactory in customSessionFactories)
                {
                    addSessionFactory(sessionFactory);
                }
            }
        }

        public virtual void initDbSqlSessionFactory()
        {
            if (dbSqlSessionFactory == null)
            {
                dbSqlSessionFactory = createDbSqlSessionFactory();
            }
            dbSqlSessionFactory.DatabaseType = databaseType;
            dbSqlSessionFactory.IdGenerator = idGenerator;
            //dbSqlSessionFactory.SqlSessionFactory = sqlSessionFactory;
            dbSqlSessionFactory.DbHistoryUsed = isDbHistoryUsed;
            dbSqlSessionFactory.DatabaseTablePrefix = databaseTablePrefix;
            dbSqlSessionFactory.TablePrefixIsSchema = tablePrefixIsSchema;
            dbSqlSessionFactory.DatabaseCatalog = databaseCatalog;
            dbSqlSessionFactory.DatabaseSchema = databaseSchema;
            dbSqlSessionFactory.setBulkInsertEnabled(isBulkInsertEnabled, databaseType);
            dbSqlSessionFactory.MaxNrOfStatementsInBulkInsert = maxNrOfStatementsInBulkInsert;
            addSessionFactory(dbSqlSessionFactory);
        }

        public virtual DbSqlSessionFactory createDbSqlSessionFactory()
        {
            return new DbSqlSessionFactory();
        }

        public virtual void addSessionFactory(ISessionFactory sessionFactory)
        {
            sessionFactories[sessionFactory.SessionType] = sessionFactory;
        }

        public virtual void initConfigurators()
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

                IEnumerable<IProcessEngineConfigurator> configuratorServiceLoader = ServiceLoader.load<IEnumerable<IProcessEngineConfigurator>>(classLoader);
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

        public virtual void configuratorsBeforeInit()
        {
            foreach (IProcessEngineConfigurator configurator in allConfigurators)
            {
                //log.info("Executing beforeInit() of {} (priority:{})", configurator.GetType(), configurator.Priority);
                configurator.beforeInit(this);
            }
        }

        public virtual void configuratorsAfterInit()
        {
            foreach (IProcessEngineConfigurator configurator in allConfigurators)
            {
                //log.info("Executing configure() of {} (priority:{})", configurator.GetType(), configurator.Priority);
                configurator.configure(this);
            }
        }

        // deployers
        // ////////////////////////////////////////////////////////////////

        public virtual void initProcessDefinitionCache()
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

        public virtual void initProcessDefinitionInfoCache()
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

        public virtual void initKnowledgeBaseCache()
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

        public virtual void initDeployers()
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
                deploymentManager = new DeploymentManager();
                deploymentManager.Deployers = deployers;

                deploymentManager.ProcessDefinitionCache = processDefinitionCache;
                deploymentManager.ProcessDefinitionInfoCache = processDefinitionInfoCache;
                deploymentManager.KnowledgeBaseCache = knowledgeBaseCache;
                deploymentManager.ProcessEngineConfiguration = this;
                deploymentManager.ProcessDefinitionEntityManager = processDefinitionEntityManager;
                deploymentManager.DeploymentEntityManager = deploymentEntityManager;
            }
        }

        public virtual void initBpmnDeployerDependencies()
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

        public virtual ICollection<IDeployer> DefaultDeployers
        {
            get
            {
                IList<IDeployer> defaultDeployers = new List<IDeployer>();

                if (bpmnDeployer == null)
                {
                    bpmnDeployer = new BpmnDeployer();
                }

                initBpmnDeployerDependencies();

                bpmnDeployer.IdGenerator = idGenerator;
                bpmnDeployer.ParsedDeploymentBuilderFactory = parsedDeploymentBuilderFactory;
                bpmnDeployer.BpmnDeploymentHelper = bpmnDeploymentHelper;
                bpmnDeployer.CachingAndArtifactsManager = cachingAndArtifactsManager;

                defaultDeployers.Add(bpmnDeployer);
                return defaultDeployers;
            }
        }

        public virtual void initListenerFactory()
        {
            if (listenerFactory == null)
            {
                DefaultListenerFactory defaultListenerFactory = new DefaultListenerFactory();
                defaultListenerFactory.ExpressionManager = expressionManager;
                listenerFactory = defaultListenerFactory;
            }
            else if ((listenerFactory is AbstractBehaviorFactory) && ((AbstractBehaviorFactory)listenerFactory).ExpressionManager == null)
            {
                ((AbstractBehaviorFactory)listenerFactory).ExpressionManager = expressionManager;
            }
        }

        public virtual void initBehaviorFactory()
        {
            if (activityBehaviorFactory == null)
            {
                DefaultActivityBehaviorFactory defaultActivityBehaviorFactory = new DefaultActivityBehaviorFactory();
                defaultActivityBehaviorFactory.ExpressionManager = expressionManager;
                activityBehaviorFactory = defaultActivityBehaviorFactory;
            }
            else if ((activityBehaviorFactory is AbstractBehaviorFactory) && ((AbstractBehaviorFactory)activityBehaviorFactory).ExpressionManager == null)
            {
                ((AbstractBehaviorFactory)activityBehaviorFactory).ExpressionManager = expressionManager;
            }
        }

        public virtual void initBpmnParser()
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
            bpmnParseHandlers.addHandlers(parseHandlers);
            bpmnParser.BpmnParserHandlers = bpmnParseHandlers;
        }

        public virtual IList<IBpmnParseHandler> DefaultBpmnParseHandlers
        {
            get
            {

                // Alphabetic list of default parse handler classes
                IList<IBpmnParseHandler> bpmnParserHandlers = new List<IBpmnParseHandler>();
                bpmnParserHandlers.Add(new BoundaryEventParseHandler());
                bpmnParserHandlers.Add(new BusinessRuleParseHandler());
                bpmnParserHandlers.Add(new CallActivityParseHandler());
                bpmnParserHandlers.Add(new CancelEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new CompensateEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new EndEventParseHandler());
                bpmnParserHandlers.Add(new ErrorEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new EventBasedGatewayParseHandler());
                bpmnParserHandlers.Add(new ExclusiveGatewayParseHandler());
                bpmnParserHandlers.Add(new InclusiveGatewayParseHandler());
                bpmnParserHandlers.Add(new IntermediateCatchEventParseHandler());
                bpmnParserHandlers.Add(new IntermediateThrowEventParseHandler());
                bpmnParserHandlers.Add(new ManualTaskParseHandler());
                bpmnParserHandlers.Add(new MessageEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new ParallelGatewayParseHandler());
                bpmnParserHandlers.Add(new ProcessParseHandler());
                bpmnParserHandlers.Add(new ReceiveTaskParseHandler());
                bpmnParserHandlers.Add(new ScriptTaskParseHandler());
                bpmnParserHandlers.Add(new SendTaskParseHandler());
                bpmnParserHandlers.Add(new SequenceFlowParseHandler());
                bpmnParserHandlers.Add(new ServiceTaskParseHandler());
                bpmnParserHandlers.Add(new SignalEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new StartEventParseHandler());
                bpmnParserHandlers.Add(new SubProcessParseHandler());
                bpmnParserHandlers.Add(new EventSubProcessParseHandler());
                bpmnParserHandlers.Add(new AdhocSubProcessParseHandler());
                bpmnParserHandlers.Add(new TaskParseHandler());
                bpmnParserHandlers.Add(new TimerEventDefinitionParseHandler());
                bpmnParserHandlers.Add(new TransactionParseHandler());
                bpmnParserHandlers.Add(new UserTaskParseHandler());

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
                                //log.info("Replacing default BpmnParseHandler " + defaultBpmnParseHandler.GetType().FullName + " with " + newBpmnParseHandler.GetType().FullName);
                                bpmnParserHandlers[i] = newBpmnParseHandler;
                            }
                        }
                    }
                }

                return bpmnParserHandlers;
            }
        }

        public virtual void initClock()
        {
            if (clock == null)
            {
                clock = new DefaultClockImpl();
            }
        }


        public virtual void initAgendaFactory()
        {
            if (this.engineAgendaFactory == null)
            {
                this.engineAgendaFactory = ProcessEngineServiceProvider.Resolve<IActivitiEngineAgendaFactory>();
            }
        }

        public virtual void initJobHandlers()
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

        public virtual void initAsyncExecutor()
        {
            if (asyncExecutor == null)
            {
                DefaultAsyncJobExecutor defaultAsyncExecutor = new DefaultAsyncJobExecutor();

                // Message queue mode
                defaultAsyncExecutor.MessageQueueMode = asyncExecutorMessageQueueMode;

                // Thread pool config
                defaultAsyncExecutor.CorePoolSize = asyncExecutorCorePoolSize;
                defaultAsyncExecutor.MaxPoolSize = asyncExecutorMaxPoolSize;
                defaultAsyncExecutor.KeepAliveTime = asyncExecutorThreadKeepAliveTime;

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
                if (!string.ReferenceEquals(asyncExecutorLockOwner, null))
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

        public virtual void initHistoryLevel()
        {
            if (historyLevel == null)
            {
                historyLevel = HistoryLevel.getHistoryLevelForKey(History);
            }
        }

        // id generator
        // /////////////////////////////////////////////////////////////

        public virtual void initIdGenerator()
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

        public virtual void initCommandContextFactory()
        {
            if (commandContextFactory == null)
            {
                commandContextFactory = new CommandContextFactory();
            }
            commandContextFactory.ProcessEngineConfiguration = this;
        }

        public virtual void initTransactionContextFactory()
        {
            if (transactionContextFactory == null)
            {
                transactionContextFactory = new StandaloneMybatisTransactionContextFactory();
            }
        }

        public virtual void initHelpers()
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

        public virtual void initVariableTypes()
        {
            if (variableTypes == null)
            {
                variableTypes = new DefaultVariableTypes();
                if (customPreVariableTypes != null)
                {
                    foreach (IVariableType customVariableType in customPreVariableTypes)
                    {
                        variableTypes.addType(customVariableType);
                    }
                }
                variableTypes.addType(new NullType());
                variableTypes.addType(new StringType(MaxLengthString));
                variableTypes.addType(new LongStringType(MaxLengthString + 1));
                variableTypes.addType(new BooleanType());
                variableTypes.addType(new ShortType());
                variableTypes.addType(new IntegerType());
                variableTypes.addType(new LongType());
                variableTypes.addType(new DateType());
                variableTypes.addType(new JodaDateType());
                variableTypes.addType(new JodaDateTimeType());
                variableTypes.addType(new DoubleType());
                variableTypes.addType(new UUIDType());
                variableTypes.addType(new JsonType(MaxLengthString, objectMapper));
                variableTypes.addType(new LongJsonType(MaxLengthString + 1, objectMapper));
                variableTypes.addType(new ByteArrayType());
                variableTypes.addType(new SerializableType(serializableVariableTypeTrackDeserializedObjects));
                variableTypes.addType(new CustomObjectType("item", typeof(ItemInstance)));
                variableTypes.addType(new CustomObjectType("message", typeof(MessageInstance)));
                if (customPostVariableTypes != null)
                {
                    foreach (IVariableType customVariableType in customPostVariableTypes)
                    {
                        variableTypes.addType(customVariableType);
                    }
                }
            }
        }

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

        public virtual void initScriptingEngines()
        {
            if (resolverFactories == null)
            {
                resolverFactories = new List<IResolverFactory>();
                resolverFactories.Add(new VariableScopeResolverFactory());
                resolverFactories.Add(new BeansResolverFactory());
            }
            //if (scriptingEngines == null)
            //{
            //    scriptingEngines = new ScriptingEngines(new ScriptBindingsFactory(this, resolverFactories));
            //}
        }

        public virtual void initExpressionManager()
        {
            if (expressionManager == null)
            {
                expressionManager = new ExpressionManager(beans);
            }
        }

        public virtual void initBusinessCalendarManager()
        {
            if (businessCalendarManager == null)
            {
                MapBusinessCalendarManager mapBusinessCalendarManager = new MapBusinessCalendarManager();
                mapBusinessCalendarManager.addBusinessCalendar(DurationBusinessCalendar.NAME, new DurationBusinessCalendar(this.clock));
                mapBusinessCalendarManager.addBusinessCalendar(DueDateBusinessCalendar.NAME, new DueDateBusinessCalendar(this.clock));
                mapBusinessCalendarManager.addBusinessCalendar(CycleBusinessCalendar.NAME, new CycleBusinessCalendar(this.clock));

                businessCalendarManager = mapBusinessCalendarManager;
            }
        }

        public virtual void initDelegateInterceptor()
        {
            if (delegateInterceptor == null)
            {
                delegateInterceptor = new DefaultDelegateInterceptor();
            }
        }

        public virtual void initEventHandlers()
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

        public virtual void initBeans()
        {
            if (beans == null)
            {
                beans = new Dictionary<object, object>();
            }
        }

        public virtual void initEventDispatcher()
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
                    this.eventDispatcher.addEventListener(listenerToAdd);
                }
            }

            if (typedEventListeners != null)
            {
                foreach (KeyValuePair<string, IList<IActivitiEventListener>> listenersToAdd in typedEventListeners.SetOfKeyValuePairs())
                {
                    // Extract types from the given string
                    ActivitiEventType[] types = ActivitiEventType.getTypesFromString(listenersToAdd.Key);

                    foreach (IActivitiEventListener listenerToAdd in listenersToAdd.Value)
                    {
                        this.eventDispatcher.addEventListener(listenerToAdd, types);
                    }
                }
            }

        }

        public virtual void initProcessValidator()
        {
            if (this.processValidator == null)
            {
                this.processValidator = (new ProcessValidatorFactory()).createDefaultProcessValidator();
            }
        }

        public virtual void initDatabaseEventLogging()
        {
            if (enableDatabaseEventLogging)
            {
                // Database event logging uses the default logging mechanism and adds
                // a specific event listener to the list of event listeners
                EventDispatcher.addEventListener(new EventLogger(clock, objectMapper));
            }
        }

        /// <summary>
        /// Called when the <seealso cref="IProcessEngine"/> is initialized, but before it is returned
        /// </summary>
        protected internal virtual void postProcessEngineInitialisation()
        {
            if (performanceSettings.ValidateExecutionRelationshipCountConfigOnBoot)
            {
                commandExecutor.execute(new ValidateExecutionRelatedEntityCountCfgCmd());
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////

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

        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return this;
            }
        }

        public virtual IDictionary<Type, ISessionFactory> SessionFactories
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

        public virtual ProcessEngineConfigurationImpl addConfigurator(IProcessEngineConfigurator configurator)
        {
            if (this.configurators == null)
            {
                this.configurators = new List<IProcessEngineConfigurator>();
            }
            this.configurators.Add(configurator);
            return this;
        }

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

        public virtual IList<IProcessEngineConfigurator> AllConfigurators
        {
            get
            {
                return allConfigurators;
            }
        }

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
        public virtual ProcessEngineConfiguration addWsEndpointAddress(string endpointName, Uri address)
        {
            this.wsOverridenEndpointAddresses.TryAdd(endpointName, address);

            return this;
        }

        /// <summary>
        /// Remove the address definition of the given web-service endpoint </summary>
        /// <param name="endpointName"> The endpoint name for which the address definition must be removed </param>
        public virtual ProcessEngineConfiguration removeWsEndpointAddress(string endpointName)
        {
            if (this.wsOverridenEndpointAddresses.TryRemove(endpointName, out var uri))
            {
                return this;
            }

            return null;
        }

        public virtual ConcurrentDictionary<string, Uri> WsOverridenEndpointAddresses
        {
            get
            {
                return this.wsOverridenEndpointAddresses;
            }
            set
            {
                this.wsOverridenEndpointAddresses.putAll(value);
            }
        }

        //public virtual ScriptingEngines ScriptingEngines
        //{
        //    get
        //    {
        //        return scriptingEngines;
        //    }
        //}

        //public virtual ProcessEngineConfigurationImpl setScriptingEngines(ScriptingEngines scriptingEngines)
        //{
        //    this.scriptingEngines = scriptingEngines;
        //    return this;
        //}

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

        public virtual IEventHandler getEventHandler(string eventType)
        {
            return eventHandlers[eventType];
        }


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

        public virtual ProcessEngineConfigurationImpl setEnableEventDispatcher(bool enableEventDispatcher)
        {
            this.enableEventDispatcher = enableEventDispatcher;
            return this;
        }

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

        public virtual bool EnableEventDispatcher
        {
            get
            {
                return enableEventDispatcher;
            }
        }

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

        public virtual IdentityLinkDataManager IdentityLinkDataManager
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

        public virtual IdentityLinkEntityManager IdentityLinkEntityManager
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

        public virtual void resetClock()
        {
            if (this.clock != null)
            {
                clock.reset();
            }
        }

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