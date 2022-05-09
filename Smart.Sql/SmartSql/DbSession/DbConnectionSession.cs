using SmartSql.Abstractions.DbSession;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using SmartSql.Abstractions.DataSource;
using SmartSql.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSql.DbSession
{
    public class DbConnectionSession : IDbConnectionSession
    {
        private readonly ILogger _logger;
        private IDbTransaction transaction;

        public Guid Id { get; private set; }
        public DbProviderFactory DbProviderFactory { get; }
        public IDataSource DataSource { get; }
        public IDbConnection Connection { get; private set; }

        public IDbTransaction Transaction
        {
            get => transaction;
            private set
            {
                transaction = value;
            }
        }

        public DbSessionLifeCycle LifeCycle { get; set; }
        public DbConnectionSession(ILogger<DbConnectionSession> logger, DbProviderFactory dbProviderFactory, IDataSource dataSource)
        {
            _logger = logger;
            Id = Guid.NewGuid();
            LifeCycle = DbSessionLifeCycle.Transient;
            DbProviderFactory = dbProviderFactory;
            DataSource = dataSource;
            CreateConnection();
        }

        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            try
            {
                OpenConnection();
                Transaction = Connection.BeginTransaction(isolationLevel);
                LifeCycle = DbSessionLifeCycle.Scoped;
            }
            catch
            {
                LifeCycle = DbSessionLifeCycle.Transient;
                throw;
            }
        }

        public void CreateConnection()
        {
            Connection = DbProviderFactory.CreateConnection();
            Connection.ConnectionString = DataSource.ConnectionString;
        }

        public void CloseConnection()
        {
            if ((Connection is object) && (Connection.State != ConnectionState.Closed))
            {
                Connection.Close();
                Connection.Dispose();
            }
            Connection = null;
        }

        public void CommitTransaction()
        {
            try
            {
                if (Transaction is null)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError("Before CommitTransaction,Please BeginTransaction first!");
                    }
                    throw new SmartSqlException("Before CommitTransaction,Please BeginTransaction first!");
                }
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
                LifeCycle = DbSessionLifeCycle.Transient;
                CloseConnection();
            }
            catch
            {
                LifeCycle = DbSessionLifeCycle.Transient;
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Transaction is object)
                {
                    if (Connection.State != ConnectionState.Closed)
                    {
                        RollbackTransaction();
                    }
                }
                else
                {
                    CloseConnection();
                }
            }
        }

        public void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                try
                {
                    Connection.Open();
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"OpenConnection Unable to open connection to { DataSource.Name }.");
                    }
                    throw new SmartSqlException($"OpenConnection Unable to open connection to { DataSource.Name }.", ex);
                }
            }
        }
        #region Async
        public Task OpenConnectionAsync()
        {
            return OpenConnectionAsync(CancellationToken.None);
        }

        public async Task OpenConnectionAsync(CancellationToken cancellationToken)
        {
            if (Connection.State != ConnectionState.Open)
            {
                try
                {
                    var connAsync = Connection as DbConnection;
                    await connAsync.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"OpenConnection Unable to open connection to { DataSource.Name }.");
                    }
                    throw new SmartSqlException($"OpenConnection Unable to open connection to { DataSource.Name }.", ex);
                }
            }
        }
        #endregion


        public void RollbackTransaction()
        {
            if (Transaction is null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Before RollbackTransaction,Please BeginTransaction first!");
                }
                throw new SmartSqlException("Before RollbackTransaction,Please BeginTransaction first!");
            }
            try
            {
                Transaction.Rollback();
                Transaction.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                throw ex;
            }
            finally
            {
                Transaction = null;
                LifeCycle = DbSessionLifeCycle.Transient;
                CloseConnection();
            }
        }

        public void Begin()
        {
            LifeCycle = DbSessionLifeCycle.Scoped;
            OpenConnection();
        }

        public void End()
        {
            LifeCycle = DbSessionLifeCycle.Transient;
            CloseConnection();
        }
    }
}
