using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Bpm.Rest.Client;
using Sys.Bpm.Services.Rest;
using System;
using System.IO;
using System.Linq;

namespace Sys.Bpmn.Test
{
    /// <summary>
    /// 单元测试上下文
    /// </summary>
    public class UnitTestContext
    {
        public IConfiguration Configuration { get; set; }

        public HttpContext HttpContext { get; private set; }

        public TestServer TestServer { get; private set; }

        public UnitTestContext()
        {

        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType)
        {
            return TestServer.Host.Services.GetService(serviceType);
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return TestServer.Host.Services.GetService<T>();
        }

        public string TenantId => "13ddb8dd-99e3-477c-8316-7928e938ce80";

        public string AuthUserId => "验证用户";

        private static UnitTestContext unitTestContext;

        public static UnitTestContext Instance
        {
            get
            {
                return unitTestContext;
            }
        }

        private static object syncRoot = new object();

        private readonly object syncClient = new object();

        /// <summary>
        /// 创建测试环境上下文
        /// </summary>
        /// <returns></returns>
        public static UnitTestContext CreateDefaultUnitTestContext()
        {
            lock (syncRoot)
            {
                if (unitTestContext == null)
                {
                    unitTestContext = new UnitTestContext();

                    unitTestContext.HttpContext = new DefaultHttpContext();

                    string root = Path.GetDirectoryName(new Uri(typeof(UnitTestContext).Assembly.CodeBase).LocalPath);

                    unitTestContext.Configuration = new ConfigurationBuilder()
                        .SetBasePath(root)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("resources\\activiti.cfg.json")
                        .Build();

                    IWebHostBuilder webHost = new WebHostBuilder()
                        .UseConfiguration(unitTestContext.Configuration)
                        .UseStartup<UnitTestStartup>();

                    unitTestContext.TestServer = new TestServer(webHost);
                }

                return unitTestContext;
            }
        }
    }

    public class UnitTestStartup
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        public UnitTestStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration)
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddLogging()
                .AddProcessEngine();

            services.UseWorkflowClient();

            ServiceDescriptor invokeSrv = services.FirstOrDefault(x => x.ServiceType == typeof(HttpInvoker));
            services.Remove(invokeSrv);

            services.AddSingleton<HttpInvoker>(sp =>
            {
                return new HttpInvoker(UnitTestContext.Instance.TestServer.CreateClient());
            });

            IMvcBuilder mvcBuilder = services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            ServiceProvider = services.BuildServiceProvider();

            return ServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(cors =>
                cors.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

            app.UseMvc();
        }
    }
}
