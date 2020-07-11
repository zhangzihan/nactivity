using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineServiceProvider
    {
        /// <summary>
        /// 
        /// </summary>
        internal static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
