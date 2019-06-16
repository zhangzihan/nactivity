using Sys.Workflow.engine.impl.persistence.entity;
using SmartSql.Abstractions.TypeHandler;
using System;
using System.Data;
using System.IO;

namespace Sys.Workflow.engine.impl.persistence
{
    public class ByteArrayRefTypeHandler : ITypeHandler
    {
        public virtual object GetValue(IDataReader dataReader, string columnName, Type targetType)
        {
            int ordinal = dataReader.GetOrdinal(columnName);
            return GetValue(dataReader, ordinal, targetType);
        }

        private ByteArrayRef initialized(IDataReader dataReader, int columnIndex)
        {
            string id = dataReader.GetValue(columnIndex)?.ToString();

            if (!string.IsNullOrWhiteSpace(id))
            {
                ByteArrayRef byteArrayRef = new ByteArrayRef(id);

                return byteArrayRef;
            }

            return null;
        }

        public virtual object GetValue(IDataReader dataReader, int columnIndex, Type targetType)
        {
            return initialized(dataReader, columnIndex);
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
}
