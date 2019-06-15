using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using SmartSql.Abstractions;
using System.Reflection;
using SmartSql.Exceptions;
using SmartSql.Utils;

namespace SmartSql.Configuration.Tags
{
    public class For : Tag
    {
        public static string FOR_KEY_SUFFIX = "";
        public override TagType Type => TagType.For;
        public string Open { get; set; }
        public string Separator { get; set; }
        public string Close { get; set; }
        public string Key { get; set; }
        public string Index { get; set; }

        public override bool IsCondition(RequestContext context)
        {
            var reqVal = GetPropertyValue(context);
            if (reqVal == null) { return false; }
            if (reqVal is IEnumerable)
            {
                return (reqVal as IEnumerable).GetEnumerator().MoveNext();
            }
            return false;
        }
        public override void BuildChildSql(RequestContext context)
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new SmartSqlException("[For] tag [Key] is required!");
            }
            if (ChildTags.Count == 0)
            {
                throw new SmartSqlException("[For] tag must have childTag!");
            }
            foreach (ITag tag in ChildTags)
            {
                if (tag is SqlText childText)
                {
                    context.Sql.Append(Open);
                    IEnumerator reqVal = (GetPropertyValue(context) as IEnumerable).GetEnumerator();
                    reqVal.MoveNext();
                    bool isDirectValue = IsDirectValue(reqVal.Current);
                    var itemSqlStr = childText.BodyText;
                    if (isDirectValue)
                    {
                        BuildItemSql_DirectValue(itemSqlStr, context);
                    }
                    else
                    {
                        string dbPrefixs = $"{context.SmartSqlContext.DbPrefix}{context.SmartSqlContext.SmartDbPrefix}#";
                        if (new Regex($"([{dbPrefixs}]{{(.*?)}})", RegexOptions.IgnoreCase).IsMatch(itemSqlStr))
                        {
                            BuildItemSql_NotDirectValue(itemSqlStr, context);
                        }
                        else
                        {
                            context.Sql.Append(childText.BodyText);
                        }
                    }
                    context.Sql.Append(Close);
                }
                else if (tag is TrimTag trimSql)
                {
                    BuildTrim_Sql(trimSql, context);
                }
                else
                {
                    throw new SmartSqlException("[For] ChildTag only support SqlText!");
                }
            }
        }

        private string KeyPrepend(string property)
        {
            return string.Join("_", property.Split(new char[] { '.' }));
        }

        private void BuildTrim_Sql(TrimTag trimSql, RequestContext context)
        {
            context.Sql.Append(" ");
            context.Sql.Append(trimSql.Prefix);

            var reqVal = GetPropertyValue(context) as IEnumerable;
            foreach (var itemVal in reqVal)
            {
                if (context.Request is IDictionary<string, object> request)
                {
                    request[Key] = ObjectUtils.ToDictionary(itemVal, true);
                }

                trimSql.BuildChildSql(context);
            }
            if (context.Request is IDictionary<string, object> req)
            {
                req.Remove(Key);
            }

            context.Sql.Append(trimSql.Suffix);
            context.Sql.Append(" ");
        }

        private void BuildItemSql_DirectValue(string itemSqlStr, RequestContext context)
        {
            string dbPrefix = GetDbProviderPrefix(context);
            string dbPrefixs = $"{context.SmartSqlContext.DbPrefix}{context.SmartSqlContext.SmartDbPrefix}#";

            var reqVal = GetPropertyValue(context) as IEnumerable;
            int item_index = 0;
            foreach (var itemVal in reqVal)
            {
                if (item_index > 0)
                {
                    context.Sql.AppendFormat(" {0} ", Separator);
                }
                string patternStr = $"([{dbPrefixs}]{{{Regex.Escape(Key)}}})";
                string key_name = $"{KeyPrepend(Property)}_{Key}{item_index}";
                context.RequestParameters.Add(key_name, itemVal);
                string key_name_dbPrefix = $"{dbPrefix}{key_name}";
                string item_sql = Regex.Replace(itemSqlStr
                                  , patternStr
                                  , key_name_dbPrefix
                                  , RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

                if (!string.IsNullOrWhiteSpace(Index))
                {
                    context.Sql.AppendFormat("{0}", item_sql.Replace("${" + Index + "}", "" + item_index));
                }
                else
                {
                    context.Sql.AppendFormat("{0}", item_sql);
                }
                item_index++;
            }
        }
        private void BuildItemSql_NotDirectValue(string itemSqlStr, RequestContext context)
        {
            string dbPrefix = GetDbProviderPrefix(context);
            string dbPrefixs = $"{context.SmartSqlContext.DbPrefix}{context.SmartSqlContext.SmartDbPrefix}#";
            var reqVal = GetPropertyValue(context) as IEnumerable;
            int item_index = 0;
            foreach (var itemVal in reqVal)
            {
                if (item_index > 0)
                {
                    context.Sql.AppendFormat(" {0} ", Separator);
                }
                string item_sql = itemSqlStr;

                var properties = itemVal.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    string patternStr = $"([{dbPrefixs}]{{{Regex.Escape(property.Name)}}})";
                    bool isHasParam = Regex.IsMatch(item_sql, patternStr, RegexOptions.IgnoreCase);
                    if (!isHasParam) { continue; }

                    var propertyVal = property.GetValue(itemVal);
                    string key_name = $"{KeyPrepend(Property)}_{property.Name}_{Key}{item_index}";
                    //context.RequestParameters.Add(key_name, itemVal);
                    string key_name_dbPrefix = $"{dbPrefix}{key_name}";

                    var paramMap = context.Statement?.ParameterMap?.Parameters?.FirstOrDefault(p => string.Compare(p.Name, property.Name, true) == 0);
                    if (paramMap != null && paramMap.Handler != null)
                    {
                        propertyVal = paramMap.Handler.ToParameterValue(propertyVal);
                    }
                    context.RequestParameters.Add(key_name, propertyVal);

                    item_sql = Regex.Replace(item_sql
                                      , (patternStr)
                                      , key_name_dbPrefix
                                      , RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
                }

                if (!string.IsNullOrWhiteSpace(Index))
                {
                    context.Sql.AppendFormat("{0}", item_sql.Replace("${" + Index + "}", "" + item_index));
                }
                else
                {
                    context.Sql.AppendFormat("{0}", item_sql);
                }
                item_index++;
            }
        }
        private bool IsDirectValue(object obj)
        {
            bool isString = obj is String;
            if (isString) { return true; }
            bool isValueType = obj is ValueType;
            if (isValueType) { return true; }
            return false;
        }
    }
}
