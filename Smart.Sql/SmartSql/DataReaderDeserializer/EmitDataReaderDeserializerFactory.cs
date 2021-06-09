using SmartSql.Abstractions.DataReaderDeserializer;
using SmartSql.Abstractions.TypeHandler;

namespace SmartSql.DataReaderDeserializer
{
    public class EmitDataReaderDeserializerFactory : IDataReaderDeserializerFactory
    {
        private IDataReaderDeserializer _dataReaderDeserializer;
        public EmitDataReaderDeserializerFactory()
        {

        }

        private readonly object syncRoot = new object();

        public IDataReaderDeserializer Create()
        {
            if (_dataReaderDeserializer is null)
            {
                lock (syncRoot)
                {
                    if (_dataReaderDeserializer is null)
                    {
                        _dataReaderDeserializer = new EmitDataReaderDeserializer();
                    }
                }
            }
            return _dataReaderDeserializer;
        }
    }
}
