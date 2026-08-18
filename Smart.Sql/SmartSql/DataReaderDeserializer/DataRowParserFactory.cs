using SmartSql.Abstractions.TypeHandler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using SmartSql.Abstractions;
using SmartSql.Utils;
using System.Globalization;
using System.Collections.Concurrent;

namespace SmartSql.DataReaderDeserializer
{
    public class DataRowParserFactory
    {
        private readonly ConcurrentDictionary<string, Func<IDataReader, RequestContext, object>> _cachedDeserializer = new();

        public DataRowParserFactory()
        {
        }

        public Func<IDataReader, RequestContext, object> GetParser(IDataReader dataReader, RequestContext requestContext, Type targetType)
        {
            bool isValueType = targetType.IsValueType || targetType == TypeUtils.StringType;

            // 值类型解析器只读取第 0 列，与结果列集无关，键无需包含列集；
            // 实体解析器按运行时列集（列名/顺序/字段类型）生成 IL，若同一语句的不同动态分支
            // 产生不同列集，必须把列集签名纳入缓存键，否则会命中为其它列集生成的解析器导致列错位。
            string key = isValueType
                ? $"{requestContext.FullSqlId}_{targetType.GUID:N}"
                : $"{requestContext.FullSqlId}_{targetType.GUID:N}_{BuildColumnSignature(dataReader)}";
            if (_cachedDeserializer.TryGetValue(key, out var deser) == false)
            {
                if (isValueType)
                {
                    deser = CreateValueTypeParserImpl(dataReader, requestContext, targetType);
                }
                else
                {
                    deser = CreateParserImpl(dataReader, requestContext, targetType);
                }
                _cachedDeserializer.AddOrUpdate(key, deser, (k, old) => deser);
            }
            return deser;
        }

        /// <summary>
        /// 构造结果列集签名（列名 + 字段类型，按列顺序拼接），用于区分同一语句不同动态分支的不同列集
        /// </summary>
        private static string BuildColumnSignature(IDataReader dataReader)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (i > 0)
                {
                    sb.Append('|');
                }
                sb.Append(dataReader.GetName(i)).Append(':').Append(dataReader.GetFieldType(i).Name);
            }
            return sb.ToString();
        }

        private MethodInfo GetRealValueMethod(Type valueType)
        {
            var typeCode = Type.GetTypeCode(valueType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetBoolean");
                    }
                case TypeCode.Byte:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetByte");
                    }
                case TypeCode.Char:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetChar");
                    }
                case TypeCode.DateTime:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetDateTime");
                    }
                case TypeCode.Decimal:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetDecimal");
                    }
                case TypeCode.Double:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetDouble");
                    }
                case TypeCode.Single:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetFloat");
                    }
                case TypeCode.Int16:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetInt16");
                    }
                case TypeCode.Int32:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetInt32");
                    }
                case TypeCode.Int64:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetInt64");
                    }
                case TypeCode.String:
                    {
                        return TypeUtils.DataRecordType.GetMethod("GetString");
                    }
                default:
                    {
                        if (valueType == typeof(Guid))
                        {
                            return TypeUtils.DataRecordType.GetMethod("GetGuid");
                        }
                        return null;
                    }
            }
        }

        private Func<IDataReader, RequestContext, object> CreateValueTypeParserImpl(IDataReader dataReader, RequestContext context, Type targetType)
        {
            var nullUnderType = Nullable.GetUnderlyingType(targetType);
            var realType = nullUnderType ?? targetType;
            int valIndex = 0;
            if (realType.IsEnum)
            {
                return (dr, ctx) =>
                {
                    var val = dr.GetValue(valIndex);
                    if (val is float || val is double || val is decimal)
                    {
                        val = Convert.ChangeType(val, Enum.GetUnderlyingType(realType), CultureInfo.InvariantCulture);
                    }
                    return val is DBNull ? null : Enum.ToObject(realType, val);
                };
            }
            return (dr, ctx) =>
            {
                var val = dr.GetValue(valIndex);
                return val is DBNull ? null : val;
            };
        }

        private Func<IDataReader, RequestContext, object> CreateParserImpl(IDataReader dataReader, RequestContext context, Type targetType)
        {
            var dynamicMethod = new DynamicMethod(
                "Deserialize" + Guid.NewGuid().ToString("N"),
                targetType,
                new[] { TypeUtils.DataReaderType, TypeUtils.RequestContextType },
                targetType,
                true);
            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.DeclareLocal(targetType);

            var targetCtor = targetType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            iLGenerator.Emit(OpCodes.Newobj, targetCtor); // [target]
            iLGenerator.Emit(OpCodes.Stloc_0);

            var columnNames = Enumerable.Range(0, dataReader.FieldCount).Select(i => dataReader.GetName(i)).ToArray();

            int colIndex = -1;
            var properties = targetType.GetProperties();
            foreach (var colName in columnNames)
            {
                colIndex++;
                var result = context.Statement?.ResultMap?.Results?.FirstOrDefault(r => string.Compare(r.Column, colName, true) == 0);
                bool hasTypeHandler = result?.Handler is object;
                string propertyName = result is not null ? result.Property : colName;
                var property = properties.FirstOrDefault(x => string.Compare(x.Name, propertyName, true) == 0);

                if (property is null) { continue; }
                if (!property.CanWrite) { continue; }

                var fieldType = dataReader.GetFieldType(colIndex);
                Type propertyType = property.PropertyType;

                Label isDbNullLabel = iLGenerator.DefineLabel();

                if (hasTypeHandler)
                {
                    iLGenerator.Emit(OpCodes.Ldloc_0);// [target]
                    EmitGetTypeHanderValue(iLGenerator, colIndex, propertyName, propertyType);
                    if (propertyType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Unbox_Any, propertyType);
                    }
                }
                else
                {
                    var nullUnderType = Nullable.GetUnderlyingType(propertyType);
                    var realType = nullUnderType ?? propertyType;

                    if (nullUnderType is not null || realType == TypeUtils.StringType)
                    {
                        iLGenerator.Emit(OpCodes.Ldarg_0);// [dataReader]
                        EmitUtils.LoadInt32(iLGenerator, colIndex);// [dataReader][index]
                        iLGenerator.Emit(OpCodes.Callvirt, TypeUtils.IsDBNullMethod);//[isDbNull-value]
                        iLGenerator.Emit(OpCodes.Brtrue, isDbNullLabel);//[empty]
                    }

                    var getRealValueMethod = GetRealValueMethod(fieldType);

                    iLGenerator.Emit(OpCodes.Ldloc_0);// [target]

                    if (realType.IsEnum)
                    {
                        EmitUtils.TypeOf(iLGenerator, realType);//[target][enumType]
                    }
                    #region GetValue
                    iLGenerator.Emit(OpCodes.Ldarg_0);// [dataReader]
                    EmitUtils.LoadInt32(iLGenerator, colIndex);// [dataReader][index]
                    if (getRealValueMethod is not null)
                    {
                        iLGenerator.Emit(OpCodes.Callvirt, getRealValueMethod);//[prop-value]
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Callvirt, TypeUtils.GetValueMethod_DataRecord);//[prop-value]
                        if (fieldType.IsValueType)
                        {
                            iLGenerator.Emit(OpCodes.Unbox_Any, fieldType);
                        }
                    }
                    #endregion
                    //[target][property-Value]
                    if (realType.IsEnum)
                    {
                        //[target][propertyType-enumType][property-Value]
                        EmitConvertEnum(iLGenerator, fieldType);
                        iLGenerator.Emit(OpCodes.Unbox_Any, realType);
                        //[target][property-Value(enum-value)]
                    }
                    else if (fieldType != realType)
                    {
                        EmitUtils.ChangeType(iLGenerator, fieldType, realType);
                    }
                    if (nullUnderType is not null)
                    {
                        iLGenerator.Emit(OpCodes.Newobj, propertyType.GetConstructor(new[] { nullUnderType }));
                    }
                }

                iLGenerator.Emit(OpCodes.Call, property.SetMethod);// stack is empty
                iLGenerator.MarkLabel(isDbNullLabel);
            }

            iLGenerator.Emit(OpCodes.Ldloc_0);// stack is [rval]
            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(Func<IDataReader, RequestContext, object>)) as Func<IDataReader, RequestContext, object>;
        }

        private static readonly MethodInfo _getResultTypeHandlerMethod = TypeUtils.RequestContextType.GetMethod("GetResultTypeHandler");
        private void EmitGetTypeHanderValue(ILGenerator iLGenerator, int colIndex, string propertyName, Type propertyType)
        {
            iLGenerator.Emit(OpCodes.Ldarg_1);// [RequestContext]
            iLGenerator.Emit(OpCodes.Ldstr, propertyName);// [RequestContext][propertyName]
            iLGenerator.Emit(OpCodes.Callvirt, _getResultTypeHandlerMethod);//[ITypeHandler]
            iLGenerator.Emit(OpCodes.Ldarg_0);//[typeHandler][dataReader]
            EmitUtils.LoadInt32(iLGenerator, colIndex);// [typeHandler][dataReader][index]
            EmitUtils.TypeOf(iLGenerator, propertyType);//[typeHandler][dataReader][index][propertyType]
            iLGenerator.Emit(OpCodes.Callvirt, TypeUtils.GetValueMethod_TypeHandler);// [property-Value]
        }

        private void EmitConvertEnum(ILGenerator iLGenerator, Type valueType)
        {
            MethodInfo enumConvertMethod;
            if (valueType == TypeUtils.StringType)
            {
                enumConvertMethod = typeof(Enum).GetMethod("Parse", new Type[] { TypeUtils.TypeType, TypeUtils.StringType });
            }
            else
            {
                enumConvertMethod = typeof(Enum).GetMethod("ToObject", new Type[] { TypeUtils.TypeType, valueType });
            }
            iLGenerator.Emit(OpCodes.Callvirt, enumConvertMethod);
        }
    }

}
