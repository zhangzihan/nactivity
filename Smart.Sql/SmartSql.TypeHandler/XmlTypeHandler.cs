using SmartSql.Abstractions.TypeHandler;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SmartSql.TypeHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlTypeHandler : ITypeHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object GetValue(IDataReader dataReader, string columnName, Type targetType)
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
        public object GetValue(IDataReader dataReader, int columnIndex, Type targetType)
        {
            var xmlStr = dataReader.GetString(columnIndex);
            if (string.IsNullOrWhiteSpace(xmlStr))
            {
                return null;
            }
            return XmlSerializeUtil.Deserialize(xmlStr, targetType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataParameter"></param>
        /// <param name="parameterValue"></param>
        public void SetParameter(IDataParameter dataParameter, object parameterValue)
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
            return XmlSerializeUtil.Serializer(value);
        }
    }
}
