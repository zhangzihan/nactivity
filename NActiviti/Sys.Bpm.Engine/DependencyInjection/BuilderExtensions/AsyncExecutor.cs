using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.engine.impl.asyncexecutor;

namespace Sys.Workflow
{
    /// <summary>
    /// 注入异步Timer执行器
    /// </summary>
    public static class ProcessEngineBuilderExtensionsAsyncExecutor
    {
        /// <summary>
        /// 注入异步Timer执行器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddAsyncExecutor(this IProcessEngineBuilder builder)
        {
            builder.Services.AddTransient<IAsyncExecutor>(sp =>
            {
                IConfigurationSection dajw = sp.GetService<IConfiguration>().GetSection("defaultAsyncJobAcquireWaitTimeInMillis");
                IConfigurationSection dtjw = sp.GetService<IConfiguration>().GetSection("defaultTimerJobAcquireWaitTimeInMillis");

                if (int.TryParse(dajw?.Value, out int iDajw) == false)
                {
                    iDajw = 10000;
                }
                if (int.TryParse(dtjw?.Value, out int iDtjw) == false)
                {
                    iDtjw = 10000;
                }

                return new DefaultAsyncJobExecutor()
                {
                    DefaultAsyncJobAcquireWaitTimeInMillis = iDajw,
                    DefaultTimerJobAcquireWaitTimeInMillis = iDtjw,
                };
            });

            return builder;
        }
    }
}
