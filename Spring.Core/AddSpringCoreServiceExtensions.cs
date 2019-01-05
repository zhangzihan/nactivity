using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spring.Core
{
    public static class AddSpringCoreServiceExtensions
    {
        public static IServiceCollection AddSpringCoreService(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            LogManager.Instance = new LogManager(loggerFactory);
            services.AddSingleton<LogManager>(LogManager.Instance);

            return services;
        }
    }

    public class LogManager
    {
        private readonly ILoggerFactory loggerFactory;

        public LogManager(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public static LogManager Instance
        {
            get;
            set;
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
