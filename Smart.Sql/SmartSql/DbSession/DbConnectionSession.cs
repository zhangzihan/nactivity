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
        private DbTransaction transaction;

        public Guid Id { get; private set; }
        public DbProviderFactory DbProviderFactory { get; }
        public IDataSource DataSource { get; }
        public DbConnection Connection { get; private set; }

        public DbTransaction Transaction
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
            BeginTransaction(IsolationLevel.RepeatableRead);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            try
            {
                OpenConnection();
                if (System.Transactions.Transaction.Current is not null)
                {
                    Connection.EnlistTransaction(System.Transactions.Transaction.Current);
                }
                else
                {
                    Transaction = Connection.BeginTransaction(isolationLevel);
                }
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
            if (Connection is not null)
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
                if (Transaction is null && System.Transactions.Transaction.Current is null)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError("Before CommitTransaction,Please BeginTransaction first!");
                    }
                    throw new SmartSqlException("Before CommitTransaction,Please BeginTransaction first!");
                }
                if (Transaction is not null)
                {
                    Transaction.Commit();
                    Transaction.Dispose();
                    Transaction = null;
                }
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
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Transaction is not null)
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
                        _logger.LogError($"OpenConnection Unable to open connection to {DataSource.Name}.");
                    }
                    throw new SmartSqlException($"OpenConnection Unable to open connection to {DataSource.Name}.", ex);
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
                    await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"OpenConnection Unable to open connection to {DataSource.Name}.");
                    }
                    throw new SmartSqlException($"OpenConnection Unable to open connection to {DataSource.Name}.", ex);
                }
            }
        }
        #endregion


        public void RollbackTransaction()
        {
            if (Transaction is null && System.Transactions.Transaction.Current is null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Before RollbackTransaction,Please BeginTransaction first!");
                }
                throw new SmartSqlException("Before RollbackTransaction,Please BeginTransaction first!");
            }
            try
            {
                if (Transaction is not null)
                {
                    Transaction.Rollback();
                    Transaction.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                throw;
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
