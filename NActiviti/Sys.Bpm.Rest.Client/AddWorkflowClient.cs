using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sys.Extentions;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Util;
using System;
using System.Net.Http;

namespace Sys.Workflow.Rest.Client
{
    public static class AddWorkflowClientExtension
    {
        internal static readonly string HTTPCLIENT_WORKFLOW = "workflowClient";

        /// <summary>
        /// Workflow HttpClient注入
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWorkflowClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSpringCoreTypeRepository();

            services.AddHttpClient(HTTPCLIENT_WORKFLOW)
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.MaxRequestContentBufferSize = int.MaxValue;
                    handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                    handler.UseDefaultCredentials = true;
                    handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
                    builder.PrimaryHandler = handler;
                    builder.Build();
                });

            services.AddSingleton<ExternalConnectorProvider>(sp =>
            {
                return new ExternalConnectorProvider(configuration);
            });

            services.AddTransient<IAccessTokenProvider, DefaultAccessTokenProvider>();

            services.AddHttpClient<DefaultHttpClientProxy>(HTTPCLIENT_WORKFLOW);

            services.AddTransient<IHttpClientProxy>(sp =>
            {
                IHttpClientProxy httpClientProxy = sp.GetService<DefaultHttpClientProxy>();

                var eco = sp.GetService<ExternalConnectorProvider>();
                var url = eco.WorkflowUrl;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    httpClientProxy.HttpClient.BaseAddress = new Uri(url);
                }

                return httpClientProxy;
            });

            services.AddTransient<WorkflowHttpClientProxyProvider>(sp =>
            {
                return new WorkflowHttpClientProxyProvider(sp.GetService<IHttpClientProxy>());
            });

            services.AddUserSession<DefaultUserSession>();

            return services;
        }
    }
}
