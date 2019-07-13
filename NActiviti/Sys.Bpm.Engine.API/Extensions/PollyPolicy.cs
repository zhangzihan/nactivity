using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Sys.Workflow
{
    /// <summary>
    /// Polly policy
    /// </summary>
    public class PollyPolicy
    {
        /// <summary>
        /// 重试策略，重试4次，间隔 500ms 1s 2s 8s
        /// </summary>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
              .WaitAndRetryAsync(4, t => TimeSpan.FromMilliseconds(Math.Pow(2, t) * 500));

        }

        /// <summary>
        /// 熔断策略，5次30s异常熔断
        /// </summary>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
