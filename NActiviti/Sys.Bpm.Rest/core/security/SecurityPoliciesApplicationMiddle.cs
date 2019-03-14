/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace org.activiti.cloud.services.core
{
    /// <summary>
    /// authentication midlleware
    /// </summary>
    public class SecurityPoliciesApplicationMiddle
    {
        private readonly RequestDelegate next;
        private readonly IOptions<SecurityPoliciesProviderOptions> options;

        /// <summary>
        /// 
        /// </summary>
        public SecurityPoliciesApplicationMiddle(RequestDelegate next, IOptions<SecurityPoliciesProviderOptions> options)
        {
            this.next = next;
            this.options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
#if DEBUG
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("/swagger"))
            {
                await next(context);
                return;
            }
#endif

            TokenUserProvider userProvider = context.RequestServices.GetRequiredService<TokenUserProvider>();

            SecurityPoliciesApplicationService spas = context.RequestServices.GetService<SecurityPoliciesApplicationService>();

            spas.User = await userProvider.FromRequestHeader(context);

            await next(context);
        }
    }
}