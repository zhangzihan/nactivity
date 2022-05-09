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

        public IDataReaderDeserializer Create()
        {
            if (_dataReaderDeserializer is null)
            {
                if (_dataReaderDeserializer is null)
                {
                    _dataReaderDeserializer = new EmitDataReaderDeserializer();
                }
            }
            return _dataReaderDeserializer;
        }
    }
}
