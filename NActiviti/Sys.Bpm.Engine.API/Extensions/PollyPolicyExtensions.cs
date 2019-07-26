using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace Sys.Workflow.Polly
{
    public static class PollyPolicyExtensions
    {

        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder clientBuilder)
        {
            IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
              .WaitAndRetryAsync(4, t => TimeSpan.FromMilliseconds(Math.Pow(2, t) * 500));

            clientBuilder.AddPolicyHandler(policy);

            return clientBuilder;
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder clientBuilder)
        {
            IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            clientBuilder.AddPolicyHandler(policy);

            return clientBuilder;
        }
    }
}
