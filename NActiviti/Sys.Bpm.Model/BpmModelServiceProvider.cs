using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Bpm
{
    public static class BpmModelServiceProvider
    {
        public static IServiceCollection AddBpmModelServiceProvider(this IServiceCollection services)
        {
            BpmnModelLoggerFactory.factory = services.BuildServiceProvider().GetService<ILoggerFactory>();

            return services;
        }
    }

    internal class BpmnModelLoggerFactory
    {
        internal static ILoggerFactory factory;

        public static ILogger LoggerService<T>()
        {
            return factory.CreateLogger<T>();
        }
    }
}
