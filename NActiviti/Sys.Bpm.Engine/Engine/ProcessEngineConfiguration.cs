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
namespace Sys.Workflow.Engine
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sys.Workflow.Engine.Cfg;
    using Sys.Workflow.Engine.Impl.Asyncexecutor;
    using Sys.Workflow.Engine.Impl.Histories;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Integration;
    using Sys.Workflow.Engine.Integration;
    using Sys.Workflow.Engine.Runtime;
    using Sys;
    using Sys.Workflow;
    using Sys.Data;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;


    /// <summary>
    /// Configuration information from which a process engine can be build.
    /// 
    /// <para>
    /// Most common is to create a process engine based on the default configuration file:
    /// 
    /// <pre>
    /// ProcessEngine processEngine = ProcessEngineConfiguration.createProcessEngineConfigurationFromResourceDefault().buildProcessEngine();
    /// </pre>
    /// 
    /// </para>
    /// 
    /// <para>
    /// To create a process engine programmatic, without a configuration file, the first option is <seealso cref="CreateStandaloneProcessEngineConfiguration()"/>
    /// 
    /// <pre>
    /// ProcessEngine processEngine = ProcessEngineConfiguration.createStandaloneProcessEngineConfiguration().buildProcessEngine();
    /// </pre>
    /// 
    /// This creates a new process engine with all the defaults to connect to a remote h2 database (jdbc:h2:tcp://localhost/activiti) in standalone mode. Standalone mode means that Activiti will manage the
    /// transactions on the JDBC connections that it creates. One transaction per service method. For a description of how to write the configuration files, see the userguide.
    /// </para>
    /// 
    /// <para>
    /// The second option is great for testing: <seealso cref="CreateStandalonInMemeProcessEngineConfiguration()"/>
    /// 
    /// <pre>
    /// ProcessEngine processEngine = ProcessEngineConfiguration.createStandaloneInMemProcessEngineConfiguration().buildProcessEngine();
    /// </pre>
    /// 
    /// This creates a new process engine with all the defaults to connect to an memory h2 database (jdbc:h2:tcp://localhost/activiti) in standalone mode. The DB schema strategy default is in this case
    /// <code>create-drop</code>. Standalone mode means that Activiti will manage the transactions on the JDBC connections that it creates. One transaction per service method.
    /// </para>
    /// 
    /// <para>
    /// On all forms of creating a process engine, you can first customize the configuration before calling the <seealso cref="BuildProcessEngine()"/> method by calling any of the setters like this:
    /// 
    /// <pre>
    /// ProcessEngine processEngine = ProcessEngineConfiguration.createProcessEngineConfigurationFromResourceDefault().setMailServerHost(&quot;gmail.com&quot;).setJdbcUsername(&quot;mickey&quot;).setJdbcPassword(&quot;mouse&quot;)
    ///     .buildProcessEngine();
    /// </pre>
    /// 
    /// </para>
    /// </summary>
    /// <seealso></seealso>
    public abstract class ProcessEngineConfiguration
    {
        /// <summary>
        /// Checks the version of the DB schema against the library when the process engine is being created and throws an exception if the versions don't match.
        /// </summary>
        public const string DB_SCHEMA_UPDATE_FALSE = "false";

        /// <summary>
        /// Creates the schema when the process engine is being created and drops the schema when the process engine is being closed.
        /// </summary>
        public const string DB_SCHEMA_UPDATE_CREATE_DROP = "create-drop";

        /// <summary>
        /// Upon building of the process engine, a check is performed and an update of the schema is performed if it is necessary.
        /// </summary>
        public const string DB_SCHEMA_UPDATE_TRUE = "true";

        /// <summary>
        /// 
        /// </summary>
        public const string DB_SCHEMA_UPDATE_CREATE = "create";

        /// <summary>
        /// 
        /// </summary>
        public const string DB_SCHEMA_UPDATE_DROP_CREATE = "drop-create";

        /// <summary>
        /// The tenant id indicating 'no tenant' </summary>
        public const string NO_TENANT_ID = "";

        /// <summary>
        /// 
        /// </summary>
        protected internal string processEngineName = ProcessEngineFactory.NAME_DEFAULT;

        /// <summary>
        /// 
        /// </summary>
        protected internal int idBlockSize = 2500;

        /// <summary>
        /// 
        /// </summary>
        protected internal string history = HistoryLevel.AUDIT.Key;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool asyncExecutorActivate = false;

        /// <summary>
        /// 
        /// </summary>
        protected internal string mailServerHost = "localhost";

        /// <summary>
        /// 
        /// </summary>
        protected internal string mailServerUsername; // by default no name and password are provided, which

        /// <summary>
        /// 
        /// </summary>
        protected internal string mailServerPassword; // means no authentication for mail server

        /// <summary>
        /// 
        /// </summary>
        protected internal int mailServerPort = 25;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool useSSL;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool useTLS;

        /// <summary>
        /// 
        /// </summary>
        protected internal string mailServerDefaultFrom = "activiti@localhost";

        /// <summary>
        /// 
        /// </summary>
        protected internal string mailSessionJndi;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<string, MailServerInfo> mailServers = new Dictionary<string, MailServerInfo>();

        /// <summary>
        /// 
        /// </summary>
        protected internal string databaseType = null;

        /// <summary>
        /// 
        /// </summary>
        protected internal string databaseSchemaUpdate = DB_SCHEMA_UPDATE_FALSE;

        /// <summary>
        /// 
        /// </summary>
        protected internal string connectionString = "";

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isDbHistoryUsed = true;

        /// <summary>
        /// 
        /// </summary>
        protected internal HistoryLevel historyLevel;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDataSource dataSource = ProcessEngineServiceProvider.Resolve<IDataSource>();

        /// <summary>
        /// 
        /// </summary>
        protected internal bool transactionsExternallyManaged;

        /// <summary>
        /// 
        /// </summary>
        protected internal IClock clock;

        /// <summary>
        /// 
        /// </summary>
        protected internal IAsyncExecutor asyncExecutor;
        /// <summary>
        /// Define the default lock time for an async job in seconds. The lock time is used when creating an async job and when it expires the async executor assumes that the job has failed. It will be
        /// retried again.
        /// </summary>
        protected internal int lockTimeAsyncJobWaitTime = 60;
        /// <summary>
        /// define the default wait time for a failed job in seconds </summary>
        protected internal int defaultFailedJobWaitTime = 10;
        /// <summary>
        /// define the default wait time for a failed async job in seconds </summary>
        protected internal int asyncFailedJobWaitTime = 10;

        /// <summary>
        /// Allows configuring a database table prefix which is used for all runtime operations of the process engine. For example, if you specify a prefix named 'PRE1.', activiti will query for executions
        /// in a table named 'PRE1.ACT_RU_EXECUTION_'.
        /// 
        /// <p />
        /// <strong>NOTE: the prefix is not respected by automatic database schema management. If you use <seealso cref="ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP"/> or
        /// <seealso cref="ProcessEngineConfiguration.DB_SCHEMA_UPDATE_TRUE"/>, activiti will create the database tables using the default names, regardless of the prefix configured here.</strong>
        /// 
        /// @since 5.9
        /// </summary>
        protected internal string databaseTablePrefix = "";

        /// <summary>
        /// Escape character for doing wildcard searches.
        /// 
        /// This will be added at then end of queries that include for example a LIKE clause.
        /// For example: SELECT * FROM table WHERE column LIKE '%\%%' ESCAPE '\';
        /// </summary>
        protected internal string databaseWildcardEscapeCharacter;

        /// <summary>
        /// database catalog to use
        /// </summary>
        protected internal string databaseCatalog = "";

        /// <summary>
        /// In some situations you want to set the schema to use for table checks / generation if the database metadata doesn't return that correctly, see https://jira.codehaus.org/browse/ACT-1220,
        /// https://jira.codehaus.org/browse/ACT-1062
        /// </summary>
        protected internal string databaseSchema;

        /// <summary>
        /// Set to true in case the defined databaseTablePrefix is a schema-name, instead of an actual table name prefix. This is relevant for checking if Activiti-tables exist, the databaseTablePrefix will
        /// not be used here - since the schema is taken into account already, adding a prefix for the table-check will result in wrong table-names.
        /// 
        /// @since 5.15
        /// </summary>
        protected internal bool tablePrefixIsSchema;

        /// <summary>
        /// 
        /// </summary>
        protected internal string xmlEncoding = "UTF-8";

        /// <summary>
        /// 
        /// </summary>
        protected internal string defaultCamelContext = "camelContext";

        /// <summary>
        /// 
        /// </summary>
        protected internal ClassLoader classLoader;
        /// <summary>
        /// Either use Class.forName or ClassLoader.loadClass for class loading. See http://forums.activiti.org/content/reflectutilloadclass-and-custom- classloader
        /// </summary>
        protected internal bool useClassForNameClassLoading = true;

        /// <summary>
        /// 
        /// </summary>
        protected internal IProcessEngineLifecycleListener processEngineLifecycleListener;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool enableProcessDefinitionInfoCache = false;

        /// <summary>
        /// 
        /// </summary>
        protected internal IActivitiEngineAgendaFactory engineAgendaFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal string webapiErrorCode = "-1000";

        /// <summary>
        /// 
        /// </summary>
        protected IConfiguration Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract Properties GetProperties();

        /// <summary>
        /// use one of the static createXxxx methods instead </summary>
        protected internal ProcessEngineConfiguration(IConfiguration configuration)
        {
            this.Configuration = configuration;

            Configuration.Bind(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract IProcessEngine BuildProcessEngine();

        // TODO add later when we have test coverage for this
        // public static ProcessEngineConfiguration
        // createJtaProcessEngineConfiguration() {
        // return new JtaProcessEngineConfiguration();
        // }

        /// <summary>
        /// 
        /// </summary>
        public abstract IRepositoryService RepositoryService { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IRuntimeService RuntimeService { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract ITaskService TaskService { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IGetBookmarkRuleProvider GetBookmarkRuleProvider { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IHistoryService HistoryService { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IManagementService ManagementService { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IUserGroupLookupProxy UserGroupLookupProxy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IIntegrationContextService IntegrationContextService { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract IIntegrationContextManager IntegrationContextManager { get; }

        // getters and setters
        // //////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual string ProcessEngineName
        {
            get
            {
                return processEngineName;
            }
            set
            {
                this.processEngineName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int IdBlockSize
        {
            get
            {
                return idBlockSize;
            }
            set
            {
                this.idBlockSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string History
        {
            get
            {
                return history;
            }
            set
            {
                this.history = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailServerHost
        {
            get
            {
                return mailServerHost;
            }
            set
            {
                this.mailServerHost = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailServerUsername
        {
            get
            {
                return mailServerUsername;
            }
            set
            {
                this.mailServerUsername = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailServerPassword
        {
            get
            {
                return mailServerPassword;
            }
            set
            {
                this.mailServerPassword = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailSessionJndi
        {
            get
            {
                return mailSessionJndi;
            }
            set
            {
                this.mailSessionJndi = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MailServerPort
        {
            get
            {
                return mailServerPort;
            }
            set
            {
                this.mailServerPort = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool MailServerUseSSL
        {
            get
            {
                return useSSL;
            }
            set
            {
                this.useSSL = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool MailServerUseTLS
        {
            get
            {
                return useTLS;
            }
            set
            {
                this.useTLS = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailServerDefaultFrom
        {
            get
            {
                return mailServerDefaultFrom;
            }
            set
            {
                this.mailServerDefaultFrom = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual MailServerInfo GetMailServer(string tenantId)
        {
            return mailServers[tenantId];
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, MailServerInfo> MailServers
        {
            get
            {
                return mailServers;
            }
            set
            {
                this.mailServers.PutAll(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseType
        {
            get
            {
                return databaseType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseSchemaUpdate
        {
            get
            {
                return databaseSchemaUpdate;
            }
            set
            {
                databaseSchemaUpdate = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDataSource DataSource
        {
            get
            {
                return ProcessEngineServiceProvider.Resolve<IDataSource>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool TransactionsExternallyManaged
        {
            get
            {
                return transactionsExternallyManaged;
            }
            set
            {
                this.transactionsExternallyManaged = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual HistoryLevel HistoryLevel
        {
            get
            {
                return historyLevel;
            }
            set
            {
                this.historyLevel = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool DbHistoryUsed
        {
            get
            {
                return isDbHistoryUsed;
            }
            set
            {
                this.isDbHistoryUsed = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AsyncExecutorActivate
        {
            get
            {
                return asyncExecutorActivate;
            }
            set
            {
                this.asyncExecutorActivate = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ClassLoader ClassLoader
        {
            get
            {
                return classLoader;
            }
            set
            {
                this.classLoader = value;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool UseClassForNameClassLoading
        {
            get
            {
                return useClassForNameClassLoading;
            }
            set
            {
                this.useClassForNameClassLoading = value;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DefaultCamelContext
        {
            get
            {
                return defaultCamelContext;
            }
            set
            {
                this.defaultCamelContext = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessEngineLifecycleListener ProcessEngineLifecycleListener
        {
            get
            {
                return processEngineLifecycleListener;
            }
            set
            {
                this.processEngineLifecycleListener = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseTablePrefix
        {
            get
            {
                return databaseTablePrefix;
            }
            set
            {
                this.databaseTablePrefix = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool TablePrefixIsSchema
        {
            get
            {
                return tablePrefixIsSchema;
            }
            set
            {
                this.tablePrefixIsSchema = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string WebApiErrorCode
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseWildcardEscapeCharacter
        {
            get
            {
                return databaseWildcardEscapeCharacter;
            }
            set
            {
                this.databaseWildcardEscapeCharacter = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseCatalog
        {
            get
            {
                return databaseCatalog;
            }
            set
            {
                this.databaseCatalog = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseSchema
        {
            get
            {
                return databaseSchema;
            }
            set
            {
                this.databaseSchema = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string XmlEncoding
        {
            get
            {
                return xmlEncoding;
            }
            set
            {
                this.xmlEncoding = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IClock Clock
        {
            get
            {
                return clock;
            }
            set
            {
                this.clock = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IAsyncExecutor AsyncExecutor
        {
            get
            {
                return asyncExecutor;
            }
            set
            {
                this.asyncExecutor = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int LockTimeAsyncJobWaitTime
        {
            get
            {
                return lockTimeAsyncJobWaitTime;
            }
            set
            {
                this.lockTimeAsyncJobWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultFailedJobWaitTime
        {
            get
            {
                return defaultFailedJobWaitTime;
            }
            set
            {
                this.defaultFailedJobWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncFailedJobWaitTime
        {
            get
            {
                return asyncFailedJobWaitTime;
            }
            set
            {
                this.asyncFailedJobWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EnableProcessDefinitionInfoCache
        {
            get
            {
                return enableProcessDefinitionInfoCache;
            }
            set
            {
                this.enableProcessDefinitionInfoCache = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IActivitiEngineAgendaFactory EngineAgendaFactory
        {
            set
            {
                this.engineAgendaFactory = value;
            }
            get
            {
                return engineAgendaFactory;
            }
        }
    }
}