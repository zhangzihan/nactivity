using System;
using System.Runtime.Serialization;

namespace SmartSql.Configuration.Tags
{
    [Serializable]
    public class PropertyArgumentNullException : Exception
    {
        public PropertyArgumentNullException(string message) : base(message)
        {
        }
    }
}