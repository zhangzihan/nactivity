using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine;
using System;

namespace Sys.Workflow
{
    public class InProcessWorkflowEngine
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public string TenantId { get; set; } = "9c1482c3-3b7d-2255-b0e8-a5b309142354";

        public class TestUser : IUserInfo
        {
            public string Id { get; set; }
            public string LoginUserId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string TenantId { get; set; }
        }

        public void Create()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(root)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("resources\\activiti.cfg.json")
                .Build();

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(Configuration)
                .AddHttpContextAccessor()
                .AddLogging(builder =>
                {
                });

            services.AddProcessEngineWithRules(Configuration);
            services.AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(ServiceProvider);

            var lifetime = new ApplicationLifetime(ServiceProvider.GetService<ILoggerFactory>().CreateLogger<ApplicationLifetime>());
            
            app.UseProcessEngine(lifetime);
        }

        public T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }
    }
}
