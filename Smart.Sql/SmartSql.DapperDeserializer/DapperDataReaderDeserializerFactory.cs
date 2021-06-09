using SmartSql.Abstractions.DataReaderDeserializer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SmartSql.DapperDeserializer
{
    public class DapperDataReaderDeserializerFactory : IDataReaderDeserializerFactory
    {
        private IDataReaderDeserializer _dataReaderDeserializer;

        private object syncRoot = new object();

        public IDataReaderDeserializer Create()
        {
            if (_dataReaderDeserializer is null)
            {
                lock (syncRoot)
                {
                    if (_dataReaderDeserializer is null)
                    {
                        _dataReaderDeserializer = new DapperDataReaderDeserializer();
                    }
                }
            }
            return _dataReaderDeserializer;
        }
    }
}
