using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spring.Core;

namespace Spring.Extensions
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
}
