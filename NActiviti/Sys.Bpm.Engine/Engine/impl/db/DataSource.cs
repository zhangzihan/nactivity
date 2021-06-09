using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartSql.Configuration;
using Sys.Workflow;
using Sys.Workflow.Options;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class DataSource : IDataSource
    {
        private static readonly ILogger<DataSource> log = ProcessEngineServiceProvider.LoggerService<DataSource>();

        private static readonly ConcurrentDictionary<string, string> dbTypes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static DataSource()
        {
            dbTypes.TryAdd("System.Data.SqlClient", "System.Data.SqlClient.SqlClientFactory,System.Data.SqlClient");
            dbTypes.TryAdd("MySql", "MySql.Data.MySqlClient.MySqlClientFactory,MySqlConnector");
            dbTypes.TryAdd("Npgsql", "Npgsql.NpgsqlFactory,Npgsql");
            dbTypes.TryAdd("H2Sql", "System.Data.H2.H2ClientFactory,H2Sharp");
        }

        //private string connectionString;
        //private string provider;

        private readonly IOptionsMonitor<DataSourceOption> options;
        private readonly Action<DataSource> changed;
        private DataSourceOption oldOption;

        /// <summary>
        /// 
        /// </summary>
        public DataSource(IOptionsMonitor<DataSourceOption> options, Action<IDataSource> changed)
        {
            this.options = options;
            this.changed = changed;
            this.oldOption = options.CurrentValue;
            this.options.OnChange((opts) =>
            {
                if (opts != this.oldOption && this.changed is object)
                {
                    this.oldOption = opts;
                    this.changed(this);
                }
            });
            //this.provider = provider;
            //this.connectionString = connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        public int PoolMaximumActiveConnections { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PoolMaximumIdleConnections { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PoolMaximumCheckoutTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PoolTimeToWait { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool PoolPingEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PoolPingQuery { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PoolPingConnectionsNotUsedFor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DefaultTransactionIsolationLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDbConnection Connection
        {
            get => GetConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ForceCloseAll()
        {
            log.LogWarning("DataSource forceCloseAll");
            try
            {
                if (this.Connection is object && this.Connection.State == ConnectionState.Open)
                {
                    this.Connection.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        public DbProviderFactory DbProviderFactory
        {
            get
            {
                string provider = options.CurrentValue.ProviderName;

                dbTypes.TryGetValue(provider, out string dbType);

                return DbProviderFactoryFactory.Create(dbType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString
        {
            get
            {
                string connectionString = options.CurrentValue.ConnectionString;

                if (options.CurrentValue.ProviderName == "H2Sql")
                {
                    return connectionString;
                }

                DbConnectionStringBuilder dsb = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };
                dsb.Remove("Provider");

                return dsb.ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDbConnection GetConnection()
        {
            IDbConnection _conn = DbProviderFactory.CreateConnection();
            _conn.ConnectionString = ConnectionString;

            return _conn;
        }
    }

    /// <summary>
    /// 
    /// </summary>

    public class RowBounds
    {
        private readonly int firstResult;
        private readonly int maxResults;

        /// <summary>
        /// 
        /// </summary>
        public RowBounds(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
        }

        /// <summary>
        /// 
        /// </summary>
        public int FirstResult => firstResult;

        /// <summary>
        /// 
        /// </summary>
        public int MaxResults => maxResults;
    }
}
