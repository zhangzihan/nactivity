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

namespace Sys.Data
{
    public class DataSource : IDataSource
    {
        private static ILogger<DataSource> log = ProcessEngineServiceProvider.LoggerService<DataSource>();

        private static ConcurrentDictionary<string, DbProviderFactory> dbTypes = new ConcurrentDictionary<string, DbProviderFactory>(StringComparer.OrdinalIgnoreCase);

        static DataSource()
        {
            //Validator.Validate();

            dbTypes["System.Data.SqlClient"] = System.Data.SqlClient.SqlClientFactory.Instance;
        }

        private string connectionString;
        private string provider;

        public DataSource(string provider, string connectionString)
        {
            Connection = getConnection(provider, connectionString);
        }

        public int PoolMaximumActiveConnections { get; set; }
        public int PoolMaximumIdleConnections { get; set; }
        public int PoolMaximumCheckoutTime { get; set; }
        public int PoolTimeToWait { get; set; }
        public bool PoolPingEnabled { get; set; }
        public string PoolPingQuery { get; set; }
        public int PoolPingConnectionsNotUsedFor { get; set; }
        public int DefaultTransactionIsolationLevel { get; set; }

        public virtual IDbConnection Connection { get; set; }

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

        public virtual IDbConnection getConnection(string provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;

            DbConnectionStringBuilder dsb = new DbConnectionStringBuilder();
            dsb.ConnectionString = connectionString;
            dsb.Remove("Provider");

            var db = dbTypes[provider]; // DbProviderFactories.GetFactory(provider);
            Connection = db.CreateConnection();
            Connection.ConnectionString = dsb.ConnectionString;

            return Connection;
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
