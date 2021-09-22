using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Options;
using System;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public static class WorkflowApplicationBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseProcessEngine(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            ProcessEngineServiceProvider.ServiceProvider = app.ApplicationServices;

            ProcessEngineConfiguration processEngineConfig =
                app.ApplicationServices.GetService<ProcessEngineConfiguration>();
            processEngineConfig.BuildProcessEngine();

            ProcessEngineFactory.InitProcessEngineFromResource();

            app.ApplicationServices.EnsureProcessEngineInit();

            app.ApplicationServices.GetService<IOptionsMonitor<ProcessEngineOption>>()
                .OnChange((opts, prop) =>
                {
                    if (ProcessEngineOption.HasChanged())
                    {
                        ProcessEngineFactory.Destroy();
                        app.ApplicationServices.EnsureProcessEngineInit();
                    }
                });

            ApplicationStopping(app, lifetime);

            return app;
        }

        private static void ApplicationStopping(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStopping.Register((context) =>
            {
                IApplicationBuilder builder = context as IApplicationBuilder;
                ProcessEngineFactory.Destroy();
            }, app, true);
        }

        private static void EnsureProcessEngineInit(this IServiceProvider serviceProvider)
        {
            var engine = serviceProvider.GetService<IProcessEngine>();
            if (engine is null)
            {
                throw new InitProcessEngineFaliedException();
            }
        }
    }
}
