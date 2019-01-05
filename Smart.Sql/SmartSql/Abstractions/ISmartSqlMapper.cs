using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SmartSql.Abstractions.Cache;
using SmartSql.Abstractions.Command;
using SmartSql.Abstractions.Config;
using SmartSql.Abstractions.DataReaderDeserializer;
using SmartSql.Abstractions.DataSource;
using SmartSql.Abstractions.DbSession;
using System;
using System.Collections.Generic;
using System.Data;

namespace SmartSql.Abstractions
{
    /// <summary>
    /// SmartSql 映射器
    /// </summary>
    public interface ISmartSqlMapper : ISmartSqlMapperAsync, IDisposable
    {
        JToken Variables { get; set; }

        IDbConnectionSessionStore SessionStore { get; }

        IDataSourceFilter DataSourceFilter { get; }

        IConfigLoader ConfigLoader { get; }

        ICommandExecuter CommandExecuter { get; }

        IDataReaderDeserializerFactory DeserializerFactory { get; }

        ILoggerFactory LoggerFactory { get; }

        ICacheManager CacheManager { get; }

        ISqlBuilder SqlBuilder { get; }

        int Execute(RequestContext context);
        T ExecuteScalar<T>(RequestContext context);
        IEnumerable<T> Query<T>(RequestContext context);
        T QuerySingle<T>(RequestContext context);

        DataTable GetDataTable(RequestContext context);
        DataSet GetDataSet(RequestContext context);

        #region Transaction
        IDbConnectionSession BeginTransaction();
        IDbConnectionSession BeginTransaction(RequestContext context);
        IDbConnectionSession BeginTransaction(IsolationLevel isolationLevel);
        IDbConnectionSession BeginTransaction(RequestContext context, IsolationLevel isolationLevel);
        void CommitTransaction();
        void RollbackTransaction();
        #endregion
        #region Scoped Session
        IDbConnectionSession BeginSession(RequestContext context);
        void EndSession();
        #endregion
    }

    public enum DataSourceChoice
    {
        Write,
        Read
    }
}
