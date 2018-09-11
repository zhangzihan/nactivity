using System;
using System.Runtime.Serialization;

namespace SmartSql.Configuration.Statements
{
    [Serializable]
    internal class BuildSqlException : Exception
    {
        public BuildSqlException()
        {
        }

        public BuildSqlException(string message) : base(message)
        {
        }

        public BuildSqlException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BuildSqlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}