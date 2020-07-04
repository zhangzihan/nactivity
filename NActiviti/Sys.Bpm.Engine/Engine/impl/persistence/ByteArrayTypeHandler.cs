using Sys.Workflow.Engine.Impl.Persistence.Entity;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace Sys.Workflow.Engine.Impl.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class ByteArrayTypeHandler : ITypeHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public virtual object GetValue(IDataReader dataReader, string columnName, Type targetType)
        {
            int ordinal = dataReader.GetOrdinal(columnName);
            return GetValue(dataReader, ordinal, targetType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="columnIndex"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public virtual object GetValue(IDataReader dataReader, int columnIndex, Type targetType)
        {
            if (dataReader.IsDBNull(columnIndex))
            {
                return null;
            }

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
                    if (offset < byteLen)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataParameter"></param>
        /// <param name="parameterValue"></param>
        public virtual void SetParameter(IDataParameter dataParameter, object parameterValue)
        {
            dataParameter.DbType = DbType.Binary;
            dataParameter.Value = ToParameterValue(parameterValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object ToParameterValue(object value)
        {
            if (value is null)
            {
                return DBNull.Value;
            }

            return value;
        }
    }
}
