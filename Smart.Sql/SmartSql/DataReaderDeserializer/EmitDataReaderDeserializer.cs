using SmartSql.Abstractions;
using SmartSql.Abstractions.DataReaderDeserializer;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SmartSql.DataReaderDeserializer
{
    public class EmitDataReaderDeserializer : IDataReaderDeserializer
    {
        DataRowParserFactory _dataRowParserFactory;
        public EmitDataReaderDeserializer()
        {
            _dataRowParserFactory = new DataRowParserFactory();
        }

        public IEnumerable<T> ToEnumerable<T>(RequestContext context, IDataReader dataReader)
        {
            try
            {
                Type targetType = typeof(T);
                if (dataReader.Read())
                {
                    var deser = _dataRowParserFactory.GetParser(dataReader, context, targetType);
                    do
                    {
                        object target = deser(dataReader, context);
                        yield return (T)target;
                    } while (dataReader.Read());
                }
            }
            finally
            {
                //while (dataReader.NextResult()) { }
                dataReader.Close();
            }
        }

        public async Task<IEnumerable<T>> ToEnumerableAsync<T>(RequestContext context, IDataReader dataReader)
        {
            var dataReaderAsync = dataReader as DbDataReader;
            try
            {
                IList<T> list = new List<T>();
                Type targetType = typeof(T);

                if (await dataReaderAsync.ReadAsync().ConfigureAwait(false))
                {
                    var deser = _dataRowParserFactory.GetParser(dataReader, context, targetType);
                    do
                    {
                        T target = (T)deser(dataReader, context);
                        list.Add(target);
                    } while (await dataReaderAsync.ReadAsync().ConfigureAwait(false));
                }
                return list;
            }
            finally
            {
                //while (await dataReaderAsync.NextResultAsync()) { }
                dataReader.Close();
            }
        }

        public T ToSingle<T>(RequestContext context, IDataReader dataReader)
        {
            try
            {
                Type targetType = typeof(T);
                if (dataReader.Read())
                {
                    var deser = _dataRowParserFactory.GetParser(dataReader, context, targetType);
                    object target = deser(dataReader, context);
                    if (targetType.IsValueType)
                    {
                        return ConvertValueType<T>(target, targetType);
                    }
                    return (T)target;
                }
                return default(T);
            }
            finally
            {
                //while (dataReader.NextResult()) { }
                dataReader.Close();
            }
        }

        public async Task<T> ToSingleAsync<T>(RequestContext context, IDataReader dataReader)
        {
            var dataReaderAsync = dataReader as DbDataReader;
            try
            {

                Type targetType = typeof(T);
                if (await dataReaderAsync.ReadAsync().ConfigureAwait(false))
                {
                    var deser = _dataRowParserFactory.GetParser(dataReader, context, targetType);
                    object target = deser(dataReader, context);
                    if (targetType.IsValueType)
                    {
                        return ConvertValueType<T>(target, targetType);
                    }
                    return (T)target;
                }
                return default(T);
            }
            finally
            {
                //while (await dataReaderAsync.NextResultAsync()) { }
                dataReader.Close();
            }
        }

        /// <summary>
        /// 值类型单值结果转换：优先直接强转，避免 Newtonsoft JSON 序列化往返开销；
        /// 类型不一致时回退到 Convert.ChangeType（兼容 Nullable&lt;T&gt; 与枚举）。
        /// </summary>
        private static T ConvertValueType<T>(object target, Type targetType)
        {
            // 数据库 NULL：返回默认值（对应可空类型为 null）
            if (target == null || target is DBNull)
            {
                return default;
            }
            // target 实际类型与 T 一致（含装箱值可直接拆箱、Nullable<T> 装箱为其底层类型）时直接强转
            if (target is T typedTarget)
            {
                return typedTarget;
            }
            // 取 Nullable<T> 的底层类型，枚举则按其底层数值类型处理
            Type convertType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (convertType.IsEnum)
            {
                // 兼容数据库返回枚举名称字符串的场景（与原 JSON 路径语义一致）
                if (target is string enumName)
                {
                    return (T)Enum.Parse(convertType, enumName);
                }
                object enumValue = Enum.ToObject(convertType, target);
                return (T)enumValue;
            }
            return (T)Convert.ChangeType(target, convertType);
        }
    }
}
