using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Sys.Net.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sys.Workflow.Polly
{
    public static class PollyPolicyExtensions
    {
        private static ILogger<IHttpClientProxy> logger;

        private static async Task OnFailed(DelegateResult<HttpResponseMessage> res, TimeSpan ts)
        {
            HttpResponseMessage response = res.Result;
            if (response is null && res.Exception is object)
            {
                logger.LogError($"【调用服务失败】" + res.Exception.Message + Environment.NewLine);
            }
            else if (response is object)
            {
                string str = await (response.Content?.ReadAsStringAsync()).ConfigureAwait(false);
                string request = null;

                if (response.RequestMessage.Content is object)
                {
                    request = await response.RequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                logger.LogError($"【调用服务失败】={(int)response.StatusCode}," +
                    $"Url={response.RequestMessage?.RequestUri?.AbsoluteUri}{Environment.NewLine}" +
                    $"{(request is object ? $"参数={request}" : "")}" +
                    $"{(res.Exception is object ? res.Exception.Message : str)}{Environment.NewLine}");
            }
        }

        private static void CreateLogger(IHttpClientBuilder clientBuilder)
        {
            if (logger == null)
            {
                logger = clientBuilder.Services.BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger<IHttpClientProxy>();
            }
        }

        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder clientBuilder)
        {
            return clientBuilder.AddRetryPolicy(async (res, ts) =>
            {
                await OnFailed(res, ts).ConfigureAwait(false);
            });
        }

        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder clientBuilder, Action<DelegateResult<HttpResponseMessage>, TimeSpan> onRetry)
        {
            CreateLogger(clientBuilder);

            IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
              .WaitAndRetryAsync(4, t => TimeSpan.FromMilliseconds(Math.Pow(2, t) * 500), onRetry);

            clientBuilder.AddPolicyHandler(policy);

            return clientBuilder;
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder clientBuilder)
        {
            return clientBuilder.AddCircuitBreakerPolicy(async (res, ts) =>
            {
                await OnFailed(res, ts).ConfigureAwait(false);
            }, () => { });
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder clientBuilder, Action<DelegateResult<HttpResponseMessage>, TimeSpan> onBreak, Action onReset)
        {
            CreateLogger(clientBuilder);

            IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30), onBreak, onReset);

            clientBuilder.AddPolicyHandler(policy);

            return clientBuilder;
        }
    }
}
