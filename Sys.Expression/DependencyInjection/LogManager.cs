using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Spring.Core
{
    class LogManager
    {
        private readonly ILoggerFactory loggerFactory;

        public LogManager(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        private static LogManager instance;
        public static LogManager Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new LogManager(NullLoggerFactory.Instance);
                }

                return instance;
            }
            internal set => instance = value;
        }

        public static ILogger GetLogger(Type type)
        {
            return Instance.PrivateGetLogger(type);
        }

        public static ILogger<T> GetLogger<T>()
        {
            return Instance.PrivateGetLogger<T>();
        }

        private ILogger PrivateGetLogger(Type type)
        {
            return loggerFactory.CreateLogger(type);
        }

        private ILogger<T> PrivateGetLogger<T>()
        {
            return loggerFactory.CreateLogger<T>();
        }
    }
}
