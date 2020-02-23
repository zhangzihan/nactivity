using Microsoft.Extensions.Logging;
using SmartSql.Abstractions;
using SmartSql.Abstractions.Command;
using SmartSql.Abstractions.DbSession;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSql.Command
{
    public class CommandExecuter : ICommandExecuter
    {
        private readonly ILogger<CommandExecuter> _logger;
        private readonly IPreparedCommand _preparedCommand;

        public event OnExecutedHandler OnExecuted;

        public CommandExecuter(
            ILogger<CommandExecuter> logger
            , IPreparedCommand preparedCommand)
        {
            _logger = logger;
            _preparedCommand = preparedCommand;
        }
        #region Sync
        public int ExecuteNonQuery(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarp((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteNonQuery();
            }, dbSession, context);
        }

        public IDataReader ExecuteReader(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarp((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(dbCommand.CommandText);
                }

                IDataReader reader = dbCommand.ExecuteReader();

                return reader;
            }, dbSession, context);
        }

        public object ExecuteScalar(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarp((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteScalar();
            }, dbSession, context);
        }

        private T ExecuteWarp<T>(Func<IDbCommand, T> excute, IDbConnectionSession dbSession, RequestContext context)
        {
            var dbCommand = _preparedCommand.Prepare(dbSession, context);
            dbSession.OpenConnection();
            T result = excute(dbCommand);
            OnExecuted?.Invoke(this, new OnExecutedEventArgs
            {
                DbSession = dbSession,
                RequestContext = context
            });
            return result;
        }
        #endregion

        #region Async
        public Task<int> ExecuteNonQueryAsync(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteNonQueryAsync();
            }, dbSession, context);
        }

        public Task<int> ExecuteNonQueryAsync(IDbConnectionSession dbSession, RequestContext context, CancellationToken cancellationToken)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteNonQueryAsync(cancellationToken);
            }, dbSession, context);
        }

        public Task<object> ExecuteScalarAsync(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteScalarAsync();
            }, dbSession, context);
        }

        public Task<object> ExecuteScalarAsync(IDbConnectionSession dbSession, RequestContext context, CancellationToken cancellationToken)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteScalarAsync(cancellationToken);
            }, dbSession, context);
        }
        public Task<DbDataReader> ExecuteReaderAsync(IDbConnectionSession dbSession, RequestContext context)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteReaderAsync();
            }, dbSession, context);
        }

        public Task<DbDataReader> ExecuteReaderAsync(IDbConnectionSession dbSession, RequestContext context, CancellationToken cancellationToken)
        {
            return ExecuteWarpAsync((dbCommand) =>
            {
                dbCommand.CommandTimeout = 0;
                return dbCommand.ExecuteReaderAsync(cancellationToken);
            }, dbSession, context);
        }

        private async Task<T> ExecuteWarpAsync<T>(Func<DbCommand, Task<T>> excute, IDbConnectionSession dbSession, RequestContext context)
        {
            var dbCommand = _preparedCommand.Prepare(dbSession, context);
            await dbSession.OpenConnectionAsync().ConfigureAwait(false);
            var dbCommandAsync = dbCommand as DbCommand;
            T result = await excute(dbCommandAsync).ConfigureAwait(false);
            OnExecuted?.Invoke(this, new OnExecutedEventArgs
            {
                DbSession = dbSession,
                RequestContext = context
            });
            return result;
        }
        #endregion
    }
}
