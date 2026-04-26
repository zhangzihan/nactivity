using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Model
{
    public static class BpmModelServiceProvider
    {
        public static IServiceCollection AddBpmModelServiceProvider(this IServiceCollection services)
        {
            // 使用 IHostedService 延迟初始化，避免 BuildServiceProvider() 创建临时容器
            services.AddHostedService<BpmnModelLoggerInitializer>();
            return services;
        }
    }

    /// <summary>
    /// 在容器构建完成后通过 IHostedService 初始化日志工厂
    /// </summary>
    internal class BpmnModelLoggerInitializer : IHostedService
    {
        public BpmnModelLoggerInitializer(ILoggerFactory loggerFactory)
        {
            BpmnModelLoggerFactory.factory = loggerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    internal class BpmnModelLoggerFactory
    {
        internal static ILoggerFactory factory;

        public static ILogger LoggerService<T>()
        {
            return (factory ?? NullLoggerFactory.Instance).CreateLogger<T>();
        }
    }
}
