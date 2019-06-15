using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcessConfigServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder Configure(this IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            builder.Services.Configure<ProcessEngineOption>(configuration);

            builder.Services.Configure<DataSourceOption>(setupAction);

            builder.Services.AddSingleton<ExternalConnectorProvider>(sp =>
            {
                return new ExternalConnectorProvider
                {
                    Configuration = configuration
                };
            });

            return builder;
        }
    }
}
