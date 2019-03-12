using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using SmartSql.Configuration;
using Microsoft.Extensions.Options;

namespace Sys.Data
{
    public sealed class DataSourceOption
    {
        public string ProviderName { get; set; }

        public string ConnectionString { get; set; }

        public static bool operator ==(DataSourceOption a, DataSourceOption b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.ProviderName?.Trim() == b.ProviderName?.Trim() && a.ConnectionString.Trim() == b.ConnectionString.Trim();
        }

        public static bool operator !=(DataSourceOption a, DataSourceOption b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == obj as DataSourceOption;
        }

        public override int GetHashCode()
        {
            return ConnectionString.GetHashCode() ^ 2;
        }
    }

    public class DataSource : IDataSource
    {
        private static readonly ILogger<DataSource> log = ProcessEngineServiceProvider.LoggerService<DataSource>();

        private static readonly ConcurrentDictionary<string, string> dbTypes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static DataSource()
        {
            dbTypes["System.Data.SqlClient"] = "System.Data.SqlClient.SqlClientFactory,System.Data.SqlClient";
            dbTypes["MySql"] = "MySql.Data.MySqlClient.MySqlClientFactory,MySql.Data";
            dbTypes["H2Sql"] = "System.Data.H2.H2ClientFactory,H2Sharp";
        }

        //private string connectionString;
        //private string provider;

        private IOptionsMonitor<DataSourceOption> options;
        private Action<DataSource> changed;
        private DataSourceOption oldOption;

        public DataSource(IOptionsMonitor<DataSourceOption> options, Action<IDataSource> changed)
        {
            this.options = options;
            this.changed = changed;
            this.oldOption = options.CurrentValue;
            this.options.OnChange((opts) =>
            {
                if (opts != this.oldOption && this.changed != null)
                {
                    this.oldOption = opts;
                    this.changed(this);
                }
            });
            //this.provider = provider;
            //this.connectionString = connectionString;
        }

        public int PoolMaximumActiveConnections { get; set; }
        public int PoolMaximumIdleConnections { get; set; }
        public int PoolMaximumCheckoutTime { get; set; }
        public int PoolTimeToWait { get; set; }
        public bool PoolPingEnabled { get; set; }
        public string PoolPingQuery { get; set; }
        public int PoolPingConnectionsNotUsedFor { get; set; }
        public int DefaultTransactionIsolationLevel { get; set; }

        public virtual IDbConnection Connection
        {
            get => getConnection();
        }

        public virtual void forceCloseAll()
        {
            log.LogWarning("DataSource forceCloseAll");
            try
            {
                if (this.Connection != null && this.Connection.State == ConnectionState.Open)
                {
                    this.Connection.Close();
                }
            }
            catch { }
        }

        public DbProviderFactory DbProviderFactory
        {
            get
            {
                string provider = options.CurrentValue.ProviderName;

                var db = dbTypes[provider];

                return DbProviderFactoryFactory.Create(db);
            }
        }

        public string ConnectionString
        {
            get
            {
                string connectionString = options.CurrentValue.ConnectionString;

                if (options.CurrentValue.ProviderName == "H2Sql")
                {
                    return connectionString;
                }

                DbConnectionStringBuilder dsb = new DbConnectionStringBuilder();
                dsb.ConnectionString = connectionString;
                dsb.Remove("Provider");

                return dsb.ConnectionString;
            }
        }

        public virtual IDbConnection getConnection()
        {
            IDbConnection _conn = DbProviderFactory.CreateConnection();
            _conn.ConnectionString = ConnectionString;

            return _conn;
        }
    }

    public class RowBounds
    {
        private int firstResult;
        private int maxResults;

        public RowBounds(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
        }
    }
}
