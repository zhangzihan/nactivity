using Microsoft.Extensions.Logging;
using SmartSql.Abstractions;
using SmartSql.Abstractions.Command;
using SmartSql.Abstractions.DbSession;
using SmartSql.Abstractions.TypeHandler;
using SmartSql.Configuration.Maps;
using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var regOptions = RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled;
            if (smartSqlContext.IgnoreParameterCase)
            {
                regOptions |= RegexOptions.IgnoreCase;
            }
            _sqlParamsTokens = new Regex(@"[" + dbPrefixs + @"]{?(([\p{L}\p{N}_]+(\[\d+\])*)(\.[\p{L}\p{N}_]+)*)}?", regOptions);
        }

        private static readonly Regex regDbParamName = new(@"(\[(\d+)\])*\.", RegexOptions.Compiled);

        // 判断 #{参数名} 后是否紧跟 ${wildcardEscapeClause} 的静态正则（原实现按参数名逐个 new Regex）
        private static readonly Regex regWildcardEscape = new(@"\#{([\p{L}\p{N}_]+)}\s*\$\{wildcardEscapeClause\}", RegexOptions.Compiled);

        // 参数定义（ParameterMap）属性匹配正则缓存，避免每次解析参数时重复构造
        private static readonly ConcurrentDictionary<string, Regex> paramPropertyRegexCache = new();

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
                            HashSet<string> parameters = new HashSet<string>();
                            sql = _sqlParamsTokens.Replace(sql, match =>
                            {
                                string paramName = match.Groups[1].Value;
                                Parameter paramMap = ParseParameter(context, paramName);
                                string propertyName = paramName;
                                string dbParamName = regDbParamName.Replace(paramName, "$2_");

                                if (context.RequestParameters is null)
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
                                if (typeHandler is object)
                                {
                                    AddParameterIfNotExists(dbCommand, dbParamName, paramVal, ref parameters, typeHandler);
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
                                        AddParameterIfNotExists(dbCommand, itemParamName, itemVal, ref parameters);
                                        item_Index++;
                                    }
                                    inParamSql.Remove(inParamSql.Length - 1, 1);
                                    inParamSql.Append(")");
                                    return inParamSql.ToString();
                                }
                                else
                                {
                                    // 用静态正则捕获参数名后与 dbParamName 比较，语义与原按名构造正则一致（区分大小写）
                                    bool hasWildcardEscape = false;
                                    foreach (Match escapeMatch in regWildcardEscape.Matches(context.Sql.ToString()))
                                    {
                                        if (escapeMatch.Groups[1].Value == dbParamName)
                                        {
                                            hasWildcardEscape = true;
                                            break;
                                        }
                                    }
                                    if (hasWildcardEscape)
                                    {
                                        paramVal = $"%{paramVal}%";
                                    }

                                    AddParameterIfNotExists(dbCommand, dbParamName, paramVal, ref parameters);
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

        private static Parameter ParseParameter(RequestContext context, string paramName)
        {
            if (context.Statement is null)
            {
                return null;
            }

            ParameterMap paramMap = context.Statement.ParameterMap;
            if (paramMap is null)
            {
                return null;
            }

            IList<Parameter> parameters = paramMap.Parameters;
            if (parameters is object)
            {
                return parameters.FirstOrDefault(p =>
                {
                    // 属性正则按配置键缓存复用，匹配行为与原实现一致（忽略大小写）
                    var propertyRegex = paramPropertyRegexCache.GetOrAdd(p.Property,
                        property => new Regex(property, RegexOptions.IgnoreCase | RegexOptions.Compiled));
                    return propertyRegex.IsMatch(paramName);
                });
            }

            return null;
        }

        private void AddParameterIfNotExists(IDbCommand dbCommand, string paramName, object paramVal, ref HashSet<string> parameters, ITypeHandler typeHandler = null)
        {
            if (parameters.Contains(paramName) == false)
            {
                IDbDataParameter cmdParameter = dbCommand.CreateParameter();
                cmdParameter.ParameterName = paramName;
                if (typeHandler is object)
                {
                    typeHandler.SetParameter(cmdParameter, paramVal);
                }
                else if (paramVal is null)
                {
                    cmdParameter.Value = DBNull.Value;
                }
                else
                {
                    //if (paramVal is Enum)
                    //{
                    //    paramVal = paramVal.GetHashCode();
                    //}
                    cmdParameter.Value = paramVal;
                    try
                    {
                        var dbtype = cmdParameter.DbType;
                    }
                    catch
                    {
                        cmdParameter.Value = paramVal?.ToString();
                    }
                }
                dbCommand.Parameters.Add(cmdParameter);
                parameters.Add(paramName);
            }
        }
    }
}
