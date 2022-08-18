using Sys.Workflow.Engine.Impl.Persistence.Entity;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using System.IO;

namespace Sys.Workflow.Engine.Impl.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class ByteArrayRefTypeHandler : ITypeHandler
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

        private ByteArrayRef Initialized(IDataReader dataReader, int columnIndex)
        {
            string id = dataReader.GetValue(columnIndex)?.ToString();

            if (!string.IsNullOrWhiteSpace(id))
            {
                ByteArrayRef byteArrayRef = new ByteArrayRef(id);

                return byteArrayRef;
            }

            return null;
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
            return Initialized(dataReader, columnIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataParameter"></param>
        /// <param name="parameterValue"></param>
        public virtual void SetParameter(IDataParameter dataParameter, object parameterValue)
        {
            dataParameter.Value = ToParameterValue(parameterValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object ToParameterValue(object value)
        {
            if (value is null || value is not ByteArrayRef)
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
}
