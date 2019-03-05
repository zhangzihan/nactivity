using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.engine;
using org.activiti.engine.repository;
using Sys.Bpm.rest.api;
using Sys.Bpm.rest.controllers;
using Sys.Bpm.Services.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        /// 读取bpmn文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ReadBpmn(string name)
        {
            string root = Path.GetDirectoryName(new Uri(typeof(UnitTestContext).Assembly.CodeBase).LocalPath);

            return File.ReadAllText(Path.Combine(root, "resources\\samples", name));
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

        private static object syncRoot = new object();

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

                    unitTestContext.TestServer = new TestServer(new WebHostBuilder()
                        .UseConfiguration(unitTestContext.Configuration)
                        .UseStartup<UnitTestStartup>());
                }

                return unitTestContext;
            }
        }

        public Deployment DeployTestProcess()
        {
            IProcessDefinitionDeployerController pddc = new ProcessDefinitionDeployerController(Resolve<IProcessEngine>(),
                Resolve<DeploymentConverter>(),
                Resolve<PageableDeploymentRespositoryService>(),
                null);

            ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

            pdd.BpmnXML = ReadBpmn("简单顺序流.bpmn");
            pdd.Name = Path.GetFileNameWithoutExtension("简单顺序流");
            pdd.TenantId = TenantId;
            pdd.EnableDuplicateFiltering = true;
            pdd.StartForm = "1";

            return pddc.Deploy(pdd).Result;
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

            IMvcBuilder mvcBuilder = services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
