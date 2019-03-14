using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Sys.Bpm.Rest.Client
{
    public static class UseWorkflowClientExtension
    {
        internal static readonly string HTTPCLIENT_WORKFLOW = "workflowClient";

        public static IServiceCollection UseWorkflowClient(this IServiceCollection services)
        {
            services.AddHttpClient(HTTPCLIENT_WORKFLOW);

            services.AddTransient<WorkflowHttpInvokerProvider>(sp =>
            {
                HttpClient httpClient = sp.GetService<IHttpClientFactory>().CreateClient();

                return new WorkflowHttpInvokerProvider(httpClient);
            });

            return services;
        }
    }
}
