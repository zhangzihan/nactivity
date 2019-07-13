using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spring.Core.TypeResolution;
using Spring.Extensions;
using Sys.Extentions;
using Sys.Net.Http;
using Sys.Workflow.Caches;
using Sys.Workflow.Engine.Bpmn.Rules;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.DB;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Options;
using Sys.Workflow.Util;
using System;
using System.Net.Http;
using Polly;
using System.Net;
using Polly.Extensions.Http;

namespace Sys.Workflow
{
    /// <summary>
    /// 流程服务依赖注入
    /// </summary>
    public static class ProcessEngineServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngine(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddProcessEngine(configuration, (option) =>
            {
                IConfiguration config = configuration.GetSection("WorkflowDataSource");

                option.ProviderName = config.GetValue<string>(nameof(option.ProviderName));
                option.Database = config.GetValue<string>(nameof(option.Database));
                option.ConnectionString = config.GetValue<string>(nameof(option.ConnectionString));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngine(this IServiceCollection services, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            IProcessEngineBuilder builder = new ProcessEngineBuilder(services);

            return builder.AddProcessEngine(configuration, setupAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngine(this IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            ConfigUtil.Configuration = configuration;

            TypeRegistry.RegisterType(typeof(TimerUtil));
            TypeRegistry.RegisterType(typeof(ExternalConnector));
            builder.Services.AddSpringCoreTypeRepository();

            builder.Services.AddSpringCoreService(builder.Services.BuildServiceProvider().GetService<ILoggerFactory>());

            builder.Services.AddSingleton<MemoryCacheProvider>();

            DbSqlSessionVersion.InitVersion(configuration);

            builder.Configure(configuration, setupAction);

            builder.Services.AddHttpClient("ExternalHttpClient")
                .AddPolicyHandler(PollyPolicy.GetRetryPolicy())
                .AddPolicyHandler(PollyPolicy.GetCircuitBreakerPolicy())
                .ConfigureHttpMessageHandlerBuilder(cb =>
                {
                    if (cb.PrimaryHandler is HttpClientHandler handler)
                    {
                        handler.MaxRequestContentBufferSize = int.MaxValue;
                        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                        handler.UseDefaultCredentials = true;
                        handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
                    }

                    //HttpClientHandler handler = new HttpClientHandler
                    //{
                    //    MaxRequestContentBufferSize = int.MaxValue,
                    //    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    //    UseDefaultCredentials = true
                    //};
                    //handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;

                    //cb.PrimaryHandler = handler;
                    //cb.Build();
                });

            builder.Services.AddTransient<IHttpClientProxy>(sp =>
            {
                try
                {
                    HttpClient client = sp.GetService<IHttpClientFactory>().CreateClient("ExternalHttpClient");
                    var url = sp.GetService<ExternalConnectorProvider>().WorkflowUrl;
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        client.BaseAddress = new Uri(url);
                    }

                    return new DefaultHttpClientProxy(client,
                        sp.GetService<IAccessTokenProvider>(),
                        sp.GetService<IHttpContextAccessor>(),
                        sp.GetService<ILoggerFactory>());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });

            builder.Services.AddSingleton<IIdGenerator>(sp => new GuidGenerator());

            builder.Services.AddUserSession<DefaultUserSession>();

            builder.Services.AddWorkflowAccessTokenProvider<DefaultAccessTokenProvider>();

            builder.Services.AddSingleton<IUserServiceProxy, DefaultUserServiceProxy>();

            builder.Services.AddTransient<IServiceWebApiHttpProxy, ServiceWebApiHttpProxy>();

            builder.AddDataSource()
                .AddDataBaseReader()
                .AddAsyncExecutor()
                .AddProcessEngineConfiguration()
                .AddProcessEngineFactory()
                .AddProcessEngineService();

            return builder;
        }
    }
}
