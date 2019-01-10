using org.activiti.engine.impl.persistence.entity;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace org.activiti.engine.impl.persistence
{
    public class ByteArrayTypeHandler : ITypeHandler
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
                long byteLen = dataReader.GetBytes(columnIndex, 0, null, 0, 0);
                long read = dataReader.GetBytes(columnIndex, offset, buffer, 0, buffer.Length);
                while (read > 0)
                {
                    offset += read;
                    ms.Write(buffer, 0, (int)read); // push downstream
                    if (read < byteLen)
                    {
                        read = dataReader.GetBytes(columnIndex, offset, buffer, 0, buffer.Length);
                    }
                    else
                    {
                        break;
                    }
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
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }
    }
}
