using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Logging
{
    public class NoneLoggerFactory : ILoggerFactory
    {
        public static readonly NoneLoggerFactory Instance = new NoneLoggerFactory();

        public ILogger CreateLogger(string name)
        {
            return NoneLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        internal ILogger GetLogger(Type type)
        {
            return Instance.CreateLogger(type);
        }

        internal ILogger GetLogger<T>()
        {
            throw new NotImplementedException();
        }
    }
}
