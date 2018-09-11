using System;
using System.Runtime.Serialization;

namespace SmartSql
{
    [Serializable]
    internal class ExcuteSQLException : Exception
    {
        public ExcuteSQLException()
        {
        }

        public ExcuteSQLException(string message) : base(message)
        {
        }

        public ExcuteSQLException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExcuteSQLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}