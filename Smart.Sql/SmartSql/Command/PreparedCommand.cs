using Microsoft.Extensions.Logging;
using SmartSql.Abstractions;
using SmartSql.Abstractions.Command;
using SmartSql.Abstractions.DbSession;
using SmartSql.Abstractions.TypeHandler;
using Spring.Expressions;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartSql.Command
{
    public class PreparedCommand : IPreparedCommand
    {
        private readonly Regex _sqlParamsTokens;
        private readonly ILogger<PreparedCommand> _logger;
        private readonly SmartSqlContext _smartSqlContext;

        public event OnPreparedHandler OnPrepared;

        ///private readonly string dbPrefixs = "";

        public PreparedCommand(
            ILogger<PreparedCommand> logger
            , SmartSqlContext smartSqlContext)
        {
            _logger = logger;
            _smartSqlContext = smartSqlContext;
            var dbPrefixs = $"{smartSqlContext.DbPrefix}{smartSqlContext.SmartDbPrefix}#";
            var regOptions = RegexOptions.Multiline | RegexOptions.CultureInvariant;
            if (smartSqlContext.IgnoreParameterCase)
            {
                regOptions |= RegexOptions.IgnoreCase;
            }
            _sqlParamsTokens = new Regex(@"[" + dbPrefixs + @"]{?(([\p{L}\p{N}_]+(\[\d+\])*)(\.[\p{L}\p{N}_]+)*)}?", regOptions);
        }

        private static readonly Regex regDbParamName = new Regex(@"(\[(\d+)\])*\.");

        public IDbCommand Prepare(IDbConnectionSession dbSession, RequestContext context)
        {
            var dbCommand = dbSession.Connection.CreateCommand();
            dbCommand.CommandType = context.CommandType;
            dbCommand.Transaction = dbSession.Transaction;
            switch (dbCommand.CommandType)
            {
                case CommandType.Text:
                    {
                        string sql = context.RealSql;
                        if (_sqlParamsTokens.IsMatch(sql))
                        {
                            sql = _sqlParamsTokens.Replace(sql, match =>
                              {
                                  string paramName = match.Groups[1].Value;
                                  var paramMap = context.Statement?.ParameterMap?.Parameters?.FirstOrDefault(p => new Regex(p.Property, RegexOptions.IgnoreCase).IsMatch(paramName));
                                  string propertyName = paramName; //paramMap != null ? paramMap.Property : paramName;
                                  string dbParamName = regDbParamName.Replace(paramName, "$2_");

                                  if (context.RequestParameters == null)
                                  {
                                      return $"{_smartSqlContext.DbPrefix}{dbParamName}";
                                  }

                                  object paramVal;
                                  if (propertyName.Contains("."))
                                  {
                                      paramVal = ExpressionEvaluator.GetValue(context.RequestParameters, propertyName);
                                  }
                                  else
                                  {
                                      context.RequestParameters.TryGetValue(propertyName, out paramVal);
                                  }

                                  ITypeHandler typeHandler = paramMap?.Handler;
                                  if (typeHandler != null)
                                  {
                                      AddParameterIfNotExists(dbCommand, dbParamName, paramVal, typeHandler);
                                      return $"{_smartSqlContext.DbPrefix}{dbParamName}";
                                  }
                                  bool isString = paramVal is String;
                                  if (paramVal is IEnumerable && !isString)
                                  {
                                      var enumParams = paramVal as IEnumerable;
                                      StringBuilder inParamSql = new StringBuilder();
                                      inParamSql.Append("(");
                                      int item_Index = 0;
                                      foreach (var itemVal in enumParams)
                                      {
                                          string itemParamName = $"{_smartSqlContext.DbPrefix}{dbParamName}_{item_Index}";
                                          inParamSql.AppendFormat("{0},", itemParamName);
                                          AddParameterIfNotExists(dbCommand, itemParamName, itemVal);
                                          item_Index++;
                                      }
                                      inParamSql.Remove(inParamSql.Length - 1, 1);
                                      inParamSql.Append(")");
                                      return inParamSql.ToString();
                                  }
                                  else
                                  {
                                      if (new Regex("\\#{" + dbParamName + "}\\s*\\${wildcardEscapeClause}").IsMatch(context.Sql.ToString()))
                                      {
                                          paramVal = $"%{paramVal}%";
                                      }

                                      AddParameterIfNotExists(dbCommand, dbParamName, paramVal);
                                      return $"{_smartSqlContext.DbPrefix}{dbParamName}";
                                  }
                              });
                        }
                        dbCommand.CommandText = sql;
                        break;
                    }
                case CommandType.StoredProcedure:
                    {
                        if (context.Request is IDataParameterCollection reqParams)
                        {
                            foreach (var reqParam in reqParams)
                            {
                                dbCommand.Parameters.Add(reqParam);
                            }
                        }
                        dbCommand.CommandText = context.SqlId;
                        break;
                    }
            }
            OnPrepared?.Invoke(this, new OnPreparedEventArgs
            {
                RequestContext = context,
                DbSession = dbSession,
                DbCommand = dbCommand
            });
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                StringBuilder dbParameterStr = new StringBuilder();
                foreach (IDbDataParameter dbParameter in dbCommand.Parameters)
                {
                    dbParameterStr.AppendFormat("{0}={1},", dbParameter.ParameterName, dbParameter.Value);
                }
                _logger.LogDebug($"PreparedCommand.Prepare->Statement.Id:[{context.FullSqlId}],Sql:[{dbCommand.CommandText}],Parameters:[{dbParameterStr}]");
            }
            return dbCommand;
        }

        private void AddParameterIfNotExists(IDbCommand dbCommand, string paramName, object paramVal, ITypeHandler typeHandler = null)
        {
            if (!dbCommand.Parameters.Contains(paramName))
            {
                var cmdParameter = dbCommand.CreateParameter();
                cmdParameter.ParameterName = paramName;
                if (paramVal == null)
                {
                    cmdParameter.Value = DBNull.Value;
                }
                else
                {
                    if (typeHandler != null)
                    {
                        typeHandler.SetParameter(cmdParameter, paramVal);
                    }
                    else
                    {
                        if (paramVal is Enum)
                        {
                            paramVal = paramVal.GetHashCode();
                        }
                        cmdParameter.Value = paramVal;
                        if (paramVal != null)
                        {
                            try
                            {
                                var dbtype = cmdParameter.DbType;
                            }
                            catch
                            {
                                cmdParameter.Value = paramVal?.ToString();
                            }
                        }
                    }
                }
                dbCommand.Parameters.Add(cmdParameter);
            }
        }
    }
}
