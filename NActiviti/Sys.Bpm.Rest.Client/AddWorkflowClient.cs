using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Sys.Extentions;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Util;
using System;
using System.Net;
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
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                .ConfigureHttpMessageHandlerBuilder(cb =>
                {
                    if (cb.PrimaryHandler is HttpClientHandler handler)
                    {
                        handler.MaxRequestContentBufferSize = int.MaxValue;
                        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                        handler.UseDefaultCredentials = true;
                        handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
                    }
                    else
                    {
                        handler = new HttpClientHandler
                        {
                            MaxRequestContentBufferSize = int.MaxValue,
                            ClientCertificateOptions = ClientCertificateOption.Automatic,
                            UseDefaultCredentials = true
                        };
                        handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;

                        cb.PrimaryHandler = handler;
                        cb.Build();
                    }
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

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
              .WaitAndRetryAsync(4, t => TimeSpan.FromMilliseconds(Math.Pow(2, t) * 500));

        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10));
        }
    }
}
