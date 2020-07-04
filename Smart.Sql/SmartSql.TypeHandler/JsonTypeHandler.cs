using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using Newtonsoft.Json;

namespace SmartSql.TypeHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonTypeHandler : ITypeHandler
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
            var jsonStr = dataReader.GetString(columnIndex);
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return null;
            }
            return JsonConvert.DeserializeObject(jsonStr, targetType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataParameter"></param>
        /// <param name="parameterValue"></param>
        public virtual void SetParameter(IDataParameter dataParameter, object parameterValue)
        {
            if (parameterValue is null)
            {
                dataParameter.Value = DBNull.Value;
            }
            else
            {
                dataParameter.Value = ToParameterValue(parameterValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object ToParameterValue(object value)
        {
            var jsonStr = JsonConvert.SerializeObject(value);

            return jsonStr;
        }
    }
}
