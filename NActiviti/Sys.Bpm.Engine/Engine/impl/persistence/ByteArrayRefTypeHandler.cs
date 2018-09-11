using org.activiti.engine.impl.persistence.entity;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace org.activiti.engine.impl.persistence
{
    public class ByteArrayRefTypeHandler : ITypeHandler
    {
        public virtual object GetValue(IDataReader dataReader, string columnName, Type targetType)
        {
            int ordinal = dataReader.GetOrdinal(columnName);
            return GetValue(dataReader, ordinal, targetType);
        }

        public virtual object GetValue(IDataReader dataReader, int columnIndex, Type targetType)
        {
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[8040];
                long offset = 0;
                long read;
                while ((read = dataReader.GetBytes(columnIndex, offset, buffer, 0, buffer.Length)) > 0)
                {
                    offset += read;
                    ms.Write(buffer, 0, (int)read); // push downstream
                }

                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public virtual void SetParameter(IDataParameter dataParameter, object parameterValue)
        {
            dataParameter.Value = ToParameterValue(parameterValue);
        }

        public object ToParameterValue(object value)
        {
            if (value == null || !(value is ByteArrayRef))
            {
                return DBNull.Value;
            }

            var val = value as ByteArrayRef;
            if (string.IsNullOrWhiteSpace(val.Id))
            {
                return DBNull.Value;
            }

            return val.Id;
        }
    }

    public interface ITypeHandler<T>
    {
        T getResult(DbDataReader rs, String columnName);

        T getResult(DbDataReader rs, int columnIndex);
    }

    public class TypeReference<T>
    {
        private Type rawType;

        protected TypeReference()
        {
            rawType = typeof(T);
        }

        public Type RawType => rawType;

        Type getSuperclassTypeParameter()
        {
            //            Type clazz = typeof(T);
            //            Type genericSuperclass = clazz.GetGenericTypeDefinition();//.getGenericSuperclass();
            //            if (genericSuperclass is T) {
            //                // try to climb up the hierarchy until meet something useful
            //                if (TypeReference.class != genericSuperclass) {
            //                return getSuperclassTypeParameter(clazz.getSuperclass());
            //    }

            //              throw new TypeException("'" + getClass() + "' extends TypeReference but misses the type parameter. "
            //                + "Remove the extension or add a type parameter to it.");
            //}

            //Type rawType = ((ParameterizedType)genericSuperclass).getActualTypeArguments()[0];
            //            // TODO remove this when Reflector is fixed to return Types
            //            if (rawType instanceof ParameterizedType) {
            //              rawType = ((ParameterizedType) rawType).getRawType();
            //            }

            return rawType;
        }

        public override string ToString()
        {
            return rawType.ToString();
        }
    }
}
