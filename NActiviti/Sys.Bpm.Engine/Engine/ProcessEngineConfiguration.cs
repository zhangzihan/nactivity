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
namespace org.activiti.engine
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using org.activiti.engine.cfg;
    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.persistence.entity.integration;
    using org.activiti.engine.integration;
    using org.activiti.engine.runtime;
    using SmartSql.Abstractions;
    using Sys;
    using Sys.Bpm;
    using Sys.Data;
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
    /// To create a process engine programmatic, without a configuration file, the first option is <seealso cref="#createStandaloneProcessEngineConfiguration()"/>
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
    /// The second option is great for testing: <seealso cref="#createStandalonInMemeProcessEngineConfiguration()"/>
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
    /// On all forms of creating a process engine, you can first customize the configuration before calling the <seealso cref="#buildProcessEngine()"/> method by calling any of the setters like this:
    /// 
    /// <pre>
    /// ProcessEngine processEngine = ProcessEngineConfiguration.createProcessEngineConfigurationFromResourceDefault().setMailServerHost(&quot;gmail.com&quot;).setJdbcUsername(&quot;mickey&quot;).setJdbcPassword(&quot;mouse&quot;)
    ///     .buildProcessEngine();
    /// </pre>
    /// 
    /// </para>
    /// </summary>
    /// <seealso cref= ProcessEngines
    ///  </seealso>
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
        /// The tenant id indicating 'no tenant' </summary>
        public const string NO_TENANT_ID = "";

        protected internal string processEngineName = ProcessEngineFactory.NAME_DEFAULT;
        protected internal int idBlockSize = 2500;
        protected internal string history = HistoryLevel.AUDIT.Key;
        protected internal bool asyncExecutorActivate = false;

        protected internal string mailServerHost = "localhost";
        protected internal string mailServerUsername; // by default no name and password are provided, which
        protected internal string mailServerPassword; // means no authentication for mail server
        protected internal int mailServerPort = 25;
        protected internal bool useSSL;
        protected internal bool useTLS;
        protected internal string mailServerDefaultFrom = "activiti@localhost";
        protected internal string mailSessionJndi;
        protected internal IDictionary<string, MailServerInfo> mailServers = new Dictionary<string, MailServerInfo>();

        protected internal string databaseType = null;
        protected internal string databaseSchemaUpdate = DB_SCHEMA_UPDATE_FALSE;
        protected internal string connectionString = "";

        protected internal bool isDbHistoryUsed = true;
        protected internal HistoryLevel historyLevel;
        protected internal IDataSource dataSource = ProcessEngineServiceProvider.Resolve<IDataSource>();
        protected internal bool transactionsExternallyManaged;

        protected internal IClock clock;
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
        /// <strong>NOTE: the prefix is not respected by automatic database schema management. If you use <seealso cref="ProcessEngineConfiguration#DB_SCHEMA_UPDATE_CREATE_DROP"/> or
        /// <seealso cref="ProcessEngineConfiguration#DB_SCHEMA_UPDATE_TRUE"/>, activiti will create the database tables using the default names, regardless of the prefix configured here.</strong>
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

        protected internal string xmlEncoding = "UTF-8";

        protected internal string defaultCamelContext = "camelContext";

        protected internal ClassLoader classLoader;
        /// <summary>
        /// Either use Class.forName or ClassLoader.loadClass for class loading. See http://forums.activiti.org/content/reflectutilloadclass-and-custom- classloader
        /// </summary>
        protected internal bool useClassForNameClassLoading = true;
        protected internal IProcessEngineLifecycleListener processEngineLifecycleListener;

        protected internal bool enableProcessDefinitionInfoCache = false;
        protected internal IActivitiEngineAgendaFactory engineAgendaFactory;

        protected IConfiguration Configuration { get; set; }

        /// <summary>
        /// use one of the static createXxxx methods instead </summary>
        protected internal ProcessEngineConfiguration(IConfiguration configuration)
        {
            this.Configuration = configuration;

            Configuration.Bind(this);
        }

        public abstract IProcessEngine buildProcessEngine();

        // TODO add later when we have test coverage for this
        // public static ProcessEngineConfiguration
        // createJtaProcessEngineConfiguration() {
        // return new JtaProcessEngineConfiguration();
        // }

        public abstract IRepositoryService RepositoryService { get; protected set; }

        public abstract IRuntimeService RuntimeService { get; protected set; }

        public abstract ITaskService TaskService { get; protected set; }

        public IGetBookmarkRuleProvider GetBookmarkRuleProvider { get; protected set; }

        public abstract IHistoryService HistoryService { get; protected set; }

        public abstract IManagementService ManagementService { get; protected set; }

        public abstract IUserGroupLookupProxy UserGroupLookupProxy { get; set; }

        public abstract IIntegrationContextService IntegrationContextService { get; }

        public abstract IIntegrationContextManager IntegrationContextManager { get; }

        // getters and setters
        // //////////////////////////////////////////////////////

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

        public virtual MailServerInfo getMailServer(string tenantId)
        {
            return mailServers[tenantId];
        }

        public virtual IDictionary<string, MailServerInfo> MailServers
        {
            get
            {
                return mailServers;
            }
            set
            {
                this.mailServers.putAll(value);
            }
        }

        public virtual string DatabaseType
        {
            get
            {
                return databaseType;
            }
        }

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

        public virtual IDataSource DataSource
        {
            get
            {
                return ProcessEngineServiceProvider.Resolve<IDataSource>();
            }
        }

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

        public ISmartSqlMapper SqlMapper
        {
            get
            {
                return ProcessEngineServiceProvider.Resolve<ISmartSqlMapper>();
            }
        }

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