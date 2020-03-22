using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Sys.Workflow
{
    public static class ProcessEngineServiceProvider
    {
        internal static IServiceProvider ServiceProvider { get; set; }

        public static T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        public static ILogger<T> LoggerService<T>()
        {
            try
            {
                ILoggerFactory logFac = ServiceProvider.GetService<ILoggerFactory>();
                if (logFac != null)
                {
                    return logFac.CreateLogger<T>();
                }

                return new NullLogger<T>();
            }
            catch
            {
                return new NullLogger<T>();
            }
        }
    }
}
