using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sys.Net.Http;
using System.Net.Http;

namespace Sys.Bpm.Rest.Client
{
    public static class UseWorkflowClientExtension
    {
        internal static readonly string HTTPCLIENT_WORKFLOW = "workflowClient";

        /// <summary>
        /// Workflow HttpClient注入
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseWorkflowClient(this IServiceCollection services)
        {
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

            services.AddSingleton<IAccessTokenProvider>(sp => AccessTokenProvider.Create(sp.GetService<ILoggerFactory>()));

            services.AddTransient<WorkflowHttpClientProxyProvider>(sp =>
            {
                HttpClient httpClient = sp.GetService<IHttpClientFactory>().CreateClient(HTTPCLIENT_WORKFLOW);

                return new WorkflowHttpClientProxyProvider(httpClient, sp.GetService<IAccessTokenProvider>(), sp.GetService<IHttpContextAccessor>());
            });

            return services;
        }
    }
}
