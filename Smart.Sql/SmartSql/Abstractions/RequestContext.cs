using SmartSql.Abstractions.TypeHandler;
using SmartSql.Configuration.Statements;
using SmartSql.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using SmartSql.Configuration;
using Sys.Expressions;

namespace SmartSql.Abstractions
{
    /// <summary>
    /// Sql 请求上下文
    /// </summary>
    public class RequestContext
    {
        private IDictionary<string, object> _requestParameters;
        private object _request;

        //public Guid Id { get; } = Guid.NewGuid();
        public SmartSqlContext SmartSqlContext { get; internal set; }
        public DataSourceChoice DataSourceChoice { get; set; } = DataSourceChoice.Write;
        public CommandType CommandType { get; set; } = CommandType.Text;
        public Statement Statement { get; internal set; }
        public StringBuilder Sql { get; internal set; }
        public bool IsStatementSql { get; internal set; }
        public String RealSql { get; set; }
        public String Scope { get; set; }
        public String SqlId { get; set; }
        public String FullSqlId => $"{Scope}.{SqlId}";
        public bool IsFirstDyChild { get; internal set; }
        public IDictionary<string, object> RequestParameters
        {
            get => _requestParameters;
            internal set => _requestParameters = value;
        }
        public JToken Variables { get; set; }

        public object Request
        {
            get => _request; set
            {
                _request = value;
            }
        }

        [Obsolete("Internal call")]
        public void Setup(SmartSqlContext smartSqlContext, ISqlBuilder sqlBuilder)
        {
            SmartSqlContext = smartSqlContext;
            SetupParameters();
            SetupSql(sqlBuilder);
        }
        internal void SetupParameters()
        {
            if (CommandType == CommandType.StoredProcedure || Request == null)
            {
                return;
            }

            bool ignoreParameterCase = SmartSqlContext.IgnoreParameterCase;
            var paramComparer = ignoreParameterCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;

            _requestParameters = new Dictionary<string, object>(paramComparer);

            ToRequestParameters(Request, ignoreParameterCase, ref _requestParameters);
        }

        private void ToRequestParameters(object request, bool ignoreParameterCase, ref IDictionary<string, object> requestParameters)
        {
            var paramComparer = ignoreParameterCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;

            if (request is IDictionary reqDicParams)
            {
                foreach (var key in reqDicParams.Keys)
                {
                    var propName = key.ToString();
                    requestParameters.Add(propName, ToParameterValue(propName, reqDicParams[key]));
                }
                return;
            }
            if (request is IEnumerable<KeyValuePair<string, object>> reqDic)
            {
                foreach (var kv in reqDic)
                {
                    requestParameters.Add(kv.Key, ToParameterValue(kv.Key, kv.Value));
                }
                return;
            }
            if (request is KeyValuePair<string, object> req)
            {
                requestParameters.Add(req.Key, ToParameterValue(req.Key, req.Value));
                return;
            }
            if (request is IEnumerable)
            {
                var @enum = (Request as IEnumerable).GetEnumerator();
                int index = 0;
                while (@enum.MoveNext())
                {
                    IDictionary<string, object> ret = new Dictionary<string, object>(paramComparer);
                    ToRequestParameters(@enum.Current, ignoreParameterCase, ref ret);
                    requestParameters.Add(index.ToString(), ret);
                    index = index + 1;
                }
                return;
            }

            var dict = ObjectUtils.ToDictionary(request, ignoreParameterCase);
            foreach (var key in dict.Keys)
            {
                var val = dict[key];
                requestParameters.Add(key, ToParameterValue(key, val));
            }
        }

        private object ToParameterValue(string paramName, object value)
        {
            var paramMap = Statement?.ParameterMap?.Parameters?.FirstOrDefault(p => p.Name == paramName);
            if (paramMap != null)
            {
                paramMap.Handler.ToParameterValue(value);
            }

            return value;
        }

        internal void SetupSql(ISqlBuilder sqlBuilder)
        {
            sqlBuilder.BuildSql(this);

            string sql = RealSql;
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            if (Variables is JObject vars && new Regex(@"\$", RegexOptions.Multiline).IsMatch(sql))
            {
                foreach (var prop in vars.Properties())
                {
                    var path = prop.Path;
                    var regex = new Regex($@"\$\{{{path}\}}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    if (regex.IsMatch(sql) == false)
                    {
                        continue;
                    }

                    string exp = prop.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(exp))
                    {
                        exp = "";
                    }
                    else
                    {
                        exp = (ExpressionManager.GetValue(Request, exp, RequestParameters) ?? "").ToString();
                    }
                    sql = regex.Replace(sql, exp);
                }

                RealSql = sql;
            }
        }


        public String Key { get { return $"{FullSqlId}:{RequestString}"; } }

        public String RequestString
        {
            get
            {
                if (RequestParameters == null) { return "Null"; }
                StringBuilder strBuilder = new StringBuilder();
                var reqParams = RequestParameters;
                foreach (var reqParam in reqParams)
                {
                    BuildSqlQueryString(strBuilder, reqParam.Key, reqParam.Value);
                }
                return strBuilder.ToString().Trim('&');
            }
        }

        private void BuildSqlQueryString(StringBuilder strBuilder, string key, object val)
        {
            if (val is IEnumerable list && !(val is String))
            {
                strBuilder.AppendFormat("&{0}=(", key);
                foreach (var item in list)
                {
                    strBuilder.AppendFormat("{0},", item);
                }
                strBuilder.Append(")");
            }
            else
            {
                strBuilder.AppendFormat("&{0}={1}", key, val);
            }
        }

        public ITypeHandler GetResultTypeHandler(string property)
        {
            return Statement?.ResultMap?.Results?.FirstOrDefault(r => r.Property == property)?.Handler;
        }
    }
}
