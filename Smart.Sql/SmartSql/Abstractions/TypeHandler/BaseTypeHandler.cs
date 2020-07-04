using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SmartSql.Abstractions.TypeHandler
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseTypeHandler : ITypeHandler
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
            return dataReader.GetValue(columnIndex);
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
        public virtual object ToParameterValue(object value)
        {
            if (value is null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }
    }
}
