using SmartSql.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace SmartSql
{
    public static class SmartSqlExtensions
    {
        public static void TransactionWrap(this ISmartSqlMapper sqlMapper, Action handler)
        {
            try
            {
                sqlMapper.BeginTransaction();
                handler();
                sqlMapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                sqlMapper.RollbackTransaction();
                throw;
            }
        }

        public static void TransactionWrap(this ISmartSqlMapper sqlMapper, IsolationLevel isolationLevel, Action handler)
        {
            try
            {
                sqlMapper.BeginTransaction(isolationLevel);
                handler();
                sqlMapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                sqlMapper.RollbackTransaction();
                throw;
            }
        }

        public async static Task TransactionWrapAsync(this ISmartSqlMapper sqlMapper, Func<Task> handler)
        {
            try
            {
                sqlMapper.BeginTransaction();
                await handler().ConfigureAwait(false);
                sqlMapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                sqlMapper.RollbackTransaction();
                throw;
            }
        }

        public async static Task TransactionWrapAsync(this ISmartSqlMapper sqlMapper, IsolationLevel isolationLevel, Func<Task> handler)
        {
            try
            {
                sqlMapper.BeginTransaction(isolationLevel);
                await handler().ConfigureAwait(false);
                sqlMapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                sqlMapper.RollbackTransaction();
                throw;
            }
        }
    }
}
