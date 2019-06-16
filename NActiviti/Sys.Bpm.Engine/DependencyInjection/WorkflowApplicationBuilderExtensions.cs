using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sys.Workflow.engine;
using Sys.Workflow.Options;
using System;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public static class WorkflowApplicationBuilderExtensions
    {
        public static IProcessEngine UseProcessEngine(this IServiceProvider serviceProvider, IApplicationLifetime applicationLifetime, string processEngineName)
        {
            ProcessEngineServiceProvider.ServiceProvider = serviceProvider;

            if (string.IsNullOrWhiteSpace(processEngineName))
            {
                return serviceProvider.GetService<IProcessEngine>();
            }

            ProcessEngineConfiguration processEngineConfig = serviceProvider.GetService<ProcessEngineConfiguration>();
            processEngineConfig.ProcessEngineName = processEngineName;

            IProcessEngine engine = processEngineConfig.BuildProcessEngine();
            ProcessEngineFactory.RegisterProcessEngine(engine);

            return engine;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseProcessEngine(this IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            app.ApplicationServices.EnsureProcessEngineInit();

            app.ApplicationServices.GetService<IOptionsMonitor<ProcessEngineOption>>()
                .OnChange((opts, prop) =>
                {
                    if (ProcessEngineOption.HasChanged())
                    {
                        ProcessEngineFactory.Instance.Destroy();
                        app.ApplicationServices.EnsureProcessEngineInit();
                    }
                });

            lifetime.ApplicationStopping.Register((context) =>
            {
                IApplicationBuilder builder = context as IApplicationBuilder;
                ProcessEngineFactory engineFact = builder.ApplicationServices.GetService<ProcessEngineFactory>();
                engineFact.Destroy();
            }, app, true);
            return app;
        }

        private static void EnsureProcessEngineInit(this IServiceProvider serviceProvider)
        {
            ProcessEngineServiceProvider.ServiceProvider = serviceProvider;

            var engine = serviceProvider.GetService<IProcessEngine>();
            if (engine == null)
            {
                throw new InitProcessEngineFaliedException();
            }
        }
    }
}
